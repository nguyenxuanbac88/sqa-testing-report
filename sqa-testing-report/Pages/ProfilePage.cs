using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class ProfilePage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public ProfilePage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        // Nhấn vào link Thành viên để vào trang Profile
        public void GoToProfilePage()
        {
            var memberLink = _wait.Until(d => d.FindElement(By.CssSelector("a[href='/Account/Profile']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", memberLink);
        }

        // Mở Modal đổi mật khẩu bằng cách click vào chữ "Thay đổi"
        public void OpenChangePasswordModal()
        {
            // Tìm thẻ span có onclick chứa hàm openChangePasswordModal
            var changeBtn = _wait.Until(d => d.FindElement(By.XPath("//span[contains(@onclick, 'openChangePasswordModal()')]")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", changeBtn);

            // Đợi modal hiển thị
            _wait.Until(d => d.FindElement(By.Id("changePasswordModal")).Displayed);
        }

        public void EnterOldPassword(string pass)
        {
            var input = _wait.Until(d => d.FindElement(By.Id("oldPassword")));
            input.Clear();
            if (!string.IsNullOrEmpty(pass)) input.SendKeys(pass);
        }

        public void EnterNewPassword(string pass)
        {
            var input = _wait.Until(d => d.FindElement(By.Id("newPassword")));
            input.Clear();
            if (!string.IsNullOrEmpty(pass)) input.SendKeys(pass);
        }

        public void EnterConfirmNewPassword(string pass)
        {
            var input = _wait.Until(d => d.FindElement(By.Id("confirmNewPassword")));
            input.Clear();
            if (!string.IsNullOrEmpty(pass)) input.SendKeys(pass);
        }

        public void SubmitChangePassword()
        {
            var btn = _driver.FindElement(By.XPath("//button[contains(text(), 'XÁC NHẬN')]"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", btn);
        }

        public string GetErrorMessage()
        {
            try
            {
                // Thường các trang này dùng Alert Bootstrap tương tự trang Login
                var alert = _wait.Until(d => d.FindElement(By.CssSelector("div.alert")));
                return alert.Text.Trim();
            }
            catch { return null; }
        }
    }
}