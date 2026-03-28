using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class RegisterPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public RegisterPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        public void GoToHomePage()
        {
            _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81");
        }

        public void SwitchToVietnamese()
        {
            try
            {
                var vnButton = _wait.Until(d => d.FindElement(By.CssSelector("button[data-lang='vi']")));
                if (!vnButton.GetAttribute("class").Contains("active"))
                {
                    vnButton.Click();
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch { }
        }

        public void OpenRegisterModal()
        {
            var btn = _wait.Until(d => d.FindElement(By.CssSelector(".btn-register")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", btn);
            _wait.Until(d => d.FindElement(By.Id("registerModal")).Displayed);
        }

        // --- ĐÃ FIX: Chỉ tìm input bên trong #registerModal ---
        public void EnterFullName(string name)
        {
            var input = _wait.Until(d => d.FindElement(By.CssSelector("#registerModal input[name='name']")));
            input.Clear();
            if (!string.IsNullOrEmpty(name)) input.SendKeys(name);
        }

        public void EnterEmail(string email)
        {
            var input = _wait.Until(d => d.FindElement(By.CssSelector("#registerModal input[name='email']")));
            input.Clear();
            if (!string.IsNullOrEmpty(email)) input.SendKeys(email);
        }

        public void EnterPhone(string phone)
        {
            var input = _wait.Until(d => d.FindElement(By.CssSelector("#registerModal input[name='phone']")));
            input.Clear();
            if (!string.IsNullOrEmpty(phone)) input.SendKeys(phone);
        }

        public void SelectGender(string genderName)
        {
            string value = genderName.Trim().ToLower() == "nam" ? "true" : "false";
            var radio = _wait.Until(d => d.FindElement(By.CssSelector($"#registerModal input[name='gender'][value='{value}']")));

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", radio);
        }

        public void SelectDOB(string dobString)
        {
            if (string.IsNullOrEmpty(dobString)) return;

            string[] parts = dobString.Split('/');
            if (parts.Length != 3) return;

            string day = int.Parse(parts[0]).ToString();
            string month = int.Parse(parts[1]).ToString();
            string year = parts[2];

            new SelectElement(_wait.Until(d => d.FindElement(By.CssSelector("#registerModal #dob_day")))).SelectByValue(day);
            new SelectElement(_wait.Until(d => d.FindElement(By.CssSelector("#registerModal #dob_month")))).SelectByValue(month);
            new SelectElement(_wait.Until(d => d.FindElement(By.CssSelector("#registerModal #dob_year")))).SelectByValue(year);
        }

        public void EnterPassword(string password)
        {
            var input = _wait.Until(d => d.FindElement(By.CssSelector("#registerModal input[name='password']")));
            input.Clear();
            if (!string.IsNullOrEmpty(password)) input.SendKeys(password);
        }

        public void EnterConfirmPassword(string confirmPassword)
        {
            var input = _wait.Until(d => d.FindElement(By.CssSelector("#registerModal input[name='confirmPassword']")));
            input.Clear();
            if (!string.IsNullOrEmpty(confirmPassword)) input.SendKeys(confirmPassword);
        }

        public void SubmitRegistration()
        {
            var btn = _wait.Until(d => d.FindElement(By.CssSelector("#registerModal form[action='/Register/Submit'] button[type='submit']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", btn);
            js.ExecuteScript("arguments[0].click();", btn);
        }

        public bool IsRegistrationSuccessful()
        {
            try
            {
                return _wait.Until(d =>
                {
                    // 1. Ưu tiên kiểm tra xem có popup Alert không
                    try
                    {
                        IAlert alert = d.SwitchTo().Alert();
                        string alertText = alert.Text;
                        alert.Accept(); // Nhấn OK để đóng Alert lại, tránh treo trình duyệt

                        // Chuyển về chữ thường và check xem có chứa từ khóa không
                        if (alertText.ToLower().Contains("thành công"))
                        {
                            return true;
                        }
                    }
                    catch (NoAlertPresentException)
                    {
                        // Nếu chưa có Alert hiện lên, bắt exception để code chạy tiếp xuống dưới
                    }

                    // 2. Dự phòng: Kiểm tra HTML hoặc URL đổi sang trang Login
                    if (d.PageSource.ToLower().Contains("thành công") || d.Url.ToLower().Contains("/login"))
                    {
                        return true;
                    }

                    return false; // Trả về false để WebDriver tiếp tục đợi và thử lại (tối đa 15s)
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        // Hàm mới để lấy text từ Alert và đóng nó lại
        public string GetAlertText()
        {
            try
            {
                // Chờ tối đa 5s cho Alert xuất hiện
                IAlert alert = _wait.Until(d => d.SwitchTo().Alert());
                string alertText = alert.Text;
                alert.Accept(); // Nhấn OK để đóng Alert
                return alertText;
            }
            catch (WebDriverTimeoutException)
            {
                return null; // Không có Alert nào xuất hiện
            }
            catch (NoAlertPresentException)
            {
                return null;
            }
        }

        public bool IsHtml5ValidationTriggered(string fieldName)
        {
            var input = _driver.FindElement(By.CssSelector($"#registerModal input[name='{fieldName}']"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            bool isInvalid = (bool)js.ExecuteScript("return !arguments[0].validity.valid;", input);
            return isInvalid;
        }
    }
}