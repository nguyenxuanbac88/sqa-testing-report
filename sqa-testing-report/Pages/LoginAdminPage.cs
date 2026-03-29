using OpenQA.Selenium;
using System.Threading;

namespace sqa_testing_report.Pages
{
    public class LoginAdminPage
    {
        private readonly IWebDriver _driver;

        // Các selector chính xác từ web của bạn
        private By _inputUsername = By.Id("Username");
        private By _inputPassword = By.Id("Password");
        private By _btnLogin = By.CssSelector("body > div > form > button");

        public LoginAdminPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void AutoLoginAdmin(string baseUrl, string username, string password)
        {
            // Thay "/login" bằng endpoint thực tế nếu web của bạn có đường dẫn khác
            _driver.Navigate().GoToUrl($"{baseUrl}");

            Thread.Sleep(1000); // Đợi form load

            _driver.FindElement(_inputUsername).SendKeys(username);
            _driver.FindElement(_inputPassword).SendKeys(password);
            _driver.FindElement(_btnLogin).Click();

            Thread.Sleep(2000); // Đợi đăng nhập thành công và load trang admin
        }
    }
}