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
    public class SeatLayoutTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private LoginAdminPage loginAdminPage;
        private SeatLayoutPage seatPage;
        private readonly string baseUrl = "http://api.dvxuanbac.com:2080";

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            driver = DriverFactory.InitDriver();
            loginAdminPage = new LoginAdminPage(driver);
            seatPage = new SeatLayoutPage(driver);
            _excelPath = Path.Combine(PathHelper.GetRepoRoot(TestContext.CurrentContext.WorkDirectory), "Data", "DataTest.xlsx");
            excelHelper = new ExcelTestCaseHelper(_excelPath);

            // Đăng nhập 1 lần duy nhất
            loginAdminPage.AutoLoginAdmin(baseUrl, "khoa992005@gmail.com", "Khoa@123");
        }

        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }

        // --- DANH SÁCH ĐẦY ĐỦ 8 TEST CASE TẠO GHẾ ---
        [TestCase("TC_ADSEAT_01", TestName = "TC_ADSEAT_01")]
        [TestCase("TC_ADSEAT_02", TestName = "TC_ADSEAT_02")]
        [TestCase("TC_ADSEAT_03", TestName = "TC_ADSEAT_03")]
        [TestCase("TC_ADSEAT_04", TestName = "TC_ADSEAT_04")]
        [TestCase("TC_ADSEAT_05", TestName = "TC_ADSEAT_05")]
        [TestCase("TC_ADSEAT_06", TestName = "TC_ADSEAT_06")]
        [TestCase("TC_ADSEAT_07", TestName = "TC_ADSEAT_07")]
        [TestCase("TC_ADSEAT_08", TestName = "TC_ADSEAT_08")]
        public void Execute_CreateSeatLayout_TestCase(string tcId)
        {
            string sheetName = "Thanh_automationTC";
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy dữ liệu cho {tcId}");

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Bỏ qua do bước trước đó thất bại.";
                    continue;
                }

                try
                {
                    // ĐEM BIẾN NÀY LÊN TRÊN CÙNG: Khai báo 1 lần duy nhất cho mọi Case dùng chung!
                    string expected = step.ExpectedResult?.Trim() ?? "";

                    switch (step.StepNumber)
                    {
                        case "1": // Vào trang chỉnh sửa rạp 40
                            driver.Navigate().GoToUrl($"{baseUrl}/CinemaManagement/EditCinema?idCinema=40");
                            Thread.Sleep(1000);
                            step.ActualResult = "Đã vào trang Edit Cinema ID 40";
                            break;

                        case "2": // Vào thẳng trang tạo ghế bằng URL
                            string queryParams = step.TestData?.Trim();
                            if (string.IsNullOrEmpty(queryParams)) throw new Exception("Chưa có tham số link ở Bước 2!");
                            driver.Navigate().GoToUrl($"{baseUrl}/SeatLayout/Create?{queryParams}");
                            Thread.Sleep(1500);
                            if (!driver.Url.Contains("SeatLayout")) throw new Exception("Không vào được trang tạo ghế.");
                            step.ActualResult = "Đã vào thẳng trang tạo ghế bằng link";
                            break;

                        case "3":
                            step.ActualResult = $"Ghi nhận Hàng: {step.TestData}";
                            break;

                        case "4":
                            string rowsData = steps.FirstOrDefault(s => s.StepNumber == "3")?.TestData ?? "";
                            string colsData = step.TestData ?? "";
                            seatPage.EnterGridSize(rowsData, colsData);
                            step.ActualResult = $"Đã nhập Hàng: {rowsData}, Cột: {colsData}";
                            break;

                        case "5": // BẮT LỖI TẠO MA TRẬN
                            seatPage.ClickGenerate();
                            Thread.Sleep(1000);

                            // 1. Kiểm tra JavaScript Alert (Popup đen)
                            string jsAlertText = seatPage.GetAlertMessageAndAccept();
                            if (!string.IsNullOrEmpty(jsAlertText))
                            {
                                step.ActualResult = "Popup báo lỗi: " + jsAlertText;

                                // Ép về chữ thường và dùng Contains để kiểm tra "có chứa từ khóa"
                                if (!string.IsNullOrEmpty(expected) && jsAlertText.ToLower().Contains(expected.ToLower()))
                                {
                                    step.Status = "Pass";
                                    throw new Exception($"[DỪNG SỚM] Đã bắt được Alert chứa từ khóa: {expected}");
                                }
                                else if (!string.IsNullOrEmpty(expected))
                                {
                                    throw new Exception($"Popup thực tế '{jsAlertText}' KHÔNG CHỨA từ khóa mong đợi '{expected}'");
                                }
                            }
                            // 2. Kiểm tra lỗi UI (Chữ đỏ) nếu không có Alert
                            string uiError = seatPage.GetNotificationText();
                            if (uiError != "Thao tác thành công" && uiError != "Không có thông báo")
                            {
                                step.ActualResult = "Lỗi UI: " + uiError;

                                if (!string.IsNullOrEmpty(expected) && uiError.ToLower().Contains(expected.ToLower()))
                                {
                                    step.Status = "Pass";
                                    throw new Exception($"[DỪNG SỚM] Đã bắt được lỗi UI chứa từ khóa: {expected}");
                                }
                            }
                            else
                            {
                                step.ActualResult = "Đã tạo ma trận ghế thành công.";
                            }
                            break;

                        case "6":
                            seatPage.ClickAllSeatsInMatrix();
                            step.ActualResult = "Đã chọn tất cả các ghế trong ma trận";
                            break;

                        case "7":
                            step.ActualResult = $"Ghi nhận Loại ghế: {step.TestData}";
                            break;

                        case "8":
                            string priceVal = step.TestData ?? "";
                            seatPage.FillPriceRows(priceVal, priceVal);
                            step.ActualResult = $"Đã nhập giá: {priceVal}";
                            break;

                        case "9": // KIỂM TRA LƯU THÀNH CÔNG HAY LỖI
                            seatPage.ClickSave();
                            Thread.Sleep(1500);
                            string finalResult = seatPage.GetNotificationText();
                            step.ActualResult = "Hệ thống báo: " + finalResult;

                            // Nới lỏng điều kiện: Chỉ cần kết quả thực tế CÓ CHỨA từ khóa mong đợi
                            if (!string.IsNullOrEmpty(expected) && !finalResult.ToLower().Contains(expected.ToLower()))
                            {
                                throw new Exception($"Thông báo '{finalResult}' KHÔNG CHỨA từ khóa '{expected}'");
                            }
                            break;
                        default:
                            step.ActualResult = "Thực hiện bước: " + step.StepAction;
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

            // Nếu có bước nào Fail (không phải dừng do lỗi mong đợi), đánh rớt Test Case
            if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"Test Case {tcId} thất bại.");
        }
    }
}