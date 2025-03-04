using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ExportExcelWorker.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly DatabaseService _databaseService;
    private readonly ExcelService _excelService;
    private readonly EmailService _emailService;
    private readonly TimeSpan _runTime;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, DatabaseService databaseService, ExcelService excelService, EmailService emailService)
    {
        _logger = logger;
        _configuration = configuration;
        _databaseService = databaseService;
        _excelService = excelService;
        _emailService = emailService;

        string runTimeConfig = _configuration["AppSettings:RunTime" ?? "20:00"];
        if (TimeSpan.TryParseExact(runTimeConfig, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan parsedTime))
        {
            _runTime = parsedTime;
        }
        else
        {
            _runTime = new TimeSpan(21, 0, 0); // ค่าเริ่มต้น 21:00 ถ้า config ผิด
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Service is starting...");

        // รัน Loop ตลอดเวลา แต่ตรวจสอบเวลา 21:00 ก่อนทำงาน
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime now = DateTime.Now;
            TimeSpan currentTime = now.TimeOfDay;

            if (currentTime.Hours == _runTime.Hours && currentTime.Minutes == _runTime.Minutes)
            {
                _logger.LogInformation("Fetching approved customers...");
                var data = await _databaseService.GetApprovedCustomersTodayAsync();

                if (data == null || !data.Any())
                {
                    _logger.LogWarning("No approved customers found today. Skipping Excel export and email.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                _logger.LogInformation($"Retrieved {data.Count()} records from the database.");
                string filePath = _excelService.ExportToExcel(data);

                if (!string.IsNullOrEmpty(filePath))
                {
                    List<string> emailRecipients = await _databaseService.GetEmailsForExportWorkerAsync();
                    _logger.LogInformation("Sending Email...");
                    _emailService.SendEmailWithAttachment(filePath, emailRecipients);
                    _logger.LogInformation("Process completed successfully!");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // ป้องกันรันซ้ำ
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
