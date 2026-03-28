using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class AdminHelperPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public AdminHelperPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Hàm đảm bảo trạng thái đã đăng nhập
        public void EnsureLogin(string username, string password)
        {
            _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:2080/");

            try
            {
                // Tìm thử ô Username. Nếu có ô này nghĩa là chưa đăng nhập (hoặc hết session)
                var userInput = _wait.Until(d => d.FindElement(By.Id("Username")));
                userInput.Clear();
                userInput.SendKeys(username);

                var passInput = _driver.FindElement(By.Id("Password"));
                passInput.Clear();
                passInput.SendKeys(password);

                var loginBtn = _driver.FindElement(By.CssSelector(".btn-login"));
                loginBtn.Click();

                // Đợi cho đến khi URL chuyển sang giao diện quản trị
                _wait.Until(d => d.Url.Contains("/Dashboard") || d.Url.Contains("/UserAdmin"));
            }
            catch (WebDriverTimeoutException)
            {
                // Nếu tìm không thấy ô Username sau 10s, nghĩa là đã đăng nhập rồi -> Bỏ qua và đi tiếp
            }
        }

        // Hàm kiểm tra trùng lặp (Đã fix dứt điểm lỗi đơ form)
        public bool IsDataExistInUserList(string keyword)
        {
            _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:2080/UserAdmin/Index");

            // 1. Tìm ô tìm kiếm và điền keyword
            var searchInput = _wait.Until(d => d.FindElement(By.Name("keyword")));
            searchInput.Clear();
            searchInput.SendKeys(keyword);

            // 2. Tìm đúng cái nút "Tìm kiếm" dựa theo HTML bạn cung cấp
            var searchBtn = _driver.FindElement(By.CssSelector("button[type='submit'].btn-primary"));

            // Dùng Javascript ép click (Cách này an toàn tuyệt đối, không bao giờ bị đơ như phím Enter)
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", searchBtn);

            // 3. Cho Selenium nhắm mắt đợi 2 giây để server tải lại cái Bảng (Bỏ qua vụ check URL gây lỗi Timeout)
            System.Threading.Thread.Sleep(2000);

            // 4. Tìm kiếm nội dung trong thẻ <tbody> theo cấu trúc HTML bảng bạn cung cấp
            var rows = _driver.FindElements(By.XPath($"//table/tbody/tr[contains(., '{keyword}')]"));

            return rows.Count > 0; // Trả về true nếu tìm thấy dòng chứa keyword
        }
    }
}