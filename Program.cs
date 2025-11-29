using Microsoft.OpenApi.Models;
using DotNetEnv;
using PowerBI_MCP.Handlers;
using PowerBI_MCP.Service;
using PowerBI_MCP.Interfaces;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "powerbi-mcp API", Version = "v1" });
});

// Dependency injection
builder.Services.AddScoped<IDocumentationService, DocumentationService>();
builder.Services.AddScoped<IConnectionService, ConnectionService>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IDaxService, DaxService>();
builder.Services.AddScoped<ConnectionHandler>();
builder.Services.AddScoped<FileHandler>();
builder.Services.AddScoped<UnusedHandler>();
builder.Services.AddScoped<DocumentationHandler>();
builder.Services.AddScoped<ComplianceHandler>();
builder.Services.AddScoped<DaxHandler>();

builder.Services.AddAuthentication("DefaultScheme")
    .AddScheme<AuthenticationSchemeOptions, DummyAuthHandler>("DefaultScheme", null);

builder.Services.AddAuthorization();

Env.Load();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "powerbi-mcp API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using ModelContextProtocol.Server;
// using System.ComponentModel;

// using PowerBI_MCP.Service;
// using PowerBI_MCP.Interfaces;
// using PowerBI_MCP.Handlers;

// var builder = Host.CreateApplicationBuilder(args);
// builder.Logging.AddConsole(consoleLogOptions =>
// {
//     consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
// });

// builder.Services.AddScoped<IDocumentationService, DocumentationService>();
// builder.Services.AddScoped<IConnectionService, ConnectionService>();
// builder.Services.AddScoped<IIssueService, IssueService>();
// builder.Services.AddScoped<ConnectionHandler>();
// builder.Services.AddScoped<FileHandler>();
// builder.Services.AddScoped<UnusedHandler>();
// builder.Services.AddScoped<DocumentationHandler>();
// builder.Services.AddScoped<ComplianceHandler>();

// builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly();

// await builder.Build().RunAsync();