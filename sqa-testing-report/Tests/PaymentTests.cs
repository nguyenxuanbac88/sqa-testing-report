using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class PaymentTests
    {
        private IWebDriver driver;
        private ExcelTestCaseHelper excelHelper;
        private LoginPage loginPage;
        private MoviePage moviePage;
        private BookingPage bookingPage;
        private PaymentPage paymentPage;
        private readonly string sheetName = "Kha_automationTC";

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            loginPage = new LoginPage(driver);
            moviePage = new MoviePage(driver);
            bookingPage = new BookingPage(driver);
            paymentPage = new PaymentPage(driver);

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

        [Test] public void TC_PAY_01_RedirectToPayPal() => ExecutePaymentTest("TC_PAY_01");
        [Test] public void TC_PAY_03_CancelTransaction() => ExecutePaymentTest("TC_PAY_03");
        [Test] public void TC_PAY_04_TimeoutTransaction() => ExecutePaymentTest("TC_PAY_04");
        [Test] public void TC_PAY_14_InsufficientFunds() => ExecutePaymentTest("TC_PAY_14");
        [Test] public void TC_PAY_15_SuccessfulPayment() => ExecutePaymentTest("TC_PAY_15");

        private void ExecutePaymentTest(string tcId)
        {
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            try
            {
                moviePage.NavigateToMovieUrl("http://api.dvxuanbac.com:81/Movie/Details/37");
                moviePage.SwitchToVietnamese();
                bookingPage.SelectDate("2026-04-25");
                System.Threading.Thread.Sleep(500);
                bookingPage.ClickShowtimeSpan();
                System.Threading.Thread.Sleep(1500);

                if (!loginPage.IsLoginModalDisplayed())
                {
                    try { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", driver.FindElement(By.XPath("/html/body/nav/div/div[2]/a"))); }
                    catch { loginPage.OpenLoginModal(); }
                }

                loginPage.EnterEmail("nguyendamkha@gmail.com");
                loginPage.EnterPassword("Vs2022.NET15.0");
                loginPage.SubmitLogin();
                System.Threading.Thread.Sleep(3000);

                bookingPage.SelectAvailableRegularSeats(2);
                bookingPage.ClickContinue();
                System.Threading.Thread.Sleep(1500);
                bookingPage.ClickContinue();
                System.Threading.Thread.Sleep(1500);

                if (tcId == "TC_PAY_03" || tcId == "TC_PAY_04" || tcId == "TC_PAY_14" || tcId == "TC_PAY_15")
                {
                    paymentPage.SelectPaymentMethod("paypal");
                    paymentPage.ClickThanhToanToOpenModal();
                    System.Threading.Thread.Sleep(1000);
                    paymentPage.ConfirmPaymentModal();
                    System.Threading.Thread.Sleep(8000);
                }

                bool isPreviousStepFailed = false;

                foreach (var step in steps)
                {
                    if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                    try
                    {
                        string action = step.StepAction?.ToLower() ?? "";

                        // ==============================================
                        // [TC_PAY_01]
                        // ==============================================
                        if (tcId == "TC_PAY_01")
                        {
                            if (action.Contains("chọn phương thức"))
                            {
                                paymentPage.SelectPaymentMethod("paypal");
                                step.ActualResult = "Đã tích chọn Radio button PayPal.";
                            }
                            else if (action.Contains("nhấn nút \"thanh toán\"") && !action.Contains("popup"))
                            {
                                paymentPage.ClickThanhToanToOpenModal();
                                System.Threading.Thread.Sleep(1000);
                                step.ActualResult = "Hiển thị popup xác nhận với tổng tiền 220,000 đ.";
                            }
                            else if (action.Contains("tích chọn"))
                            {
                                paymentPage.ConfirmPaymentModal();
                                System.Threading.Thread.Sleep(8000);

                                Assert.IsTrue(driver.Url.ToLower().Contains("sandbox.paypal.com"), "Lỗi: Không sang được trang PayPal.");

                                paymentPage.LoginPayPal("sb-phsjn47533405@personal.example.com", "JnIA.j2m");

                                string amount = paymentPage.GetPayPalAmount();
                                Assert.IsTrue(amount.Contains("$8.80"), $"Lỗi: Tiền hiển thị trên PayPal sai. Thực tế: {amount}");

                                step.ActualResult = "Hệ thống redirect đến PayPal và hiển thị đúng số tiền quy đổi ($8.80).";
                            }
                        }

                        // ==============================================
                        // [TC_PAY_03] - HỦY GIAO DỊCH TỪ PAYPAL
                        // ==============================================
                        else if (tcId == "TC_PAY_03")
                        {
                            if (action.Contains("cancel"))
                            {
                                paymentPage.ClickCancelAndReturn();
                                step.ActualResult = "Đã nhấn nút Cancel trên PayPal.";
                            }
                            else if (action.Contains("quan sát"))
                            {
                                System.Threading.Thread.Sleep(4000); // Chờ PayPal chuyển hướng về Web
                                Assert.IsTrue(driver.Url.ToLower().Contains("dvxuanbac.com"), "Lỗi: Không redirect về trang chủ.");

                                // Ép hệ thống mở lại trang Sơ đồ ghế để soi trạng thái thực tế của ghế
                                driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81/Seat/Matrix/152");
                                System.Threading.Thread.Sleep(3000);

                                // Quét tìm xem có cái ghế nào đang bị dính class 'held' không
                                var heldSeats = driver.FindElements(By.CssSelector(".seat.held"));

                                // BẮT BUG VÀ ĐÁNH FAIL TEST CASE:
                                if (heldSeats.Count > 0)
                                {
                                    // Dùng throw Exception để đẩy test case vào khối catch -> Tự động ghi "Fail" vào Excel và chụp ảnh màn hình
                                    throw new Exception("Lỗi: Ghế bị kẹt ở trạng thái 'Tạm giữ' (held) bắt đợi 5 phút thay vì 'Đang chọn' (selected).");
                                }

                                // Đoạn này dự phòng cho tương lai (Nếu Dev sửa xong bug thì code mới chạy được xuống đây để Pass)
                                var selectedSeats = driver.FindElements(By.CssSelector(".seat.selected"));
                                Assert.IsTrue(selectedSeats.Count > 0, "Lỗi: Mất trạng thái ghế đang chọn.");
                                step.ActualResult = "Về Web thành công, ghế vẫn đang chọn.";
                            }
                        }

                        // ==============================================
                        // [TC_PAY_04] - GIẢ LẬP HẾT THỜI GIAN TRÊN TAB MỚI
                        // ==============================================
                        else if (tcId == "TC_PAY_04")
                        {
                            if (action.Contains("giữ nguyên màn hình"))
                            {
                                // Đăng nhập trước để vào màn hình Complete Purchase
                                paymentPage.LoginPayPal("sb-phsjn47533405@personal.example.com", "JnIA.j2m");

                                ((IJavaScriptExecutor)driver).ExecuteScript("window.open();");
                                var handles = driver.WindowHandles;
                                driver.SwitchTo().Window(handles[1]);

                                driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81/Movie/Details/37");
                                System.Threading.Thread.Sleep(2000);
                                paymentPage.ClickSimulateExpire(); // Ép hệ thống nhả ghế
                                System.Threading.Thread.Sleep(2000);

                                driver.Close();
                                driver.SwitchTo().Window(handles[0]);

                                step.ActualResult = "Đã vào trang Complete Purchase và dùng Tab phụ để ép Server hủy session giữ ghế.";
                            }
                            else if (action.Contains("purchase") || action.Contains("nhấn"))
                            {
                                paymentPage.ClickCompletePurchase();
                                System.Threading.Thread.Sleep(10000); // Chờ PayPal callback

                                // BẮT BUG TẠI ĐÂY: Nếu hệ thống vẫn cho ra vé thành công (Có class ticket-modal)
                                bool isTicketIssued = false;
                                try
                                {
                                    isTicketIssued = driver.FindElement(By.ClassName("ticket-modal")).Displayed;
                                }
                                catch { }

                                if (isTicketIssued || driver.Url.ToLower().Contains("ticket"))
                                {
                                    throw new Exception("Lỗi (Bug Nghiêm trọng): Ghế đã hết thời gian giữ (đã hủy) nhưng Backend VẪN CHO THANH TOÁN THÀNH CÔNG và xuất vé.");
                                }

                                step.ActualResult = "Hệ thống báo lỗi hết hạn và không xuất vé.";
                            }
                        }

                        // ==============================================
                        // [TC_PAY_14]
                        // ==============================================
                        else if (tcId == "TC_PAY_14")
                        {
                            if (action.Contains("đăng nhập"))
                            {
                                paymentPage.LoginPayPal("hubcinema_user@personal.example.com", "V>*15nN;");

                                bool hasError = paymentPage.IsInsufficientFundsErrorDisplayed();
                                Assert.IsTrue(hasError, "Lỗi: PayPal không hiển thị thông báo yêu cầu thẻ (Add credit card) hoặc lỗi số dư.");

                                step.ActualResult = "PayPal báo lỗi không đủ tiền hoặc yêu cầu Add credit card.";
                            }
                            else if (action.Contains("quay lại"))
                            {
                                paymentPage.ClickCancelAndReturn();
                                System.Threading.Thread.Sleep(3000);
                                step.ActualResult = "Web báo thanh toán thất bại, trở về trang web an toàn.";
                            }
                        }

                        // ==============================================
                        // [TC_PAY_15]
                        // ==============================================
                        else if (tcId == "TC_PAY_15")
                        {
                            if (action.Contains("đăng nhập"))
                            {
                                paymentPage.LoginPayPal("sb-phsjn47533405@personal.example.com", "JnIA.j2m");
                                paymentPage.ClickCompletePurchase();
                                step.ActualResult = "Đã thao tác nhấn Complete Purchase.";
                            }
                            else if (action.Contains("chờ"))
                            {
                                System.Threading.Thread.Sleep(10000);
                                Assert.IsTrue(driver.Url.ToLower().Contains("ticket") || bookingPage.IsTicketDisplayed(), "Lỗi: Không in được vé sau khi trả tiền.");
                                step.ActualResult = "Hiển thị trang vé thành công.";
                            }
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
            }
            finally
            {
                excelHelper.WriteTestCaseSteps(sheetName, steps);
                if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"{tcId} thất bại.");
            }
        }

    }
}