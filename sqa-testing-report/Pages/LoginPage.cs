using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public LoginPage(IWebDriver driver)
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
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                    js.ExecuteScript("arguments[0].click();", vnButton);
                    System.Threading.Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cảnh báo: Không thể chuyển sang tiếng Việt - " + ex.Message);
            }
        }

        public void OpenLoginModal()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("if(typeof openLoginModal === 'function') openLoginModal();");
            _wait.Until(d => d.FindElement(By.Id("loginModal")).Displayed);
            System.Threading.Thread.Sleep(500);
        }

        public void EnterEmail(string email)
        {
            var input = _wait.Until(d => d.FindElement(By.CssSelector("#loginModal input[name='email']")));
            input.Clear();
            if (!string.IsNullOrEmpty(email)) input.SendKeys(email);
        }

        public void EnterPassword(string password)
        {
            var input = _wait.Until(d => d.FindElement(By.CssSelector("#loginModal input[name='password']")));
            input.Clear();
            if (!string.IsNullOrEmpty(password)) input.SendKeys(password);
        }

        public void SubmitLogin()
        {
            var btn = _wait.Until(d => d.FindElement(By.CssSelector("#loginModal form[action='/Login/Login'] button[type='submit']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", btn);
        }

        // Kiểm tra xem đã đăng nhập thành công chưa (Dựa vào nút Thành viên)
        public bool IsLoggedIn()
        {
            try
            {
                var memberLink = _wait.Until(d => d.FindElement(By.CssSelector("a[href='/Account/Profile']")));
                return memberLink.Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        // ĐỌC LỖI TỪ JS ALERT (Nếu có)
        public string GetAlertText()
        {
            try
            {
                IAlert alert = _wait.Until(d => d.SwitchTo().Alert());
                string alertText = alert.Text;
                try { alert.Accept(); } catch { }
                return alertText;
            }
            catch { return null; }
        }

        // ĐỌC LỖI TỪ THẺ HTML DIV (Dành cho các lỗi Unauthorized)
        public string GetHtmlErrorMessage()
        {
            try
            {
                var alertDiv = _wait.Until(d => d.FindElement(By.CssSelector("div.alert")));
                // Xóa các ký tự xuống dòng và khoảng trắng thừa của nút Close
                return alertDiv.Text.Replace("\r\n", " ").Replace("\n", " ").Trim();
            }
            catch (WebDriverTimeoutException) { return null; }
            catch (NoSuchElementException) { return null; }
        }

        public bool IsHtml5ValidationTriggered(string fieldName)
        {
            var input = _driver.FindElement(By.CssSelector($"#loginModal input[name='{fieldName}']"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            return (bool)js.ExecuteScript("return !arguments[0].validity.valid;", input);
        }

        // Bổ sung cho TC_BOOK_01: Kiểm tra popup Đăng nhập có đang hiển thị không
        public bool IsLoginModalDisplayed()
        {
            try
            {
                var modal = _wait.Until(d => d.FindElement(By.Id("loginModal")));
                return modal.Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}