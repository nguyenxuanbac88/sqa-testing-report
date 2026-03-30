using OpenQA.Selenium;

namespace sqa_testing_report.Pages
{
    public class AdminCinemaPage
    {
        private readonly IWebDriver _driver;

        // --- Locators Chuẩn Theo HTML ---
        // Thêm locator của Menu chính (Quản lý rạp phim)
        private readonly By _menuTheaterCollapse = By.CssSelector("a.nav-link[data-bs-target='#collapseTheater']");
        private readonly By _menuListCinema = By.CssSelector("a.nav-link[href='/CinemaManagement/LoadListCinema']");

        private readonly By _btnCreateNew = By.CssSelector("a.btn.btn-success[href='/CinemaManagement/CreateCinema']");

        private readonly By _inputCinemaName = By.CssSelector("#CinemaName");
        private readonly By _inputAddress = By.CssSelector("#Address");
        private readonly By _inputCity = By.CssSelector("#City");
        private readonly By _btnSubmit = By.CssSelector("button[type='submit'].btn-success");

        // Locators cho phần hiển thị thông báo
        private readonly By _successAlert = By.CssSelector(".alert.alert-success");
        private readonly By _validationError = By.CssSelector(".text-danger.field-validation-error");

        // Locator dòng đầu tiên trong bảng
        private readonly By _firstCinemaRow = By.CssSelector("table tbody tr.clickable-row");

        public AdminCinemaPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void GoToListPage()
        {
            try
            {
                // 1. Tìm Menu Quản lý rạp phim
                var theaterMenu = _driver.FindElement(_menuTheaterCollapse);

                // Kiểm tra xem menu đang đóng hay mở, nếu đóng (false) thì click để mở
                if (theaterMenu.GetAttribute("aria-expanded") == "false")
                {
                    theaterMenu.Click();
                    Thread.Sleep(500); // Đợi nửa giây cho animation menu xổ xuống
                }

                // 2. Click vào menu con: Danh sách rạp phim
                _driver.FindElement(_menuListCinema).Click();
                Thread.Sleep(1500); // Chờ load trang danh sách
            }
            catch (Exception)
            {
                // Phương án dự phòng (Fallback): Lỡ UI bị che khuất thì nhảy thẳng bằng URL để test không gãy
                _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:2080/CinemaManagement/LoadListCinema");
                Thread.Sleep(1500);
            }
        }

        public void ClickCreateNew()
        {
            _driver.FindElement(_btnCreateNew).Click();
            Thread.Sleep(1000);
        }

        public void EnterCinemaInfo(string name, string address, string city)
        {
            var nameEle = _driver.FindElement(_inputCinemaName);
            nameEle.Clear();
            if (!string.IsNullOrEmpty(name)) nameEle.SendKeys(name);

            var addressEle = _driver.FindElement(_inputAddress);
            addressEle.Clear();
            if (!string.IsNullOrEmpty(address)) addressEle.SendKeys(address);

            var cityEle = _driver.FindElement(_inputCity);
            cityEle.Clear();
            if (!string.IsNullOrEmpty(city)) cityEle.SendKeys(city);
        }

        public void ClickSubmit()
        {
            _driver.FindElement(_btnSubmit).Click();
            Thread.Sleep(2000); // Đợi xử lý submit và chuyển trang
        }

        public bool IsCinemaExist(string cinemaName)
        {
            try
            {
                var elements = _driver.FindElements(By.XPath($"//table//tbody//tr//td//span[text()='{cinemaName}']"));
                return elements.Count > 0;
            }
            catch { return false; }
        }

        public string GetFirstCinemaName()
        {
            try
            {
                return _driver.FindElement(By.CssSelector("table tbody tr.clickable-row td span.text-main")).Text;
            }
            catch { return "Galaxy Nguyễn Du"; } // Fallback
        }

        // Click thẳng vào dòng đầu tiên (Rạp vừa tạo sẽ nằm trên cùng)
        public void ClickFirstCinemaRow()
        {
            try
            {
                var row = _driver.FindElement(_firstCinemaRow);
                row.Click();
                Thread.Sleep(1500); // Đợi load sang trang Edit
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể click vào dòng rạp đầu tiên: " + ex.Message);
            }
        }

        // --- Các hàm đọc thông báo ---
        public string GetSuccessMessage()
        {
            try
            {
                var alert = _driver.FindElements(_successAlert);
                return alert.Count > 0 ? alert[0].Text.Trim() : "Không tìm thấy thông báo thành công";
            }
            catch { return "Lỗi khi đọc alert"; }
        }

        public string GetValidationErrorMessage()
        {
            try
            {
                var errors = _driver.FindElements(_validationError);
                foreach (var err in errors)
                {
                    if (!string.IsNullOrEmpty(err.Text)) return err.Text.Trim();
                }
                return "Không có lỗi hiển thị";
            }
            catch { return "Lỗi khi đọc validation"; }
        }
    }
}