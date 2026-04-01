using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;

namespace sqa_testing_report.Pages
{
    public class SeatLayoutPage
    {
        private readonly IWebDriver _driver;
        public SeatLayoutPage(IWebDriver driver) => _driver = driver;

        // --- SELECTOR BẠN CUNG CẤP ---
        // Nút Ghế ở trang danh sách/edit rạp

        private By _inputRows = By.Id("rows");
        private By _inputCols = By.Id("cols");
        private By _btnGenerate = By.Id("generateMatrix");
        private By _seatMatrixContainer = By.Id("seatMatrix");

        // Selector cho ô nhập giá hàng A (dòng 1) và hàng B (dòng 2)
        private By _inputPriceRowA = By.CssSelector("#seatTypeRows > tr:nth-child(1) > td:nth-child(3) > input.form-control.form-control-sm");
        private By _inputPriceRowB = By.CssSelector("#seatTypeRows > tr:nth-child(2) > td:nth-child(3) > input.form-control.form-control-sm");

        // Nút Lưu cuối cùng
        private By _btnSave = By.CssSelector("#layoutSidenav_content > main > form > button");

        // --- CÁC HÀM THAO TÁC ---

        public void ClickGoToSeatLayoutByRoomId(string roomId)
        {
            // Tìm chính xác thẻ <a> có chứa idRoom trong đường link ẩn (href)
            By dynamicBtn = By.XPath($"//a[contains(@href, 'idRoom={roomId}')]");

            // Vì chỉ có 1 trang nên chắc chắn nút phải nằm ở đây, cho nó chờ tối đa 5s
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            var btn = wait.Until(d => d.FindElement(dynamicBtn));

            // Cuộn chuột tới đúng vị trí cái nút đó (phòng trường hợp nút nằm tuốt dưới cùng)
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btn);
            System.Threading.Thread.Sleep(500); // Đợi cuộn mượt mà một chút

            // Bấm click! Nếu bị phần tử khác đè lên thì dùng JS ép click luôn
            try
            {
                btn.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
            }
        }
        public void EnterGridSize(string rows, string cols)
        {
            var r = _driver.FindElement(_inputRows);
            r.Clear(); r.SendKeys(rows);
            var c = _driver.FindElement(_inputCols);
            c.Clear(); c.SendKeys(cols);
        }

        public void ClickGenerate() => _driver.FindElement(_btnGenerate).Click();

        // Bước 6: Bấm vào các ô trong ma trận (thường là các thẻ <td> hoặc <div> bên trong #seatMatrix)
        public void ClickAllSeatsInMatrix()
        {
            var seats = _driver.FindElements(By.CssSelector("#seatMatrix td, #seatMatrix .seat"));
            foreach (var seat in seats)
            {
                if (seat.Displayed) seat.Click();
            }
        }

        public void FillPriceRows(string priceA, string priceB)
        {
            if (!string.IsNullOrEmpty(priceA))
            {
                var a = _driver.FindElement(_inputPriceRowA);
                a.Clear(); a.SendKeys(priceA);
            }
            if (!string.IsNullOrEmpty(priceB))
            {
                var b = _driver.FindElement(_inputPriceRowB);
                b.Clear(); b.SendKeys(priceB);
            }
        }

        public void ClickSave()
        {
            var btn = _driver.FindElement(_btnSave);

            // 1. Kéo tuột xuống tận ĐÁY của trang web (Bất chấp ma trận dài bao nhiêu)
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

            // Đợi 1.5 giây để trình duyệt kịp "thở" và vẽ xong 10.000 cái ghế
            System.Threading.Thread.Sleep(1500);

            // 2. Định vị lại cái nút một lần nữa cho chắc cú (kéo nút nằm sát mép dưới màn hình)
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(false);", btn);
            System.Threading.Thread.Sleep(500);

            try
            {
                // 3. Thử click 
                btn.Click();
            }
            catch
            {
                // 4. Búa tạ JS Click lờ đi mọi sự che khuất
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
            }
        }

        public string GetNotificationText()
        {
            try
            {
                // Vét lỗi từ các span báo đỏ hoặc alert
                var errorSpans = _driver.FindElements(By.CssSelector("span.text-danger, .alert"));
                string msg = "";
                foreach (var s in errorSpans) if (s.Displayed) msg += s.Text + " | ";
                return string.IsNullOrEmpty(msg) ? "Thao tác thành công" : msg.TrimEnd(' ', '|');
            }
            catch { return "Không có thông báo"; }
        }
        // Hàm chuyên trị Popup của trình duyệt (JavaScript Alert)
        public string GetAlertMessageAndAccept()
        {
            try
            {
                // Chờ tối đa 2 giây xem có popup nào bật ra không
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                wait.Until(d =>
                {
                    try { d.SwitchTo().Alert(); return true; }
                    catch (NoAlertPresentException) { return false; }
                });

                // Nếu có, chuyển hướng sang cái Alert đó
                IAlert alert = _driver.SwitchTo().Alert();
                string alertText = alert.Text; // Lấy chữ ra ("Vui lòng nhập số hàng...")

                alert.Accept(); // Nhấn nút OK để đóng popup

                return alertText;
            }
            catch (WebDriverTimeoutException)
            {
                return null; // Không có popup nào hiện ra cả
            }
        }
    }
}