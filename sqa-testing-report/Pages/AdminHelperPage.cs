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

        // Hàm kiểm tra trùng lặp (Tương tự Ctrl+F trong bảng)
        public bool IsDataExistInUserList(string keyword)
        {
            _driver.Navigate().GoToUrl("http://api.dvxuanbac.com:2080/UserAdmin/Index");

            // 1. Tìm ô tìm kiếm và điền keyword
            var searchInput = _wait.Until(d => d.FindElement(By.Name("keyword")));
            searchInput.Clear();
            searchInput.SendKeys(keyword);

            // 2. TỐI ƯU: Nhấn luôn phím Enter ngay tại ô input (thay vì tìm nút Click)
            searchInput.SendKeys(Keys.Enter);

            // 3. Đợi cho đến khi trang reload lại và URL có chứa chữ "keyword="
            // Việc này đáng tin cậy hơn là dùng Thread.Sleep(1000)
            _wait.Until(d => d.Url.Contains("keyword="));

            // 4. Kiểm tra xem có bất kỳ dòng (tr) nào trong phần THÂN BẢNG (tbody) chứa keyword không
            // Dùng //tbody//tr để tránh việc Selenium tìm nhầm lên phần thead (tiêu đề cột)
            var rows = _driver.FindElements(By.XPath($"//tbody//tr[contains(., '{keyword}')]"));

            return rows.Count > 0; // Trả về true nếu bị trùng (tìm thấy)
        }
    }
}