using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class BookingPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public BookingPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        // BƯỚC 1.1: Chọn ngày chiếu dựa vào thuộc tính data-date
        public void SelectDate(string dateVal)
        {
            var dateBtn = _wait.Until(d => d.FindElement(By.CssSelector($"button[data-date='{dateVal}']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", dateBtn);
            System.Threading.Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", dateBtn);
        }

        // BƯỚC 1.2: Click suất chiếu bằng Xpath cụ thể
        public void ClickShowtimeSpan()
        {
            var showtimeSpan = _wait.Until(d => d.FindElement(By.XPath("//*[@id='showtime-list']/div/div/span")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", showtimeSpan);
            System.Threading.Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", showtimeSpan);
        }

        // BƯỚC 3: TỰ ĐỘNG CHỌN GHẾ TRỐNG THEO SỐ LƯỢNG YÊU CẦU
        public void SelectAvailableRegularSeats(int count)
        {
            // Tìm tất cả các div có class 'seat' VÀ 'available' (Loại trừ ghế VIP, ghế Đôi, hoặc ghế đã chọn/bán)
            var xpath = "//div[contains(@class, 'seat') and contains(@class, 'available') and not(contains(@class, 'vip')) and not(contains(@class, 'double-seat')) and not(contains(@class, 'selected'))]";

            var availableSeats = _wait.Until(d => d.FindElements(By.XPath(xpath)));

            if (availableSeats.Count < count)
            {
                throw new Exception($"Không đủ ghế trống! Yêu cầu {count} ghế, nhưng rạp chỉ còn {availableSeats.Count} ghế thường.");
            }

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;

            // Lặp để click đúng số lượng ghế
            for (int i = 0; i < count; i++)
            {
                js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", availableSeats[i]);
                System.Threading.Thread.Sleep(400); // Đợi chút cho mượt
                js.ExecuteScript("arguments[0].click();", availableSeats[i]);
                System.Threading.Thread.Sleep(500); // Đợi web tính tiền
            }
        }

        // BƯỚC 3 (Kiểm tra): Kiểm tra tổng tiền có đang hiển thị trên giao diện không
        public bool IsTotalPriceDisplayed()
        {
            try
            {
                var total = _wait.Until(d => d.FindElement(By.Id("totalAmount")));
                return total.Displayed && !string.IsNullOrEmpty(total.Text) && total.Text.Trim() != "0";
            }
            catch { return false; }
        }

        // BƯỚC 4: Nhấn nút Tiếp tục
        public void ClickContinue()
        {
            // Tìm nút có chữ 'Tiếp tục' hoặc 'Continue' hoặc id='btnContinue'
            var btn = _wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Tiếp tục') or contains(text(), 'Continue') or @id='btnContinue']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", btn);
            System.Threading.Thread.Sleep(300);
            js.ExecuteScript("arguments[0].click();", btn);
        }
    }
}