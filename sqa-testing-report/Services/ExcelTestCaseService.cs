using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using sqa_testing_report.Models;

namespace sqa_testing_report.Services
{
    // D?ch v? ??c/ghi d? li?u testcase t? file Excel
    public class ExcelTestCaseService
    {
        private readonly string _excelPath;

        // Kh?i t?o v?i ???ng d?n file excel (m?c ??nh Data/DataTest.xlsx)
        public ExcelTestCaseService(string excelPath = null)
        {
            _excelPath = excelPath ?? Path.Combine(AppContext.BaseDirectory, "Data", "DataTest.xlsx");
        }

        // ??c t?t c? b??c c?a 1 test case d?a trên sheetName và testCaseId
        // Tr? v? danh sách TestCaseStep
        public List<TestCaseStep> ReadTestCaseById(string sheetName, string testCaseId)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
                throw new ArgumentException("Tên sheet không ???c r?ng", nameof(sheetName));

            if (string.IsNullOrWhiteSpace(testCaseId))
                throw new ArgumentException("TestCase ID không ???c r?ng", nameof(testCaseId));

            if (!File.Exists(_excelPath))
                throw new FileNotFoundException("File Excel không t?n t?i", _excelPath);

            using var wb = new XLWorkbook(_excelPath);

            if (!wb.Worksheets.Any(ws => string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Sheet '{sheetName}' không t?n t?i trong file Excel");

            var ws = wb.Worksheet(sheetName);

            // Tìm hàng tiêu ?? (header row) - gi? ??nh là hàng ??u tiên có các header
            var headerRow = ws.FirstRowUsed();
            if (headerRow == null)
                return new List<TestCaseStep>();

            // Xây b?ng map t? tên c?t sang index
            var headerCells = headerRow.Cells().Select((c, idx) => new { Name = (c.GetString() ?? string.Empty).Trim(), Index = c.Address.ColumnNumber })
                                               .Where(h => !string.IsNullOrEmpty(h.Name))
                                               .ToDictionary(h => h.Name, h => h.Index, StringComparer.OrdinalIgnoreCase);

            // Danh sách tên c?t mà ta quan tâm - có th? m? r?ng ho?c ch?nh n?u header khác
            string[] neededColumns = new[] {
                "No.",
                "Test Requirement ID",
                "Test Case ID",
                "Test Objective",
                "Pre-conditions",
                "Step #",
                "Step Action",
                "Test Data",
                "Expected Result",
                "Actual Result",
                "Status",
                "Screenshots"
            };

            // Tìm c?t Test Case ID
            if (!headerCells.TryGetValue("Test Case ID", out int tcidCol))
            {
                // th? các bi?n th? tên c?t khác (n?u có) - vi?t thêm rules n?u c?n
                var alt = headerCells.Keys.FirstOrDefault(k => k.IndexOf("Test Case", StringComparison.OrdinalIgnoreCase) >= 0);
                if (alt != null)
                    tcidCol = headerCells[alt];
                else
                    throw new ArgumentException("Không tìm th?y c?t 'Test Case ID' trong sheet");
            }

            var results = new List<TestCaseStep>();

            // Duy?t t? hàng k? ti?p c?a header ??n h?t d? li?u
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var cellTcid = row.Cell(tcidCol).GetString()?.Trim();
                if (string.Equals(cellTcid, testCaseId, StringComparison.OrdinalIgnoreCase))
                {
                    var step = new TestCaseStep();

                    // L?y t?ng tr??ng n?u c?t t?n t?i
                    headerCells.TryGetValue("No.", out int noCol);
                    headerCells.TryGetValue("Test Requirement ID", out int reqCol);
                    headerCells.TryGetValue("Test Objective", out int objCol);
                    headerCells.TryGetValue("Pre-conditions", out int preCol);
                    headerCells.TryGetValue("Step #", out int stepNumCol);
                    headerCells.TryGetValue("Step Action", out int actionCol);
                    headerCells.TryGetValue("Test Data", out int dataCol);
                    headerCells.TryGetValue("Expected Result", out int expCol);
                    headerCells.TryGetValue("Actual Result", out int actCol);
                    headerCells.TryGetValue("Status", out int statusCol);
                    headerCells.TryGetValue("Screenshots", out int shotCol);

                    step.No = noCol > 0 ? row.Cell(noCol).GetString() : string.Empty;
                    step.TestRequirementID = reqCol > 0 ? row.Cell(reqCol).GetString() : string.Empty;
                    step.TestCaseID = cellTcid;
                    step.TestObjective = objCol > 0 ? row.Cell(objCol).GetString() : string.Empty;
                    step.PreConditions = preCol > 0 ? row.Cell(preCol).GetString() : string.Empty;
                    step.StepNumber = stepNumCol > 0 ? row.Cell(stepNumCol).GetString() : string.Empty;
                    step.StepAction = actionCol > 0 ? row.Cell(actionCol).GetString() : string.Empty;
                    step.TestData = dataCol > 0 ? row.Cell(dataCol).GetString() : string.Empty;
                    step.ExpectedResult = expCol > 0 ? row.Cell(expCol).GetString() : string.Empty;
                    step.ActualResult = actCol > 0 ? row.Cell(actCol).GetString() : string.Empty;
                    step.Status = statusCol > 0 ? row.Cell(statusCol).GetString() : string.Empty;
                    step.Screenshots = shotCol > 0 ? row.Cell(shotCol).GetString() : string.Empty;
                    step.RowNumber = row.RowNumber();

                    results.Add(step);
                }
            }

            return results;
        }

        // Ghi c?p nh?t 1 danh sách step (ví d? c?p nh?t ActualResult/Status) tr? l?i sheet
        // Hàm này s? ghi d?a trên RowNumber c?a TestCaseStep
        public void WriteTestCaseSteps(string sheetName, List<TestCaseStep> stepsToUpdate)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
                throw new ArgumentException("Tên sheet không ???c r?ng", nameof(sheetName));

            if (stepsToUpdate == null || stepsToUpdate.Count == 0)
                return;

            if (!File.Exists(_excelPath))
                throw new FileNotFoundException("File Excel không t?n t?i", _excelPath);

            using var wb = new XLWorkbook(_excelPath);

            if (!wb.Worksheets.Any(ws => string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Sheet '{sheetName}' không t?n t?i trong file Excel");

            var ws = wb.Worksheet(sheetName);
            var headerRow = ws.FirstRowUsed();
            if (headerRow == null)
                throw new InvalidOperationException("Sheet không có header");

            var headerCells = headerRow.Cells().Select((c, idx) => new { Name = (c.GetString() ?? string.Empty).Trim(), Index = c.Address.ColumnNumber })
                                               .Where(h => !string.IsNullOrEmpty(h.Name))
                                               .ToDictionary(h => h.Name, h => h.Index, StringComparer.OrdinalIgnoreCase);

            // Tìm các c?t c?n c?p nh?t
            headerCells.TryGetValue("Actual Result", out int actCol);
            headerCells.TryGetValue("Status", out int statusCol);
            headerCells.TryGetValue("Screenshots", out int shotCol);

            foreach (var step in stepsToUpdate)
            {
                if (step.RowNumber <= 0)
                    continue; // b? qua n?u không bi?t row

                var row = ws.Row(step.RowNumber);
                if (actCol > 0)
                    row.Cell(actCol).Value = step.ActualResult ?? string.Empty;
                if (statusCol > 0)
                    row.Cell(statusCol).Value = step.Status ?? string.Empty;
                if (shotCol > 0)
                    row.Cell(shotCol).Value = step.Screenshots ?? string.Empty;
            }

            // L?u ?è file
            wb.Save();
        }
    }
}