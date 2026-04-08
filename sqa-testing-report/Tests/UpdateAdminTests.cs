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
    public class UpdateAdminTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private LoginAdminPage loginAdminPage;
        private UpdateAdminPage updateAdminPage;
        private readonly string baseUrl = "http://api.dvxuanbac.com:2080";

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            driver = DriverFactory.InitDriver();
            loginAdminPage = new LoginAdminPage(driver);
            updateAdminPage = new UpdateAdminPage(driver);

            // Lấy đường dẫn Excel chuẩn
            _excelPath = Path.Combine(PathHelper.GetRepoRoot(TestContext.CurrentContext.WorkDirectory), "Data", "DataTest.xlsx");
            excelHelper = new ExcelTestCaseHelper(_excelPath);

            // Đăng nhập 1 lần duy nhất cho toàn bộ class test
            loginAdminPage.AutoLoginAdmin(baseUrl, "khoa992005@gmail.com", "Khoa@123");
        }

        // --- ĐÃ CẬP NHẬT THEO Ý BẠN: GIẢI PHÓNG DRIVER TẠI ĐÂY ---
        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            if (driver != null)
            {
                driver.Quit();    // Đóng trình duyệt
                driver.Dispose(); // Giải phóng hoàn toàn tài nguyên driver
            }
        }

        [TestCase("TC_ADUPDATEMV_01", TestName = "TC_ADUPDATEMV_01")]
        [TestCase("TC_ADUPDATEMV_02", TestName = "TC_ADUPDATEMV_02")]
        [TestCase("TC_ADUPDATEMV_03", TestName = "TC_ADUPDATEMV_03")]
        [TestCase("TC_ADUPDATEMV_04", TestName = "TC_ADUPDATEMV_04")]
        [TestCase("TC_ADUPDATEMV_05", TestName = "TC_ADUPDATEMV_05")]
        [TestCase("TC_ADUPDATEMV_06", TestName = "TC_ADUPDATEMV_06")]
        [TestCase("TC_ADUPDATEMV_07", TestName = "TC_ADUPDATEMV_07")]
        [TestCase("TC_ADUPDATEMV_08", TestName = "TC_ADUPDATEMV_08")]
        [TestCase("TC_ADUPDATEMV_09", TestName = "TC_ADUPDATEMV_09")]
        [TestCase("TC_ADUPDATEMV_10", TestName = "TC_ADUPDATEMV_10")]
        [TestCase("TC_ADUPDATEMV_11", TestName = "TC_ADUPDATEMV_11")]
        [TestCase("TC_ADUPDATEMV_12", TestName = "TC_ADUPDATEMV_12")]
        [TestCase("TC_ADUPDATEMV_13", TestName = "TC_ADUPDATEMV_13")]
        [TestCase("TC_ADUPDATEMV_14", TestName = "TC_ADUPDATEMV_14")]
        [TestCase("TC_ADUPDATEMV_15", TestName = "TC_ADUPDATEMV_15")]
        [TestCase("TC_ADUPDATEMV_16", TestName = "TC_ADUPDATEMV_16")]
        [TestCase("TC_ADUPDATEMV_17", TestName = "TC_ADUPDATEMV_17")]
        [TestCase("TC_ADUPDATEMV_18", TestName = "TC_ADUPDATEMV_18")]
        [TestCase("TC_ADUPDATEMV_19", TestName = "TC_ADUPDATEMV_19")]
        

        public void Execute_UpdateMovie_TestCase(string tcId)
        {
            string sheetName = "Khoa_automationTC";
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy dữ liệu cho {tcId}");

            // Điều hướng đến trang sửa phim (Ví dụ ID là 12)
            driver.Navigate().GoToUrl($"{baseUrl}/MovieManagement/EditMovie/117");
            Thread.Sleep(1500);

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    continue;
                }

                try
                {
                    if (int.TryParse(step.StepNumber, out int stepNum))
                    {
                        // Bước 1 đến 11: Logic nhập liệu/xóa trống
                        if (stepNum >= 1 && stepNum <= 11)
                        {
                            // Chỉ thực hiện điền form tổng thể một lần duy nhất tại bước nhập liệu đầu tiên tìm thấy
                            if (step == steps.First(s => int.Parse(s.StepNumber) <= 11))
                            {
                                updateAdminPage.FillDataBySteps(steps);
                            }

                            // Cập nhật kết quả vào Actual Result trong Excel cho đẹp
                            step.ActualResult = string.IsNullOrWhiteSpace(step.TestData)
                                ? "Đã xóa trống trường này (theo yêu cầu của Khoa)"
                                : $"Đã sửa thành: {step.TestData}";
                        }
                        // Bước 12: Nhấn nút Lưu phim
                        else if (stepNum == 12 || step.StepAction.ToLower().Contains("lưu"))
                        {
                            updateAdminPage.ClickSave();
                            string msg = updateAdminPage.GetSystemNotificationText();
                            step.ActualResult = $"Hệ thống báo: {msg}";

                            // So sánh với kết quả mong đợi
                            string expected = step.ExpectedResult?.Trim() ?? "";
                            if (!string.IsNullOrEmpty(expected) && !msg.Contains(expected, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new Exception($"Thông báo thực tế '{msg}' không khớp với mong đợi '{expected}'");
                            }
                        }
                    }
                    step.Status = "Pass";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi thực thi: " + ex.Message;

                    // Chụp ảnh màn hình lỗi (chỉ trên Windows)
                    if (OperatingSystem.IsWindows())
                        step.Screenshots = ScreenshotHelper.Capture(tcId);

                    isPreviousStepFailed = true;
                }
            }

            // Ghi lại toàn bộ kết quả của các bước vào file Excel
            excelHelper.WriteTestCaseSteps(sheetName, steps);

            // Chốt hạ trạng thái cho Test Explorer
            if (steps.Any(s => s.Status == "Fail"))
            {
                Assert.Fail($"Test Case {tcId} bị thất bại. Vui lòng kiểm tra file Excel để biết chi tiết.");
            }
        }
    }
}