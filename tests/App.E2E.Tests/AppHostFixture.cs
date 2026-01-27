using System.Collections.Concurrent;
using System.Diagnostics;

namespace App.E2E.Tests;

public sealed class AppHostFixture : IAsyncLifetime
{
    private const string DefaultBaseUrl = "http://127.0.0.1:5010";
    private readonly ConcurrentQueue<string> outputLines = new();
    private Process? hostProcess;

    public Uri BaseUri { get; private set; } = null!;
    public string BaseUrl => BaseUri.ToString().TrimEnd('/');

    public async ValueTask InitializeAsync()
    {
        var baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = Environment.GetEnvironmentVariable("E2E_APP_URL") ?? DefaultBaseUrl;
            BaseUri = new Uri(NormalizeBaseUrl(baseUrl));
            StartHostProcess(BaseUri);
        }
        else
        {
            BaseUri = new Uri(NormalizeBaseUrl(baseUrl));
        }

        await WaitForHealthyAsync();
    }

    public ValueTask DisposeAsync()
    {
        if (hostProcess is not null)
        {
            try
            {
                if (!hostProcess.HasExited)
                {
                    hostProcess.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException)
            {
                // Process already exited.
            }
            finally
            {
                hostProcess.Dispose();
            }
        }

        return ValueTask.CompletedTask;
    }

    private void StartHostProcess(Uri baseUri)
    {
        var root = FindSolutionRoot();
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(Path.Combine("src", "App.Host", "App.Host.csproj"));
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(baseUri.ToString());

        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_URLS"] = baseUri.ToString();

        hostProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        hostProcess.OutputDataReceived += (_, args) => CaptureOutput(args.Data);
        hostProcess.ErrorDataReceived += (_, args) => CaptureOutput(args.Data);
        hostProcess.Start();
        hostProcess.BeginOutputReadLine();
        hostProcess.BeginErrorReadLine();
    }

    private void CaptureOutput(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return;
        }

        outputLines.Enqueue(data);
        while (outputLines.Count > 30 && outputLines.TryDequeue(out _)) { }
    }

    private async Task WaitForHealthyAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var endpoint = new Uri(BaseUri, "/sample-form");
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);

        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Wait and retry until the host is ready.
            }

            await Task.Delay(500);
        }

        var output = string.Join(Environment.NewLine, outputLines);
        throw new TimeoutException(
            $"App.Host did not become ready at {endpoint}. Output:{Environment.NewLine}{output}"
        );
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var trimmed = baseUrl.Trim();
        return trimmed.EndsWith('/') ? trimmed.TrimEnd('/') : trimmed;
    }

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "FormValidationTest.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate solution root for App.Host.");
    }
}
