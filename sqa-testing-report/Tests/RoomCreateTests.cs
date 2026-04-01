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
    public class RoomCreateTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private LoginAdminPage loginAdminPage;
        private RoomCreatePage roomPage;
        private readonly string baseUrl = "http://api.dvxuanbac.com:2080";

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            driver = DriverFactory.InitDriver();
            loginAdminPage = new LoginAdminPage(driver);
            roomPage = new RoomCreatePage(driver);
            _excelPath = Path.Combine(PathHelper.GetRepoRoot(TestContext.CurrentContext.WorkDirectory), "Data", "DataTest.xlsx");
            excelHelper = new ExcelTestCaseHelper(_excelPath);

            loginAdminPage.AutoLoginAdmin(baseUrl, "khoa992005@gmail.com", "Khoa@123");
        }

        [OneTimeTearDown]
        public void GlobalTearDown() { if (driver != null) { driver.Quit(); driver.Dispose(); } }

        // Hàm so sánh "Smart Match" không phân biệt hoa/thường, dấu chấm than
        private bool IsSmartMatch(string actual, string expected)
        {
            if (string.IsNullOrEmpty(actual) || string.IsNullOrEmpty(expected)) return false;
            string cleanActual = actual.ToLower().Replace("!", "").Replace(".", "").Trim();
            string cleanExpected = expected.ToLower().Replace("!", "").Replace(".", "").Trim();
            return cleanActual.Contains(cleanExpected) || cleanExpected.Contains(cleanActual);
        }

        [TestCase("TC_ADCREATERM_01", TestName = "TC_ADCREATERM_01")]
        [TestCase("TC_ADCREATERM_02", TestName = "TC_ADCREATERM_02")]
        [TestCase("TC_ADCREATERM_03", TestName = "TC_ADCREATERM_03")]
        [TestCase("TC_ADCREATERM_04", TestName = "TC_ADCREATERM_04")]
        [TestCase("TC_ADCREATERM_05", TestName = "TC_ADCREATERM_05")]
        [TestCase("TC_ADCREATERM_06", TestName = "TC_ADCREATERM_06")]
        [TestCase("TC_ADCREATERM_07", TestName = "TC_ADCREATERM_07")]
        public void Execute_CreateRoom_TestCase(string tcId)
        {
            string sheetName = "Thanh_automationTC";
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy data cho {tcId}");

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed) { step.Status = "Fail"; step.ActualResult = "Bỏ qua do bước trước đó thất bại."; continue; }

                try
                {
                    string expected = step.ExpectedResult?.Trim() ?? "";

                    switch (step.StepNumber)
                    {
                        case "1": // Chọn rạp chiếu -> Đi thẳng vào trang Edit Cinema ID 40
                            driver.Navigate().GoToUrl($"{baseUrl}/CinemaManagement/EditCinema?idCinema=40");
                            Thread.Sleep(1000);
                            step.ActualResult = "Đã vào trang thông tin rạp chiếu (ID 40)";
                            break;

                        case "2": // Chọn thêm phòng chiếu
                            roomPage.ClickAddRoom();
                            Thread.Sleep(1000); // Chờ chuyển sang trang tạo phòng
                            step.ActualResult = "Đã nhấn nút Thêm phòng chiếu";
                            break;

                        case "3": // Nhập RoomName
                            roomPage.EnterRoomName(step.TestData);
                            step.ActualResult = $"Nhập RoomName: {step.TestData}";
                            break;

                        case "4": // Nhập RoomImageURL
                            roomPage.EnterRoomImageURL(step.TestData);
                            step.ActualResult = $"Nhập RoomImageURL: {step.TestData}";
                            break;

                        case "5": // Nhập RoomType
                            roomPage.EnterRoomType(step.TestData);
                            step.ActualResult = $"Nhập RoomType: {step.TestData}";
                            break;

                        case "6": // Chọn Status
                            roomPage.SetStatus(step.TestData);
                            step.ActualResult = $"Chọn Status: {step.TestData}";
                            break;

                        case "7": // Nhấn nút Lưu và Validate
                            roomPage.ClickSave();
                            Thread.Sleep(1500); // Chờ server xử lý hoặc validate đỏ

                            string finalResult = roomPage.GetNotificationText();
                            // Nếu form hợp lệ bị đẩy đi (không có lỗi), hệ thống có thể chuyển trang và báo rỗng
                            if (string.IsNullOrEmpty(finalResult) && driver.Url.Contains("EditCinema"))
                            {
                                finalResult = "Tạo phòng chiếu thành công";
                            }

                            step.ActualResult = "Hệ thống báo: " + finalResult;

                            // KIỂM TRA MONG ĐỢI TỪ EXCEL
                            if (!string.IsNullOrEmpty(expected))
                            {
                                if (IsSmartMatch(finalResult, expected))
                                {
                                    step.Status = "Pass";
                                    throw new Exception($"[DỪNG SỚM] Kết quả thực tế khớp mong đợi: {expected}");
                                }
                                else
                                {
                                    throw new Exception($"Thông báo thực tế '{finalResult}' KHÔNG KHỚP mong đợi '{expected}'");
                                }
                            }
                            break;
                    }
                    step.Status = "Pass";
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("[DỪNG SỚM]"))
                    {
                        step.Status = "Pass";
                        isPreviousStepFailed = true;
                    }
                    else
                    {
                        step.Status = "Fail";
                        step.ActualResult = "LỖI TẠI BƯỚC NÀY: " + ex.Message;
                        if (OperatingSystem.IsWindows()) step.Screenshots = ScreenshotHelper.Capture(tcId);
                        isPreviousStepFailed = true;
                    }
                }
            }
            excelHelper.WriteTestCaseSteps(sheetName, steps);
            if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"TC {tcId} thất bại.");
        }
    }
}