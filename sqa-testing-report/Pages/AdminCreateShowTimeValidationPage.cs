using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class AdminCreateShowTimeValidationPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private readonly By _menuSchedule = By.CssSelector("a[data-bs-target='#collapseSchedule']");
        private readonly By _linkTimeline = By.CssSelector("a[href='/ShowtimeManagement/Timeline']");

        private readonly By _cinemaSelect = By.Id("cinemaSelect");
        private readonly By _timelineDateInput = By.Id("dateInput");
        private readonly By _btnLoadTimeline = By.Id("loadTimeline");
        private readonly By _btnAddShowtime = By.CssSelector("button.btn.btn-sm.btn-success.mt-4");

        private readonly By _movieSelect = By.Id("movieSelect");
        private readonly By _roomSelect = By.Id("roomSelect");
        private readonly By _timeInput = By.Name("GioChieu");
        private readonly By _priceInput = By.XPath("//input[@type='number' or contains(@name, 'Gia') or contains(@name, 'ChiPhi')]");
        private readonly By _typeInput = By.Name("TypeSuatChieu");
        private readonly By _btnSave = By.XPath("//button[@type='submit' and contains(text(), 'Lưu')]");
        private readonly By _btnCancelModal = By.CssSelector("button[data-bs-dismiss='modal']");

        private readonly By _dangerAlert = By.CssSelector(".alert.alert-danger");
        private readonly By _rowLabels = By.CssSelector(".g-row-label");
        // --- BỔ SUNG LOCATOR TIMELINE WRAPPER ---
        private readonly By _timelineWrapper = By.Id("timelineWrapper");

        public AdminCreateShowTimeValidationPage(IWebDriver driver)
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
            var cinemaDropdown = new SelectElement(_wait.Until(d => d.FindElement(_cinemaSelect)));
            cinemaDropdown.SelectByText(cinemaName);

            var dateEle = _driver.FindElement(_timelineDateInput);
            string formattedDate = date.ToString("yyyy-MM-dd");

            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript($"arguments[0].value = '{formattedDate}';", dateEle);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", dateEle);
        }

        public void ClickLoadTimeline()
        {
            _wait.Until(d => d.FindElement(_btnLoadTimeline)).Click();
            Thread.Sleep(1500);
        }

        public List<string> GetBookedRooms()
        {
            var alerts = _driver.FindElements(_dangerAlert);
            if (alerts.Count > 0 && alerts[0].Displayed) return new List<string>();

            var roomLabels = _driver.FindElements(_rowLabels);
            return roomLabels.Select(e => e.Text.Trim()).ToList();
        }

        public void ClickAddShowtime()
        {
            _wait.Until(d => d.FindElement(_btnAddShowtime)).Click();
            Thread.Sleep(1000);
        }

        public void CloseModal()
        {
            _driver.FindElement(_btnCancelModal).Click();
            Thread.Sleep(500);
        }

        // --- HÀM BẮT CHÍNH XÁC Ô NGÀY TRONG MODAL ---
        private IWebElement GetModalDateElement()
        {
            var dateInputs = _driver.FindElements(By.CssSelector("input[type='date']"));
            // Lấy ô ngày cuối cùng đang được hiển thị (thường là ô trong Modal đè lên trên)
            return dateInputs.LastOrDefault(e => e.Displayed);
        }

        public void FillShowtimeFormDynamic(string movieName, string roomName, string dateString, string startTime, string price, string typeValue)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;

            // Phim
            var movieSelectEle = _driver.FindElement(_movieSelect);
            if (string.IsNullOrEmpty(movieName)) new SelectElement(movieSelectEle).SelectByIndex(0);
            else new SelectElement(movieSelectEle).SelectByText(movieName);

            // Phòng
            var roomSelectEle = _driver.FindElement(_roomSelect);
            if (!string.IsNullOrEmpty(roomName)) new SelectElement(roomSelectEle).SelectByText(roomName);

            // Ngày (Xóa chuẩn bằng JS trên đúng element Modal)
            if (dateString != null)
            {
                var dateEle = GetModalDateElement();
                if (dateEle != null)
                {
                    if (string.IsNullOrEmpty(dateString))
                    {
                        // Gán rỗng sẽ đưa form về trạng thái mm/dd/yyyy mặc định
                        js.ExecuteScript("arguments[0].value = '';", dateEle);
                        js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", dateEle);
                        Thread.Sleep(300);
                    }
                    else
                    {
                        DateTime parsedDate = DateTime.Parse(dateString);
                        js.ExecuteScript($"arguments[0].value = '{parsedDate:yyyy-MM-dd}';", dateEle);
                        js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", dateEle);
                    }
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

        public void ClickSaveOnly()
        {
            _driver.FindElement(_btnSave).Click();
            Thread.Sleep(500);
        }

        public string GetValidationMessage(string fieldName)
        {
            IWebElement ele = fieldName switch
            {
                "Date" => GetModalDateElement(),
                "Time" => _driver.FindElement(_timeInput),
                "Price" => _driver.FindElement(_priceInput),
                "Type" => _driver.FindElement(_typeInput),
                "Movie" => _driver.FindElement(_movieSelect),
                _ => null
            };
            return ele?.GetAttribute("validationMessage") ?? "";
        }

        public bool IsModalOpen()
        {
            try { return _driver.FindElement(By.Id("addShowtimeModal")).Displayed; }
            catch { return false; }
        }

        public bool IsErrorAlertDisplayed()
        {
            try { return _driver.FindElement(By.CssSelector(".alert.alert-danger, .error-msg")).Displayed; }
            catch { return false; }
        }
        // --- BỔ SUNG HÀM TÌM SUẤT CHIẾU TRÊN TIMELINE ---
        public bool IsShowtimeDisplayedOnTimeline(string roomName, string movieName, string startTime)
        {
            try
            {
                _wait.Until(d => d.FindElement(_timelineWrapper));
                string xpath = $"//div[contains(@class, 'g-row')][.//div[contains(@class, 'g-row-label') and contains(text(), '{roomName}')]]//div[contains(@class, 'g-bar')][.//span[contains(@class, 'g-bar-text') and contains(text(), '{movieName}')] and .//span[contains(@class, 'g-bar-time') and contains(text(), '{startTime}')]]";

                var showtimes = _driver.FindElements(By.XPath(xpath));
                return showtimes.Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}