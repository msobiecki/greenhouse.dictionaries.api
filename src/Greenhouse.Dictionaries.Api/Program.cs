using Greenhouse.Dictionaries.Infrastructure;
using Greenhouse.Dictionaries.Application;
using Greenhouse.Dictionaries.Presentation;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.UseInfrastructure();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddPresentation();

var app = builder.Build();

app.UsePresentation();

app.Run();

