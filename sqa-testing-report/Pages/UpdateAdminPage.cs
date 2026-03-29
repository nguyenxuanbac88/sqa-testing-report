using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using sqa_testing_report.Models; // Đảm bảo đúng namespace của class TestCaseStep

namespace sqa_testing_report.Pages
{
    public class UpdateAdminPage
    {
        private readonly IWebDriver _driver;
        public UpdateAdminPage(IWebDriver driver) => _driver = driver;

        // --- Locators chuẩn 11 trường nhập liệu ---
        private readonly Dictionary<string, By> _fields = new Dictionary<string, By>
        {
            { "1",  By.Id("MovieName") },
            { "2",  By.Id("Genre") },
            { "3",  By.Id("Duration") },
            { "4",  By.Id("Description") },
            { "5",  By.Id("Director") },
            { "6",  By.Id("ReleaseDate") },
            { "7",  By.Id("CoverURL") },
            { "8",  By.Id("TrailerURL") },
            { "9",  By.Id("AgeRestriction") },
            { "10", By.Id("Producer") },
            { "11", By.Id("Actors") }
        };

        private By _btnSave = By.CssSelector("#layoutSidenav_content > main > div > div > div.card-body > form > div.d-flex.justify-content-between > button");
        private By _systemNotification = By.CssSelector("#layoutSidenav_content > main > div > div.alert");

        // --- HÀM XỬ LÝ THÔNG MINH ---
        public void FillDataBySteps(List<TestCaseStep> steps)
        {
            foreach (var field in _fields)
            {
                // Tìm xem trong Excel có dòng Step Number này không
                var stepData = steps.FirstOrDefault(s => s.StepNumber == field.Key);

                if (stepData != null)
                {
                    // NẾU CÓ DÒNG NÀY TRONG EXCEL:
                    var element = _driver.FindElement(field.Value);
                    element.Clear(); // Xóa sạch trước

                    if (!string.IsNullOrWhiteSpace(stepData.TestData))
                    {
                        // Nếu TestData có chữ thì mới điền vào
                        element.SendKeys(stepData.TestData);
                    }
                    // Nếu TestData trống -> Ô trên web sẽ trống luôn (đúng ý Khoa)
                }
                // NẾU KHÔNG CÓ DÒNG NÀY TRONG EXCEL -> Bỏ qua, giữ nguyên dữ liệu cũ
            }
        }

        public void ClickSave()
        {
            IWebElement btnSave = _driver.FindElement(_btnSave);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btnSave);
            System.Threading.Thread.Sleep(500);
            try { btnSave.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnSave); }
        }

        public string GetSystemNotificationText()
        {
            try
            {
                // 1. Ưu tiên 1: Bắt popup HTML5 của trình duyệt (ví dụ: "Please fill out this field")
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                string html5Msg = (string)js.ExecuteScript("return document.querySelector(':invalid') ? document.querySelector(':invalid').validationMessage : '';");
                if (!string.IsNullOrWhiteSpace(html5Msg)) return html5Msg.Trim();

                // 2. Ưu tiên 2: Chờ và gom tất cả thông báo hiển thị trên giao diện (Alert + Span lỗi)
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                return wait.Until(d =>
                {
                    string finalMessage = "";

                    // A. Lấy thông báo từ các thẻ SPAN dưới ô nhập liệu (Selector Khoa vừa đưa)
                    // Mình dùng selector rộng hơn để bắt TẤT CẢ các span lỗi trong form
                    var validationSpans = d.FindElements(By.CssSelector("#layoutSidenav_content > main > div > div > div.card-body > form span"));
                    foreach (var span in validationSpans)
                    {
                        if (span.Displayed && !string.IsNullOrWhiteSpace(span.Text))
                        {
                            finalMessage += span.Text.Trim() + " | ";
                        }
                    }

                    // B. Lấy thêm thông báo từ ALERT tổng (nếu có)
                    try
                    {
                        var alert = d.FindElement(_systemNotification);
                        if (alert.Displayed && !string.IsNullOrWhiteSpace(alert.Text))
                        {
                            finalMessage += alert.Text.Trim() + " ";
                        }
                    }
                    catch (NoSuchElementException) { /* Không có alert thì thôi */ }

                    finalMessage = finalMessage.TrimEnd(' ', '|');

                    // Nếu có chữ thì trả về, không thì trả null để Wait tiếp tục đợi đến khi timeout
                    return !string.IsNullOrEmpty(finalMessage) ? finalMessage : null;
                });
            }
            catch (WebDriverTimeoutException)
            {
                return "Hệ thống không trả về thông báo lỗi nào.";
            }
        }
    }
}