using System.Runtime.CompilerServices;

namespace App.E2E.Tests;

internal static class TestReporter
{
    public static void Step(
        ITestOutputHelper output,
        string message,
        [CallerMemberName] string? testName = null
    )
    {
        output.WriteLine($"E2E|{testName}|{message}");
    }
}
