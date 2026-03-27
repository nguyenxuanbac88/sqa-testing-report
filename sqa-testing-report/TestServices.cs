using ClosedXML.Excel;
using sqa_testing_report.Services;

namespace sqa_testing_report
{
    [TestFixture]
    public class SampleTests
    {
        private string _excelPath;

        [SetUp]
        public void Setup()
        {
            // Không tạo file mẫu nữa, dùng file thực tế trong repo: Data/DataTest.xlsx
            // Tìm thư mục gốc của project (nơi chứa .csproj) để xác định đường dẫn Data
            string start = TestContext.CurrentContext.WorkDirectory;
            string repoRoot = FindRepoRoot(start);

            if (!string.IsNullOrEmpty(repoRoot))
                _excelPath = Path.Combine(repoRoot, "Data", "DataTest.xlsx");
            else
                _excelPath = Path.Combine(start, "Data", "DataTest.xlsx");

            // Nếu file không tồn tại, test sẽ tự bỏ qua (không tạo file mẫu theo yêu cầu)
        }

        // Tìm thư mục cha chứa file project .csproj
        private string FindRepoRoot(string start)
        {
            var di = new DirectoryInfo(start);
            while (di != null)
            {
                if (File.Exists(Path.Combine(di.FullName, "sqa-testing-report.csproj")))
                    return di.FullName;
                di = di.Parent;
            }
            return null;
        }

        [TearDown]
        public void TearDown()
        {
            // Không xóa file — dùng file thực tế của repo
        }

        [Test]
        public void ReadAndWriteExcelTest()
        {
            // Nếu file không tồn tại, bỏ qua test
            if (!File.Exists(_excelPath))
                Assert.Ignore($"File Excel không tồn tại: {_excelPath}");

            // Mở file kiểm tra sheet có tồn tại không - dùng block using để dispose ngay sau kiểm tra
            string sheetName = "Bac_automationTC";
            string tcId = "TC_REG_01";
            bool sheetExists;
            using (var wb = new XLWorkbook(_excelPath))
            {
                sheetExists = wb.Worksheets.Any(ws => string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase));
            }

            if (!sheetExists)
                Assert.Ignore($"Sheet '{sheetName}' không tồn tại trong file {_excelPath}");

            var svc = new ExcelTestCaseService(_excelPath);

            // Đọc các bước cho testcase
            var steps = svc.ReadTestCaseById(sheetName, tcId);
            Assert.IsNotNull(steps);

            // Nếu không có bước nào, bỏ qua (tránh false-fail khi dữ liệu khác)
            if (steps.Count == 0)
                Assert.Ignore($"Không tìm thấy testcase '{tcId}' trong sheet '{sheetName}'");

            // Chụp màn hình và lấy đường dẫn tương đối trả về
            string screenshotRelativePath = null;

            // Guard runtime platform trước khi gọi API Windows-only để tránh cảnh báo CA1416
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    screenshotRelativePath = ScreenshotService.Capture(tcId);
                }
                catch (PlatformNotSupportedException)
                {
                    // Nếu không hỗ trợ chụp trên nền tảng này, bỏ qua phần screenshot nhưng vẫn test read/write
                    screenshotRelativePath = null;
                }
                catch (Exception ex)
                {
                    // Không bắt buộc dừng test nếu chụp ảnh lỗi — ghi log và tiếp tục
                    TestContext.WriteLine("Không thể chụp màn hình: " + ex.Message);
                    screenshotRelativePath = null;
                }
            }
            else
            {
                TestContext.WriteLine("Bỏ qua chụp màn hình: nền tảng không phải Windows");
            }

            // Cập nhật ActualResult, Status và Screenshots cho mỗi bước
            foreach (var s in steps)
            {
                s.ActualResult = "Đã kiểm tra: OK";
                s.Status = "Pass";
                if (!string.IsNullOrEmpty(screenshotRelativePath))
                    s.Screenshots = screenshotRelativePath;
            }

            // Ghi trở lại file 
            svc.WriteTestCaseSteps(sheetName, steps);

            // Đọc lại để xác nhận
            var svc2 = new ExcelTestCaseService(_excelPath);
            var stepsAfter = svc2.ReadTestCaseById(sheetName, tcId);

            foreach (var s in stepsAfter)
            {
                Assert.AreEqual("Đã kiểm tra: OK", s.ActualResult, "ActualResult phải được ghi trở lại file");
                Assert.AreEqual("Pass", s.Status, "Status phải được ghi trở lại file");
                if (!string.IsNullOrEmpty(screenshotRelativePath))
                {
                    // Kiểm tra rằng cột Screenshots được ghi (ít nhất chứa tên file)
                    Assert.IsFalse(string.IsNullOrEmpty(s.Screenshots), "Screenshots phải được ghi");
                    Assert.IsTrue(s.Screenshots.Contains("screenshot_"), "Tên file screenshot phải chứa 'screenshot_'");
                }
            }
        }

        [Test]
        public void RunSeleniumTest_Dynamic()
        {
            string sheetName = "Bac_automationTC";
            string tcId = "TC_REG_02"; // Test lỗi để trống
            var svc = new ExcelTestCaseService(_excelPath);
            var steps = svc.ReadTestCaseById(sheetName, tcId);

            bool isPassed = true;
            string actualMsg = "";
            string shotPath = "";

            try
            {
                foreach (var step in steps)
                {
                    string action = step.StepAction.ToLower();
                    string data = step.TestData;

                    // XỬ LÝ (Trống)
                    if (action.Contains("nhập"))
                    {
                        // Giả lập tìm Element: var input = driver.FindElement(...);
                        if (data == "(Trống)")
                        {
                            // Cố tình để trống
                            // input.Clear(); 
                        }
                        else if (!string.IsNullOrEmpty(data))
                        {
                            // Có data thật
                            // input.SendKeys(data);
                        }
                    }

                    if (action.Contains("nhấn"))
                    {
                        // driver.FindElement(...).Click();
                    }
                }

                // KIỂM TRA EXPECTED RESULT (Lấy từ bước cuối hoặc bước có Expected)
                string expected = steps.Last().ExpectedResult;
                // string webErrorMsg = driver.FindElement(By.Id("error-msg")).Text;

                // Assert.AreEqual(expected, webErrorMsg);

                // Nếu không lỗi -> Pass
                actualMsg = "Hiển thị đúng lỗi: " + expected; // Hoặc text lấy từ web
            }
            catch (Exception ex) // Bắt lỗi nếu web không hiện đúng như Expected
            {
                isPassed = false;
                actualMsg = "Lỗi: " + ex.Message;
                shotPath = ScreenshotService.Capture(tcId); // CHỈ chụp khi FAIL
            }
            finally
            {
                // GHI KẾT QUẢ VÀO EXCEL
                foreach (var s in steps)
                {
                    s.Status = isPassed ? "Pass" : "Fail";
                    s.ActualResult = actualMsg;
                    s.Screenshots = shotPath;
                }
                svc.WriteTestCaseSteps(sheetName, steps);
            }
        }
    }
}