using ClosedXML.Excel;
using sqa_testing_report.Models;

namespace sqa_testing_report.Services
{
    public class ExcelTestCaseHelper
    {
        private readonly string _excelPath;

        public ExcelTestCaseHelper(string excelPath = null)
        {
            _excelPath = excelPath ?? Path.Combine(AppContext.BaseDirectory, "Data", "DataTest.xlsx");
        }

        // Đã nâng cấp hàm đọc an toàn hơn bằng .Value.ToString()
        private string GetCellValue(IXLCell cell)
        {
            try
            {
                if (cell.IsMerged())
                {
                    return cell.MergedRange().FirstCell().Value.ToString()?.Trim() ?? string.Empty;
                }
                return cell.Value.ToString()?.Trim() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        private void SetCellValue(IXLCell cell, string value)
        {
            if (cell.IsMerged())
            {
                cell.MergedRange().FirstCell().Value = value;
            }
            else
            {
                cell.Value = value;
            }
        }

        public List<TestCaseStep> ReadTestCaseById(string sheetName, string testCaseId)
        {
            if (!File.Exists(_excelPath))
                throw new FileNotFoundException("File Excel không tồn tại", _excelPath);

            using var wb = new XLWorkbook(_excelPath);
            var ws = wb.Worksheet(sheetName);
            var headerRow = ws.FirstRowUsed();
            if (headerRow == null) return new List<TestCaseStep>();

            var headerCells = headerRow.Cells().Select((c, idx) => new { Name = (c.GetString() ?? string.Empty).Trim(), Index = c.Address.ColumnNumber })
                                               .Where(h => !string.IsNullOrEmpty(h.Name))
                                               .ToDictionary(h => h.Name, h => h.Index, StringComparer.OrdinalIgnoreCase);

            if (!headerCells.TryGetValue("Test Case ID", out int tcidCol))
                throw new ArgumentException("Không tìm thấy cột 'Test Case ID'");

            var results = new List<TestCaseStep>();

            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var cellTcid = GetCellValue(row.Cell(tcidCol));

                if (string.Equals(cellTcid, testCaseId, StringComparison.OrdinalIgnoreCase))
                {
                    var step = new TestCaseStep();

                    // Đã bổ sung lấy cột Step #
                    headerCells.TryGetValue("No.", out int noCol);
                    headerCells.TryGetValue("Step #", out int stepNumCol); // <-- FIX Ở ĐÂY
                    headerCells.TryGetValue("Step Action", out int actionCol);
                    headerCells.TryGetValue("Test Data", out int dataCol);
                    headerCells.TryGetValue("Expected Result", out int expCol);

                    step.No = noCol > 0 ? GetCellValue(row.Cell(noCol)) : string.Empty;
                    step.TestCaseID = cellTcid;
                    step.StepNumber = stepNumCol > 0 ? GetCellValue(row.Cell(stepNumCol)) : string.Empty; // <-- FIX Ở ĐÂY
                    step.StepAction = actionCol > 0 ? GetCellValue(row.Cell(actionCol)) : string.Empty;
                    step.TestData = dataCol > 0 ? GetCellValue(row.Cell(dataCol)) : string.Empty;
                    step.ExpectedResult = expCol > 0 ? GetCellValue(row.Cell(expCol)) : string.Empty;
                    step.RowNumber = row.RowNumber();

                    results.Add(step);
                }
            }
            return results;
        }

        public void WriteTestCaseSteps(string sheetName, List<TestCaseStep> stepsToUpdate)
        {
            using var wb = new XLWorkbook(_excelPath);
            var ws = wb.Worksheet(sheetName);
            var headerRow = ws.FirstRowUsed();

            var headerCells = headerRow.Cells().Select((c, idx) => new { Name = (c.GetString() ?? string.Empty).Trim(), Index = c.Address.ColumnNumber })
                                               .Where(h => !string.IsNullOrEmpty(h.Name))
                                               .ToDictionary(h => h.Name, h => h.Index, StringComparer.OrdinalIgnoreCase);

            headerCells.TryGetValue("Actual Result", out int actCol);
            headerCells.TryGetValue("Status", out int statusCol);
            headerCells.TryGetValue("Screenshots", out int shotCol);

            foreach (var step in stepsToUpdate)
            {
                if (step.RowNumber <= 0) continue;

                var row = ws.Row(step.RowNumber);

                // Đã sửa lại thành != null để cho phép xóa dữ liệu cũ (ghi đè chuỗi rỗng)
                if (actCol > 0 && step.ActualResult != null)
                    SetCellValue(row.Cell(actCol), step.ActualResult);

                if (statusCol > 0 && step.Status != null)
                    SetCellValue(row.Cell(statusCol), step.Status);

                if (shotCol > 0 && step.Screenshots != null)
                    SetCellValue(row.Cell(shotCol), step.Screenshots);
            }
            wb.Save();
        }
    }
}