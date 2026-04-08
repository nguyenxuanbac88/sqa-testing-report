using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace sqa_testing_report.Pages
{
    public class FoodEditPage
    {
        private readonly IWebDriver _driver;
        public FoodEditPage(IWebDriver driver) => _driver = driver;

        // --- LOCATORS ---
        private By _inputName = By.Id("FoodName");
        private By _inputDesc = By.Id("Description");
        private By _inputPrice = By.Id("Price");
        private By _inputImage = By.Id("ImageURL");
        // Thay dòng _btnUpdate cũ bằng dòng mới này:
        private By _btnUpdate = By.CssSelector("#layoutSidenav_content > main > div > div > div > div > div.card-body > form > div.d-flex.justify-content-between > button");

        // Vùng bắt thông báo lỗi / thành công
        private By _notificationArea = By.CssSelector(".alert, span.text-danger, .field-validation-error, .toast-message");

        // --- HÀM THAO TÁC (Tự động xóa nội dung cũ, gõ nội dung mới nếu có) ---
        private void FillData(By locator, string text)
        {
            var el = _driver.FindElement(locator);
            el.Clear(); // Luôn xóa dữ liệu cũ đang có sẵn trên màn hình Edit

            // Nếu Excel có dữ liệu thì mới gõ vào. Nếu Excel bỏ trống -> Ô trên web sẽ bị trống
            if (!string.IsNullOrEmpty(text))
            {
                el.SendKeys(text);
            }
        }

        public void EditName(string text) => FillData(_inputName, text);
        public void EditDescription(string text) => FillData(_inputDesc, text);
        public void EditPrice(string text) => FillData(_inputPrice, text);
        public void EditImageURL(string text) => FillData(_inputImage, text);

        public void ClickUpdate()
        {
            var btn = _driver.FindElement(_btnUpdate);
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