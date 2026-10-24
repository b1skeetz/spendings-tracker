using AutoMapper;
using TemplateProject.Api;
using TemplateProject.Api.Endpoints;
using TemplateProject.Api.Middlewares;
using TemplateProject.Application;
using TemplateProject.Infrastructure;
using TemplateProject.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddApi(builder.Configuration);

// Configure the HTTP request pipeline.
var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<ExceptionFilterMiddleware>();

app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors(c =>
    c.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "TemplateProject API V1");
    });
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.RegisterFeatureEndpoints();

app.Run();
