// using Microsoft.OpenApi.Models;
// using DotNetEnv;
// using ModelContextProtocol.Server;
// using ModelContextProtocol.AspNetCore;
// using PowerBI_MCP.Handlers;
// using PowerBI_MCP.Service;
// using PowerBI_MCP.Interfaces;
// using System.Reflection;
// using Microsoft.AspNetCore.Builder;

// var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddControllersWithViews();

// // Swagger
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "powerbi-mcp API", Version = "v1" });
// });

// // Dependency injection
// builder.Services.AddScoped<IConnectionService, ConnectionService>();
// builder.Services.AddScoped<ConnectionHandler>();
// builder.Services.AddScoped<FileHandler>();
// builder.Services.AddScoped<DocumentationHandler>();

// // ✅ MCP over HTTP transport
// builder.Services
//     .AddMcpServer()
//     .WithStdioServerTransport()
//     .WithToolsFromAssembly(typeof(Program).Assembly);

// Env.Load();

// var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c =>
//     {
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "powerbi-mcp API v1");
//         c.RoutePrefix = string.Empty;
//     });
// }

// app.UseHttpsRedirection();
// app.UseRouting();
// app.UseAuthorization();

// app.MapStaticAssets();
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}")
//     .WithStaticAssets();

// // ✅ Map MCP endpoint
// app.MapMcp();

// app.Run();

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

using PowerBI_MCP.Service;
using PowerBI_MCP.Interfaces;
using PowerBI_MCP.Handlers;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Dependency injection
builder.Services.AddScoped<IConnectionService, ConnectionService>();
builder.Services.AddScoped<ConnectionHandler>();
builder.Services.AddScoped<FileHandler>();
builder.Services.AddScoped<DocumentationHandler>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();