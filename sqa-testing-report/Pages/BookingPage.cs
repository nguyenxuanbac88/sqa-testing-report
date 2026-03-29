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

        public void SelectDate(string dateVal)
        {
            var dateBtn = _wait.Until(d => d.FindElement(By.CssSelector($"button[data-date='{dateVal}']")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", dateBtn);
        }

        public void ClickShowtimeSpan()
        {
            var showtimeSpan = _wait.Until(d => d.FindElement(By.CssSelector("span.showtime-badge")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", showtimeSpan);
        }

        // Lấy danh sách ghế trống (Loại trừ VIP và Ghế đôi theo đúng logic test của bạn)
        public IList<IWebElement> GetAvailableSeatsList()
        {
            var xpath = "//div[contains(@class, 'seat-row')]//div[contains(@class, 'seat') and contains(@class, 'available') and not(contains(@class, 'vip')) and not(contains(@class, 'double-seat')) and not(contains(@class, 'selected'))]";
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
                System.Threading.Thread.Sleep(300); // Đợi DOM update class 'selected'
            }
        }

        public void ClickAnySoldSeat()
        {
            // Dựa theo HTML bạn cung cấp, ghế đã bán có class 'confirmed'
            var soldSeats = _driver.FindElements(By.CssSelector(".seat.confirmed"));
            if (soldSeats.Count == 0) throw new Exception("Không có ghế nào đang ở trạng thái đã bán để test.");

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", soldSeats[0]);
        }

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
                    // Đợi toast/modal hiển thị thông báo lỗi (nếu web dùng UI thay vì Browser Alert)
                    var modalBody = _wait.Until(d => d.FindElement(By.CssSelector(".modal-body, .alert, .toast")));
                    return modalBody.Text;
                }
                catch { return ""; }
            }
        }

        public bool IsTotalPriceDisplayed()
        {
            try
            {
                var total = _wait.Until(d => d.FindElement(By.XPath("//*[@id='totalAmount' or @id='totalAmountDisplay']")));
                return total.Displayed && !string.IsNullOrEmpty(total.Text) && total.Text.Trim() != "0";
            }
            catch { return false; }
        }

        public void ClickContinue()
        {
            var btn = _wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Tiếp tục') or @id='btnDatVe' or @id='continueBtn']")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", btn);
        }

        public void ClickSimulateExpire()
        {
            var expireBtn = _wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Giả lập hết thời gian')]")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", expireBtn);
        }

        // Tương tác trang Combo dựa trên HTML cung cấp
        public void SelectCombo(int comboId, int clicks)
        {
            var btnPlus = _wait.Until(d => d.FindElement(By.CssSelector($"button[onclick*='adjustQty({comboId}, 1)']")));
            for (int i = 0; i < clicks; i++)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", btnPlus);
                System.Threading.Thread.Sleep(500);
            }
        }

        // Các hàm thanh toán PayPal giữ nguyên vì bạn đã làm rất tốt
        public void SelectPayPalAndClickThanhToan()
        {
            var paypalRadio = _wait.Until(d => d.FindElement(By.Id("paypalRadio")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", paypalRadio);

            var btnThanhToan = _wait.Until(d => d.FindElement(By.Id("openModalBtn")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", btnThanhToan);
        }

        public void ConfirmPaymentModal()
        {
            var chkTerms = _wait.Until(d => d.FindElement(By.Id("agreeCheckbox")));
            if (!chkTerms.Selected) ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", chkTerms);

            var confirmBtn = _wait.Until(d => d.FindElement(By.Id("confirmBtn")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", confirmBtn);
        }

        public void LoginPayPalAndPay(string email, string pass)
        {
            WebDriverWait longWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            var emailInput = longWait.Until(d => d.FindElement(By.Id("email")));
            emailInput.Clear(); emailInput.SendKeys(email);

            var passInput = longWait.Until(d => d.FindElement(By.Id("password")));
            passInput.Clear(); passInput.SendKeys(pass);

            var loginBtn = _driver.FindElement(By.Id("btnLogin"));
            loginBtn.Click();

            var completeBtn = longWait.Until(d => d.FindElement(By.XPath("//button[@data-id='payment-submit-btn']")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", completeBtn);
        }

        public bool IsTicketDisplayed()
        {
            try { return _wait.Until(d => d.FindElement(By.ClassName("ticket-modal"))).Displayed; }
            catch { return false; }
        }
    }
}