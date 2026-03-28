using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class BookingTests
    {
        private IWebDriver driver;
        private ExcelTestCaseHelper excelHelper;
        private LoginPage loginPage;
        private MoviePage moviePage;
        private BookingPage bookingPage;
        private readonly string sheetName = "Bac_automationTC";

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            loginPage = new LoginPage(driver);
            moviePage = new MoviePage(driver);
            bookingPage = new BookingPage(driver);

            string start = AppContext.BaseDirectory;
            string repoRoot = PathHelper.GetRepoRoot(start);
            string excelPath = Path.Combine(repoRoot ?? start, "Data", "DataTest.xlsx");
            excelHelper = new ExcelTestCaseHelper(excelPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null) { driver.Quit(); driver.Dispose(); }
        }

        [Test]
        public void TC_BOOK_01_KiemTraLuongChonGhe()
        {
            string tcId = "TC_BOOK_01";
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            // TIỀN ĐỀ: Mở trang chi tiết phim (Đã test trên ID 131)
            moviePage.NavigateToMovieUrl("http://api.dvxuanbac.com:81/Movie/Details/131");
            moviePage.SwitchToVietnamese();

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                try
                {
                    string action = step.StepAction?.ToLower() ?? "";

                    if (step.StepNumber == "1" && action.Contains("click chọn suất chiếu"))
                    {
                        // 1. Nhấn chọn tab ngày chiếu (VD: 2026-04-20)
                        bookingPage.SelectDate("2026-04-20");
                        System.Threading.Thread.Sleep(1000); // Đợi load danh sách giờ chiếu

                        // 2. Nhấn chọn giờ chiếu (Tự động trigger Modal đăng nhập)
                        bookingPage.ClickShowtimeSpan();
                        System.Threading.Thread.Sleep(1500); // Đợi popup bung lên

                        // 3. Kiểm tra Modal
                        Assert.IsTrue(loginPage.IsLoginModalDisplayed(), "Popup Đăng Nhập không hiển thị sau khi click suất chiếu.");
                        step.ActualResult = "Hiển thị popup 'Đăng Nhập Tài Khoản' thành công.";
                    }
                    else if (step.StepNumber == "2" && action.Contains("nhập tài khoản"))
                    {
                        string email = "nguyendamkha@gmail.com";
                        string pass = "Vs2022.NET15.0";

                        loginPage.EnterEmail(email);
                        loginPage.EnterPassword(pass);
                        loginPage.SubmitLogin();

                        System.Threading.Thread.Sleep(3000);

                        Assert.IsTrue(driver.Url.ToLower().Contains("seat") || driver.Url.ToLower().Contains("booking"), "Không chuyển hướng đến trang Chọn Ghế sau khi login.");
                        step.ActualResult = "Đăng nhập và chuyển hướng thành công đến trang 'Chọn ghế'.";
                    }
                    else if (step.StepNumber == "3" && action.Contains("chọn 2 ghế"))
                    {
                        bookingPage.SelectAvailableRegularSeats(2);

                        System.Threading.Thread.Sleep(1000);
                        Assert.IsTrue(bookingPage.IsTotalPriceDisplayed(), "Tổng tiền không được cập nhật sau khi chọn ghế.");

                        step.ActualResult = "Đã tự động chọn 2 ghế trống ngẫu nhiên. Ghế đổi màu và hiển thị tổng tiền.";
                    }
                    else if (step.StepNumber == "4" && action.Contains("nhấn nút \"tiếp tục\""))
                    {
                        bookingPage.ClickContinue();
                        System.Threading.Thread.Sleep(2000);

                        Assert.IsTrue(driver.Url.ToLower().Contains("combo") || driver.Url.ToLower().Contains("product"), "Không chuyển sang trang Chọn Combo/Sản phẩm.");
                        step.ActualResult = "Đã chuyển sang trang Chọn Combo / Sản phẩm.";
                    }

                    step.Status = "Pass";
                    step.Screenshots = "";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;
                    if (OperatingSystem.IsWindows()) step.Screenshots = ScreenshotHelper.Capture(tcId);
                    isPreviousStepFailed = true;
                }
            }

            excelHelper.WriteTestCaseSteps(sheetName, steps);
            if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"{tcId} thất bại.");
        }
    }
}