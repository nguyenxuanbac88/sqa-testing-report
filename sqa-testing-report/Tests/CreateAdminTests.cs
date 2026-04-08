using NUnit.Framework;
using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;
using System;
using System.Linq;
using System.Threading;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class CreateAdminTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;

        private LoginAdminPage loginAdminPage;
        private CreateAdminPage createAdminPage;
        private readonly string baseUrl = "http://api.dvxuanbac.com:2080";

        // 1. OneTimeSetUp: CHỈ CHẠY 1 LẦN DUY NHẤT LÚC BẮT ĐẦU NHẤN "RUN ALL"
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            driver = DriverFactory.InitDriver();
            loginAdminPage = new LoginAdminPage(driver);
            createAdminPage = new CreateAdminPage(driver);

            string start = TestContext.CurrentContext.WorkDirectory;
            string repoRoot = PathHelper.GetRepoRoot(start);
            _excelPath = repoRoot != null ? Path.Combine(repoRoot, "Data", "DataTest.xlsx") : Path.Combine(start, "Data", "DataTest.xlsx");

            excelHelper = new ExcelTestCaseHelper(_excelPath);

            // Đăng nhập 1 LẦN DUY NHẤT cho toàn bộ bộ Test
            loginAdminPage.AutoLoginAdmin(baseUrl, "khoa992005@gmail.com", "Khoa@123");
        }

        // 2. SetUp: CHẠY TRƯỚC MỖI TEST CASE ĐỂ RESET LẠI FORM TRẮNG
        [SetUp]
        public void SetupForEachTest()
        {
            createAdminPage.GoToPage(baseUrl);
            Thread.Sleep(1500); // Chờ form tải xong mới nhập
        }

        // 3. OneTimeTearDown: CHỈ ĐÓNG TRÌNH DUYỆT KHI ĐÃ CHẠY XONG HẾT CÁC CASE
        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }

        [TestCase("TC_ADCREATEMV_01")]
        [TestCase("TC_ADCREATEMV_02")]
        [TestCase("TC_ADCREATEMV_03")]
        [TestCase("TC_ADCREATEMV_04")]
        [TestCase("TC_ADCREATEMV_05")]
        [TestCase("TC_ADCREATEMV_06")]
        [TestCase("TC_ADCREATEMV_07")]
        [TestCase("TC_ADCREATEMV_08")]
        [TestCase("TC_ADCREATEMV_09")]
        [TestCase("TC_ADCREATEMV_10")]
        [TestCase("TC_ADCREATEMV_11")]
        [TestCase("TC_ADCREATEMV_12")]
        [TestCase("TC_ADCREATEMV_13")]
        [TestCase("TC_ADCREATEMV_14")]
        [TestCase("TC_ADCREATEMV_15")]
        [TestCase("TC_ADCREATEMV_16")]
        [TestCase("TC_ADCREATEMV_17")]
        [TestCase("TC_ADCREATEMV_18")]
        [TestCase("TC_ADCREATEMV_19")]
        [TestCase("TC_ADCREATEMV_20")]
        [TestCase("TC_ADCREATEMV_21")]
        // Thêm bao nhiêu TestCase vào đây cũng được, nó sẽ chạy vèo vèo
        public void Execute_CreateMovie_TestCase(string tcId)
        {
            string sheetName = "Khoa_automationTC";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId} trong file Excel");

            // --- Chuẩn bị Data ---
            string name = steps.FirstOrDefault(s => s.StepNumber == "1")?.TestData ?? "";
            string genre = steps.FirstOrDefault(s => s.StepNumber == "2")?.TestData ?? "";
            string duration = steps.FirstOrDefault(s => s.StepNumber == "3")?.TestData ?? "";
            string director = steps.FirstOrDefault(s => s.StepNumber == "4")?.TestData ?? "";
            string startDate = steps.FirstOrDefault(s => s.StepNumber == "5")?.TestData ?? "";
            string endDate = steps.FirstOrDefault(s => s.StepNumber == "6")?.TestData ?? "";
            string coverUrl = steps.FirstOrDefault(s => s.StepNumber == "7")?.TestData ?? "";
            string trailerUrl = steps.FirstOrDefault(s => s.StepNumber == "8")?.TestData ?? "";
            string desc = steps.FirstOrDefault(s => s.StepNumber == "9")?.TestData ?? "";
            string ageLimit = steps.FirstOrDefault(s => s.StepNumber == "10")?.TestData ?? "";
            string producer = steps.FirstOrDefault(s => s.StepNumber == "11")?.TestData ?? "";
            string actor = steps.FirstOrDefault(s => s.StepNumber == "12")?.TestData ?? "";

            bool isPreviousStepFailed = false;

            // --- THỰC THI CÁC BƯỚC ---
            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Fail do bước trước đó đã thất bại.";
                    step.Screenshots = null;
                    continue;
                }

                try
                {
                    string action = step.StepAction.ToLower();

                    if (int.TryParse(step.StepNumber, out int stepNum) && stepNum >= 1 && stepNum <= 12)
                    {
                        if (stepNum == 1)
                        {
                            createAdminPage.FillMovieData(name, genre, duration, director, startDate, endDate,
                                                          coverUrl, trailerUrl, desc, ageLimit, producer, actor);
                        }
                        step.ActualResult = $"Đã nhập: {step.TestData}";
                    }

                    else if (step.StepNumber == "13" && action.Contains("lưu phim"))
                    {
                        createAdminPage.ClickSave();

                        string actualNotification = createAdminPage.GetSystemNotificationText();
                        step.ActualResult = $"Hệ thống báo: {actualNotification}";
                        string expectedResult = step.ExpectedResult?.Trim() ?? "";

                        if (!string.IsNullOrEmpty(expectedResult))
                        {
                            bool isMatch = actualNotification.IndexOf(expectedResult, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (!isMatch)
                            {
                                if (expectedResult.Contains("Chuyển sang") || expectedResult.Contains("Danh sách"))
                                {
                                    Thread.Sleep(2000);
                                    if (!driver.Url.Contains("LoadListMovie"))
                                    {
                                        throw new Exception($"Không chuyển trang. URL: {driver.Url}. Hệ thống: {actualNotification}");
                                    }
                                    step.ActualResult = "Đã lưu và chuyển hướng về danh sách phim thành công.";
                                }
                                else
                                {
                                    throw new Exception($"Mong đợi: '{expectedResult}' NHƯNG hệ thống báo: '{actualNotification}'");
                                }
                            }
                        }
                    }

                    step.Status = "Pass";
                    step.Screenshots = "";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;
                    if (OperatingSystem.IsWindows())
                        step.Screenshots = ScreenshotHelper.Capture(tcId);

                    isPreviousStepFailed = true;
                }
            }

            excelHelper.WriteTestCaseSteps(sheetName, steps);

            if (steps.Any(s => s.Status == "Fail"))
            {
                Assert.Fail($"Test Case {tcId} FAIL. Đã chụp ảnh và lưu log vào Excel.");
            }
        }
    }
}