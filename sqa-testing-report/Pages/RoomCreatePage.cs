using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace sqa_testing_report.Pages
{
    public class RoomCreatePage
    {
        private readonly IWebDriver _driver;
        public RoomCreatePage(IWebDriver driver) => _driver = driver;

        // --- LOCATORS ---
        private By _btnAddRoom = By.CssSelector("#layoutSidenav_content > main > div > h3 > a");
        private By _inputRoomName = By.Id("RoomName");
        private By _inputRoomImageURL = By.Id("RoomImageURL");
        private By _inputRoomType = By.Id("RoomType");
        private By _inputStatus = By.Id("Status");
        private By _btnSave = By.CssSelector("#layoutSidenav_content > main > div > div > div.card-body.p-4 > form > div.d-flex.justify-content-between.align-items-center.mt-4 > button");

        // Vùng bắt thông báo: Chữ đỏ validate, thông báo alert/toast
        private By _notificationArea = By.CssSelector(".alert, span.text-danger, .field-validation-error, .toast-message");

        // --- CÁC HÀM THAO TÁC ---
        public void ClickAddRoom()
        {
            var btn = _driver.FindElement(_btnAddRoom);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btn);
            System.Threading.Thread.Sleep(500);
            try { btn.Click(); } catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn); }
        }

        public void EnterRoomName(string text) { var el = _driver.FindElement(_inputRoomName); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }
        public void EnterRoomImageURL(string text) { var el = _driver.FindElement(_inputRoomImageURL); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }
        public void EnterRoomType(string text) { var el = _driver.FindElement(_inputRoomType); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }

        // Hàm thông minh: Xử lý Status (Checkbox hoặc Select)
        public void SetStatus(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var el = _driver.FindElement(_inputStatus);

            // Nếu là Checkbox (Nút gạt)
            if (el.GetAttribute("type") == "checkbox")
            {
                bool shouldBeChecked = (text.Trim().ToLower() == "on" || text.Trim().ToLower() == "true");
                if (el.Selected != shouldBeChecked)
                {
                    // Dùng JS click để chống lỗi bị đè
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", el);
                }
            }
            // Nếu lỡ nó là Dropdown (Thẻ Select)
            else if (el.TagName.ToLower() == "select")
            {
                new SelectElement(el).SelectByText(text);
            }
        }

        public void ClickSave()
        {
            var btn = _driver.FindElement(_btnSave);
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

                return msg.TrimEnd(' ', '|');
            }
            catch { return ""; }
        }
    }
}