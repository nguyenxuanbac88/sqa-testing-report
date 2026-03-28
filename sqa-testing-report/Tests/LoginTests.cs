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
            string currentPassword = "";

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

                    if (action.Contains("nhập email"))
                    {
                        currentEmail = testData; // Lưu lại để dùng cho vòng lặp
                        loginPage.EnterEmail(testData);
                        step.ActualResult = $"Đã nhập Email: '{testData}'";
                    }
                    else if (action.Contains("nhập mật khẩu"))
                    {
                        currentPassword = testData; // Lưu lại để dùng cho vòng lặp
                        loginPage.EnterPassword(testData);
                        step.ActualResult = $"Đã nhập Mật khẩu.";
                    }
                    else if (action.Contains("đăng nhập") && action.Contains("nhấn"))
                    {
                        if (stepNum == "1" && tcId == "TC_LOGIN_01")
                        {
                            step.ActualResult = "Đã mở form đăng nhập.";
                            continue;
                        }

                        // --- XỬ LÝ ĐẶC BIỆT CHO TC_LOGIN_08 (LẶP 5 LẦN) ---
                        if (tcId == "TC_LOGIN_08")
                        {
                            for (int i = 0; i < 4; i++) // Chạy 4 lần lỗi trước
                            {
                                loginPage.SubmitLogin();
                                System.Threading.Thread.Sleep(1500);

                                // Nếu Modal bị đóng, mở lại
                                try { driver.FindElement(By.CssSelector("input[name='email']")).Clear(); }
                                catch { loginPage.OpenLoginModal(); }

                                loginPage.EnterEmail(currentEmail);
                                loginPage.EnterPassword(currentPassword);
                            }
                            // Lần submit thứ 5 sẽ được gọi ở bên dưới chung với luồng chính
                        }

                        // --- XỬ LÝ ĐẶC BIỆT CHO TC_LOGIN_12 (ĐA THIẾT BỊ) ---
                        if (tcId == "TC_LOGIN_12")
                        {
                            loginPage.SubmitLogin(); // Trình duyệt 1 đăng nhập
                            System.Threading.Thread.Sleep(1500);

                            // Mở thêm một trình duyệt thứ 2 (Device 2)
                            IWebDriver driver2 = null;
                            try
                            {
                                driver2 = DriverFactory.InitDriver();
                                var loginPage2 = new LoginPage(driver2);
                                loginPage2.GoToHomePage();
                                loginPage2.SwitchToVietnamese();
                                loginPage2.OpenLoginModal();
                                loginPage2.EnterEmail(currentEmail);
                                loginPage2.EnterPassword(currentPassword);
                                loginPage2.SubmitLogin();
                                System.Threading.Thread.Sleep(1500);
                            }
                            finally
                            {
                                if (driver2 != null) { driver2.Quit(); driver2.Dispose(); }
                            }

                            VerifyExpectedResult(tcId, step);
                            step.ActualResult = $"Đã mô phỏng 2 thiết bị cùng lúc. {step.ActualResult}";
                            continue;
                        }

                        // Luồng Submit bình thường
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
            // Lấy thông báo lỗi (Ưu tiên lấy từ JS Alert, nếu không có thì dò thẻ Div HTML)
            string alertText = loginPage.GetAlertText() ?? loginPage.GetHtmlErrorMessage();

            switch (tcId)
            {
                case "TC_LOGIN_01":
                case "TC_LOGIN_09":
                case "TC_LOGIN_12":
                    bool isLoggedIn = loginPage.IsLoggedIn();
                    if (!isLoggedIn)
                    {
                        Assert.IsTrue(isLoggedIn, $"Đăng nhập thất bại. Server trả về lỗi: '{alertText ?? "Không có thông báo lỗi"}'");
                    }
                    step.ActualResult = "Đăng nhập thành công, hệ thống đã hiển thị nút 'Thành viên'.";
                    break;

                case "TC_LOGIN_02":
                    Assert.IsTrue(loginPage.IsHtml5ValidationTriggered("email"), "Không trigger lỗi trống Email.");
                    step.ActualResult = "Hệ thống chặn submit do trống Email.";
                    break;

                case "TC_LOGIN_03":
                    Assert.IsTrue(loginPage.IsHtml5ValidationTriggered("email"), "Không trigger lỗi sai định dạng Email.");
                    step.ActualResult = "Hệ thống chặn submit do Email sai định dạng.";
                    break;

                case "TC_LOGIN_05":
                    Assert.IsTrue(loginPage.IsHtml5ValidationTriggered("password"), "Không trigger lỗi trống mật khẩu.");
                    step.ActualResult = "Hệ thống chặn submit do trống mật khẩu.";
                    break;

                default:
                    Assert.IsNotNull(alertText, "Không nhận được thông báo lỗi từ server.");
                    Assert.IsFalse(alertText.ToLower().Contains("thành công"), $"Lỗi bảo mật: Đăng nhập sai nhưng hệ thống báo thành công.");

                    step.ActualResult = "Hệ thống hiển thị lỗi: " + alertText;
                    break;
            }
        }
    }
}