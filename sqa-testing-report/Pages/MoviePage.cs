using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class MoviePage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public MoviePage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        public void GoToMovieListPage()
        {
            _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81");
        }

        public void SwitchToVietnamese()
        {
            try
            {
                var vnButton = _wait.Until(d => d.FindElement(By.CssSelector("button[data-lang='vi']")));
                if (!vnButton.GetAttribute("class").Contains("active"))
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                    js.ExecuteScript("arguments[0].click();", vnButton);
                    System.Threading.Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cảnh báo: Không tìm thấy nút đổi ngôn ngữ - " + ex.Message);
            }
        }

        public void SelectMovieByName(string movieData)
        {
            // Lọc tên phim sạch sẽ từ Excel
            string movieName = movieData.Replace("Phim:", "").Replace("Phim: ", "")
                                        .Replace("“", "").Replace("”", "").Replace("\"", "")
                                        .Trim();

            // XPATH SIÊU BỀN VỮNG: Tìm thẻ <a> chứa thẻ div có class 'movie-title' chứa chữ Conan
            // Dù phim ở vị trí div[8] hay div[100] vẫn click trúng
            var movieLink = _wait.Until(d => d.FindElement(By.XPath($"//a[.//div[contains(@class, 'movie-title') and contains(text(), '{movieName}')]]")));

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", movieLink);
            System.Threading.Thread.Sleep(1000);

            js.ExecuteScript("arguments[0].click();", movieLink);
            System.Threading.Thread.Sleep(1500);
        }

        // Tối giản và linh hoạt theo yêu cầu của bạn
        public bool IsDetailedInfoDisplayed()
        {
            try
            {
                var title = _wait.Until(d => d.FindElement(By.CssSelector("h3.fw-bold"))).Displayed;
                var country = _wait.Until(d => d.FindElement(By.XPath("//p[strong[contains(text(), 'Quốc gia')]]"))).Displayed;
                return title && country;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi hiển thị thông tin phim: " + ex.Message);
                return false;
            }
        }

        public bool IsContentAndScheduleVisible()
        {
            try
            {
                // Bắt đúng class theo HTML bạn gửi
                var contentHeader = _wait.Until(d => d.FindElement(By.XPath("//h5[contains(text(), 'Nội dung phim')]"))).Displayed;
                var showtimeHeader = _wait.Until(d => d.FindElement(By.XPath("//h5[contains(text(), 'Lịch chiếu')]"))).Displayed;
                return contentHeader && showtimeHeader;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi hiển thị Nội dung/Lịch chiếu: " + ex.Message);
                return false;
            }
        }

        public void NavigateToMovieUrl(string fullUrl)
        {
            // Lấy nguyên đường dẫn đầy đủ từ Excel như bạn yêu cầu
            _driver.Navigate().GoToUrl(fullUrl);
        }

        public string GetFullPageText()
        {
            return _driver.FindElement(By.TagName("body")).Text;
        }

        // =======================================================
        // CÁC HÀM BỔ SUNG CHO CHỨC NĂNG ĐẶT VÉ (Không ảnh hưởng code cũ)
        // =======================================================

        public void SelectDateTabByDateAttribute(string dateVal)
        {
            // Tìm theo thuộc tính data-date (VD: "2026-04-20")
            var dateTab = _wait.Until(d => d.FindElement(By.CssSelector($"button[data-date='{dateVal}']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", dateTab);
            System.Threading.Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", dateTab);
        }

        public void ClickShowtimeBadge(string time)
        {
            // Tìm thẻ span chứa giờ chiếu (VD: "08:00")
            var showtime = _wait.Until(d => d.FindElement(By.XPath($"//span[contains(@class, 'showtime-badge') and contains(text(), '{time}')]")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", showtime);
            System.Threading.Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", showtime);
        }
    }
}