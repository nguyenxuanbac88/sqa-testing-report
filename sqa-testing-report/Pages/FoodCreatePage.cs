using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace sqa_testing_report.Pages
{
    public class FoodCreatePage
    {
        private readonly IWebDriver _driver;
        public FoodCreatePage(IWebDriver driver) => _driver = driver;

        // --- LOCATORS ---
        private By _inputName = By.Id("FoodName");
        private By _inputDesc = By.Id("Description");
        private By _inputPrice = By.Id("Price");
        private By _inputImage = By.Id("ImageURL");
        private By _btnCreate = By.CssSelector("#layoutSidenav_content > main > form > div.d-grid > button");

        // Bắt mọi thể loại thông báo: alert, chữ đỏ dưới ô input...
        private By _notificationArea = By.CssSelector(".alert, span.text-danger, .field-validation-error, .toast-message");

        // --- CÁC HÀM THAO TÁC ---
        public void EnterName(string text) { var el = _driver.FindElement(_inputName); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }
        public void EnterDescription(string text) { var el = _driver.FindElement(_inputDesc); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }
        public void EnterPrice(string text) { var el = _driver.FindElement(_inputPrice); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }
        public void EnterImageURL(string text) { var el = _driver.FindElement(_inputImage); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }

        public void ClickCreate()
        {
            var btn = _driver.FindElement(_btnCreate);
            // Kéo xuống giữa màn hình và dùng búa tạ JS phòng khi bị che
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btn);
            System.Threading.Thread.Sleep(500);
            try { btn.Click(); } catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn); }
        }

        public string GetNotificationText()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
                var elements = wait.Until(d => d.FindElements(_notificationArea));
                string msg = "";
                foreach (var el in elements)
                    if (el.Displayed && !string.IsNullOrWhiteSpace(el.Text))
                        msg += el.Text.Trim() + " | ";

                return string.IsNullOrEmpty(msg) ? "Thao tác thành công" : msg.TrimEnd(' ', '|');
            }
            catch { return "Không có thông báo"; }
        }
    }
}