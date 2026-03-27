using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class CinemaPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public CinemaPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        public void GoToGetAllCinemas()
        {
            _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81/Cinema/GetAllCinemas");
        }

        public void SwitchToVietnamese()
        {
            try
            {
                var vnButton = _wait.Until(d => d.FindElement(By.CssSelector("button[data-lang='vi']")));
                if (!vnButton.GetAttribute("class").Contains("active"))
                {
                    vnButton.Click();
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch { }
        }

        // --- CÁC HÀM CHO TC_CINEMA_01 ---
        public void VerifyCinemasListDisplayed()
        {
            _wait.Until(d => d.FindElement(By.ClassName("accordion")));
            var cities = _driver.FindElements(By.CssSelector(".accordion-button"));
            if (cities.Count == 0) throw new Exception("Không tìm thấy danh sách khu vực/thành phố.");
        }

        public void ClickRegion(string regionName)
        {
            // Tìm nút Accordion khu vực (VD: Bến Tre)
            var regionBtn = _wait.Until(d => d.FindElement(By.XPath($"//button[contains(@class, 'accordion-button') and contains(., '{regionName}')]")));

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", regionBtn);

            // Check xem menu đã mở chưa, chưa mở thì mới click
            if (regionBtn.GetAttribute("aria-expanded") != "true")
            {
                js.ExecuteScript("arguments[0].click();", regionBtn);
                System.Threading.Thread.Sleep(500); // Đợi hiệu ứng xổ xuống
            }
        }

        public bool IsCinemaInList(string cinemaName)
        {
            // Kiểm tra xem link tên rạp có xuất hiện không
            return _driver.FindElements(By.XPath($"//a[contains(., '{cinemaName}')]")).Count > 0;
        }

        // --- CÁC HÀM CHO TC_CINEMA_05 ---
        public void SelectCinema(string cinemaName)
        {
            var cinemaLink = _wait.Until(d => d.FindElement(By.XPath($"//a[contains(., '{cinemaName}')]")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", cinemaLink);
            js.ExecuteScript("arguments[0].click();", cinemaLink);
            _wait.Until(d => d.Url.Contains("/GetCinema"));
        }

        public bool CheckCinemaInfoDisplay()
        {
            string html = _driver.PageSource;
            return (html.Contains("Địa chỉ:") || html.Contains("Address:")) &&
                   (html.Contains("Thành phố:") || html.Contains("City:")) &&
                   html.Contains("Hotline:");
        }

        public bool CheckMoviesSectionDisplay()
        {
            return _driver.PageSource.Contains("Movies") || _driver.PageSource.Contains("Phim");
        }

        // --- CÁC HÀM CHO TC_CINEMA_09 (Đã làm trước đó) ---
        public void ScrollToMoviesSection()
        {
            var moviesSection = _driver.FindElements(By.XPath("//*[contains(text(), 'Movies') or contains(text(), 'Phim')]")).FirstOrDefault();
            if (moviesSection != null)
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("arguments[0].scrollIntoView(true);", moviesSection);
            }
        }

        public void SelectDateTab(string dateVal)
        {
            var dateTab = _wait.Until(d => d.FindElement(By.CssSelector($"button[data-date='{dateVal}']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", dateTab);
            System.Threading.Thread.Sleep(1000);
        }

        public void ClickShowtime(string dateVal, string movieName, string time)
        {
            var movieCard = _wait.Until(d => d.FindElement(By.XPath(
                $"//div[contains(@class, 'movie-card') and @data-date='{dateVal}']//h6[contains(text(), '{movieName}')]/ancestor::div[contains(@class, 'movie-card')]"
            )));
            var showtimeBtn = movieCard.FindElement(By.XPath($".//a[contains(text(), '{time}')]"));

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", showtimeBtn);
            _wait.Until(d => d.Url.Contains("/Movie/Details") || d.Url.Contains("/Seat"));
        }
    }
}