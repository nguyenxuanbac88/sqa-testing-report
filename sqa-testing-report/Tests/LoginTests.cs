using OpenQA.Selenium;
using sqa_testing_report.Models;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class LoginTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private LoginPage loginPage;
        private readonly string sheetName = "Bac_automationTC";

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            loginPage = new LoginPage(driver);

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

        #region Danh sách Test Cases
        [Test] public void TC_LOGIN_01_DangNhapHopLe() => ExecuteLoginTest("TC_LOGIN_01");
        [Test] public void TC_LOGIN_02_DeTrongEmail() => ExecuteLoginTest("TC_LOGIN_02");
        [Test] public void TC_LOGIN_03_EmailSaiDinhDang() => ExecuteLoginTest("TC_LOGIN_03");
        [Test] public void TC_LOGIN_04_SaiMatKhau() => ExecuteLoginTest("TC_LOGIN_04");
        [Test] public void TC_LOGIN_05_DeTrongMatKhau() => ExecuteLoginTest("TC_LOGIN_05");
        [Test] public void TC_LOGIN_06_TaiKhoanKhongTonTai() => ExecuteLoginTest("TC_LOGIN_06");
        [Test] public void TC_LOGIN_07_EmailChuaDangKy() => ExecuteLoginTest("TC_LOGIN_07");
        [Test] public void TC_LOGIN_08_KhoaTaiKhoan5Lan() => ExecuteLoginTest("TC_LOGIN_08");
        [Test] public void TC_LOGIN_09_PhanBietHoaThuong() => ExecuteLoginTest("TC_LOGIN_09");
        [Test] public void TC_LOGIN_10_SQLInjection() => ExecuteLoginTest("TC_LOGIN_10");
        [Test] public void TC_LOGIN_12_DangNhapNhieuThietBi() => ExecuteLoginTest("TC_LOGIN_12");
        #endregion

        private void ExecuteLoginTest(string tcId)
        {
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            loginPage.GoToHomePage();
            loginPage.SwitchToVietnamese();
            loginPage.OpenLoginModal();

            bool isPreviousStepFailed = false;
            string currentEmail = "";

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
                    string action = step.StepAction?.ToLower() ?? "";
                    string testData = step.TestData?.Trim() ?? "";
                    string stepNum = step.StepNumber?.Trim() ?? "";

                    // 1. Logic Nhập Email
                    if (action.Contains("nhập email"))
                    {
                        currentEmail = testData;
                        loginPage.EnterEmail(testData);
                        step.ActualResult = $"Đã nhập Email: '{testData}'";
                    }
                    // 2. Logic Đặc biệt cho TC_08: Nhập mật khẩu sai 5 lần (Bao gồm cả nhấn Đăng nhập)
                    else if (action.Contains("sai 5 lần"))
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            // Đảm bảo Modal luôn mở và Email luôn được điền trước mỗi lần nhấn
                            try { loginPage.EnterEmail(currentEmail); }
                            catch { loginPage.OpenLoginModal(); loginPage.EnterEmail(currentEmail); }

                            loginPage.EnterPassword(testData);
                            loginPage.SubmitLogin();
                            System.Threading.Thread.Sleep(1500); // Chờ server phản hồi lỗi Unauthorized
                        }
                        step.ActualResult = "Hệ thống đã thực hiện 5 lần đăng nhập thất bại liên tiếp.";
                    }
                    // 3. Logic Nhập mật khẩu (Thường hoặc Đúng cho bước 3 của TC_08)
                    else if (action.Contains("nhập mật khẩu"))
                    {
                        // Nếu modal bị đóng sau chuỗi lỗi trên, mở lại nó
                        try { loginPage.EnterEmail(currentEmail); }
                        catch { loginPage.OpenLoginModal(); loginPage.EnterEmail(currentEmail); }

                        loginPage.EnterPassword(testData);
                        step.ActualResult = $"Đã thực hiện: {step.StepAction}";
                    }
                    // 4. Logic Nhấn nút Đăng nhập
                    else if (action.Contains("đăng nhập") && action.Contains("nhấn"))
                    {
                        if (stepNum == "1" && tcId == "TC_LOGIN_01")
                        {
                            step.ActualResult = "Đã mở form đăng nhập.";
                            continue;
                        }

                        // Xử lý Đa thiết bị (TC_12)
                        if (tcId == "TC_LOGIN_12")
                        {
                            loginPage.SubmitLogin();
                            System.Threading.Thread.Sleep(1500);
                            IWebDriver driver2 = null;
                            try
                            {
                                driver2 = DriverFactory.InitDriver();
                                var loginPage2 = new LoginPage(driver2);
                                loginPage2.GoToHomePage(); loginPage2.SwitchToVietnamese(); loginPage2.OpenLoginModal();
                                loginPage2.EnterEmail(currentEmail);
                                // Lấy mật khẩu từ bước "nhập mật khẩu" trước đó trong Excel
                                loginPage2.EnterPassword(steps.FirstOrDefault(s => s.StepAction.ToLower().Contains("nhập mật khẩu"))?.TestData ?? "");
                                loginPage2.SubmitLogin(); System.Threading.Thread.Sleep(1500);
                            }
                            finally { if (driver2 != null) { driver2.Quit(); driver2.Dispose(); } }
                            VerifyExpectedResult(tcId, step);
                            step.ActualResult = $"Đã mô phỏng 2 thiết bị. {step.ActualResult}";
                            continue;
                        }

                        // Nhấn Submit cuối cùng (Cho bước 4 của TC_08 và các case khác)
                        loginPage.SubmitLogin();
                        System.Threading.Thread.Sleep(1500);
                        VerifyExpectedResult(tcId, step);
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
            string alertText = loginPage.GetAlertText() ?? loginPage.GetHtmlErrorMessage();

            switch (tcId)
            {
                case "TC_LOGIN_01":
                case "TC_LOGIN_09":
                case "TC_LOGIN_12":
                    bool isLoggedIn = loginPage.IsLoggedIn();
                    if (!isLoggedIn) Assert.IsTrue(isLoggedIn, $"Đăng nhập thất bại: {alertText}");
                    step.ActualResult = "Đăng nhập thành công, hệ thống hiển thị nút 'Thành viên'.";
                    break;

                case "TC_LOGIN_08":
                    // Chốt chặn quan trọng: Sau khi thực hiện xong Bước 4, hệ thống KHÔNG ĐƯỢC hiện Profile
                    bool isEntered = loginPage.IsLoggedIn();
                    if (isEntered)
                    {
                        Assert.Fail("Lỗi bảo mật: Hệ thống vẫn cho đăng nhập thành công bằng mật khẩu đúng sau khi đã nhập sai 5 lần liên tiếp (Không có cơ chế khóa).");
                    }
                    step.ActualResult = "Hệ thống chặn đăng nhập đúng mật khẩu thành công. Thông báo nhận được: " + alertText;
                    break;

                case "TC_LOGIN_02":
                case "TC_LOGIN_03":
                case "TC_LOGIN_05":
                    string field = tcId == "TC_LOGIN_05" ? "password" : "email";
                    Assert.IsTrue(loginPage.IsHtml5ValidationTriggered(field), $"Không trigger lỗi HTML5 cho {field}");
                    step.ActualResult = "Hệ thống chặn submit tại giao diện thành công.";
                    break;

                default:
                    Assert.IsNotNull(alertText, "Không nhận được thông báo lỗi từ server.");
                    Assert.IsFalse(alertText.ToLower().Contains("thành công"), "Lỗi bảo mật: Hệ thống báo thành công sai logic.");
                    step.ActualResult = "Hệ thống hiển thị lỗi: " + alertText;
                    break;
            }
        }
    }
}