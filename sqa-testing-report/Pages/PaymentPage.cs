using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace sqa_testing_report.Pages
{
    public class PaymentPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public PaymentPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        public void SelectPaymentMethod(string methodName)
        {
            var radioBtn = _wait.Until(d => d.FindElement(By.XPath($"//input[@type='radio' and contains(@value, '{methodName.ToLower()}')]")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radioBtn);
        }

        public void ClickThanhToanToOpenModal()
        {
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

        // ĐÃ SỬA: Đăng nhập chuẩn 4 bước Split Login
        public void LoginPayPal(string email, string pass)
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Bước 1: Nhập Email
            var emailInput = wait.Until(d => d.FindElement(By.Id("email")));
            emailInput.Clear();
            emailInput.SendKeys(email);

            // Bước 2: Bấm Tiếp theo
            var btnNext = wait.Until(d => d.FindElement(By.Id("btnNext")));
            btnNext.Click();

            // Bước 3: Chờ form Password xuất hiện và nhập Password
            var passInput = wait.Until(d =>
            {
                var el = d.FindElement(By.Id("password"));
                return (el != null && el.Displayed) ? el : null;
            });
            passInput.Clear();
            passInput.SendKeys(pass);

            // Bước 4: Bấm Đăng nhập
            var btnLogin = wait.Until(d => d.FindElement(By.Id("btnLogin")));
            btnLogin.Click();

            System.Threading.Thread.Sleep(5000); // Chờ load trang Complete Purchase
        }

        public string GetPayPalAmount()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            try
            {
                var amountText = wait.Until(d => d.FindElement(By.XPath("//*[@data-testid='header-cart-total' or @data-testid='fi-amount']")));
                return amountText.Text;
            }
            catch { return ""; }
        }

        public void ClickCompletePurchase()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            var completeBtn = wait.Until(d => d.FindElement(By.XPath("//button[@data-id='payment-submit-btn' or contains(text(), 'Complete Purchase')]")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", completeBtn);
        }

        // ĐÃ SỬA: Dùng By.Id("cancelLink") để nhận diện chớp nhoáng
        public void ClickCancelAndReturn()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            var cancelLink = wait.Until(d => d.FindElement(By.Id("cancelLink")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", cancelLink);
        }

        public void ClickSimulateExpire()
        {
            var expireBtn = _wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Giả lập hết thời gian')]")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", expireBtn);
        }

        public bool IsInsufficientFundsErrorDisplayed()
        {
            try
            {
                // Bắt chính xác đoạn text 'Add credit or debit card' từ HTML thực tế của PayPal
                var errorMsg = _driver.FindElement(By.XPath("//*[contains(text(), 'Add credit or debit card') or contains(text(), 'Add credit card')]"));
                return errorMsg.Displayed;
            }
            catch { return false; }
        }
    }
}