using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;

namespace sqa_testing_report.Pages
{
    public class MovieListAdminPage
    {
        private readonly IWebDriver _driver;
        public MovieListAdminPage(IWebDriver driver) => _driver = driver;

        // --- LOCATORS ---
        private By _inputSearch = By.CssSelector("body > nav > form > div > input");
        private By _btnPage2 = By.CssSelector("#layoutSidenav_content > main > div > div.table-responsive > nav > ul > li:nth-child(2) > a");

        // Lấy tất cả các dòng trong bảng để đếm và kiểm tra nội dung
        private By _tableRows = By.CssSelector("tbody tr");

        // --- HÀM THAO TÁC ---
        public void EnterSearchKeyword(string keyword)
        {
            var searchBox = _driver.FindElement(_inputSearch);
            searchBox.Clear();
            if (!string.IsNullOrEmpty(keyword)) searchBox.SendKeys(keyword);
        }

        public void SubmitSearch()
        {
            var searchBox = _driver.FindElement(_inputSearch);
            searchBox.SendKeys(Keys.Enter);
        }

        public void ClickPage2()
        {
            var btn = _driver.FindElement(_btnPage2);
            // Dùng JS ép click phòng khi bị footer che
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
        }

        // Lấy số lượng phim đang hiển thị trên bảng
        public int GetTableRowCount()
        {
            try { return _driver.FindElements(_tableRows).Count; }
            catch { return 0; }
        }

        // Lấy toàn bộ text của bảng để đối chiếu xem có chứa từ khóa không
        public string GetTableText()
        {
            try { return _driver.FindElement(By.TagName("tbody")).Text; }
            catch { return ""; }
        }

        public void ScrollToBottom()
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            System.Threading.Thread.Sleep(500);
        }
    }
}