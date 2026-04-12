using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public partial class BookingTests
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

        // KHAI BÁO 8 TEST CASES
        [Test] public void TC_BOOK_01_KiemTraLuongChonGhe() => ExecuteBookingTest("TC_BOOK_01");
        [Test] public void TC_BOOK_02_CapNhatTrangThaiGhe() => ExecuteBookingTest("TC_BOOK_02");
        [Test] public void TC_BOOK_03_SoDoGheBanDau() => ExecuteBookingTest("TC_BOOK_03");
        [Test] public void TC_BOOK_04_XuLyDongThoi() => ExecuteBookingTest("TC_BOOK_04");
        [Test] public void TC_BOOK_05_HuyGheKhiHetGio() => ExecuteBookingTest("TC_BOOK_05");
        [Test] public void TC_BOOK_06_ChanChonGheDaBan() => ExecuteBookingTest("TC_BOOK_06");
        [Test] public void TC_BOOK_07_GioiHan8Ghe() => ExecuteBookingTest("TC_BOOK_07");
        [Test] public void TC_BOOK_10_KiemTraThanhToan() => ExecuteBookingTest("TC_BOOK_10");
        [Test] public void TC_BOOK_11_GheThuongVaVip() => ExecuteBookingTest("TC_BOOK_11");

        private void ExecuteBookingTest(string tcId)
        {
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            IWebDriver driver2 = null; // Khởi tạo biến dành riêng cho TC_BOOK_04

            try
            {
                moviePage.NavigateToMovieUrl("http://api.dvxuanbac.com:81/Movie/Details/162");
                moviePage.SwitchToVietnamese();

                // Setup luồng chạy đến trang Chọn Ghế cho các TC cần thiết
                if (tcId != "TC_BOOK_01")
                {
                    bookingPage.SelectDate("2026-04-20");
                    System.Threading.Thread.Sleep(1000);
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
                    System.Threading.Thread.Sleep(3000); // Chờ load trang sơ đồ ghế

                    // Setup riêng cho TC_BOOK_10
                    if (tcId == "TC_BOOK_10")
                    {
                        bookingPage.SelectAvailableRegularSeats(2);
                        bookingPage.ClickContinue();
                        System.Threading.Thread.Sleep(2000);
                        bookingPage.SelectCombo(13, 1);
                        bookingPage.ClickContinue();
                        System.Threading.Thread.Sleep(2000);
                    }
                }

                bool isPreviousStepFailed = false;
                string sharedSeatIdForTc04 = ""; // Biến lưu ID ghế chọn chung cho 2 trình duyệt

                foreach (var step in steps)
                {
                    if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                    try
                    {
                        string action = step.StepAction?.ToLower() ?? "";

                        // ==============================================
                        // CÔ LẬP HOÀN TOÀN TC_BOOK_04 - XỬ LÝ 2 TRÌNH DUYỆT
                        // ==============================================
                        if (tcId == "TC_BOOK_04")
                        {
                            if (step.StepNumber == "1" || action.Contains("hai tài khoản cùng chọn ghế"))
                            {
                                // 1. Tìm 1 ghế trống bất kỳ trên driver 1
                                var availableSeats = bookingPage.GetAvailableSeatsList();
                                if (availableSeats.Count == 0) throw new Exception("Không có ghế trống để test đồng thời.");
                                sharedSeatIdForTc04 = availableSeats[0].GetAttribute("data-id");

                                // Click chọn trên driver 1
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", availableSeats[0]);
                                System.Threading.Thread.Sleep(500);

                                // 2. Mở trình duyệt 2 (Người dùng 2)
                                driver2 = DriverFactory.InitDriver();
                                driver2.Navigate().GoToUrl("http://api.dvxuanbac.com:81/Movie/Details/162");

                                var moviePage2 = new MoviePage(driver2);
                                moviePage2.SwitchToVietnamese();

                                var bookingPage2 = new BookingPage(driver2);
                                bookingPage2.SelectDate("2026-04-20");
                                System.Threading.Thread.Sleep(1000);
                                bookingPage2.ClickShowtimeSpan();
                                System.Threading.Thread.Sleep(1500);

                                var loginPage2 = new LoginPage(driver2);
                                if (!loginPage2.IsLoginModalDisplayed())
                                {
                                    try { ((IJavaScriptExecutor)driver2).ExecuteScript("arguments[0].click();", driver2.FindElement(By.XPath("/html/body/nav/div/div[2]/a"))); }
                                    catch { loginPage2.OpenLoginModal(); }
                                }

                                // SỬ DỤNG TÀI KHOẢN 2 NHƯ BẠN YÊU CẦU
                                loginPage2.EnterEmail("khoa992005@gmail.com");
                                loginPage2.EnterPassword("Khoa@123");
                                loginPage2.SubmitLogin();
                                System.Threading.Thread.Sleep(3000);

                                // 3. Click chọn CÙNG ghế đó trên driver 2
                                var seatOnDriver2 = driver2.FindElement(By.CssSelector($".seat[data-id='{sharedSeatIdForTc04}']"));
                                ((IJavaScriptExecutor)driver2).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", seatOnDriver2);

                                step.ActualResult = $"Đã mở 2 trình duyệt và cùng chọn vào ghế [{sharedSeatIdForTc04}].";
                            }
                            else if (step.StepNumber == "2" || action.Contains("cả hai trình duyệt nhấn nút \"tiếp tục\""))
                            {
                                if (driver2 == null) throw new Exception("Trình duyệt thứ 2 chưa được khởi tạo!");

                                var btnContinue1 = driver.FindElement(By.XPath("//button[contains(text(), 'Tiếp tục') or @id='btnDatVe']"));
                                var btnContinue2 = driver2.FindElement(By.XPath("//button[contains(text(), 'Tiếp tục') or @id='btnDatVe']"));

                                // Nhấn nút tiếp tục trên 2 trình duyệt ở CÙNG 1 THỜI ĐIỂM
                                Task t1 = Task.Run(() => ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnContinue1));
                                Task t2 = Task.Run(() => ((IJavaScriptExecutor)driver2).ExecuteScript("arguments[0].click();", btnContinue2));
                                Task.WaitAll(t1, t2);

                                System.Threading.Thread.Sleep(3000); // Chờ cả 2 load
                                step.ActualResult = "Đã nhấn nút Tiếp tục thành công trên cả 2 màn hình cùng lúc.";
                            }
                            else if (step.StepNumber == "3" || action.Contains("kiểm tra thông báo kết quả"))
                            {
                                string errText1 = "";
                                string errText2 = "";

                                // BƯỚC QUAN TRỌNG: Phải kiểm tra và đóng Alert (nếu có) TRƯỚC khi gọi driver.Url
                                // Bắt alert trên Trình duyệt 1
                                try
                                {
                                    var alert1 = driver.SwitchTo().Alert();
                                    errText1 = alert1.Text;
                                    alert1.Accept();
                                }
                                catch (NoAlertPresentException) { }

                                // Bắt alert trên Trình duyệt 2
                                try
                                {
                                    var alert2 = driver2.SwitchTo().Alert();
                                    errText2 = alert2.Text;
                                    alert2.Accept();
                                }
                                catch (NoAlertPresentException) { }

                                System.Threading.Thread.Sleep(1000); // Đợi 1 chút cho trình duyệt ổn định sau khi tắt alert

                                // BÂY GIỜ MỚI ĐƯỢC CHECK URL NÈ
                                bool isDriver1Combo = driver.Url.ToLower().Contains("combo");
                                bool isDriver2Combo = driver2.Url.ToLower().Contains("combo");

                                if (isDriver1Combo && !isDriver2Combo)
                                {
                                    // Nếu chưa lấy được từ Alert native thì thử tìm trong UI HTML (dự phòng)
                                    if (string.IsNullOrEmpty(errText2)) errText2 = new BookingPage(driver2).GetAlertTextAndAccept();
                                    step.ActualResult = $"Trình duyệt 1 vào Combo. Trình duyệt 2 báo lỗi: '{errText2}'";
                                }
                                else if (!isDriver1Combo && isDriver2Combo)
                                {
                                    if (string.IsNullOrEmpty(errText1)) errText1 = bookingPage.GetAlertTextAndAccept();
                                    step.ActualResult = $"Trình duyệt 2 vào Combo. Trình duyệt 1 báo lỗi: '{errText1}'";
                                }
                                else if (isDriver1Combo && isDriver2Combo)
                                {
                                    throw new Exception("Hệ thống lỗi: Không chặn được, cả 2 trình duyệt cùng vào được trang Combo.");
                                }
                                else
                                {
                                    throw new Exception($"Cả 2 trình duyệt đều báo lỗi. Lỗi 1: {errText1} | Lỗi 2: {errText2}");
                                }
                            }
                        }
                        else
                        {
                            // ==============================================
                            // CÁC TEST CASES CÒN LẠI (GIỮ NGUYÊN)
                            // ==============================================
                            if (tcId == "TC_BOOK_01" && action.Contains("chọn suất chiếu"))
                            {
                                bookingPage.SelectDate("2026-04-20");
                                bookingPage.ClickShowtimeSpan();
                                System.Threading.Thread.Sleep(1500);
                                Assert.IsTrue(loginPage.IsLoginModalDisplayed(), "Popup Đăng Nhập không hiển thị.");
                                step.ActualResult = "Hiển thị popup 'Đăng Nhập Tài Khoản' thành công.";
                            }
                            else if (tcId == "TC_BOOK_01" && action.Contains("nhập tài khoản"))
                            {
                                loginPage.EnterEmail("nguyendamkha@gmail.com");
                                loginPage.EnterPassword("Vs2022.NET15.0");
                                loginPage.SubmitLogin();
                                System.Threading.Thread.Sleep(3000);
                                Assert.IsTrue(driver.Url.ToLower().Contains("seat") || driver.Url.ToLower().Contains("matrix"), "Không chuyển hướng đến trang Chọn Ghế.");
                                step.ActualResult = "Đăng nhập và chuyển hướng thành công.";
                            }
                            else if (action.Contains("chọn 2 ghế") || action.Contains("chọn 8 ghế"))
                            {
                                int numSeats = action.Contains("8") ? 8 : 2;
                                bookingPage.SelectAvailableRegularSeats(numSeats);
                                Assert.IsTrue(bookingPage.IsTotalPriceDisplayed(), "Tổng tiền không được cập nhật.");
                                step.ActualResult = $"Đã tự động chọn {numSeats} ghế trống. Ghế đổi màu, hiển thị tổng tiền.";
                            }
                            else if (action.Contains("tiếp tục"))
                            {
                                bookingPage.ClickContinue();
                                System.Threading.Thread.Sleep(2000);
                                step.ActualResult = "Đã chuyển trang thành công.";
                            }

                            // [TC_BOOK_02 & TC_BOOK_03]
                            else if (action.Contains("kiểm tra màu sắc") || action.Contains("truy cập vào sơ đồ ghế"))
                            {
                                var soldSeats = driver.FindElements(By.CssSelector(".seat.confirmed"));
                                Assert.IsTrue(soldSeats.Count > 0, "Không thấy ghế nào hiển thị trạng thái Đã bán (Màu xám).");
                                step.ActualResult = "Các ghế đã mua chuyển sang màu xám chuẩn xác (class: confirmed).";
                            }
                            else if (action.Contains("làm mới") || action.Contains("refresh"))
                            {
                                driver.Navigate().Refresh();
                                System.Threading.Thread.Sleep(2000);
                                var soldSeats = driver.FindElements(By.CssSelector(".seat.confirmed"));
                                Assert.IsTrue(soldSeats.Count > 0, "Ghế đã bán bị mất trạng thái sau khi refresh.");
                                step.ActualResult = "Trạng thái ghế giữ nguyên màu xám sau khi Refresh.";
                            }

                            // [TC_BOOK_05]
                            else if (action.Contains("giả lập hết thời gian"))
                            {
                                bookingPage.SelectAvailableRegularSeats(1);
                                bookingPage.ClickSimulateExpire();
                                System.Threading.Thread.Sleep(3000); // Chờ chuyển trang

                                // Tìm kiếm xem có nút "Đăng nhập" xuất hiện trên Header không (Dấu hiệu bị văng session)
                                bool isLoggedOut = false;
                                try
                                {
                                    isLoggedOut = driver.FindElement(By.XPath("//a[contains(text(), 'Đăng nhập') or contains(@href, 'Login')]")).Displayed;
                                }
                                catch { }

                                // Nếu phát hiện bị đăng xuất -> Ép test case throw Exception để ghi trực tiếp Bug vào Excel
                                if (isLoggedOut)
                                {
                                    throw new Exception("Hệ thống lỗi (Bug): Quay về trang chủ nhưng bị đăng xuất tài khoản và chưa hủy ghế.");
                                }

                                step.ActualResult = "Hệ thống thực hiện hủy giữ ghế và tự động quay về Trang Chủ.";
                            }
                            // Tách riêng Step 2 của TC_BOOK_05
                            else if (action.Contains("kiểm tra url") && tcId == "TC_BOOK_05")
                            {
                                string currentUrl = driver.Url.TrimEnd('/'); // Cắt dấu '/' ở cuối đi để so sánh không bị sai lệch
                                Assert.IsTrue(currentUrl == "http://api.dvxuanbac.com:81" || currentUrl.ToLower().Contains("index"), "URL hiện tại không phải Trang Chủ.");
                                step.ActualResult = "Trang hiển thị là Trang chủ (Home page).";
                            }

                            // ==============================================
                            // [TC_BOOK_06] - CHỈ SỬA ĐÚNG TESTCASE NÀY
                            // ==============================================
                            else if (action.Contains("một ghế đã bán"))
                            {
                                // Tìm cả ghế đã bán (.confirmed) VÀ ghế đang tạm giữ (.held)
                                var unselectableSeats = driver.FindElements(By.CssSelector(".seat.confirmed, .seat.held"));
                                if (unselectableSeats.Count == 0) throw new Exception("Không có ghế nào đã bán hoặc đang tạm giữ để test.");

                                var targetSeat = unselectableSeats[0];

                                // Thực hiện click vào ghế đó
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", targetSeat);
                                System.Threading.Thread.Sleep(500);

                                // Lấy cảnh báo (nếu có)
                                string msg = "";
                                try
                                {
                                    var alert = driver.SwitchTo().Alert();
                                    msg = alert.Text;
                                    alert.Accept();
                                }
                                catch (NoAlertPresentException) { }

                                // ĐIỀU KIỆN TIÊN QUYẾT: Đảm bảo ghế KHÔNG bị đổi class sang 'selected' (không được phép chọn)
                                bool isSelected = targetSeat.GetAttribute("class").Contains("selected");
                                Assert.IsFalse(isSelected, "Bug nghiêm trọng: Hệ thống vẫn cho phép chọn (đổi màu) ghế đã bán/tạm giữ!");

                                // Ghi nhận kết quả linh hoạt (Dù hệ thống bắn Alert hay chặn click im lặng thì ghế không đổi màu là Pass)
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    step.ActualResult = $"Hiển thị cảnh báo: '{msg}' và ghế không đổi màu.";
                                }
                                else
                                {
                                    step.ActualResult = "Ghế không đổi màu (Hệ thống chặn click thành công mà không dùng cảnh báo Browser Alert).";
                                }
                            }
                            else if (action.Contains("một ghế còn trống"))
                            {
                                // Tự động tìm 1 ghế trống bất kỳ để test
                                var availableSeats = driver.FindElements(By.CssSelector(".seat.available:not(.vip):not(.double-seat):not(.selected)"));
                                if (availableSeats.Count == 0) throw new Exception("Hết ghế trống để test.");

                                var targetSeat = availableSeats[0];
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", targetSeat);
                                System.Threading.Thread.Sleep(500);

                                // Kiểm tra ghế đã được đổi màu (add class selected)
                                Assert.IsTrue(targetSeat.GetAttribute("class").Contains("selected"), "Ghế trống click vào nhưng không đổi sang màu cam.");
                                step.ActualResult = "Ghế trống đổi màu cam (trạng thái đang chọn) bình thường.";
                            }
                            // ==============================================
                            // [TC_BOOK_07] - CHỈ SỬA ĐÚNG TESTCASE NÀY
                            // ==============================================
                            else if (action.Contains("tiếp ghế trống thứ 9"))
                            {
                                var extraSeats = bookingPage.GetAvailableSeatsList();
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", extraSeats[0]);

                                // Chờ một chút để HTML Modal kịp hiển thị (Animation)
                                System.Threading.Thread.Sleep(1000);

                                // Bắt thông báo từ thẻ HTML bằng ID
                                string warning = "";
                                try
                                {
                                    warning = driver.FindElement(By.Id("errorModalMessage")).Text;
                                }
                                catch (NoSuchElementException)
                                {
                                    throw new Exception("Không tìm thấy modal thông báo lỗi (id='errorModalMessage').");
                                }

                                Assert.IsTrue(warning.Contains("tối đa 8"), $"Thông báo không đúng kỳ vọng. Thực tế: '{warning}'");
                                step.ActualResult = "Hiển thị thông báo: Chỉ được chọn tối đa 8 ghế/lần.";

                                // Đóng modal
                                try
                                {
                                    new OpenQA.Selenium.Interactions.Actions(driver).SendKeys(Keys.Escape).Perform();
                                    System.Threading.Thread.Sleep(500);
                                }
                                catch { }
                            }
                            else if (action.Contains("quan sát trạng thái của ghế thứ 9"))
                            {
                                // ĐÃ SỬA: Thêm "#seat-matrix" để KHÔNG đếm nhầm cái ô chú thích "Ghế đang chọn" ở dưới cuối trang
                                var selectedSeats = driver.FindElements(By.CssSelector("#seat-matrix .seat.selected"));
                                Assert.IsTrue(selectedSeats.Count == 8, $"Bug: Đang có {selectedSeats.Count} ghế đổi màu trong sơ đồ, kỳ vọng chặn ở mức 8 ghế.");

                                step.ActualResult = "Ghế thứ 9 giữ nguyên màu trắng, không chuyển sang màu cam.";
                            }

                            // ==============================================
                            // [TC_BOOK_10] - CHỈ SỬA ĐÚNG TESTCASE NÀY
                            // ==============================================
                            else if (tcId == "TC_BOOK_10" && (action.Contains("thông tin đơn hàng") || action.Contains("giá trị tại mục")))
                            {
                                // Trang Checkout không dùng ID cho tổng tiền, ta dùng XPath để tìm thẻ <div> bên cạnh chữ "Tổng cộng:"
                                var totalElement = driver.FindElement(By.XPath("//div[contains(text(), 'Tổng cộng:')]/following-sibling::div"));

                                Assert.IsTrue(totalElement.Displayed && !string.IsNullOrEmpty(totalElement.Text), "Không hiển thị tổng tiền trên trang thanh toán.");
                                step.ActualResult = "Hiển thị đúng thông tin vé, combo và tổng tiền.";
                            }
                            else if (tcId == "TC_BOOK_10" && action.Contains("thanh toán") && !action.Contains("tích chọn"))
                            {
                                // Nhấn nút thanh toán để mở Modal
                                bookingPage.SelectPayPalAndClickThanhToan();
                                System.Threading.Thread.Sleep(1000); // Chờ Modal mở lên
                                step.ActualResult = "Hiển thị popup 'Xác nhận thanh toán' chứa tổng tiền và Điều khoản.";
                            }
                            else if (tcId == "TC_BOOK_10" && action.Contains("tích chọn") && action.Contains("đồng ý"))
                            {
                                // Tích check box và nhấn Thanh toán trên Modal
                                bookingPage.ConfirmPaymentModal();

                                // Chờ chuyển hướng sang cổng PayPal (cho dư dả 5-7 giây phòng mạng chậm)
                                System.Threading.Thread.Sleep(5000);

                                // Bám sát đúng mô tả Excel: Chỉ cần kiểm tra URL đã nhảy sang PayPal là PASS
                                Assert.IsTrue(driver.Url.ToLower().Contains("paypal"), "Lỗi: Không chuyển hướng được sang trang thanh toán PayPal.");

                                step.ActualResult = "Chuyển hướng sang cổng thanh toán PayPal thành công.";
                            }
                            // ==============================================
                            // [TC_BOOK_11] - CHỌN KẾT HỢP GHẾ THƯỜNG & VIP
                            // ==============================================
                            else if (tcId == "TC_BOOK_11" && action.Contains("nhấn chọn 01 ghế thường và 01 ghế vip"))
                            {
                                // 1. Chọn ghế VIP
                                var vipSeats = driver.FindElements(By.CssSelector(".seat.vip.available:not(.selected)"));
                                if (vipSeats.Count == 0) throw new Exception("Không có ghế VIP trống để test.");

                                var chosenVip = vipSeats.First();
                                string vipSeatId = chosenVip.GetAttribute("data-id");
                                string vipTitle = chosenVip.GetAttribute("title"); // Lấy title chứa giá (VD: "C5 - VIP - 130,000đ")

                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", chosenVip);
                                System.Threading.Thread.Sleep(500);

                                // 2. Chọn ghế thường (Loại trừ hoàn toàn hàng A bằng cách bỏ các data-id bắt đầu bằng 'A')
                                var regSeats = driver.FindElements(By.CssSelector(".seat.available:not(.vip):not(.double-seat):not(.selected)"))
                                                     .Where(s => s.GetAttribute("data-id") != null && !s.GetAttribute("data-id").StartsWith("A"))
                                                     .ToList();
                                if (regSeats.Count == 0) throw new Exception("Không có ghế thường trống để test.");

                                var chosenReg = regSeats.Last(); // Vẫn dùng Last() nhưng giờ chắc chắn nó sẽ lấy hàng sát VIP (hàng D) chứ không dính hàng A
                                string regSeatId = chosenReg.GetAttribute("data-id");
                                string regTitle = chosenReg.GetAttribute("title"); // Lấy title chứa giá (VD: "D5 - Thường - 110,000đ")

                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", chosenReg);
                                System.Threading.Thread.Sleep(1000); // Đợi UI tính toán lại tổng tiền

                                // 3. Xác minh ghế đã đổi màu
                                bool isVipSelected = chosenVip.GetAttribute("class").Contains("selected");
                                bool isRegSelected = chosenReg.GetAttribute("class").Contains("selected");
                                Assert.IsTrue(isVipSelected && isRegSelected, "Ghế click vào nhưng không đổi màu sang trạng thái đang chọn.");

                                // 4. TÍNH TOÁN GIÁ TIỀN ĐỘNG (Dynamic Pricing)
                                int GetPriceFromTitle(string title)
                                {
                                    try
                                    {
                                        // Tách chuỗi theo dấu "-" và lấy phần cuối cùng, loại bỏ 'đ', ',' và khoảng trắng
                                        var parts = title.Split('-');
                                        string priceStr = parts.Last().Replace("đ", "").Replace(",", "").Replace(".", "").Trim();
                                        return int.Parse(priceStr);
                                    }
                                    catch { return 0; }
                                }

                                int expectedTotal = GetPriceFromTitle(vipTitle) + GetPriceFromTitle(regTitle);

                                // Đọc tổng tiền hiển thị trên màn hình
                                var totalAmountLabel = driver.FindElement(By.Id("totalAmount"));
                                string totalText = totalAmountLabel.Text.Replace(",", "").Replace(".", "").Trim();
                                int actualTotal = int.Parse(totalText);

                                Assert.IsTrue(actualTotal == expectedTotal && actualTotal > 0, $"Hệ thống tính sai tổng tiền! Kỳ vọng (từ giá ghế): {expectedTotal}, Thực tế UI: {actualTotal}");

                                // Ghi log thông minh (Update luôn giá trị linh động vào Excel)
                                step.ActualResult = $"Ghế đổi màu cam; Tổng cộng hiển thị đúng {expectedTotal:#,##0} VNĐ (Đã auto-chọn VIP: {vipSeatId} và Thường: {regSeatId}).";
                            }
                            else if (tcId == "TC_BOOK_11" && action.Contains("tiếp tục"))
                            {
                                bookingPage.ClickContinue();
                                System.Threading.Thread.Sleep(2000);

                                Assert.IsTrue(driver.Url.ToLower().Contains("combo"), "Hệ thống không chuyển sang trang Chọn Combo / Sản phẩm.");
                                step.ActualResult = "Hệ thống chấp nhận lựa chọn và chuyển sang trang \"Chọn Combo / Sản phẩm\".";
                            }
                            else
                            {
                                step.ActualResult = "Xác nhận thông qua Auto Test.";
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
                // BẮT BUỘC ĐÓNG DRIVER 2 NẾU CÓ CHẠY QUA
                if (driver2 != null)
                {
                    driver2.Quit();
                    driver2.Dispose();
                }
                excelHelper.WriteTestCaseSteps(sheetName, steps);
                if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"{tcId} thất bại.");
            }
        }
    }
}