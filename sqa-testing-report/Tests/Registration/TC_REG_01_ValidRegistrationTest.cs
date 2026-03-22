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
    /// Test Case ID: TC_REG_01
    /// Mô tả: Kiểm tra đăng ký hợp lệ (English mode)
    /// Mục tiêu: Xác nhận người dùng có thể đăng ký thành công khi nhập đúng dữ liệu
    /// </summary>
    [TestFixture]
    public class TC_REG_01_ValidRegistrationTest
    {
        private IWebDriver _driver = null!;
        private WebDriverWait _wait = null!;
        private const string BaseUrl = "http://api.dvxuanbac.com:81/";

        // ====== TEST DATA ======
        private const string FullName = "Nguyễn Văn A";
        private const string Email = "khoa9920051@gmail.com";
        private const string PhoneNumber = "019001009";
        private const string Gender = "Male";
        private const string BirthDay = "1";
        private const string BirthMonth = "10";
        private const string BirthYear = "2005";
        private const string Password = "Kho@09092005";
        private const string ConfirmPassword = "Kho@09092005";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");

            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

            TestContext.WriteLine("========================================");
            TestContext.WriteLine("  FEATURE ID : F1.1.1");
            TestContext.WriteLine("  TEST CASE  : TC_REG_01");
            TestContext.WriteLine("  MÔ TẢ      : Kiểm tra đăng ký hợp lệ");
            TestContext.WriteLine("  MODE       : English (EN)");
            TestContext.WriteLine("========================================");
        }

        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                TestContext.WriteLine("\n[CLEANUP] Đóng trình duyệt...");
                _driver.Quit();
                _driver.Dispose();
            }
        }

        /// <summary>
        /// TC_REG_01: Kiểm tra đăng ký thành công với dữ liệu hợp lệ
        /// </summary>
        [Test]
        [Category("Registration")]
        [Category("Smoke")]
        [Description("Kiểm tra người dùng có thể đăng ký thành công khi nhập đầy đủ và đúng dữ liệu")]
        public void TC_REG_01_RegisterWithValidData_ShouldSucceed()
        {
            // ===== STEP 0: Truy cập trang web =====
            LogStep(0, "Truy cập trang web", BaseUrl);
            try
            {
                _driver.Navigate().GoToUrl(BaseUrl);
                _wait.Until(d => d.FindElement(By.TagName("body")));
                Thread.Sleep(2000); // Đợi trang tải hoàn chỉnh
                LogResult(0, "PASS", "Trang web đã tải thành công");
            }
            catch (Exception ex)
            {
                LogResult(0, "FAIL", $"Không thể truy cập trang web: {ex.Message}");
                Assert.Fail($"Không thể truy cập trang web: {ex.Message}");
            }

            // ===== STEP 1: Nhấn vào nút "Register" =====
            LogStep(1, "Nhấn vào nút 'Register'", "Tìm và click nút Register trên header");
            try
            {
                // Website mặc định ở English mode - nút hiển thị "Register"
                // Sử dụng JavaScript để tìm nút Register linh hoạt
                var registerButton = _wait.Until(d =>
                {
                    var allElements = d.FindElements(By.CssSelector("button, a"));
                    foreach (var el in allElements)
                    {
                        try
                        {
                            string text = el.Text.Trim().ToLower();
                            string className = el.GetDomAttribute("class") ?? "";

                            bool isRegisterText = text == "register" ||
                                                  text == "đăng ký" ||
                                                  text == "sign up";
                            bool isRegisterClass = className.Contains("btn-success");

                            if ((isRegisterText || isRegisterClass) && el.Displayed && el.Enabled)
                            {
                                // Loại bỏ nút submit REGISTER trong form modal
                                if (!className.Contains("w-100"))
                                    return el;
                            }
                        }
                        catch { continue; }
                    }
                    return null;
                });

                if (registerButton != null)
                {
                    registerButton.Click();
                }
                else
                {
                    // Fallback: click bằng JavaScript
                    ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                        var btns = document.querySelectorAll('button, a');
                        for (var i = 0; i < btns.length; i++) {
                            var t = btns[i].textContent.trim().toLowerCase();
                            var c = btns[i].className || '';
                            if ((t === 'register' || c.indexOf('btn-success') >= 0) && 
                                !c.includes('w-100')) {
                                btns[i].click();
                                break;
                            }
                        }
                    ");
                }

                Thread.Sleep(2000); // Đợi modal animation mở
                LogResult(1, "PASS", "Đã nhấn nút Register, form đăng ký hiển thị");
            }
            catch (Exception ex)
            {
                LogResult(1, "FAIL", $"Lỗi khi nhấn nút Register: {ex.Message}");
                Assert.Fail($"Lỗi khi nhấn nút Register: {ex.Message}");
            }

            // ===== STEP 2: Nhập Họ và tên =====
            LogStep(2, "Nhập họ và tên", FullName);
            try
            {
                // English placeholder: "Enter your full name"
                var nameInput = _wait.Until(d =>
                    d.FindElement(By.CssSelector("input[placeholder='Enter your full name']"))
                );

                nameInput.Clear();
                nameInput.SendKeys(FullName);

                string enteredValue = nameInput.GetDomProperty("value") ?? "";
                Assert.That(enteredValue, Is.EqualTo(FullName),
                    $"Giá trị nhập không khớp. Mong đợi: '{FullName}', Thực tế: '{enteredValue}'");
                LogResult(2, "PASS", $"Đã nhập họ tên: '{enteredValue}'");
            }
            catch (Exception ex)
            {
                LogResult(2, "FAIL", $"Lỗi khi nhập họ tên: {ex.Message}");
                Assert.Fail($"Lỗi khi nhập họ tên: {ex.Message}");
            }

            // ===== STEP 3: Nhập Email =====
            LogStep(3, "Nhập email", Email);
            try
            {
                // English placeholder: "Enter your email"
                var emailInput = _wait.Until(d =>
                    d.FindElement(By.CssSelector("input[placeholder='Enter your email']"))
                );

                emailInput.Clear();
                emailInput.SendKeys(Email);

                string enteredValue = emailInput.GetDomProperty("value") ?? "";
                Assert.That(enteredValue, Is.EqualTo(Email),
                    $"Email nhập không khớp. Mong đợi: '{Email}', Thực tế: '{enteredValue}'");
                LogResult(3, "PASS", $"Đã nhập email: '{enteredValue}'");
            }
            catch (Exception ex)
            {
                LogResult(3, "FAIL", $"Lỗi khi nhập email: {ex.Message}");
                Assert.Fail($"Lỗi khi nhập email: {ex.Message}");
            }

            // ===== STEP 4: Nhập Số điện thoại =====
            LogStep(4, "Nhập số điện thoại", PhoneNumber);
            try
            {
                // English placeholder: "Enter your phone number"
                var phoneInput = _wait.Until(d =>
                    d.FindElement(By.CssSelector("input[placeholder='Enter your phone number']"))
                );

                phoneInput.Clear();
                phoneInput.SendKeys(PhoneNumber);

                string enteredValue = phoneInput.GetDomProperty("value") ?? "";
                Assert.That(enteredValue, Is.EqualTo(PhoneNumber),
                    $"SĐT nhập không khớp. Mong đợi: '{PhoneNumber}', Thực tế: '{enteredValue}'");
                LogResult(4, "PASS", $"Đã nhập SĐT: '{enteredValue}'");
            }
            catch (Exception ex)
            {
                LogResult(4, "FAIL", $"Lỗi khi nhập SĐT: {ex.Message}");
                Assert.Fail($"Lỗi khi nhập SĐT: {ex.Message}");
            }

            // ===== STEP 5: Chọn Giới tính: Male =====
            LogStep(5, "Chọn giới tính", Gender);
            try
            {
                var genderMale = _wait.Until(d =>
                    d.FindElement(By.Id("genderMale"))
                );

                // Sử dụng JavaScript click vì radio button có thể bị che bởi label
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", genderMale);
                Thread.Sleep(300);

                bool isSelected = genderMale.Selected;
                Assert.That(isSelected, Is.True, "Radio button Male chưa được chọn");
                LogResult(5, "PASS", "Đã chọn giới tính: Male");
            }
            catch (Exception ex)
            {
                LogResult(5, "FAIL", $"Lỗi khi chọn giới tính: {ex.Message}");
                Assert.Fail($"Lỗi khi chọn giới tính: {ex.Message}");
            }

            // ===== STEP 6: Chọn Ngày sinh: 01/10/2005 =====
            LogStep(6, "Chọn ngày sinh", "01/10/2005");
            try
            {
                // Chọn Day = 1
                var dayDropdown = _wait.Until(d => d.FindElement(By.Id("dob_day")));
                new SelectElement(dayDropdown).SelectByValue(BirthDay);
                TestContext.WriteLine($"   -> Đã chọn ngày: {BirthDay}");

                // Chọn Month = 10
                var monthDropdown = _wait.Until(d => d.FindElement(By.Id("dob_month")));
                new SelectElement(monthDropdown).SelectByValue(BirthMonth);
                TestContext.WriteLine($"   -> Đã chọn tháng: {BirthMonth}");

                // Chọn Year = 2005
                var yearDropdown = _wait.Until(d => d.FindElement(By.Id("dob_year")));
                new SelectElement(yearDropdown).SelectByValue(BirthYear);
                TestContext.WriteLine($"   -> Đã chọn năm: {BirthYear}");

                LogResult(6, "PASS", $"Đã chọn ngày sinh: {BirthDay}/{BirthMonth}/{BirthYear}");
            }
            catch (Exception ex)
            {
                LogResult(6, "FAIL", $"Lỗi khi chọn ngày sinh: {ex.Message}");
                Assert.Fail($"Lỗi khi chọn ngày sinh: {ex.Message}");
            }

            // ===== STEP 7: Nhập Mật khẩu =====
            LogStep(7, "Nhập mật khẩu", "********");
            try
            {
                var passwordInput = _wait.Until(d =>
                    d.FindElement(By.Id("registerPassword"))
                );

                passwordInput.Clear();
                passwordInput.SendKeys(Password);

                string enteredValue = passwordInput.GetDomProperty("value") ?? "";
                Assert.That(enteredValue, Is.EqualTo(Password), "Mật khẩu nhập không khớp");
                LogResult(7, "PASS", "Đã nhập mật khẩu thành công");
            }
            catch (Exception ex)
            {
                LogResult(7, "FAIL", $"Lỗi khi nhập mật khẩu: {ex.Message}");
                Assert.Fail($"Lỗi khi nhập mật khẩu: {ex.Message}");
            }

            // ===== STEP 8: Nhập lại Mật khẩu =====
            LogStep(8, "Nhập lại mật khẩu", "********");
            try
            {
                var confirmPasswordInput = _wait.Until(d =>
                    d.FindElement(By.Id("registerConfirmPassword"))
                );

                confirmPasswordInput.Clear();
                confirmPasswordInput.SendKeys(ConfirmPassword);

                string enteredValue = confirmPasswordInput.GetDomProperty("value") ?? "";
                Assert.That(enteredValue, Is.EqualTo(ConfirmPassword), "Mật khẩu xác nhận nhập không khớp");
                LogResult(8, "PASS", "Đã nhập lại mật khẩu thành công");
            }
            catch (Exception ex)
            {
                LogResult(8, "FAIL", $"Lỗi khi nhập lại mật khẩu: {ex.Message}");
                Assert.Fail($"Lỗi khi nhập lại mật khẩu: {ex.Message}");
            }

            // ===== STEP 9: Nhấn nút "REGISTER" (Submit) =====
            LogStep(9, "Nhấn nút 'REGISTER' để submit form", "Submit registration form");
            try
            {
                // Nút REGISTER có class: btn w-100 text-white fw-bold
                var submitButton = _wait.Until(d =>
                {
                    var buttons = d.FindElements(By.CssSelector("button.btn.w-100.text-white.fw-bold"));
                    foreach (var btn in buttons)
                    {
                        if (btn.Displayed && btn.Enabled &&
                            btn.Text.Contains("REGISTER", StringComparison.OrdinalIgnoreCase))
                            return btn;
                    }
                    return null;
                });

                if (submitButton == null)
                {
                    LogResult(9, "FAIL", "Không tìm thấy nút REGISTER (submit)");
                    Assert.Fail("Không tìm thấy nút REGISTER (submit)");
                }

                submitButton!.Click();
                TestContext.WriteLine("   -> Đã nhấn nút REGISTER");
                Thread.Sleep(3000); // Đợi response từ server

                LogResult(9, "PASS", "Đã nhấn nút REGISTER thành công");
            }
            catch (Exception ex)
            {
                LogResult(9, "FAIL", $"Lỗi khi nhấn nút REGISTER: {ex.Message}");
                Assert.Fail($"Lỗi khi nhấn nút REGISTER: {ex.Message}");
            }

            // ===== STEP 10: Kiểm tra kết quả đăng ký =====
            LogStep(10, "Kiểm tra kết quả đăng ký", "Mong đợi: Thông báo 'Đăng ký thành công' hoặc chuyển hướng trang");
            try
            {
                bool isSuccess = false;
                string resultMessage = "";

                // Cách 0: Kiểm tra browser alert (server trả về lỗi qua JavaScript alert)
                try
                {
                    var alert = _driver.SwitchTo().Alert();
                    string alertText = alert.Text;
                    TestContext.WriteLine($"   -> Browser Alert: '{alertText}'");

                    // Kiểm tra nội dung alert
                    if (alertText.Contains("thành công", StringComparison.OrdinalIgnoreCase) ||
                        alertText.Contains("success", StringComparison.OrdinalIgnoreCase))
                    {
                        isSuccess = true;
                        resultMessage = $"Đăng ký thành công! Alert: '{alertText}'";
                    }
                    else
                    {
                        // Alert có lỗi từ server
                        resultMessage = $"Server trả về lỗi qua Alert: '{alertText}'";
                    }

                    alert.Accept(); // Đóng alert
                    Thread.Sleep(1000);
                }
                catch (NoAlertPresentException)
                {
                    // Không có alert, tiếp tục kiểm tra cách khác
                }

                // Cách 1: Tìm thông báo thành công (toast/text trên trang)
                if (!isSuccess && string.IsNullOrEmpty(resultMessage))
                {
                    try
                    {
                        var successElement = new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(d =>
                        {
                            var messages = d.FindElements(By.XPath(
                                "//*[contains(text(),'thành công')] | " +
                                "//*[contains(text(),'Success')] | " +
                                "//*[contains(text(),'successfully')] | " +
                                "//*[contains(text(),'Registration successful')] | " +
                                "//*[contains(@class,'toast-success')] | " +
                                "//*[contains(@class,'alert-success')] | " +
                                "//*[contains(@class,'swal2')]"
                            ));
                            foreach (var msg in messages)
                            {
                                if (msg.Displayed) return msg;
                            }
                            return null;
                        });

                        if (successElement != null)
                        {
                            isSuccess = true;
                            resultMessage = $"Thông báo thành công: '{successElement.Text}'";
                        }
                    }
                    catch (WebDriverTimeoutException) { }
                }

                // Cách 2: Kiểm tra URL thay đổi (chuyển hướng)
                if (!isSuccess && string.IsNullOrEmpty(resultMessage))
                {
                    string currentUrl = _driver.Url;
                    if (currentUrl != BaseUrl && !currentUrl.Contains("register"))
                    {
                        isSuccess = true;
                        resultMessage = $"Đã chuyển hướng đến: {currentUrl}";
                    }
                }

                // Cách 3: Kiểm tra modal đã đóng
                if (!isSuccess && string.IsNullOrEmpty(resultMessage))
                {
                    try
                    {
                        var modals = _driver.FindElements(By.CssSelector(".custom-modal-box"));
                        bool modalVisible = false;
                        foreach (var modal in modals)
                        {
                            if (modal.Displayed) { modalVisible = true; break; }
                        }
                        if (!modalVisible)
                        {
                            isSuccess = true;
                            resultMessage = "Modal đăng ký đã đóng sau khi submit";
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        isSuccess = true;
                        resultMessage = "Modal đăng ký đã đóng sau khi submit";
                    }
                }

                // Ghi kết quả
                if (isSuccess)
                {
                    LogResult(10, "PASS", resultMessage);
                    TestContext.WriteLine("\n========================================");
                    TestContext.WriteLine("  KẾT QUẢ TỔNG THỂ: PASS");
                    TestContext.WriteLine("  Đăng ký thành công!");
                    TestContext.WriteLine("========================================");
                }
                else
                {
                    string failReason = string.IsNullOrEmpty(resultMessage)
                        ? "Không phát hiện thông báo thành công hoặc chuyển hướng sau khi đăng ký"
                        : resultMessage;
                    LogResult(10, "FAIL", failReason);
                    TestContext.WriteLine("\n========================================");
                    TestContext.WriteLine($"  KẾT QUẢ TỔNG THỂ: FAIL");
                    TestContext.WriteLine($"  Lý do: {failReason}");
                    TestContext.WriteLine("========================================");
                    Assert.Fail(failReason);
                }
            }
            catch (AssertionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogResult(10, "FAIL", $"Lỗi không xác định: {ex.Message}");
                Assert.Fail($"Lỗi kiểm tra kết quả đăng ký: {ex.Message}");
            }
        }

        #region Helper Methods

        private void LogStep(int stepNumber, string stepName, string detail)
        {
            TestContext.WriteLine($"\n--- STEP {stepNumber}: {stepName} ---");
            TestContext.WriteLine($"   Chi tiết: {detail}");
        }

        private void LogResult(int stepNumber, string status, string message)
        {
            string icon = status == "PASS" ? "✅" : "❌";
            TestContext.WriteLine($"   {icon} [{status}] Step {stepNumber}: {message}");
        }

        #endregion
    }
}
