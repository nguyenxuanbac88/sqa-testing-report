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
    public class FoodCreateTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private LoginAdminPage loginAdminPage;
        private FoodCreatePage foodPage;
        private readonly string baseUrl = "http://api.dvxuanbac.com:2080";

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            driver = DriverFactory.InitDriver();
            loginAdminPage = new LoginAdminPage(driver);
            foodPage = new FoodCreatePage(driver);
            _excelPath = Path.Combine(PathHelper.GetRepoRoot(TestContext.CurrentContext.WorkDirectory), "Data", "DataTest.xlsx");
            excelHelper = new ExcelTestCaseHelper(_excelPath);

            loginAdminPage.AutoLoginAdmin(baseUrl, "khoa992005@gmail.com", "Khoa@123");
        }

        [OneTimeTearDown]
        public void GlobalTearDown() { if (driver != null) { driver.Quit(); driver.Dispose(); } }

        // Hàm so sánh thông minh chống rác
        private bool IsSmartMatch(string actual, string expected)
        {
            if (string.IsNullOrEmpty(actual) || string.IsNullOrEmpty(expected)) return false;
            string cleanActual = actual.ToLower().Replace("!", "").Replace(".", "").Trim();
            string cleanExpected = expected.ToLower().Replace("!", "").Replace(".", "").Trim();
            return cleanActual.Contains(cleanExpected) || cleanExpected.Contains(cleanActual);
        }

        // --- Danh sách 7 Test Case Combo ---
        [TestCase("TC_FOODCRE_01", TestName = "TC_FOODCRE_01")]
        [TestCase("TC_FOODCRE_02", TestName = "TC_FOODCRE_02")]
        [TestCase("TC_FOODCRE_03", TestName = "TC_FOODCRE_03")]
        [TestCase("TC_FOODCRE_04", TestName = "TC_FOODCRE_04")]
        [TestCase("TC_FOODCRE_05", TestName = "TC_FOODCRE_05")]
        [TestCase("TC_FOODCRE_06", TestName = "TC_FOODCRE_06")]
        [TestCase("TC_FOODCRE_07", TestName = "TC_FOODCRE_07")]
        public void Execute_CreateFood_TestCase(string tcId)
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
                        case "1": // Vào trang tạo món ăn
                            driver.Navigate().GoToUrl($"{baseUrl}/FoodManagement/CreateFood");
                            Thread.Sleep(1000);
                            step.ActualResult = "Đã vào trang tạo món ăn (Combo)";
                            break;

                        case "2": // Tên món ăn
                            foodPage.EnterName(step.TestData);
                            step.ActualResult = $"Đã nhập tên món: {step.TestData}";
                            break;

                        case "3": // Mô tả
                            foodPage.EnterDescription(step.TestData);
                            step.ActualResult = $"Đã nhập mô tả: {step.TestData}";
                            break;

                        case "4": // Giá
                            foodPage.EnterPrice(step.TestData);
                            step.ActualResult = $"Đã nhập giá: {step.TestData}";
                            break;

                        case "5": // Link hình ảnh
                            foodPage.EnterImageURL(step.TestData);
                            step.ActualResult = $"Đã nhập link hình ảnh: {step.TestData}";
                            break;

                        case "6": // Nhấn nút Tạo (hoặc Cập nhật theo ghi chú trong Excel)
                            foodPage.ClickCreate();
                            Thread.Sleep(1500);

                            string finalResult = foodPage.GetNotificationText();
                            step.ActualResult = "Hệ thống báo: " + finalResult;

                            // BẮT BUỘC PHẢI SO SÁNH VỚI EXPECTED RESULT CỦA KHOA
                            if (!string.IsNullOrEmpty(expected))
                            {
                                if (IsSmartMatch(finalResult, expected))
                                {
                                    step.Status = "Pass";
                                    // Dùng Exception để ngắt vòng lặp gọn gàng
                                    throw new Exception($"[DỪNG SỚM] Kết quả thực tế đã khớp với mong đợi: {expected}");
                                }
                                else
                                {
                                    // Nếu web báo "Thành công" mà Khoa mong đợi "Lỗi giá âm" -> ĐÁNH FAIL NGAY LẬP TỨC
                                    throw new Exception($"Thông báo '{finalResult}' KHÔNG KHỚP mong đợi '{expected}'");
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