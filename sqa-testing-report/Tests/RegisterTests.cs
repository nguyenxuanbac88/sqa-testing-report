using OpenQA.Selenium;
using sqa_testing_report.Models;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class RegisterTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private RegisterPage registerPage;
        private readonly string sheetName = "Bac_automationTC";

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            registerPage = new RegisterPage(driver);
            string start = TestContext.CurrentContext.WorkDirectory;
            string repoRoot = PathHelper.GetRepoRoot(start);
            _excelPath = repoRoot != null ? Path.Combine(repoRoot, "Data", "DataTest.xlsx") : Path.Combine(start, "Data", "DataTest.xlsx");
            excelHelper = new ExcelTestCaseHelper(_excelPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null) { driver.Quit(); driver.Dispose(); }
        }

        #region Các hàm Test Case riêng biệt gọi lại hàm chung

        [Test]
        public void TC_REG_01_KiemTraDangKiHopLe() => ExecuteRegisterTest("TC_REG_01", isSuccessCase: true);

        [Test]
        public void TC_REG_02_KiemTraLoiKhiDeTrongHoTen() => ExecuteRegisterTest("TC_REG_02");

        [Test]
        public void TC_REG_03_KiemTraLoiEmailKhongHopLe() => ExecuteRegisterTest("TC_REG_03");

        [Test]
        public void TC_REG_04_KiemTraLoiSDTKhongHopLe() => ExecuteRegisterTest("TC_REG_04");

        [Test]
        public void TC_REG_05_KiemTraLoiMatKhauYeu() => ExecuteRegisterTest("TC_REG_05");

        // SỬA Ở ĐÂY: Thêm isSuccessCase: true để báo hàm chung tự sinh Email và SĐT động, chống lỗi trùng lặp
        [Test]
        public void TC_REG_06_KiemTraLoiNhapLaiMatKhauKhongKhop() => ExecuteRegisterTest("TC_REG_06", isSuccessCase: true);

        [Test]
        public void TC_REG_07_KiemTraDangKyVoiEmailDaTonTai() => ExecuteRegisterTest("TC_REG_07");

        [Test]
        public void TC_REG_08_KiemTraDangKyVoiKhoangTrang() => ExecuteRegisterTest("TC_REG_08", addSpaces: true);

        [Test]
        public void TC_REG_09_KiemTraDangKyVoiNgaySinhTuongLai() => ExecuteRegisterTest("TC_REG_09");

        #endregion

        /// <summary>
        /// Hàm thực thi chung cho tất cả các Test Case Register
        /// </summary>
        private void ExecuteRegisterTest(string tcId, bool isSuccessCase = false, bool addSpaces = false)
        {
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            registerPage.GoToHomePage();
            registerPage.SwitchToVietnamese();

            bool isPreviousStepFailed = false;

            // Tạo data động cho trường hợp được gọi
            string dynamicEmail = $"testuser{DateTime.Now:yyyyMMddHHmmss}@gmail.com";
            string dynamicPhone = $"09{DateTime.Now:HHmmssff}";

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Đánh giá Fail do bước trước đó đã thất bại.";
                    continue;
                }

                try
                {
                    string stepNum = step.StepNumber?.Trim();
                    string testData = step.TestData?.Trim() ?? "";

                    // Xử lý TC_REG_08: Thêm khoảng trắng vào 2 đầu
                    if (addSpaces && !string.IsNullOrEmpty(testData) && stepNum != "1" && stepNum != "9")
                    {
                        testData = "   " + testData + "   ";
                    }

                    switch (stepNum)
                    {
                        case "1":
                            registerPage.OpenRegisterModal();
                            System.Threading.Thread.Sleep(500);
                            step.ActualResult = "Đã mở form đăng ký.";
                            break;
                        case "2":
                            registerPage.EnterFullName(testData);
                            step.ActualResult = $"Đã nhập Họ tên: '{testData}'";
                            break;
                        case "3":
                            if (isSuccessCase) testData = dynamicEmail;
                            registerPage.EnterEmail(testData);
                            step.ActualResult = $"Đã nhập Email: '{testData}'";
                            break;
                        case "4":
                            if (isSuccessCase) testData = dynamicPhone;
                            registerPage.EnterPhone(testData);
                            step.ActualResult = $"Đã nhập SĐT: '{testData}'";
                            break;
                        case "5":
                            registerPage.SelectGender(testData);
                            step.ActualResult = $"Đã chọn giới tính: {testData}";
                            break;
                        case "6":
                            string dob = testData.Contains(" ") ? testData.Split(' ')[0] : testData;
                            registerPage.SelectDOB(dob);
                            step.ActualResult = $"Đã chọn ngày sinh: {dob}";
                            break;
                        case "7":
                            registerPage.EnterPassword(testData);
                            step.ActualResult = "Đã nhập mật khẩu.";
                            break;
                        case "8":
                            registerPage.EnterConfirmPassword(testData);
                            step.ActualResult = "Đã nhập xác nhận mật khẩu.";
                            break;
                        case "9":
                            registerPage.SubmitRegistration();
                            System.Threading.Thread.Sleep(1000);
                            VerifyExpectedResult(tcId, step);
                            break;
                    }

                    step.Status = "Pass";
                    step.Screenshots = "";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;
                    if (OperatingSystem.IsWindows()) step.Screenshots = ScreenshotHelper.Capture(tcId);
                    isPreviousStepFailed = true;
                }
            }

            excelHelper.WriteTestCaseSteps(sheetName, steps);
            if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"Test Case {tcId} thất bại.");
        }

        /// <summary>
        /// Logic kiểm tra kết quả mong đợi riêng cho từng loại lỗi
        /// </summary>
        private void VerifyExpectedResult(string tcId, TestCaseStep step)
        {
            // Lấy nội dung Alert (nếu có)
            string alertText = registerPage.GetAlertText();

            switch (tcId)
            {
                case "TC_REG_01":
                case "TC_REG_08":
                    Assert.IsNotNull(alertText, "Không thấy thông báo thành công.");
                    Assert.IsTrue(alertText.ToLower().Contains("thành công"), $"Thông báo thực tế: {alertText}");
                    step.ActualResult = "Hệ thống báo: " + alertText;
                    break;

                case "TC_REG_02":
                    Assert.IsTrue(registerPage.IsHtml5ValidationTriggered("name"), "Không trigger lỗi trống tên.");
                    step.ActualResult = "Hệ thống chặn submit do trống tên.";
                    break;

                case "TC_REG_03":
                    Assert.IsTrue(registerPage.IsHtml5ValidationTriggered("email"), "Không trigger lỗi định dạng email.");
                    step.ActualResult = "Hệ thống chặn submit do email sai định dạng.";
                    break;

                case "TC_REG_04":
                    Assert.IsNotNull(alertText, "Không thấy Alert báo lỗi SĐT.");
                    Assert.IsTrue(alertText.Contains("Số điện thoại không hợp lệ"), $"Lỗi thực tế: {alertText}");
                    step.ActualResult = "Hệ thống báo lỗi SĐT không hợp lệ thành công.";
                    break;

                case "TC_REG_05":
                    Assert.IsNotNull(alertText, "Không thấy Alert báo lỗi mật khẩu yếu.");
                    Assert.IsTrue(alertText.Contains("không đúng định dạng"), $"Lỗi thực tế: {alertText}");
                    step.ActualResult = "Hệ thống báo lỗi mật khẩu không đúng định dạng thành công.";
                    break;

                case "TC_REG_06":
                    Assert.IsNotNull(alertText, "Không thấy Alert báo lỗi mật khẩu không khớp.");
                    Assert.IsTrue(alertText.Contains("không khớp"), $"Lỗi thực tế: {alertText}");
                    step.ActualResult = "Hệ thống báo lỗi mật khẩu không khớp thành công.";
                    break;

                default:
                    // Xử lý chung cho các lỗi Alert khác (TC 07, 09)
                    Assert.IsNotNull(alertText, "Không nhận được thông báo lỗi từ server.");
                    step.ActualResult = "Hệ thống hiển thị lỗi: " + alertText;
                    break;
            }
        }
    }
}