using sqa_testing_report.Pages;
using sqa_testing_report.Services;

namespace sqa_testing_report.Tests
{
    // Cùng khai báo partial class với Part 1
    public partial class AdminCreateShowTimeTests
    {
        [Test] public void TC_SHOWTIME_01_BoTrongNgayChieu() => ExecuteValidationTest("TC_SHOWTIME_01");
        [Test] public void TC_SHOWTIME_02_BoTrongThoiGianBatDau() => ExecuteValidationTest("TC_SHOWTIME_02");
        [Test] public void TC_SHOWTIME_03_BoTrongChiPhi() => ExecuteValidationTest("TC_SHOWTIME_03");
        [Test] public void TC_SHOWTIME_04_BoTrongLoaiSuatChieu() => ExecuteValidationTest("TC_SHOWTIME_04");
        [Test] public void TC_SHOWTIME_05_BoTrongPhim() => ExecuteValidationTest("TC_SHOWTIME_05");
        [Test] public void TC_SHOWTIME_06_ChonLoaiKhuyaVaoBanNgay() => ExecuteValidationTest("TC_SHOWTIME_06");

        // Hàm Runner linh hoạt cho các case check Validation
        private void ExecuteValidationTest(string tcId)
        {
            var steps = _excelHelper.ReadTestCaseById(_sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId} trong file Excel");

            // --- SỬ DỤNG TRANG VALIDATION MỚI TẠO ---
            var validationPage = new AdminCreateShowTimeValidationPage(_driver);

            try
            {
                // ==============================
                // PRE-CONDITIONS
                // ==============================
                _loginPage.GoToPage();
                _loginPage.Login("khoa992005@gmail.com", "Khoa@123");
                Assert.IsTrue(_loginPage.IsLoginSuccessful(), "Pre-condition thất bại: Không thể đăng nhập.");

                // --- DATA CHUẨN ĐỂ ĐIỀN FORM ---
                string cinema = "Galaxy Bến Tre";
                string room = "Phòng 1";
                DateTime targetDate = new DateTime(2026, 5, 15);
                string dateToFill = "";
                string movie = "Khá";
                string time = "08:00 AM";
                string price = "10000";
                string type = "1";

                int maxRetries = 10;
                bool isDateFound = false;
                bool isPreviousStepFailed = false;
                bool isScreenshotCaptured = false;

                foreach (var step in steps)
                {
                    if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                    try
                    {
                        string actualMsg = "";
                        string action = step.StepAction?.ToLower() ?? "";

                        // Mapping các hành động đọc từ file Excel
                        if (action.Contains("chọn menu"))
                        {
                            validationPage.NavigateToTimeline();
                            actualMsg = "Đã mở trang Quản lý suất chiếu.";
                        }
                        else if (action.Contains("thêm lịch chiếu"))
                        {
                            // ---- VÒNG LẶP CHECK PHÒNG TRƯỚC KHI THÊM ----
                            if (!isDateFound)
                            {
                                for (int i = 0; i < maxRetries; i++)
                                {
                                    validationPage.SelectCinemaAndDate(cinema, targetDate);
                                    validationPage.ClickLoadTimeline();

                                    var bookedRooms = validationPage.GetBookedRooms();

                                    if (!bookedRooms.Contains(room))
                                    {
                                        isDateFound = true;
                                        break;
                                    }
                                    else
                                    {
                                        targetDate = targetDate.AddDays(1);
                                    }
                                }
                                Assert.IsTrue(isDateFound, $"Lỗi: Không tìm thấy ngày nào trống cho {room} trong vòng {maxRetries} ngày (từ 13/05/2026).");

                                // Cập nhật dateToFill: Bắt buộc truyền ngày đầy đủ, trừ TC_01 cố ý gửi chuỗi rỗng
                                dateToFill = tcId == "TC_SHOWTIME_01" ? "" : targetDate.ToString("yyyy-MM-dd");
                            }

                            validationPage.ClickAddShowtime();
                            actualMsg = $"Đã check Tải lịch chiếu, chốt ngày {targetDate:dd/MM/yyyy} cho {room}. Form Thêm lịch chiếu hiển thị.";
                        }
                        else if (action.Contains("chọn rạp") || action.Contains("phòng"))
                        {
                            actualMsg = $"Đã chọn rạp {cinema} và phòng {room}.";
                        }
                        else if (action.Contains("không chọn ngày chiếu") || action.Contains("chọn ngày chiếu"))
                        {
                            actualMsg = string.IsNullOrEmpty(dateToFill) ? "Đã xóa trắng/bỏ trống ô Ngày chiếu (hiển thị mặc định mm/dd/yyyy)." : $"Đã nhập Ngày chiếu {targetDate:dd/MM/yyyy}.";
                        }
                        else if (action.Contains("phim"))
                        {
                            if (tcId == "TC_SHOWTIME_05") movie = "";
                            actualMsg = string.IsNullOrEmpty(movie) ? "Đã bỏ trống ô Phim." : "Đã chọn Phim.";
                        }
                        else if (action.Contains("thời gian"))
                        {
                            if (tcId == "TC_SHOWTIME_02") time = "";
                            actualMsg = string.IsNullOrEmpty(time) ? "Đã bỏ trống Thời gian bắt đầu." : "Đã nhập Thời gian.";
                        }
                        else if (action.Contains("chi phí"))
                        {
                            if (tcId == "TC_SHOWTIME_03") price = "";
                            actualMsg = string.IsNullOrEmpty(price) ? "Đã bỏ trống Chi phí." : "Đã nhập Chi phí.";
                        }
                        else if (action.Contains("loại suất chiếu"))
                        {
                            if (tcId == "TC_SHOWTIME_04") type = "";
                            if (tcId == "TC_SHOWTIME_06") { type = "2"; time = "08:00 AM"; }
                            actualMsg = string.IsNullOrEmpty(type) ? "Đã bỏ trống Loại suất chiếu." : $"Đã nhập Loại {type}.";
                        }
                        else if (action.Contains("nhấn nút lưu") || action.Contains("nhấn lưu"))
                        {
                            // Thực hiện điền form
                            validationPage.FillShowtimeFormDynamic(movie, room, dateToFill, time, price, type);
                            validationPage.ClickSaveOnly();

                            // ---- KIỂM TRA EXPECTED RESULT ----
                            if (step.ExpectedResult.Contains("Vui lòng điền"))
                            {
                                string fieldToCheck = tcId switch
                                {
                                    "TC_SHOWTIME_01" => "Date",
                                    "TC_SHOWTIME_02" => "Time",
                                    "TC_SHOWTIME_03" => "Price",
                                    "TC_SHOWTIME_04" => "Type",
                                    "TC_SHOWTIME_05" => "Movie",
                                    _ => ""
                                };

                                string validationMsg = validationPage.GetValidationMessage(fieldToCheck);
                                Assert.IsTrue(!string.IsNullOrEmpty(validationMsg), $"Lỗi: Không hiển thị popup validation HTML5 tại ô {fieldToCheck}.");
                                actualMsg = $"Hệ thống báo lỗi popup mặc định: '{validationMsg}' tại ô trống và chặn không cho Lưu.";
                            }
                            else if (step.ExpectedResult.Contains("chặn") || step.ExpectedResult.Contains("không tạo"))
                            {
                                // ---- BỔ SUNG LOGIC CHECK TIMELINE CHO TC_06 THEO YÊU CẦU ----

                                // Nếu Modal vẫn còn mở (bị chặn tại UI), ta cần đóng nó lại để tương tác với màn hình ngoài
                                if (validationPage.IsModalOpen())
                                {
                                    validationPage.CloseModal();
                                }

                                // Load lại Timeline để nghiệm thu thực tế
                                validationPage.SelectCinemaAndDate(cinema, targetDate);
                                validationPage.ClickLoadTimeline();

                                // Kiểm tra xem suất chiếu có bị lọt qua khe cửa hẹp tạo thành công không
                                bool isCreated = validationPage.IsShowtimeDisplayedOnTimeline(room, movie, time);

                                Assert.IsFalse(isCreated, "Lỗi Nghiêm Trọng: Hệ thống báo chặn nhưng suất chiếu KHÔNG HỢP LỆ VẪN ĐƯỢC TẠO trên Timeline!");

                                actualMsg = "Hệ thống chặn thành công. Đã kiểm tra lại Timeline và xác nhận suất chiếu không được tạo.";
                            }
                        }

                        if (string.IsNullOrEmpty(actualMsg)) actualMsg = "Đã thực hiện step.";

                        step.ActualResult = actualMsg;
                        step.Status = "Pass";
                        step.Screenshots = "";
                    }
                    catch (Exception ex)
                    {
                        step.Status = "Fail";
                        step.ActualResult = "Lỗi: " + ex.Message.Split('\n')[0];
                        if (!isScreenshotCaptured && OperatingSystem.IsWindows())
                        {
                            step.Screenshots = ScreenshotHelper.Capture(tcId);
                            isScreenshotCaptured = true;
                        }
                        isPreviousStepFailed = true;
                    }
                }
            }
            finally
            {
                _excelHelper.WriteTestCaseSteps(_sheetName, steps);
                if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"{tcId} thất bại.");
            }
        }
    }
}