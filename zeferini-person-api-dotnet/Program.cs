using Microsoft.OpenApi.Models;
using FluentValidation;
using ZeferiniPersonApi.DTOs;
using ZeferiniPersonApi.Models;
using ZeferiniPersonApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ZeferiniPersonApi.Filters.FluentValidationActionFilter>();
});
builder.Services.AddTransient<IValidator<CreatePersonDto>, CreatePersonDtoValidator>();
builder.Services.AddTransient<IValidator<UpdatePersonDto>, UpdatePersonDtoValidator>();
builder.Services.AddTransient<IValidator<Person>, PersonValidator>();
builder.Services.AddTransient<IValidator<Event>, EventValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Zeferini Person API",
        Version = "v1",
        Description = "API for managing persons with Event Sourcing"
    });
});

// Register application services
builder.Services.AddSingleton<IEventsService, EventsService>();
builder.Services.AddScoped<IPersonService, PersonService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Zeferini Person API v1");
    options.RoutePrefix = string.Empty;
});
app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
app.Run($"http://0.0.0.0:{port}");
public partial class Program { } // For integration testing purposes

