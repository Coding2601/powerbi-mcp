using Microsoft.OpenApi.Models;
using DotNetEnv;

using PowerBI_MCP.Handlers;
using PowerBI_MCP.Service;
using PowerBI_MCP.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "powerbi-mcp API", Version = "v1" });
});

builder.Services.AddScoped<IConnectionService, ConnectionService>();

builder.Services.AddScoped<ConnectionHandler>();
builder.Services.AddScoped<FileHandler>();

Env.Load();

var app = builder.Build();

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
