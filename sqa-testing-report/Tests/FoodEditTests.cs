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
    public class FoodEditTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private LoginAdminPage loginAdminPage;
        private FoodEditPage foodEditPage;
        private readonly string baseUrl = "http://api.dvxuanbac.com:2080";

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            driver = DriverFactory.InitDriver();
            loginAdminPage = new LoginAdminPage(driver);
            foodEditPage = new FoodEditPage(driver);
            _excelPath = Path.Combine(PathHelper.GetRepoRoot(TestContext.CurrentContext.WorkDirectory), "Data", "DataTest.xlsx");
            excelHelper = new ExcelTestCaseHelper(_excelPath);

            loginAdminPage.AutoLoginAdmin(baseUrl, "khoa992005@gmail.com", "Khoa@123");
        }

        [OneTimeTearDown]
        public void GlobalTearDown() { if (driver != null) { driver.Quit(); driver.Dispose(); } }

        private bool IsSmartMatch(string actual, string expected)
        {
            if (string.IsNullOrEmpty(actual) || string.IsNullOrEmpty(expected)) return false;
            string cleanActual = actual.ToLower().Replace("!", "").Replace(".", "").Trim();
            string cleanExpected = expected.ToLower().Replace("!", "").Replace(".", "").Trim();
            return cleanActual.Contains(cleanExpected) || cleanExpected.Contains(cleanActual);
        }

        [TestCase("TC_FOODEDIT_01", TestName = "TC_FOODEDIT_01")]
        [TestCase("TC_FOODEDIT_02", TestName = "TC_FOODEDIT_02")]
        [TestCase("TC_FOODEDIT_03", TestName = "TC_FOODEDIT_03")]
        [TestCase("TC_FOODEDIT_04", TestName = "TC_FOODEDIT_04")]
        [TestCase("TC_FOODEDIT_05", TestName = "TC_FOODEDIT_05")]
        [TestCase("TC_FOODEDIT_06", TestName = "TC_FOODEDIT_06")]
        [TestCase("TC_FOODEDIT_07", TestName = "TC_FOODEDIT_07")]
        public void Execute_EditFood_TestCase(string tcId)
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
                        case "1": // Đi thẳng vào link Edit Food 94
                            driver.Navigate().GoToUrl($"{baseUrl}/FoodManagement/EditFood?idFood=94");
                            Thread.Sleep(1000);
                            step.ActualResult = "Đã vào trang chỉnh sửa món ăn ID 94";
                            break;

                        case "2": // Sửa Tên
                            foodEditPage.EditName(step.TestData);
                            step.ActualResult = $"Sửa tên: {step.TestData}";
                            break;

                        case "3": // Sửa Giá
                            foodEditPage.EditPrice(step.TestData);
                            step.ActualResult = $"Sửa giá: {step.TestData}";
                            break;

                        case "4": // Sửa Mô tả
                            foodEditPage.EditDescription(step.TestData);
                            step.ActualResult = $"Sửa mô tả: {step.TestData}";
                            break;

                        case "5": // Sửa Link ảnh
                            foodEditPage.EditImageURL(step.TestData);
                            step.ActualResult = $"Sửa link ảnh: {step.TestData}";
                            break;

                        case "6": // Nhấn Cập nhật
                            string currentUrlBeforeClick = driver.Url;
                            foodEditPage.ClickUpdate();
                            Thread.Sleep(1500);

                            // 1. Kiểm tra trường hợp chuyển trang (Case 01 mong đợi Chuyển trang)
                            if (expected.ToLower().Contains("chuyển sang trang danh sách"))
                            {
                                if (driver.Url != currentUrlBeforeClick && !driver.Url.Contains("EditFood"))
                                {
                                    step.Status = "Pass";
                                    step.ActualResult = "Hệ thống đã chuyển sang trang danh sách.";
                                    throw new Exception($"[DỪNG SỚM] Đã cập nhật thành công và chuyển trang!");
                                }
                                else
                                {
                                    throw new Exception($"Kỳ vọng chuyển trang nhưng hệ thống vẫn đang kẹt lại ở: {driver.Url}");
                                }
                            }

                            // 2. Bắt các thông báo lỗi (Các Case còn lại mong đợi bắt lỗi UI)
                            string finalResult = foodEditPage.GetNotificationText();
                            step.ActualResult = "Hệ thống báo: " + finalResult;

                            if (!string.IsNullOrEmpty(expected))
                            {
                                if (IsSmartMatch(finalResult, expected))
                                {
                                    step.Status = "Pass";
                                    throw new Exception($"[DỪNG SỚM] Kết quả thực tế đã khớp với mong đợi: {expected}");
                                }
                                else
                                {
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