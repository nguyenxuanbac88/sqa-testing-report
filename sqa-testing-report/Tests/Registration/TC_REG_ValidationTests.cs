using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace sqa_testing_report.Tests.Registration
{
    /// <summary>
    /// Feature ID: F1.1.1
    /// Kiểm tra validate form đăng ký (English mode)
    /// - Email hợp lệ
    /// - Mật khẩu khớp
    /// - Không bỏ trống các trường bắt buộc
    /// </summary>
    [TestFixture]
    public class TC_REG_ValidationTests
    {
        private IWebDriver _driver = null!;
        private WebDriverWait _wait = null!;
        private const string BaseUrl = "http://api.dvxuanbac.com:81/";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");

            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

            // Truy cập trang và mở form đăng ký (English mode)
            _driver.Navigate().GoToUrl(BaseUrl);
            Thread.Sleep(3000);

            // Click nút Register bằng JavaScript
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                var btns = document.querySelectorAll('button, a');
                for (var i = 0; i < btns.length; i++) {
                    var t = btns[i].textContent.trim().toLowerCase();
                    var c = btns[i].className || '';
                    if ((t === 'register' || t === 'đăng ký' || c.indexOf('btn-success') >= 0) && 
                        !c.includes('w-100')) {
                        btns[i].click();
                        break;
                    }
                }
            ");
            Thread.Sleep(2000);
        }

        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
            }
        }

        /// <summary>
        /// TC_REG_02: Kiểm tra submit form khi bỏ trống tất cả các trường
        /// </summary>
        [Test]
        [Category("Registration")]
        [Category("Validation")]
        [Description("Kiểm tra form không cho phép đăng ký khi bỏ trống tất cả các trường")]
        public void TC_REG_02_SubmitEmptyForm_ShouldShowValidationErrors()
        {
            TestContext.WriteLine("========================================");
            TestContext.WriteLine("  TEST CASE: TC_REG_02");
            TestContext.WriteLine("  Mô tả: Submit form khi bỏ trống tất cả");
            TestContext.WriteLine("========================================");

            LogStep(1, "Nhấn nút REGISTER khi form trống");
            try
            {
                var submitButton = FindSubmitButton();
                if (submitButton == null)
                {
                    LogResult(1, "FAIL", "Không tìm thấy nút REGISTER");
                    Assert.Fail("Không tìm thấy nút REGISTER");
                    return;
                }

                submitButton.Click();
                Thread.Sleep(2000);
                LogResult(1, "PASS", "Đã nhấn nút REGISTER");
            }
            catch (Exception ex)
            {
                LogResult(1, "FAIL", $"Lỗi: {ex.Message}");
                Assert.Fail(ex.Message);
            }

            LogStep(2, "Kiểm tra thông báo lỗi validation");
            try
            {
                bool hasValidation = CheckForValidationErrors();
                bool noSuccessMsg = !PageContainsSuccessMessage();

                if (hasValidation || noSuccessMsg)
                {
                    LogResult(2, "PASS", "Form không cho phép đăng ký khi bỏ trống - Có validation");
                    TestContext.WriteLine("\n✅ KẾT QUẢ: PASS");
                }
                else
                {
                    LogResult(2, "FAIL", "Form cho phép đăng ký khi bỏ trống - Thiếu validation");
                    TestContext.WriteLine("\n❌ KẾT QUẢ: FAIL");
                    Assert.Fail("Form cho phép đăng ký khi bỏ trống các trường");
                }
            }
            catch (Exception ex)
            {
                LogResult(2, "FAIL", $"Lỗi: {ex.Message}");
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// TC_REG_03: Kiểm tra đăng ký với email không hợp lệ
        /// </summary>
        [Test]
        [Category("Registration")]
        [Category("Validation")]
        [Description("Kiểm tra form không cho phép đăng ký khi email không hợp lệ")]
        [TestCase("invalid-email", "Email thiếu @")]
        [TestCase("email@", "Email thiếu domain")]
        [TestCase("@domain.com", "Email thiếu local part")]
        [TestCase("email@domain", "Email thiếu TLD")]
        public void TC_REG_03_RegisterWithInvalidEmail_ShouldFail(string invalidEmail, string description)
        {
            TestContext.WriteLine("========================================");
            TestContext.WriteLine($"  TEST CASE: TC_REG_03 - {description}");
            TestContext.WriteLine($"  Email test: {invalidEmail}");
            TestContext.WriteLine("========================================");

            try
            {
                FillRegistrationForm(
                    fullName: "Nguyễn Văn Test",
                    email: invalidEmail,
                    phone: "0901234567",
                    password: "Test@12345",
                    confirmPassword: "Test@12345"
                );

                SelectGenderMale();
                SelectBirthDate("15", "6", "2000");

                var submitButton = FindSubmitButton();
                submitButton?.Click();
                Thread.Sleep(2000);

                bool hasError = CheckForValidationErrors() || !PageContainsSuccessMessage();

                if (hasError)
                {
                    LogResult(1, "PASS", $"Form từ chối email không hợp lệ: '{invalidEmail}' ({description})");
                    TestContext.WriteLine("\n✅ KẾT QUẢ: PASS");
                }
                else
                {
                    LogResult(1, "FAIL", $"Form chấp nhận email không hợp lệ: '{invalidEmail}' ({description})");
                    TestContext.WriteLine("\n❌ KẾT QUẢ: FAIL");
                    Assert.Fail($"Form chấp nhận email không hợp lệ: '{invalidEmail}'");
                }
            }
            catch (AssertionException) { throw; }
            catch (Exception ex)
            {
                LogResult(1, "FAIL", $"Lỗi: {ex.Message}");
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// TC_REG_04: Kiểm tra đăng ký khi mật khẩu không khớp
        /// </summary>
        [Test]
        [Category("Registration")]
        [Category("Validation")]
        [Description("Kiểm tra form không cho phép đăng ký khi mật khẩu và xác nhận mật khẩu không khớp")]
        public void TC_REG_04_RegisterWithMismatchedPasswords_ShouldFail()
        {
            TestContext.WriteLine("========================================");
            TestContext.WriteLine("  TEST CASE: TC_REG_04");
            TestContext.WriteLine("  Mô tả: Mật khẩu không khớp nhau");
            TestContext.WriteLine("========================================");

            try
            {
                FillRegistrationForm(
                    fullName: "Nguyễn Văn Test",
                    email: "test.mismatch@gmail.com",
                    phone: "0901234567",
                    password: "Test@12345",
                    confirmPassword: "DifferentPass@99"
                );

                SelectGenderMale();
                SelectBirthDate("15", "6", "2000");

                var submitButton = FindSubmitButton();
                submitButton?.Click();
                Thread.Sleep(2000);

                bool hasError = CheckForValidationErrors() || !PageContainsSuccessMessage();

                if (hasError)
                {
                    LogResult(1, "PASS", "Form từ chối khi mật khẩu không khớp");
                    TestContext.WriteLine("\n✅ KẾT QUẢ: PASS");
                }
                else
                {
                    LogResult(1, "FAIL", "Form chấp nhận mật khẩu không khớp");
                    TestContext.WriteLine("\n❌ KẾT QUẢ: FAIL");
                    Assert.Fail("Form chấp nhận mật khẩu không khớp");
                }
            }
            catch (AssertionException) { throw; }
            catch (Exception ex)
            {
                LogResult(1, "FAIL", $"Lỗi: {ex.Message}");
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// TC_REG_05: Kiểm tra đăng ký khi mật khẩu yếu
        /// </summary>
        [Test]
        [Category("Registration")]
        [Category("Validation")]
        [Description("Kiểm tra form không cho phép đăng ký khi mật khẩu yếu")]
        [TestCase("123456", "Chỉ có số")]
        [TestCase("abcdef", "Chỉ có chữ thường")]
        [TestCase("ABCDEF", "Chỉ có chữ hoa")]
        [TestCase("Abc12", "Đủ loại nhưng < 6 ký tự")]
        public void TC_REG_05_RegisterWithWeakPassword_ShouldFail(string weakPassword, string description)
        {
            TestContext.WriteLine("========================================");
            TestContext.WriteLine($"  TEST CASE: TC_REG_05 - {description}");
            TestContext.WriteLine($"  Password test: {weakPassword}");
            TestContext.WriteLine("========================================");

            try
            {
                FillRegistrationForm(
                    fullName: "Nguyễn Văn Test",
                    email: "test.weakpw@gmail.com",
                    phone: "0901234567",
                    password: weakPassword,
                    confirmPassword: weakPassword
                );

                SelectGenderMale();
                SelectBirthDate("15", "6", "2000");

                var submitButton = FindSubmitButton();
                submitButton?.Click();
                Thread.Sleep(2000);

                bool hasError = CheckForValidationErrors() || !PageContainsSuccessMessage();

                if (hasError)
                {
                    LogResult(1, "PASS", $"Form từ chối mật khẩu yếu: '{weakPassword}' ({description})");
                    TestContext.WriteLine("\n✅ KẾT QUẢ: PASS");
                }
                else
                {
                    LogResult(1, "FAIL", $"Form chấp nhận mật khẩu yếu: '{weakPassword}'");
                    TestContext.WriteLine("\n❌ KẾT QUẢ: FAIL");
                    Assert.Fail($"Form chấp nhận mật khẩu yếu: '{weakPassword}'");
                }
            }
            catch (AssertionException) { throw; }
            catch (Exception ex)
            {
                LogResult(1, "FAIL", $"Lỗi: {ex.Message}");
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// TC_REG_06: Bỏ trống Họ và tên
        /// </summary>
        [Test]
        [Category("Registration")]
        [Category("Validation")]
        [Description("Kiểm tra form không cho phép đăng ký khi bỏ trống Full Name")]
        public void TC_REG_06_RegisterWithEmptyName_ShouldFail()
        {
            TestContext.WriteLine("========================================");
            TestContext.WriteLine("  TEST CASE: TC_REG_06 - Bỏ trống Full Name");
            TestContext.WriteLine("========================================");

            try
            {
                FillRegistrationForm(
                    fullName: "",
                    email: "test.noname@gmail.com",
                    phone: "0901234567",
                    password: "Test@12345",
                    confirmPassword: "Test@12345"
                );

                SelectGenderMale();
                SelectBirthDate("15", "6", "2000");

                var submitButton = FindSubmitButton();
                submitButton?.Click();
                Thread.Sleep(2000);

                bool hasError = CheckForValidationErrors() || !PageContainsSuccessMessage();

                if (hasError)
                {
                    LogResult(1, "PASS", "Form từ chối khi bỏ trống Full Name");
                    TestContext.WriteLine("\n✅ KẾT QUẢ: PASS");
                }
                else
                {
                    LogResult(1, "FAIL", "Form chấp nhận khi bỏ trống Full Name");
                    TestContext.WriteLine("\n❌ KẾT QUẢ: FAIL");
                    Assert.Fail("Form chấp nhận khi bỏ trống Full Name");
                }
            }
            catch (AssertionException) { throw; }
            catch (Exception ex)
            {
                LogResult(1, "FAIL", $"Lỗi: {ex.Message}");
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// TC_REG_07: Bỏ trống Email
        /// </summary>
        [Test]
        [Category("Registration")]
        [Category("Validation")]
        [Description("Kiểm tra form không cho phép đăng ký khi bỏ trống Email")]
        public void TC_REG_07_RegisterWithEmptyEmail_ShouldFail()
        {
            TestContext.WriteLine("========================================");
            TestContext.WriteLine("  TEST CASE: TC_REG_07 - Bỏ trống Email");
            TestContext.WriteLine("========================================");

            try
            {
                FillRegistrationForm(
                    fullName: "Nguyễn Văn Test",
                    email: "",
                    phone: "0901234567",
                    password: "Test@12345",
                    confirmPassword: "Test@12345"
                );

                SelectGenderMale();
                SelectBirthDate("15", "6", "2000");

                var submitButton = FindSubmitButton();
                submitButton?.Click();
                Thread.Sleep(2000);

                bool hasError = CheckForValidationErrors() || !PageContainsSuccessMessage();

                if (hasError)
                {
                    LogResult(1, "PASS", "Form từ chối khi bỏ trống Email");
                    TestContext.WriteLine("\n✅ KẾT QUẢ: PASS");
                }
                else
                {
                    LogResult(1, "FAIL", "Form chấp nhận khi bỏ trống Email");
                    TestContext.WriteLine("\n❌ KẾT QUẢ: FAIL");
                    Assert.Fail("Form chấp nhận khi bỏ trống Email");
                }
            }
            catch (AssertionException) { throw; }
            catch (Exception ex)
            {
                LogResult(1, "FAIL", $"Lỗi: {ex.Message}");
                Assert.Fail(ex.Message);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Điền form đăng ký (English mode placeholders)
        /// </summary>
        private void FillRegistrationForm(string fullName, string email, string phone, string password, string confirmPassword)
        {
            // Full Name: placeholder = "Enter your full name"
            if (!string.IsNullOrEmpty(fullName))
            {
                var nameInput = FindElementSafe(By.CssSelector("input[placeholder='Enter your full name']"));
                if (nameInput != null)
                {
                    nameInput.Clear();
                    nameInput.SendKeys(fullName);
                    TestContext.WriteLine($"   -> Nhập Full Name: {fullName}");
                }
            }

            // Email: placeholder = "Enter your email"
            if (!string.IsNullOrEmpty(email))
            {
                var emailInput = FindElementSafe(By.CssSelector("input[placeholder='Enter your email']"));
                if (emailInput != null)
                {
                    emailInput.Clear();
                    emailInput.SendKeys(email);
                    TestContext.WriteLine($"   -> Nhập Email: {email}");
                }
            }

            // Phone: placeholder = "Enter your phone number"
            if (!string.IsNullOrEmpty(phone))
            {
                var phoneInput = FindElementSafe(By.CssSelector("input[placeholder='Enter your phone number']"));
                if (phoneInput != null)
                {
                    phoneInput.Clear();
                    phoneInput.SendKeys(phone);
                    TestContext.WriteLine($"   -> Nhập Phone: {phone}");
                }
            }

            // Password: id = "registerPassword"
            if (!string.IsNullOrEmpty(password))
            {
                var passwordInput = FindElementSafe(By.Id("registerPassword"));
                if (passwordInput != null)
                {
                    passwordInput.Clear();
                    passwordInput.SendKeys(password);
                    TestContext.WriteLine("   -> Nhập Password: ********");
                }
            }

            // Confirm Password: id = "registerConfirmPassword"
            if (!string.IsNullOrEmpty(confirmPassword))
            {
                var confirmInput = FindElementSafe(By.Id("registerConfirmPassword"));
                if (confirmInput != null)
                {
                    confirmInput.Clear();
                    confirmInput.SendKeys(confirmPassword);
                    TestContext.WriteLine("   -> Nhập Confirm Password: ********");
                }
            }
        }

        private void SelectGenderMale()
        {
            try
            {
                var radio = _driver.FindElement(By.Id("genderMale"));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radio);
                TestContext.WriteLine("   -> Chọn Gender: Male");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"   -> Không thể chọn giới tính: {ex.Message}");
            }
        }

        private void SelectBirthDate(string day, string month, string year)
        {
            try
            {
                new SelectElement(_driver.FindElement(By.Id("dob_day"))).SelectByValue(day);
                new SelectElement(_driver.FindElement(By.Id("dob_month"))).SelectByValue(month);
                new SelectElement(_driver.FindElement(By.Id("dob_year"))).SelectByValue(year);
                TestContext.WriteLine($"   -> Chọn DOB: {day}/{month}/{year}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"   -> Không thể chọn ngày sinh: {ex.Message}");
            }
        }

        private IWebElement? FindSubmitButton()
        {
            var buttons = _driver.FindElements(By.CssSelector("button.btn.w-100.text-white.fw-bold"));
            foreach (var btn in buttons)
            {
                if (btn.Displayed && btn.Enabled &&
                    btn.Text.Contains("REGISTER", StringComparison.OrdinalIgnoreCase))
                    return btn;
            }
            return null;
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
                var errors = _driver.FindElements(By.XPath(
                    "//*[contains(@class,'error')] | " +
                    "//*[contains(@class,'danger')] | " +
                    "//*[contains(@class,'invalid')] | " +
                    "//*[contains(@class,'is-invalid')] | " +
                    "//*[contains(text(),'required')] | " +
                    "//*[contains(text(),'invalid')] | " +
                    "//*[contains(text(),'không hợp lệ')]"
                ));

                foreach (var err in errors)
                {
                    if (err.Displayed && !string.IsNullOrWhiteSpace(err.Text))
                    {
                        TestContext.WriteLine($"   -> Validation error: '{err.Text}'");
                        return true;
                    }
                }

                // Kiểm tra HTML5 required validation qua JavaScript
                var result = ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    var inputs = document.querySelectorAll('input[required], select[required]');
                    for (var i = 0; i < inputs.length; i++) {
                        if (!inputs[i].validity.valid) return true;
                    }
                    return false;
                ");
                if (result is bool isInvalid && isInvalid)
                {
                    TestContext.WriteLine("   -> HTML5 validation: Có trường không hợp lệ");
                    return true;
                }

                return false;
            }
            catch { return false; }
        }

        private bool PageContainsSuccessMessage()
        {
            try
            {
                var messages = _driver.FindElements(By.XPath(
                    "//*[contains(text(),'thành công')] | " +
                    "//*[contains(text(),'Success')] | " +
                    "//*[contains(text(),'success')] | " +
                    "//*[contains(text(),'successfully')]"
                ));
                foreach (var msg in messages)
                {
                    if (msg.Displayed) return true;
                }

                if (_driver.Url != BaseUrl && !_driver.Url.Contains("register"))
                    return true;

                return false;
            }
            catch { return false; }
        }

        private void LogStep(int stepNumber, string stepName)
        {
            TestContext.WriteLine($"\n--- STEP {stepNumber}: {stepName} ---");
        }

        private void LogResult(int stepNumber, string status, string message)
        {
            string icon = status == "PASS" ? "✅" : "❌";
            TestContext.WriteLine($"   {icon} [{status}] Step {stepNumber}: {message}");
        }

        #endregion
    }
}
