using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace sqa_testing_report.Pages
{
    public class CreateAdminPage
    {
        private readonly IWebDriver _driver;

        public CreateAdminPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // --- Danh sách Locators ---
        private By _inputMovieName = By.Id("MovieName");
        private By _inputGenre = By.Id("Genre");
        private By _inputDuration = By.Id("Duration");
        private By _inputDirector = By.Id("Director");
        private By _inputReleaseDate = By.Id("ReleaseDate");
        private By _inputEndDate = By.Id("EndDate");
        private By _inputCoverUrl = By.Id("CoverURL");
        private By _inputTrailerUrl = By.Id("TrailerURL");
        private By _inputDescription = By.Id("Description");
        private By _inputAgeRestriction = By.Id("AgeRestriction");
        private By _inputProducer = By.Id("Producer");
        private By _inputActors = By.Id("Actors");

        private By _btnSave = By.CssSelector("#layoutSidenav_content > main > div > form > div.mt-4 > button");

        // Selector bắt thông báo (cả thành công lẫn thất bại)
        private By _systemNotification = By.CssSelector("#layoutSidenav_content > main > div > div.alert");

        public void GoToPage(string baseUrl)
        {
            // Thay đổi URL này thành đúng đường dẫn trang tạo phim của bạn nếu khác
            _driver.Navigate().GoToUrl($"{baseUrl}/MovieManagement/Create");
        }

        public void FillMovieData(string name, string genre, string duration, string director,
                                  string releaseDate, string endDate, string coverUrl,
                                  string trailerUrl, string desc, string ageRestriction,
                                  string producer, string actors)
        {
            _driver.FindElement(_inputMovieName).SendKeys(name);
            _driver.FindElement(_inputGenre).SendKeys(genre);
            _driver.FindElement(_inputDuration).SendKeys(duration);
            _driver.FindElement(_inputDirector).SendKeys(director);
            _driver.FindElement(_inputReleaseDate).SendKeys(releaseDate);
            _driver.FindElement(_inputEndDate).SendKeys(endDate);
            _driver.FindElement(_inputCoverUrl).SendKeys(coverUrl);
            _driver.FindElement(_inputTrailerUrl).SendKeys(trailerUrl);
            _driver.FindElement(_inputDescription).SendKeys(desc);
            _driver.FindElement(_inputAgeRestriction).SendKeys(ageRestriction);
            _driver.FindElement(_inputProducer).SendKeys(producer);
            _driver.FindElement(_inputActors).SendKeys(actors);
        }

        public void ClickSave()
        {
            IWebElement btnSave = _driver.FindElement(_btnSave);
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;

            // Cuộn màn hình đưa nút Lưu vào chính giữa viewport để tránh bị thanh menu che
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btnSave);
            System.Threading.Thread.Sleep(500);

            try
            {
                btnSave.Click();
            }
            catch (ElementClickInterceptedException)
            {
                // Ép click bằng Javascript nếu bị che
                js.ExecuteScript("arguments[0].click();", btnSave);
            }
        }

        // --- ĐÃ NÂNG CẤP: Bắt thêm lỗi Popup HTML5 của trình duyệt ---
        public string GetSystemNotificationText()
        {
            try
            {
                // 1. KIỂM TRA LỖI POPUP TRÌNH DUYỆT (HTML5 Validation) TRƯỚC
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                // Tìm ô input đang bị lỗi (pseudo-class :invalid) và lấy câu thông báo của nó
                string html5ValidationMsg = (string)js.ExecuteScript("return document.querySelector(':invalid') ? document.querySelector(':invalid').validationMessage : '';");

                if (!string.IsNullOrWhiteSpace(html5ValidationMsg))
                {
                    return html5ValidationMsg.Trim(); // Trả về luôn nếu bắt được popup (VD: "Please enter a valid value...")
                }

                // 2. NẾU KHÔNG CÓ POPUP TRÌNH DUYỆT, TÌM TIẾP LỖI TỪ HỆ THỐNG (.alert hoặc span)
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                return wait.Until(d =>
                {
                    string result = "";

                    try
                    {
                        var alert = d.FindElement(_systemNotification);
                        if (alert.Displayed && !string.IsNullOrWhiteSpace(alert.Text))
                            result += alert.Text.Trim() + " ";
                    }
                    catch (NoSuchElementException) { }

                    var validationSpans = d.FindElements(By.CssSelector("#layoutSidenav_content > main > div > form > div.row.g-3 span"));
                    foreach (var span in validationSpans)
                    {
                        if (span.Displayed && !string.IsNullOrWhiteSpace(span.Text))
                            result += span.Text.Trim() + " | ";
                    }

                    result = result.TrimEnd(' ', '|');
                    return !string.IsNullOrEmpty(result) ? result : null;
                });
            }
            catch (WebDriverTimeoutException)
            {
                return "Không có thông báo nào xuất hiện từ hệ thống";
            }
        }
    }
}