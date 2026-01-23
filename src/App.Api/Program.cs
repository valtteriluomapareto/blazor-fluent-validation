using App.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();

var app = builder.Build();

app.MapApiEndpoints();

app.Run();

public partial class Program;
