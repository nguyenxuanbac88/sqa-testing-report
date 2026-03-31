using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class AdminCreateShowTimePage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        // --- Locators Menu ---
        private readonly By _menuSchedule = By.CssSelector("a[data-bs-target='#collapseSchedule']");
        private readonly By _linkTimeline = By.CssSelector("a[href='/ShowtimeManagement/Timeline']");

        // --- Locators Timeline Page ---
        private readonly By _cinemaSelect = By.Id("cinemaSelect");
        private readonly By _dateInput = By.Id("dateInput"); // Đã update locator ID chuẩn
        private readonly By _btnLoadTimeline = By.Id("loadTimeline");
        // Update theo đúng class bạn cung cấp
        private readonly By _btnAddShowtime = By.CssSelector("button.btn.btn-sm.btn-success.mt-4");

        // --- Locators Modal Thêm lịch chiếu ---
        private readonly By _movieSelect = By.Id("movieSelect");
        private readonly By _roomSelect = By.Id("roomSelect");
        private readonly By _timeInput = By.Name("GioChieu");
        private readonly By _priceInput = By.XPath("//input[@type='number' or contains(@name, 'Gia') or contains(@name, 'ChiPhi')]");
        private readonly By _typeInput = By.Name("TypeSuatChieu");
        private readonly By _btnSave = By.XPath("//button[@type='submit' and contains(text(), 'Lưu')]");
        private readonly By _btnCancelModal = By.CssSelector("button[data-bs-dismiss='modal']");

        // --- Locators Kết quả Timeline ---
        private readonly By _timelineWrapper = By.Id("timelineWrapper");
        private readonly By _rowLabels = By.CssSelector(".g-row-label");
        private readonly By _dangerAlert = By.CssSelector(".alert.alert-danger");

        public AdminCreateShowTimePage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void NavigateToTimeline()
        {
            try
            {
                _wait.Until(d => d.FindElement(_menuSchedule)).Click();
                Thread.Sleep(500);
                _wait.Until(d => d.FindElement(_linkTimeline)).Click();
            }
            catch
            {
                _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:2080/ShowtimeManagement/Timeline");
            }
        }

        public void SelectCinemaAndDate(string cinemaName, DateTime date)
        {
            // Chọn rạp
            var cinemaDropdown = new SelectElement(_wait.Until(d => d.FindElement(_cinemaSelect)));
            cinemaDropdown.SelectByText(cinemaName);

            // Nhập ngày bằng JS qua đúng ID dateInput
            var dateEle = _driver.FindElement(_dateInput);
            string formattedDate = date.ToString("yyyy-MM-dd");

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript($"arguments[0].value = '{formattedDate}';", dateEle);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", dateEle);
        }

        public void ClickLoadTimeline()
        {
            _wait.Until(d => d.FindElement(_btnLoadTimeline)).Click();
            Thread.Sleep(1500); // Chờ API tải dữ liệu
        }

        // Lấy danh sách các phòng đã có lịch chiếu trên Timeline
        public List<string> GetBookedRooms()
        {
            var alerts = _driver.FindElements(_dangerAlert);
            if (alerts.Count > 0 && alerts[0].Displayed)
            {
                // Thông báo "Không có lịch chiếu" => Không có phòng nào bị chiếm
                return new List<string>();
            }

            var roomLabels = _driver.FindElements(_rowLabels);
            return roomLabels.Select(e => e.Text.Trim()).ToList();
        }

        public void ClickAddShowtime()
        {
            _wait.Until(d => d.FindElement(_btnAddShowtime)).Click();
            Thread.Sleep(1000); // Chờ Modal mở
        }

        // Lấy tất cả phòng chiếu có trong Dropdown của Modal
        public List<string> GetAllRoomsFromModal()
        {
            var roomDropdown = new SelectElement(_wait.Until(d => d.FindElement(_roomSelect)));
            // Bỏ qua option đầu tiên (-- Chọn phòng chiếu --)
            return roomDropdown.Options.Skip(1).Select(e => e.Text.Trim()).ToList();
        }

        public void CloseModal()
        {
            _driver.FindElement(_btnCancelModal).Click();
            Thread.Sleep(500);
        }

        public void FillShowtimeDetails(string movieName, string roomName, string startTime, string price, string typeValue)
        {
            new SelectElement(_wait.Until(d => d.FindElement(_movieSelect))).SelectByText(movieName);
            new SelectElement(_driver.FindElement(_roomSelect)).SelectByText(roomName);

            var timeEle = _driver.FindElement(_timeInput);
            DateTime parsedTime = DateTime.Parse(startTime);
            string time24h = parsedTime.ToString("HH:mm");

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript($"arguments[0].value = '{time24h}';", timeEle);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", timeEle);

            var priceEle = _driver.FindElement(_priceInput);
            priceEle.Clear();
            priceEle.SendKeys(price);

            var typeEle = _driver.FindElement(_typeInput);
            typeEle.Clear();
            typeEle.SendKeys(typeValue); // 1 = suất chiếu thường
        }

        public void SaveShowtime()
        {
            _driver.FindElement(_btnSave).Click();
            Thread.Sleep(2000); // Chờ Modal đóng
        }

        public bool IsShowtimeDisplayedOnTimeline(string roomName, string movieName, string startTime)
        {
            try
            {
                _wait.Until(d => d.FindElement(_timelineWrapper));
                // Tìm dòng của phòng -> Tìm bar chứa tên phim VÀ giờ chiếu
                string xpath = $"//div[contains(@class, 'g-row')][.//div[contains(@class, 'g-row-label') and contains(text(), '{roomName}')]]//div[contains(@class, 'g-bar')][.//span[contains(@class, 'g-bar-text') and contains(text(), '{movieName}')] and .//span[contains(@class, 'g-bar-time') and contains(text(), '{startTime}')]]";

                var showtimes = _driver.FindElements(By.XPath(xpath));
                return showtimes.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        // --- BỔ SUNG: Hàm đếm tổng số suất chiếu của một phòng ---
        public int CountShowtimesInRoom(string roomName)
        {
            try
            {
                string xpath = $"//div[contains(@class, 'g-row')][.//div[contains(@class, 'g-row-label') and contains(text(), '{roomName}')]]//div[contains(@class, 'g-bar')]";
                var showtimes = _driver.FindElements(By.XPath(xpath));
                return showtimes.Count;
            }
            catch
            {
                return 0;
            }
        }

        // =========================================================================
        // CÁC HÀM BỔ SUNG CHO PHẦN VALIDATION FORM (TEST CASE 01 -> 06)
        // =========================================================================

        public void FillShowtimeFormDynamic(string movieName, string roomName, string dateString, string startTime, string price, string typeValue)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;

            // Phim (Nếu null -> chọn index 0: "-- Chọn phim --")
            var movieSelectEle = _driver.FindElement(_movieSelect);
            if (string.IsNullOrEmpty(movieName)) new SelectElement(movieSelectEle).SelectByIndex(0);
            else new SelectElement(movieSelectEle).SelectByText(movieName);

            // Phòng
            var roomSelectEle = _driver.FindElement(_roomSelect);
            if (!string.IsNullOrEmpty(roomName)) new SelectElement(roomSelectEle).SelectByText(roomName);

            // Ngày (Xử lý xóa mô phỏng bàn phím cực kỳ chi tiết)
            if (dateString != null)
            {
                var dateEle = _driver.FindElement(_dateInput);
                if (string.IsNullOrEmpty(dateString))
                {
                    // Mô phỏng thao tác user: Focus vào ô input
                    dateEle.Click();
                    Thread.Sleep(200);

                    // Cách 1: Bôi đen toàn bộ và xóa (Chuẩn nhất trên Windows)
                    dateEle.SendKeys(Keys.Control + "a");
                    dateEle.SendKeys(Keys.Backspace);

                    // Cách 2 (Dự phòng): Lùi lại và xóa lần lượt yyyy, dd, mm như bạn mô tả
                    for (int i = 0; i < 3; i++)
                    {
                        dateEle.SendKeys(Keys.Right); // Nhảy đến ô yyyy
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        dateEle.SendKeys(Keys.Backspace); // Xóa phần tử hiện tại
                        dateEle.SendKeys(Keys.Left);      // Lùi qua phần tử tiếp theo
                    }

                    // Kích hoạt sự kiện để trình duyệt nhận diện dữ liệu đã rỗng
                    js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", dateEle);
                }
                else
                {
                    DateTime parsedDate = DateTime.Parse(dateString);
                    js.ExecuteScript($"arguments[0].value = '{parsedDate:yyyy-MM-dd}';", dateEle);
                    js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", dateEle);
                }
            }

            // Thời gian
            var timeEle = _driver.FindElement(_timeInput);
            if (string.IsNullOrEmpty(startTime)) js.ExecuteScript("arguments[0].value = '';", timeEle);
            else
            {
                DateTime parsedTime = DateTime.Parse(startTime);
                js.ExecuteScript($"arguments[0].value = '{parsedTime:HH:mm}';", timeEle);
            }
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", timeEle);

            // Chi phí
            var priceEle = _driver.FindElement(_priceInput);
            priceEle.Clear();
            if (!string.IsNullOrEmpty(price)) priceEle.SendKeys(price);

            // Loại
            var typeEle = _driver.FindElement(_typeInput);
            typeEle.Clear();
            if (!string.IsNullOrEmpty(typeValue)) typeEle.SendKeys(typeValue);
        }

        // Click Save nhưng không chờ đợi quá lâu để kiểm tra validation
        public void ClickSaveOnly()
        {
            _driver.FindElement(_btnSave).Click();
            Thread.Sleep(500); // Đợi popup báo lỗi HTML5 bật lên
        }

        // Lấy thông báo lỗi mặc định của thẻ HTML5 Required
        public string GetValidationMessage(string fieldName)
        {
            IWebElement ele = fieldName switch
            {
                "Date" => _driver.FindElement(_dateInput),
                "Time" => _driver.FindElement(_timeInput),
                "Price" => _driver.FindElement(_priceInput),
                "Type" => _driver.FindElement(_typeInput),
                "Movie" => _driver.FindElement(_movieSelect),
                _ => null
            };
            return ele?.GetAttribute("validationMessage") ?? "";
        }

        // Kiểm tra xem Modal có còn mở hay không
        public bool IsModalOpen()
        {
            try { return _driver.FindElement(By.Id("addShowtimeModal")).Displayed; }
            catch { return false; }
        }

        // Kiểm tra có Alert báo lỗi Custom nào không
        public bool IsErrorAlertDisplayed()
        {
            try { return _driver.FindElement(By.CssSelector(".alert.alert-danger, .error-msg")).Displayed; }
            catch { return false; }
        }
    }
}