using OpenQA.Selenium;

namespace sqa_testing_report.Pages
{
    public class AdminLoginPage
    {
        private readonly IWebDriver _driver;

        // --- Locators ---
        private readonly By _usernameInput = By.CssSelector("#Username");
        private readonly By _passwordInput = By.CssSelector("#Password");
        private readonly By _submitBtn = By.CssSelector("button[type='submit']");
        private readonly By _dashboardHeader = By.XPath("//h2[contains(text(), 'Tổng quan hệ thống')]");

        // --- Locators cho Error Messages ---
        private readonly By _generalError = By.CssSelector(".error-msg");
        private readonly By _usernameError = By.CssSelector("span[data-valmsg-for='Username']");
        private readonly By _passwordError = By.CssSelector("span[data-valmsg-for='Password']");

        public AdminLoginPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // --- Actions ---
        public void GoToPage()
        {
            _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:2080");
        }

        public void Login(string username, string password)
        {
            var userEle = _driver.FindElement(_usernameInput);
            userEle.Clear();
            userEle.SendKeys(username);

            var passEle = _driver.FindElement(_passwordInput);
            passEle.Clear();
            passEle.SendKeys(password);

            _driver.FindElement(_submitBtn).Click();
        }

        public bool IsLoginSuccessful()
        {
            try
            {
                Thread.Sleep(3000);
                var elements = _driver.FindElements(_dashboardHeader);
                return elements.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        // --- Lấy chính xác Text lỗi trên màn hình ---
        public string GetErrorMessage()
        {
            try
            {
                // Đợi một chút để UI render thông báo lỗi
                Thread.Sleep(500);

                // Ưu tiên kiểm tra lỗi chung (Sai tài khoản/mật khẩu)
                var generalErr = _driver.FindElements(_generalError);
                if (generalErr.Count > 0 && !string.IsNullOrEmpty(generalErr[0].Text))
                    return generalErr[0].Text.Trim();

                // Lỗi trống Email
                var userErr = _driver.FindElements(_usernameError);
                if (userErr.Count > 0 && !string.IsNullOrEmpty(userErr[0].Text))
                    return userErr[0].Text.Trim();

                // Lỗi trống Password
                var passErr = _driver.FindElements(_passwordError);
                if (passErr.Count > 0 && !string.IsNullOrEmpty(passErr[0].Text))
                    return passErr[0].Text.Trim();

                return "Không tìm thấy thông báo lỗi hiển thị trên UI";
            }
            catch
            {
                return "Lỗi khi đọc thông báo lỗi từ UI";
            }
        }
    }
}