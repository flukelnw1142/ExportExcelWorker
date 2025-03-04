using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using ExportExcelWorker.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build());

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<ExcelService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// ให้ Service ตอบสนองเร็วขึ้น
host.Services.GetRequiredService<ILogger<Worker>>().LogInformation("Starting Worker Service...");
host.Run();
