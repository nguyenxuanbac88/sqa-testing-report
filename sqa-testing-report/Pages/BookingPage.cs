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
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void SelectDate(string dateVal)
        {
            var dateBtn = _wait.Until(d => d.FindElement(By.CssSelector($"button[data-date='{dateVal}']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", dateBtn);
            System.Threading.Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", dateBtn);
        }

        public void ClickShowtimeSpan()
        {
            var showtimeSpan = _wait.Until(d => d.FindElement(By.XPath("//*[@id='showtime-list']/div/div/span")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", showtimeSpan);
            System.Threading.Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", showtimeSpan);
        }

        // TỰ ĐỘNG LẤY DANH SÁCH GHẾ TRỐNG
        public IList<IWebElement> GetAvailableSeatsList()
        {
            var xpath = "//div[contains(@class, 'seat') and contains(@class, 'available') and not(contains(@class, 'vip')) and not(contains(@class, 'double-seat')) and not(contains(@class, 'selected'))]";
            return _wait.Until(d => d.FindElements(By.XPath(xpath)));
        }

        public void SelectAvailableRegularSeats(int count)
        {
            var availableSeats = GetAvailableSeatsList();
            if (availableSeats.Count < count) throw new Exception($"Không đủ ghế trống! Cần {count}.");

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            for (int i = 0; i < count; i++)
            {
                js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", availableSeats[i]);
                System.Threading.Thread.Sleep(300);
                js.ExecuteScript("arguments[0].click();", availableSeats[i]);
                System.Threading.Thread.Sleep(300);
            }
        }

        // TÌM VÀ CLICK 1 GHẾ ĐÃ BÁN BẤT KỲ
        public void ClickAnySoldSeat()
        {
            // Tìm ghế có class sold, booked hoặc unavailable
            var soldSeat = _wait.Until(d => d.FindElement(By.XPath("//div[contains(@class, 'seat') and (contains(@class, 'sold') or contains(@class, 'booked'))]")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", soldSeat);
            System.Threading.Thread.Sleep(300);
            js.ExecuteScript("arguments[0].click();", soldSeat);
        }

        // LẤY TEXT TỪ POPUP CẢNH BÁO (JS ALERT HOẶC MODAL)
        public string GetAlertTextAndAccept()
        {
            try
            {
                var alert = _wait.Until(d => d.SwitchTo().Alert());
                string text = alert.Text;
                alert.Accept();
                return text;
            }
            catch
            {
                try
                {
                    // Dự phòng nếu dùng modal HTML thay vì JS Alert
                    var modalBody = _driver.FindElement(By.CssSelector(".modal-body, .alert, .toast"));
                    return modalBody.Text;
                }
                catch { return ""; }
            }
        }

        public bool IsTotalPriceDisplayed()
        {
            try
            {
                var total = _wait.Until(d => d.FindElement(By.Id("totalAmount")));
                return total.Displayed && !string.IsNullOrEmpty(total.Text) && total.Text.Trim() != "0";
            }
            catch { return false; }
        }

        public void ClickContinue()
        {
            var btn = _wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Tiếp tục') or contains(text(), 'Continue') or @id='btnContinue']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", btn);
            System.Threading.Thread.Sleep(300);
            js.ExecuteScript("arguments[0].click();", btn);
        }

        public void ClickSimulateExpire()
        {
            var expireBtn = _wait.Until(d => d.FindElement(By.Id("testExpire"))); // Nhấn nút giả lập hết giờ
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", expireBtn);
            System.Threading.Thread.Sleep(300);
            js.ExecuteScript("arguments[0].click();", expireBtn);
        }

        // CÁC HÀM CHO TRANG THANH TOÁN (TC_10)
        public void CheckTermsAndCheckout()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            try
            {
                var chkTerms = _driver.FindElement(By.Id("agreeTerms")); // Sửa ID tùy thực tế web bạn
                if (!chkTerms.Selected) js.ExecuteScript("arguments[0].click();", chkTerms);
            }
            catch { }

            var btnCheckout = _wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Thanh toán') or contains(text(), 'Checkout')]")));
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", btnCheckout);
            System.Threading.Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", btnCheckout);
        }
    }
}