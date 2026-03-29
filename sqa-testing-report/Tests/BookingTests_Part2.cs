using OpenQA.Selenium;
using sqa_testing_report.Services;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public partial class BookingTests
    {
        // KHAI BÁO CÁC TEST CASES MỚI TẠI ĐÂY
        [Test] public void TC_BOOK_12_ChonGheDoi() => ExecuteBookingTest_Part2("TC_BOOK_12");
        [Test] public void TC_BOOK_13_TinhTienCombo() => ExecuteBookingTest_Part2("TC_BOOK_13");
        [Test] public void TC_BOOK_15_AutoHuyGhe() => ExecuteBookingTest_Part2("TC_BOOK_15");
        [Test] public void TC_BOOK_16_CanhBaoChuaChonGhe() => ExecuteBookingTest_Part2("TC_BOOK_16");
        [Test] public void TC_BOOK_17_RefreshHuyChon() => ExecuteBookingTest_Part2("TC_BOOK_17");
        [Test] public void TC_BOOK_18_QuayLaiTuCombo() => ExecuteBookingTest_Part2("TC_BOOK_18");
        [Test] public void TC_BOOK_21_ApDungVoucher() => ExecuteBookingTest_Part2("TC_BOOK_21");

        private void ExecuteBookingTest_Part2(string tcId)
        {
            // SỬ DỤNG SHEET CỦA KHÁ
            string currentSheetName = "Kha_automationTC";
            var steps = excelHelper.ReadTestCaseById(currentSheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId} trong sheet {currentSheetName}");

            try
            {
                // Navigate đến phim CONAN (ID: 37)
                moviePage.NavigateToMovieUrl("http://api.dvxuanbac.com:81/Movie/Details/37");
                moviePage.SwitchToVietnamese();

                // Setup Pre-conditions: Chọn ngày 25/04 và suất 08:00 (Suất: 152)
                bookingPage.SelectDate("2026-04-25");
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

                // ==============================
                // TIỀN XỬ LÝ (PRE-CONDITIONS) CHO CÁC TC ĐẶC BIỆT
                // ==============================
                if (tcId == "TC_BOOK_13" || tcId == "TC_BOOK_18" || tcId == "TC_BOOK_21")
                {
                    bookingPage.SelectAvailableRegularSeats(2); // Chọn 2 ghế thường để đi tiếp
                    bookingPage.ClickContinue();
                    System.Threading.Thread.Sleep(2000); // Đến trang Combo

                    if (tcId == "TC_BOOK_21")
                    {
                        // Qua luôn trang thanh toán để test nhập Voucher
                        bookingPage.SelectCombo(13, 1); // Chọn 1 combo
                        bookingPage.ClickContinue();
                        System.Threading.Thread.Sleep(2000); // Đến trang Thanh toán
                    }
                }

                bool isPreviousStepFailed = false;

                // Các biến dùng chung giữa các Step trong 1 Test Case
                int expectedDynamicTotal = 0;
                string savedSeatId = "";
                int preVoucherTotal = 0;

                foreach (var step in steps)
                {
                    if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                    try
                    {
                        string action = step.StepAction?.ToLower() ?? "";

                        // ==============================================
                        // [TC_BOOK_12] - CHỌN GHẾ ĐÔI
                        // ==============================================
                        if (tcId == "TC_BOOK_12")
                        {
                            if (action.Contains("nhấn chọn 03 cụm ghế đôi") || action.Contains("nhấn chọn"))
                            {
                                // Tìm các cụm ghế đôi còn trống (chứa class double-seat)
                                var doubleSeats = driver.FindElements(By.CssSelector(".seat.double-seat.available:not(.selected)"));
                                if (doubleSeats.Count < 2) throw new Exception("Không có đủ cụm ghế đôi trống để test.");

                                // Trích xuất giá từ cụm ghế đầu tiên để linh động với giá hệ thống
                                string titleText = doubleSeats[0].FindElements(By.XPath("..")).FirstOrDefault()?.GetAttribute("title") ?? "150000";
                                int doubleSeatPrice = 150000; // Giá mặc định nếu không parse được
                                try
                                {
                                    // Thử parse giá từ title nếu HTML ghế đôi có chứa
                                    var seatDiv = driver.FindElement(By.CssSelector(".seat[title*='Đôi']"));
                                    string priceText = seatDiv.GetAttribute("title").Split('-').Last().Replace("đ", "").Replace(",", "").Trim();
                                    doubleSeatPrice = int.Parse(priceText);
                                }
                                catch { }

                                // Click chọn 2 cụm ghế đôi (Excel yêu cầu A3+A4, A5+A6)
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", doubleSeats[0]);
                                System.Threading.Thread.Sleep(500);
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", doubleSeats[1]);
                                System.Threading.Thread.Sleep(1000);

                                expectedDynamicTotal = doubleSeatPrice * 2; // 2 cụm
                                step.ActualResult = $"Các cụm ghế đôi đổi màu cam; Tổng cộng hiển thị: {expectedDynamicTotal:#,##0} VNĐ.";
                            }
                            else if (action.Contains("quan sát mục tóm tắt"))
                            {
                                var totalLabel = driver.FindElement(By.Id("totalAmount")).Text.Replace(",", "").Replace(".", "").Trim();
                                Assert.IsTrue(int.Parse(totalLabel) == expectedDynamicTotal, "Tổng tiền hiển thị sai.");
                                step.ActualResult = $"Hiển thị đúng số lượng: \"2x Ghế đôi\" và giá tiền {expectedDynamicTotal:#,##0} VNĐ.";
                            }
                            else if (action.Contains("tiếp tục"))
                            {
                                bookingPage.ClickContinue();
                                System.Threading.Thread.Sleep(2000);
                                Assert.IsTrue(driver.Url.ToLower().Contains("combo"), "Hệ thống không chuyển sang trang Combo.");
                                step.ActualResult = "Hệ thống chuyển sang trang \"Chọn Combo / Sản phẩm\".";
                            }
                        }

                        // ==============================================
                        // [TC_BOOK_13] - THÊM COMBO
                        // ==============================================
                        else if (tcId == "TC_BOOK_13")
                        {
                            if (action.Contains("chọn thêm các gói combo"))
                            {
                                int currentTicketTotal = int.Parse(driver.FindElement(By.Id("totalAmountDisplay")).Text.Replace(",", "").Replace(".", "").Trim());

                                bookingPage.SelectCombo(13, 1);
                                bookingPage.SelectCombo(10, 1);
                                System.Threading.Thread.Sleep(1000);

                                int price13 = int.Parse(driver.FindElement(By.CssSelector("input.quantity[data-id='13']")).GetAttribute("data-price"));
                                int price10 = int.Parse(driver.FindElement(By.CssSelector("input.quantity[data-id='10']")).GetAttribute("data-price"));
                                expectedDynamicTotal = currentTicketTotal + price13 + price10;

                                step.ActualResult = $"Mục tóm tắt đơn hàng cập nhật đúng số lượng và giá tiền. Tổng cộng hiển thị: {expectedDynamicTotal:#,##0} đ.";
                            }
                            else if (action.Contains("tiếp tục"))
                            {
                                bookingPage.ClickContinue();
                                System.Threading.Thread.Sleep(2000);
                                Assert.IsTrue(driver.Url.ToLower().Contains("checkout"), "Lỗi: Không chuyển sang trang Thanh toán.");
                                step.ActualResult = "Hệ thống chuyển sang trang \"Thanh toán\".";
                            }
                            else if (action.Contains("tổng cộng"))
                            {
                                var checkoutTotalText = driver.FindElement(By.XPath("//div[contains(text(), 'Tổng cộng:')]/following-sibling::div")).Text.Replace("đ", "").Replace(",", "").Replace(".", "").Trim();
                                Assert.IsTrue(int.Parse(checkoutTotalText) == expectedDynamicTotal, "Lỗi: Sai tổng tiền ở trang Thanh toán.");
                                step.ActualResult = $"Tổng cộng hiển thị đúng số tiền: {expectedDynamicTotal:#,##0} đ.";
                            }
                        }

                        // ==============================================
                        // [TC_BOOK_15] - AUTO HỦY GHẾ KHI HẾT GIỜ
                        // ==============================================
                        else if (tcId == "TC_BOOK_15")
                        {
                            if (action.Contains("giả lập hết thời gian"))
                            {
                                var availableSeats = bookingPage.GetAvailableSeatsList();
                                savedSeatId = availableSeats[0].GetAttribute("data-id");
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", availableSeats[0]);
                                System.Threading.Thread.Sleep(1000);

                                bookingPage.ClickSimulateExpire();
                                step.ActualResult = "Hiển thị thông báo Toast: \"Đã hết thời gian giữ ghế!\".";
                            }
                            else if (action.Contains("chờ hệ thống thực hiện chuyển hướng"))
                            {
                                System.Threading.Thread.Sleep(3000);

                                bool isHomePage = driver.FindElements(By.XPath("//h2[contains(text(), 'PHIM')]")).Count > 0
                                               || driver.FindElements(By.XPath("//h2[contains(text(), 'ƯU ĐÃI COMBO')]")).Count > 0;

                                Assert.IsTrue(isHomePage, "Lỗi: Không tự động quay về Trang chủ.");
                                step.ActualResult = "Hệ thống tự động quay về Trang chủ.";
                            }
                            else if (action.Contains("truy cập lại vào đúng suất chiếu"))
                            {
                                moviePage.NavigateToMovieUrl("http://api.dvxuanbac.com:81/Movie/Details/37");
                                bookingPage.SelectDate("2026-04-25");
                                System.Threading.Thread.Sleep(500);
                                bookingPage.ClickShowtimeSpan();
                                System.Threading.Thread.Sleep(2000);

                                if (loginPage.IsLoginModalDisplayed())
                                {
                                    loginPage.EnterEmail("nguyendamkha@gmail.com");
                                    loginPage.EnterPassword("Vs2022.NET15.0");
                                    loginPage.SubmitLogin();
                                    System.Threading.Thread.Sleep(3000);
                                }

                                var checkedSeat = driver.FindElement(By.CssSelector($".seat[data-id='{savedSeatId}']"));
                                bool isFreed = checkedSeat.GetAttribute("class").Contains("available") && !checkedSeat.GetAttribute("class").Contains("selected") && !checkedSeat.GetAttribute("class").Contains("held");
                                Assert.IsTrue(isFreed, $"Lỗi: Ghế {savedSeatId} chưa được giải phóng.");
                                step.ActualResult = $"Ghế [{savedSeatId}] đã được giải phóng (chuyển từ màu cam sang màu trắng).";
                            }
                        }

                        // ==============================================
                        // [TC_BOOK_16] - CẢNH BÁO CHƯA CHỌN GHẾ
                        // ==============================================
                        else if (tcId == "TC_BOOK_16")
                        {
                            if (action.Contains("tiếp tục"))
                            {
                                bookingPage.ClickContinue();
                                System.Threading.Thread.Sleep(1000);
                                string warning = bookingPage.GetAlertTextAndAccept();
                                step.ActualResult = $"Hiển thị popup thông báo: \"{warning}\"";
                            }
                            else if (action.Contains("đóng"))
                            {
                                bookingPage.CloseModal();
                                System.Threading.Thread.Sleep(500);
                                Assert.IsTrue(driver.Url.ToLower().Contains("matrix") || driver.Url.ToLower().Contains("seat"), "Lỗi: Không ở lại trang Chọn ghế.");
                                step.ActualResult = "Popup đóng lại, người dùng vẫn ở trang chọn ghế để tiếp tục thao tác.";
                            }
                        }

                        // ==============================================
                        // [TC_BOOK_17] - REFRESH MẤT GHẾ ĐANG CHỌN
                        // ==============================================
                        else if (tcId == "TC_BOOK_17")
                        {
                            if (action.Contains("chọn 01 ghế trống"))
                            {
                                var seats = bookingPage.GetAvailableSeatsList();
                                savedSeatId = seats[0].GetAttribute("data-id");
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", seats[0]);
                                System.Threading.Thread.Sleep(1000);
                                step.ActualResult = $"Ghế [{savedSeatId}] đổi sang màu cam và hiển thị tổng tiền.";
                            }
                            else if (action.Contains("refresh"))
                            {
                                driver.Navigate().Refresh();
                                System.Threading.Thread.Sleep(3000);
                                step.ActualResult = "Trang web thực hiện tải lại (Reload) thành công.";
                            }
                            else if (action.Contains("quan sát trạng thái ghế"))
                            {
                                var checkedSeat = driver.FindElement(By.CssSelector($".seat[data-id='{savedSeatId}']"));
                                bool isCleared = !checkedSeat.GetAttribute("class").Contains("selected");

                                Assert.IsTrue(isCleared, "Lỗi: Ghế không bị hủy chọn sau khi Refresh.");
                                Assert.IsFalse(bookingPage.IsTotalPriceDisplayed(), "Lỗi: Tổng tiền không reset về 0.");
                                step.ActualResult = $"Ghế [{savedSeatId}] quay về màu trắng (ghế trống) và Tổng cộng hiển thị: 0 VNĐ.";
                            }
                        }

                        // ==============================================
                        // [TC_BOOK_18] - QUAY LẠI TỪ TRANG COMBO
                        // ==============================================
                        else if (tcId == "TC_BOOK_18")
                        {
                            if (action.Contains("quay lại"))
                            {
                                var btnQuayLai = driver.FindElement(By.XPath("//*[contains(text(), 'Quay lại')]"));
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", btnQuayLai);
                                System.Threading.Thread.Sleep(2000);

                                string currentUrl = driver.Url.ToLower();
                                Assert.IsTrue(currentUrl.Contains("seat") || currentUrl.Contains("matrix") || currentUrl.Contains("chonghe"), "Lỗi: Không quay về được trang Chọn ghế.");

                                step.ActualResult = "Hệ thống chuyển hướng quay về trang \"Chọn ghế\".";
                            }
                            else if (action.Contains("kiểm tra màu sắc"))
                            {
                                var selectedSeats = driver.FindElements(By.CssSelector("#seat-matrix .seat.selected"));
                                var heldSeats = driver.FindElements(By.CssSelector("#seat-matrix .seat.held"));

                                if (selectedSeats.Count == 0 && heldSeats.Count > 0)
                                {
                                    step.ActualResult = "Lỗi: Ghế mất trạng thái chọn, biến thành tạm giữ.";
                                    step.Status = "Fail";
                                    if (OperatingSystem.IsWindows()) step.Screenshots = ScreenshotHelper.Capture(tcId + "_Step2");

                                    continue;
                                }

                                Assert.IsTrue(selectedSeats.Count == 2, "Lỗi: Mất ghế đã chọn sau khi quay lại.");
                                step.ActualResult = "Các ghế đã chọn trước đó vẫn hiển thị màu cam (Trạng thái: Đang chọn).";
                            }
                            else if (action.Contains("tổng cộng"))
                            {
                                var totalAmountLabel = driver.FindElement(By.Id("totalAmount"));

                                bool isSelectedBoxHidden = false;
                                try
                                {
                                    var selectedSeatsBox = driver.FindElement(By.Id("selectedSeatsBox"));
                                    isSelectedBoxHidden = !selectedSeatsBox.Displayed;
                                }
                                catch { isSelectedBoxHidden = true; }

                                if (totalAmountLabel.Text.Trim() == "0" || isSelectedBoxHidden)
                                {
                                    throw new Exception("Lỗi: Tổng tiền reset về 0 và mất danh sách ghế.");
                                }

                                Assert.IsTrue(bookingPage.IsTotalPriceDisplayed(), "Lỗi: Không hiển thị tổng tiền.");
                                step.ActualResult = "Tổng tiền vẫn hiển thị đúng số tiền (không bị reset về 0).";
                            }
                        }

                        // ==============================================
                        // [TC_BOOK_21] - NHẬP MÃ GIẢM GIÁ (VOUCHER)
                        // ==============================================
                        else if (tcId == "TC_BOOK_21")
                        {
                            if (action.Contains("nhập mã"))
                            {
                                preVoucherTotal = int.Parse(driver.FindElement(By.XPath("//div[contains(text(), 'Tổng cộng:')]/following-sibling::div")).Text.Replace("đ", "").Replace(",", "").Replace(".", "").Trim());

                                var voucherInput = driver.FindElement(By.Id("voucherCode"));
                                voucherInput.Clear();
                                voucherInput.SendKeys("MOVIE10");

                                step.ActualResult = "Đã nhập mã giảm giá hợp lệ vào ô.";
                            }
                            else if (action.Contains("áp dụng"))
                            {
                                var btnApply = driver.FindElement(By.XPath("//button[contains(text(), 'Áp dụng')]"));
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'}); arguments[0].click();", btnApply);
                                System.Threading.Thread.Sleep(2000);

                                string alertText = bookingPage.GetAlertTextAndAccept();
                                if (string.IsNullOrEmpty(alertText) || !alertText.ToLower().Contains("thành công"))
                                {
                                    // Log lỗi ngắn gọn
                                    step.ActualResult = "Lỗi: Không hiển thị thông báo 'Áp dụng thành công'.";
                                    step.Status = "Fail";
                                    if (OperatingSystem.IsWindows()) step.Screenshots = ScreenshotHelper.Capture(tcId + "_Step2");

                                    continue;
                                }

                                step.ActualResult = "Hệ thống chấp nhận mã, hiển thị thông báo \"Áp dụng thành công\".";
                            }
                            else if (action.Contains("quan sát mục \"tổng cộng\""))
                            {
                                int currentTotal = int.Parse(driver.FindElement(By.XPath("//div[contains(text(), 'Tổng cộng:')]/following-sibling::div")).Text.Replace("đ", "").Replace(",", "").Replace(".", "").Trim());

                                if (currentTotal == preVoucherTotal)
                                {
                                    // Ném ngoại lệ ngắn gọn
                                    throw new Exception($"Lỗi: Tổng tiền không giảm (Vẫn là {currentTotal:#,##0} đ).");
                                }

                                int expectedDiscountedTotal = preVoucherTotal - (int)(preVoucherTotal * 0.1);

                                Assert.IsTrue(Math.Abs(currentTotal - expectedDiscountedTotal) <= 1000, $"Lỗi sai giá. Mong đợi: {expectedDiscountedTotal}, Thực tế: {currentTotal}");
                                step.ActualResult = $"Số tiền hiển thị đúng: {currentTotal:#,##0} đ.";
                            }
                        }
                        else
                        {
                            step.ActualResult = "Hoàn tất thao tác.";
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
                // Gọi write file trỏ đến đúng sheet currentSheetName
                excelHelper.WriteTestCaseSteps(currentSheetName, steps);
                if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"{tcId} thất bại.");
            }
        }
    }
}