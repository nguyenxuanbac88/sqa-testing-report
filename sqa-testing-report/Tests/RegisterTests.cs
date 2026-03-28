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
        private AdminHelperPage adminHelper;
        private readonly string sheetName = "Bac_automationTC";

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            registerPage = new RegisterPage(driver);
            adminHelper = new AdminHelperPage(driver);

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

        // CẦN DATA MỚI -> checkAdminForUnique: true
        [Test]
        public void TC_REG_01_KiemTraDangKiHopLe() => ExecuteRegisterTest("TC_REG_01", checkAdminForUnique: true);

        [Test]
        public void TC_REG_02_KiemTraLoiKhiDeTrongHoTen() => ExecuteRegisterTest("TC_REG_02");

        [Test]
        public void TC_REG_03_KiemTraLoiEmailKhongHopLe() => ExecuteRegisterTest("TC_REG_03");

        [Test]
        public void TC_REG_04_KiemTraLoiSDTKhongHopLe() => ExecuteRegisterTest("TC_REG_04");

        [Test]
        public void TC_REG_05_KiemTraLoiMatKhauYeu() => ExecuteRegisterTest("TC_REG_05");

        // CẦN DATA MỚI -> checkAdminForUnique: true
        [Test]
        public void TC_REG_06_KiemTraLoiNhapLaiMatKhauKhongKhop() => ExecuteRegisterTest("TC_REG_06", checkAdminForUnique: true);

        // --- HÀM SETUP DATA CHO TC_REG_07 ---
        [Test]
        public void Setup_Data_For_TC_REG_07()
        {
            string tcId = "TC_REG_07";
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            var emailStep = steps.FirstOrDefault(s => s.StepNumber?.Trim() == "3");
            string emailToCreate = emailStep?.TestData?.Trim() ?? "nguyenkhauser1@gmail.com";
            string randomPhone = $"09{DateTime.Now:HHmmssff}";

            registerPage.GoToHomePage();
            registerPage.SwitchToVietnamese();
            registerPage.OpenRegisterModal();
            System.Threading.Thread.Sleep(500);

            registerPage.EnterFullName("User TC07 Setup");
            registerPage.EnterEmail(emailToCreate);
            registerPage.EnterPhone(randomPhone);
            registerPage.SelectGender("Nam");
            registerPage.SelectDOB("01/01/2000");
            registerPage.EnterPassword("Kho@09092005");
            registerPage.EnterConfirmPassword("Kho@09092005");

            registerPage.SubmitRegistration();
            System.Threading.Thread.Sleep(1000);

            string alertText = registerPage.GetAlertText();
            Assert.IsNotNull(alertText, "Không nhận được thông báo sau khi nhấn đăng ký data mẫu.");
            Assert.IsTrue(alertText.ToLower().Contains("thành công"), $"Không thể tạo data mẫu. Hệ thống báo: {alertText}");
            TestContext.WriteLine($"✅ Đã tạo thành công tài khoản tiền đề với Email: {emailToCreate} và SĐT: {randomPhone}");
        }

        // CẦN DATA ĐÃ TỒN TẠI -> checkAdminForExisting: true
        [Test]
        public void TC_REG_07_KiemTraDangKyVoiEmailDaTonTai() => ExecuteRegisterTest("TC_REG_07", checkAdminForExisting: true);

        // --- 5 TEST CASE MỚI ---
        [Test]
        public void TC_REG_08_KiemTraDangKyVoiKhoangTrang() => ExecuteRegisterTest("TC_REG_08", addSpaces: true);

        [Test]
        public void TC_REG_09_KiemTraDangKyVoiNgaySinhTuongLai() => ExecuteRegisterTest("TC_REG_09");

        [Test]
        public void TC_REG_10_KiemTraDangKyVoiSQLInjection() => ExecuteRegisterTest("TC_REG_10");

        [Test]
        public void TC_REG_11_KiemTraGioiHanDoDaiHoTen() => ExecuteRegisterTest("TC_REG_11");

        [Test]
        public void TC_REG_12_KiemTraDangKyVoiSDTChuaChuCai() => ExecuteRegisterTest("TC_REG_12");

        #endregion

        /// <summary>
        /// Hàm thực thi chung cho tất cả các Test Case Register
        /// </summary>
        private void ExecuteRegisterTest(string tcId, bool checkAdminForUnique = false, bool checkAdminForExisting = false, bool addSpaces = false)
        {
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            string dynamicEmail = "";
            string dynamicPhone = "";

            if (checkAdminForUnique || checkAdminForExisting)
            {
                adminHelper.EnsureLogin("khoa992005@gmail.com", "Khoa@123");

                if (checkAdminForUnique)
                {
                    dynamicEmail = $"testuser{DateTime.Now:yyyyMMddHHmmss}@gmail.com";
                    bool isPhoneUnique = false;
                    while (!isPhoneUnique)
                    {
                        dynamicPhone = $"09{DateTime.Now:HHmmssff}";
                        bool phoneExists = adminHelper.IsDataExistInUserList(dynamicPhone);
                        if (!phoneExists) isPhoneUnique = true;
                        else System.Threading.Thread.Sleep(500);
                    }
                }
                else if (checkAdminForExisting)
                {
                    var emailStep = steps.FirstOrDefault(s => s.StepNumber?.Trim() == "3");
                    if (emailStep != null && !string.IsNullOrEmpty(emailStep.TestData))
                    {
                        string emailToTest = emailStep.TestData.Trim();
                        bool emailExists = adminHelper.IsDataExistInUserList(emailToTest);
                        if (!emailExists)
                        {
                            Assert.Fail($"Lỗi Tiền đề (Pre-condition): Email '{emailToTest}' trong file Excel CHƯA tồn tại trong Admin. Hãy cập nhật lại Data Test trong file Excel!");
                        }
                    }
                }
            }

            registerPage.GoToHomePage();
            registerPage.SwitchToVietnamese();

            bool isPreviousStepFailed = false;

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

                    // CỘNG KHOẢNG TRẮNG CHO TC_REG_08 (Bỏ qua Form load (1), Giới tính (5), Ngày sinh (6), Submit (9))
                    if (addSpaces && !string.IsNullOrEmpty(testData) && stepNum != "1" && stepNum != "5" && stepNum != "6" && stepNum != "9")
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
                            // HỖ TRỢ ĐẶC BIỆT CHO TC_REG_11: Gen ra chuỗi > 255 ký tự
                            if (tcId == "TC_REG_11" && testData.Contains("255"))
                            {
                                testData = new string('A', 256);
                            }
                            registerPage.EnterFullName(testData);

                            // Log độ dài vừa phải vào Excel để không làm phình file
                            string logName = testData.Length > 50 ? testData.Substring(0, 20) + "..." : testData;
                            step.ActualResult = $"Đã nhập Họ tên: '{logName}'";
                            break;
                        case "3":
                            if (checkAdminForUnique) testData = dynamicEmail;
                            registerPage.EnterEmail(testData);
                            step.ActualResult = $"Đã nhập Email: '{testData}'";
                            break;
                        case "4":
                            if (checkAdminForUnique) testData = dynamicPhone;
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

        private void VerifyExpectedResult(string tcId, TestCaseStep step)
        {
            string alertText = registerPage.GetAlertText();

            switch (tcId)
            {
                case "TC_REG_01":
                    // TC_REG_08 ĐÃ ĐƯỢC TÁCH RA DO MONG ĐỢI LÀ LỖI
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
                case "TC_REG_07":
                    Assert.IsNotNull(alertText, "Không thấy Alert báo lỗi trùng Email/SĐT.");
                    Assert.IsTrue(alertText.Contains("đã tồn tại"), $"Lỗi thực tế: {alertText}");
                    step.ActualResult = "Hệ thống chặn đăng ký và báo lỗi đã tồn tại thành công.";
                    break;

                // --- XỬ LÝ CHUNG CHO 5 TEST CASE TỪ 08 - 12 ---
                case "TC_REG_08":
                case "TC_REG_09":
                case "TC_REG_10":
                case "TC_REG_11":
                case "TC_REG_12":
                    // Đảm bảo tuyệt đối hệ thống không được văng ra dòng chữ "thành công"
                    if (!string.IsNullOrEmpty(alertText))
                    {
                        Assert.IsFalse(alertText.ToLower().Contains("thành công"), $"Lỗi bảo mật/logic: Hệ thống cho phép đăng ký thành công sai logic. Alert: {alertText}");
                        step.ActualResult = "Hệ thống chặn và hiển thị Alert: " + alertText;
                    }
                    else
                    {
                        // Nếu không có Alert văng ra, có nghĩa form HTML5 hoặc Front-end đã chặn chặn submit
                        step.ActualResult = "Hệ thống chặn submit tại giao diện (không có popup Server trả về).";
                    }
                    break;

                default:
                    Assert.IsNotNull(alertText, "Không nhận được thông báo lỗi từ server.");
                    step.ActualResult = "Hệ thống hiển thị lỗi: " + alertText;
                    break;
            }
        }
    }
}