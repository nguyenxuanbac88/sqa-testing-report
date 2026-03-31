using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class AdminCreateShowTimeTests
    {
        private IWebDriver _driver;
        private AdminLoginPage _loginPage;
        private AdminCreateShowTimePage _showTimePage;
        private ExcelTestCaseHelper _excelHelper;

        private readonly string _sheetName = "Kha_automationTC";

        [SetUp]
        public void Setup()
        {
            _driver = DriverFactory.InitDriver();
            _loginPage = new AdminLoginPage(_driver);
            _showTimePage = new AdminCreateShowTimePage(_driver);

            string start = AppContext.BaseDirectory;
            string repoRoot = PathHelper.GetRepoRoot(start);
            string excelPath = Path.Combine(repoRoot ?? start, "Data", "DataTest.xlsx");
            _excelHelper = new ExcelTestCaseHelper(excelPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (_driver != null) { _driver.Quit(); _driver.Dispose(); }
        }

        [Test]
        public void TC_SHOW_02_KiemTraTaoLichChieu()
        {
            string tcId = "TC_SHOW_02";
            var steps = _excelHelper.ReadTestCaseById(_sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId} trong file Excel");

            try
            {
                // ==============================
                // PRE-CONDITIONS
                // ==============================
                _loginPage.GoToPage();
                _loginPage.Login("khoa992005@gmail.com", "Khoa@123");
                Assert.IsTrue(_loginPage.IsLoginSuccessful(), "Pre-condition thất bại.");

                _showTimePage.NavigateToTimeline();
                Thread.Sleep(1000);

                // --- BIẾN DỮ LIỆU ĐỘNG ---
                DateTime targetDate = new DateTime(2026, 4, 15);
                string cinemaToTest = "Galaxy Bến Tre";
                string movieToTest = "Phim Điện Ảnh Thám Tử Lừng Danh Conan: Dư Ảnh Của Độc Nhãn";
                string startTimeToTest = "08:00 AM";
                string selectedRoom = "";
                int maxRetries = 10; // Thử tối đa 10 ngày kế tiếp nếu phòng full
                bool isRoomFound = false;

                // ==============================
                // EVALUATE STEPS
                // ==============================
                bool isPreviousStepFailed = false;

                foreach (var step in steps)
                {
                    if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                    try
                    {
                        string actualMsg = "";

                        switch (step.StepNumber)
                        {
                            case "1":
                            case "2":
                                // Gộp logic Step 1 & 2 thành Vòng lặp quét phòng động
                                if (!isRoomFound)
                                {
                                    for (int i = 0; i < maxRetries; i++)
                                    {
                                        _showTimePage.SelectCinemaAndDate(cinemaToTest, targetDate);
                                        _showTimePage.ClickLoadTimeline();

                                        var bookedRooms = _showTimePage.GetBookedRooms();

                                        _showTimePage.ClickAddShowtime();
                                        var allRooms = _showTimePage.GetAllRoomsFromModal();

                                        // Tìm phòng trống (có trong allRooms nhưng chưa có trong bookedRooms)
                                        var freeRooms = allRooms.Except(bookedRooms).ToList();

                                        if (freeRooms.Count > 0)
                                        {
                                            selectedRoom = freeRooms.First(); // Lấy phòng trống đầu tiên
                                            isRoomFound = true;
                                            break; // Đã tìm thấy phòng, thoát vòng lặp giữ Modal mở
                                        }
                                        else
                                        {
                                            // Hết phòng, đóng Modal, cộng thêm 1 ngày và thử lại
                                            _showTimePage.CloseModal();
                                            targetDate = targetDate.AddDays(1);
                                        }
                                    }

                                    Assert.IsTrue(isRoomFound, $"Không tìm thấy phòng trống nào trong {maxRetries} ngày liên tiếp tính từ 15/04/2026.");
                                }
                                actualMsg = $"Đã check và tìm được phòng trống: {selectedRoom} vào ngày {targetDate:dd/MM/yyyy}. Modal Thêm lịch chiếu đang mở.";
                                break;

                            case "3":
                                // Nhập thông tin với Phòng Động vừa tìm được
                                _showTimePage.FillShowtimeDetails(
                                    movieName: movieToTest,
                                    roomName: selectedRoom,
                                    startTime: startTimeToTest,
                                    price: "15000",
                                    typeValue: "1" // Suất chiếu thường
                                );
                                _showTimePage.SaveShowtime();
                                actualMsg = $"Đã nhập đủ thông tin cho phòng {selectedRoom}, loại suất chiếu 1. Nhấn Lưu và Modal đóng lại.";
                                break;

                            case "4":
                                // Chọn lại đúng Rạp và Ngày vừa chốt được ở trên
                                _showTimePage.SelectCinemaAndDate(cinemaToTest, targetDate);
                                _showTimePage.ClickLoadTimeline();

                                // Validate kết quả tạo mới (check cả Phòng, Tên phim và Giờ bắt đầu)
                                bool isCreated = _showTimePage.IsShowtimeDisplayedOnTimeline(selectedRoom, movieToTest, startTimeToTest);
                                Assert.IsTrue(isCreated, $"Lỗi: Lịch chiếu vừa tạo KHÔNG hiển thị trên Timeline (Phòng: {selectedRoom}, Ngày: {targetDate:dd/MM/yyyy}).");

                                actualMsg = $"Lịch chiếu vừa tạo hiển thị chính xác trên Timeline của {selectedRoom} lúc {startTimeToTest}.";
                                break;

                            default:
                                actualMsg = "Bỏ qua step không xác định.";
                                break;
                        }

                        step.ActualResult = actualMsg;
                        step.Status = "Pass";
                        step.Screenshots = "";
                    }
                    catch (Exception ex)
                    {
                        step.Status = "Fail";
                        step.ActualResult = "Lỗi: " + ex.Message;
                        if (OperatingSystem.IsWindows())
                        {
                            step.Screenshots = ScreenshotHelper.Capture($"{tcId}_Step{step.StepNumber}");
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