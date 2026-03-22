using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Threading;

namespace sqa_testing_report.Tests.Login
{
    [TestFixture]
    public class TC_LOGIN_Tests
    {
        private IWebDriver _driver = null!;
        private WebDriverWait _wait = null!;
        private readonly string BaseUrl = "http://api.dvxuanbac.com:81/";
        private string _screenshotDirectory = null!;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Tạo thư mục lưu screenshots nếu chưa có
            _screenshotDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots");
            if (!Directory.Exists(_screenshotDirectory))
            {
                Directory.CreateDirectory(_screenshotDirectory);
            }
        }

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");

            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

            _driver.Navigate().GoToUrl(BaseUrl);
            Thread.Sleep(2000); // Chờ trang tải

            // Click nút Login (ở chế độ EN)
            TestContext.WriteLine("-> Click nút Login trên header");
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                var els = document.querySelectorAll('button, a');
                for (var i = 0; i < els.length; i++) {
                    var authText = els[i].textContent.trim().toLowerCase();
                    if (authText === 'login' || authText === 'đăng nhập') {
                        els[i].click();
                        break;
                    }
                }
            ");
            Thread.Sleep(2000); // Chờ modal bật lên
        }

        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                // Bonus: Chụp màn hình khi test FAIL
                if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                {
                    try
                    {
                        var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                        string testName = TestContext.CurrentContext.Test.Name;
                        string safeTestName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
                        string fileName = $"{safeTestName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        string filepath = Path.Combine(_screenshotDirectory, fileName);
                        screenshot.SaveAsFile(filepath);
                        TestContext.AddTestAttachment(filepath, "Screenshot of failure");
                        TestContext.WriteLine($"\n📸 [SCREENSHOT CAPTURED] Saved to: {filepath}");
                    }
                    catch (Exception ex)
                    {
                        TestContext.WriteLine($"Không thể chụp màn hình: {ex.Message}");
                    }
                }

                _driver.Quit();
                _driver.Dispose();
            }
        }

        private void FillLoginForm(string email, string password)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var emailInput = FindElementSafe(By.CssSelector("input[placeholder='Enter Email']")) 
                                 ?? FindElementSafe(By.CssSelector("input[placeholder='Nhập Email']"));
                if (emailInput != null)
                {
                    emailInput.Clear();
                    emailInput.SendKeys(email);
                    TestContext.WriteLine($"-> Nhập Email: {email}");
                }
            }

            if (!string.IsNullOrEmpty(password))
            {
                var pwdInput = FindElementSafe(By.Id("loginPassword"))
                               ?? FindElementSafe(By.CssSelector("input[placeholder='Enter Password']"));
                if (pwdInput != null)
                {
                    pwdInput.Clear();
                    pwdInput.SendKeys(password);
                    TestContext.WriteLine("-> Nhập Password: ********");
                }
            }
        }

        private void ClickSubmitLogin()
        {
            var btnLogin = FindElementSafe(By.XPath("//button[contains(@class,'w-100') and (contains(translate(text(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), 'login') or contains(text(), 'Đăng nhập'))]"));
            if (btnLogin != null)
            {
                btnLogin.Click();
                TestContext.WriteLine("-> Bấm nút submit LOGIN");
            }
            else
            {
                TestContext.WriteLine("-> Không tìm thấy nút submit LOGIN");
                Assert.Fail("Không thể bấm nút submit vì không tìm thấy element.");
            }
        }

        private IWebElement? FindElementSafe(By by)
        {
            try
            {
                var elements = _driver.FindElements(by);
                foreach (var el in elements)
                {
                    if (el.Displayed) return el;
                }
                return null;
            }
            catch { return null; }
        }

        private bool CheckForValidationErrors()
        {
            try
            {
                // Kiểm tra validation errors hiển thị từ server (alert, error text...)
                var errors = _driver.FindElements(By.XPath(
                    "//*[contains(@class,'error')] | " +
                    "//*[contains(@class,'danger')] | " +
                    "//*[contains(@class,'invalid')] | " +
                    "//*[contains(text(),'không đúng')] | " +
                    "//*[contains(text(),'không tồn tại')] | " +
                    "//*[contains(text(),'không hợp lệ')] | " +
                    "//*[contains(text(),'Vui lòng nhập')]"
                ));

                foreach (var err in errors)
                {
                    if (err.Displayed && !string.IsNullOrWhiteSpace(err.Text))
                    {
                        TestContext.WriteLine($"-> Lỗi hiển thị: '{err.Text}'");
                        return true;
                    }
                }

                // Kiểm tra HTML5 required validaton cho các form
                var jsResult = ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    var inputs = document.querySelectorAll('input[required]');
                    for (var i = 0; i < inputs.length; i++) {
                        if (!inputs[i].validity.valid) return true;
                    }
                    return false;
                ");
                if (jsResult is bool isInvalid && isInvalid)
                {
                    TestContext.WriteLine("-> Bị chặn lại bởi HTML5 Form validation");
                    return true;
                }

                // Kiểm tra alert của Browser
                try
                {
                    var alert = _driver.SwitchTo().Alert();
                    string alertText = alert.Text;
                    TestContext.WriteLine($"-> Lỗi Browser Alert: '{alertText}'");
                    alert.Accept();
                    return true;
                }
                catch (NoAlertPresentException) { }

                return false;
            }
            catch { return false; }
        }

        private bool CheckIfLoggedInSuccessfully()
        {
            Thread.Sleep(3000); // Chờ load
            try
            {
                // Kiểm tra redirect
                string currentUrl = _driver.Url;
                if (!currentUrl.Contains("login") && currentUrl != BaseUrl)
                {
                    TestContext.WriteLine($"-> Đã chuyển hướng tới: {currentUrl}");
                    return true;
                }

                // Cập nhật: Sau khi đăng nhập xong, trang hiển thị "Membership" và "Logout"
                var isLoggedIn = (bool)((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    var els = document.querySelectorAll('*');
                    for (var i = 0; i < els.length; i++) {
                        var style = window.getComputedStyle(els[i]);
                        if (style.display !== 'none' && style.visibility !== 'hidden') {
                            var text = els[i].textContent.trim().toLowerCase();
                            if (text === 'logout' || text === 'membership' || text === 'đăng xuất') {
                                return true;
                            }
                        }
                    }
                    return false;
                ");

                var jwtToken = ((IJavaScriptExecutor)_driver).ExecuteScript("return window.localStorage.getItem('token') || window.localStorage.getItem('jwt');");
                if (jwtToken != null && jwtToken.ToString()!.Length > 0)
                {
                    TestContext.WriteLine($"-> Xác nhận chứa JWT Token trong LocalStorage: {jwtToken.ToString()!.Substring(0, 15)}...");
                    return true; // Token là dấu hiệu chắc chắn nhất
                }

                return isLoggedIn;
            }
            catch { return false; }
        }

        // ==========================================
        // KHAI BÁO CÁC TEST CASES ĐƯỢC YÊU CẦU
        // ==========================================

        [Test, Category("Login"), Category("Positive")]
        [Description("TC_LOGIN_01 – Đăng nhập hợp lệ")]
        public void TC_LOGIN_01_ValidLogin_ShouldSucceed()
        {
            TestContext.WriteLine("======== TC_LOGIN_01 ========");
            FillLoginForm("khoa992005@gmail.com", "Khoa@123");
            ClickSubmitLogin();

            bool isSuccess = CheckIfLoggedInSuccessfully();
            if (isSuccess)
            {
                TestContext.WriteLine("✅ PASS - Đăng nhập thành công và JWT/Session được ghi nhận.");
                Assert.Pass("Đăng nhập hợp lệ thành công");
            }
            else
            {
                TestContext.WriteLine("❌ FAIL - Không thể tìm thấy dấu hiệu đăng nhập thành công.");
                Assert.Fail("Đăng nhập thất bại (không chuyển trang, hoặc không thấy token)");
            }
        }

        [Test, Category("Login"), Category("Negative")]
        [Description("TC_LOGIN_02 – Để trống Email")]
        public void TC_LOGIN_02_EmptyEmail_ShouldFail()
        {
            TestContext.WriteLine("======== TC_LOGIN_02 ========");
            FillLoginForm("", "Kho@09092005");
            ClickSubmitLogin();

            bool hasError = CheckForValidationErrors() || !CheckIfLoggedInSuccessfully();
            Assert.That(hasError, Is.True, "Form không chặn yêu cầu khi để trống Email.");
            TestContext.WriteLine("✅ PASS - Bị chặn thành công khi bỏ trống Email");
        }

        [Test, Category("Login"), Category("Negative")]
        [Description("TC_LOGIN_03 – Email sai định dạng")]
        public void TC_LOGIN_03_InvalidEmailFormat_ShouldFail()
        {
            TestContext.WriteLine("======== TC_LOGIN_03 ========");
            FillLoginForm("abc123", "Kho@09092005");
            ClickSubmitLogin();

            bool hasError = CheckForValidationErrors() || !CheckIfLoggedInSuccessfully();
            Assert.That(hasError, Is.True, "Form không chặn yêu cầu khi email sai format.");
            TestContext.WriteLine("✅ PASS - Bị chặn thành công với format email sai: abc123");
        }

        [Test, Category("Login"), Category("Negative")]
        [Description("TC_LOGIN_04 – Sai mật khẩu")]
        public void TC_LOGIN_04_IncorrectPassword_ShouldFail()
        {
            TestContext.WriteLine("======== TC_LOGIN_04 ========");
            FillLoginForm("khoa992005@gmail.com", "SaiPass123");
            ClickSubmitLogin();

            bool hasError = CheckForValidationErrors() || !CheckIfLoggedInSuccessfully();
            Assert.That(hasError, Is.True, "Đăng nhập với password sai nhưng không thấy lỗi.");
            TestContext.WriteLine("✅ PASS - Bị báo lỗi hoặc từ chối đăng nhập thành công khi mật khẩu sai.");
        }

        [Test, Category("Login"), Category("Negative")]
        [Description("TC_LOGIN_05 – Để trống mật khẩu")]
        public void TC_LOGIN_05_EmptyPassword_ShouldFail()
        {
            TestContext.WriteLine("======== TC_LOGIN_05 ========");
            FillLoginForm("khoa992005@gmail.com", "");
            ClickSubmitLogin();

            bool hasError = CheckForValidationErrors() || !CheckIfLoggedInSuccessfully();
            Assert.That(hasError, Is.True, "Form không chặn yêu cầu khi để trống Mật khẩu.");
            TestContext.WriteLine("✅ PASS - Bị báo lỗi thành công khi bỏ trống Mật khẩu");
        }

        [Test, Category("Login"), Category("Negative")]
        [Description("TC_LOGIN_06 – Tài khoản không tồn tại")]
        public void TC_LOGIN_06_AccountNotExists_ShouldFail()
        {
            TestContext.WriteLine("======== TC_LOGIN_06 ========");
            FillLoginForm("notexist@gmail.com", "Test123@");
            ClickSubmitLogin();

            bool hasError = CheckForValidationErrors() || !CheckIfLoggedInSuccessfully();
            Assert.That(hasError, Is.True, "Không hiển thị lỗi cho tài khoản không tồn tại.");
            TestContext.WriteLine("✅ PASS - Báo lỗi chính xác về tài khoản không tồn tại.");
        }
    }
}
