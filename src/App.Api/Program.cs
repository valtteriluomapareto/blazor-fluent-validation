using App.Contracts;
using App.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost(
    "/api/sample-form",
    async (SampleForm model, IValidator<SampleForm> validator) =>
    {
        var result = await validator.ValidateAsync(model);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray()
                );

            var errorCodes = result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorCode).ToArray()
                );

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed."
            };
            problemDetails.Extensions["errorCodes"] = errorCodes;

            return Results.BadRequest(problemDetails);
        }

        return Results.Ok(new SampleFormResponse("Form is valid."));
    }
);

app.Run();
