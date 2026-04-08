using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace sqa_testing_report.Pages
{
    public class NewsCreatePage
    {
        private readonly IWebDriver _driver;
        public NewsCreatePage(IWebDriver driver) => _driver = driver;

        // --- LOCATORS DO KHOA CUNG CẤP ---
        private By _inputTitle = By.Id("Title");
        private By _inputSubtitle = By.Id("Subtitle");
        private By _inputSlug = By.Id("Slug");
        private By _inputThumbnail = By.Id("ThumbnailUrl");
        private By _selectStatus = By.Id("Status");
        private By _selectCategory = By.Id("CategoryId");

        private By _btnSave = By.CssSelector("#layoutSidenav_content > main > div > div > div > form > button");

        // Khung thông báo thành công/lỗi do Khoa chỉ định + Bắt luôn cả mấy dòng span lỗi chữ đỏ dưới ô input
        private By _notificationArea = By.CssSelector("#layoutSidenav_content > main > div > div > div > div, span.text-danger, .field-validation-error");

        // --- CÁC HÀM THAO TÁC ---
        public void EnterTitle(string text) { var el = _driver.FindElement(_inputTitle); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }
        public void EnterSubtitle(string text) { var el = _driver.FindElement(_inputSubtitle); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }
        public void EnterSlug(string text) { var el = _driver.FindElement(_inputSlug); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }
        public void EnterThumbnail(string text) { var el = _driver.FindElement(_inputThumbnail); el.Clear(); if (!string.IsNullOrEmpty(text)) el.SendKeys(text); }

        public void SelectStatus(string visibleText)
        {
            if (string.IsNullOrEmpty(visibleText)) return;
            var select = new SelectElement(_driver.FindElement(_selectStatus));
            try { select.SelectByText(visibleText); } catch { /* Bỏ qua nếu text sai */ }
        }

        public void SelectCategory(string visibleText)
        {
            if (string.IsNullOrEmpty(visibleText)) return;
            var select = new SelectElement(_driver.FindElement(_selectCategory));
            try { select.SelectByText(visibleText); } catch { /* Bỏ qua nếu text sai */ }
        }

        // --- XỬ LÝ ĐẶC BIỆT CHO TINYMCE ---
        public void EnterContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            try
            {
                // Cách 1: Chui vào iframe của TinyMCE (thường id có đuôi là _ifr hoặc dùng thẻ iframe)
                var iframe = _driver.FindElement(By.CssSelector("iframe[id$='_ifr'], iframe.tox-edit-area__iframe"));
                _driver.SwitchTo().Frame(iframe);

                var body = _driver.FindElement(By.Id("tinymce"));
                body.Clear();
                body.SendKeys(content);

                // Quan trọng: Gõ xong phải "Chui ra" lại trang web chính
                _driver.SwitchTo().DefaultContent();
            }
            catch
            {
                // Cách 2: Nếu chui iframe thất bại, dùng búa tạ JavaScript nhét thẳng nội dung vào Editor
                _driver.SwitchTo().DefaultContent();
                ((IJavaScriptExecutor)_driver).ExecuteScript("if(window.tinymce) { tinymce.activeEditor.setContent(arguments[0]); }", "<p>" + content + "</p>");
            }
        }

        public void ClickSave()
        {
            var btn = _driver.FindElement(_btnSave);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btn);
            System.Threading.Thread.Sleep(500);
            try { btn.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn); }
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