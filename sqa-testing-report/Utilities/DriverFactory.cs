using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace sqa_testing_report.Utilities
{
    public static class DriverFactory
    {
        public static IWebDriver InitDriver()
        {
            var options = new ChromeOptions();
            // options.AddArgument("--headless"); // Mở comment nếu muốn chạy ngầm

            var driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            return driver;
        }
    }
}