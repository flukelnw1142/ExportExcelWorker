using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExportExcelWorker.Service
{
    public class ExcelService
    {
        private readonly string _outputPath;

        public ExcelService(IConfiguration configuration)
        {
            _outputPath = configuration.GetSection("AppSettings")["OutputPath"];

            // ตรวจสอบว่ามีเครื่องหมาย "\" ท้าย path หรือไม่ ถ้าไม่มีให้เพิ่ม
            if (!string.IsNullOrEmpty(_outputPath) && !_outputPath.EndsWith("\\"))
            {
                _outputPath += "\\";
            }
        }

        public string ExportToExcel(IEnumerable<dynamic> data)
        {
            if (data == null || !data.Any())
            {
                Console.WriteLine("No data to export.");
                return null;
            }

            if (string.IsNullOrEmpty(_outputPath))
            {
                Console.WriteLine("Error: Output path is not configured properly.");
                return null;
            }

            if (!Directory.Exists(_outputPath))
            {
                Console.WriteLine($"Creating directory: {_outputPath}");
                Directory.CreateDirectory(_outputPath);
            }

            string filePath = Path.Combine(_outputPath, $"ApprovedCustomers_{DateTime.Now:yyyyMMdd}.xlsx");

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Approved Customers");
                    int row = 1, col = 1;

                    var firstItem = data.FirstOrDefault();
                    if (firstItem == null)
                    {
                        Console.WriteLine("No data structure found.");
                        return null;
                    }

                    var fieldNames = ((IDictionary<string, object>)firstItem).Keys.ToList();

                    foreach (var field in fieldNames)
                    {
                        worksheet.Cell(row, col).Value = field;
                        col++;
                    }
                    worksheet.Row(row).Style.Font.Bold = true;

                    foreach (var item in data)
                    {
                        row++;
                        col = 1;
                        foreach (var field in fieldNames)
                        {
                            worksheet.Cell(row, col).Value = ((IDictionary<string, object>)item)[field]?.ToString();
                            col++;
                        }
                    }

                    workbook.SaveAs(filePath);
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File creation failed.");
                    return null;
                }

                Console.WriteLine($"Excel file successfully created: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while exporting Excel: {ex.Message}");
                return null;
            }
        }

    }
}
