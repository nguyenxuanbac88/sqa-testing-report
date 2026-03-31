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
                string movieToTest = "Khá";
                string startTimeToTest = "08:00 AM";
                string selectedRoom = "";
                int maxRetries = 10;
                bool isRoomFound = false;

                // ==============================
                // EVALUATE STEPS
                // ==============================
                bool isPreviousStepFailed = false;
                bool isScreenshotCaptured = false; // Cờ kiểm soát chỉ chụp 1 ảnh

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
                                if (!isRoomFound)
                                {
                                    for (int i = 0; i < maxRetries; i++)
                                    {
                                        _showTimePage.SelectCinemaAndDate(cinemaToTest, targetDate);
                                        _showTimePage.ClickLoadTimeline();

                                        var bookedRooms = _showTimePage.GetBookedRooms();

                                        _showTimePage.ClickAddShowtime();
                                        var allRooms = _showTimePage.GetAllRoomsFromModal();
                                        var freeRooms = allRooms.Except(bookedRooms).ToList();

                                        if (freeRooms.Count > 0)
                                        {
                                            selectedRoom = freeRooms.First();
                                            isRoomFound = true;
                                            break;
                                        }
                                        else
                                        {
                                            _showTimePage.CloseModal();
                                            targetDate = targetDate.AddDays(1);
                                        }
                                    }

                                    Assert.IsTrue(isRoomFound, $"Hết phòng trống trong {maxRetries} ngày.");
                                }
                                actualMsg = $"Đã mở Modal, chọn ngày {targetDate:dd/MM/yyyy} và phòng {selectedRoom}.";
                                break;

                            case "3":
                                _showTimePage.FillShowtimeDetails(
                                    movieName: movieToTest,
                                    roomName: selectedRoom,
                                    startTime: startTimeToTest,
                                    price: "15000",
                                    typeValue: "1"
                                );
                                _showTimePage.SaveShowtime();
                                actualMsg = "Đã nhập thông tin và lưu thành công.";
                                break;

                            case "4":
                                _showTimePage.SelectCinemaAndDate(cinemaToTest, targetDate);
                                _showTimePage.ClickLoadTimeline();

                                bool isCreated = _showTimePage.IsShowtimeDisplayedOnTimeline(selectedRoom, movieToTest, startTimeToTest);
                                Assert.IsTrue(isCreated, "Lịch chiếu không hiển thị trên Timeline.");

                                actualMsg = "Lịch chiếu hiển thị đúng trên Timeline.";
                                break;

                            default:
                                actualMsg = "Bỏ qua step.";
                                break;
                        }

                        step.ActualResult = actualMsg;
                        step.Status = "Pass";
                        step.Screenshots = "";
                    }
                    catch (Exception ex)
                    {
                        step.Status = "Fail";
                        // Lấy dòng lỗi đầu tiên cho ngắn gọn
                        step.ActualResult = "Lỗi: " + ex.Message.Split('\n')[0];

                        if (!isScreenshotCaptured && OperatingSystem.IsWindows())
                        {
                            // Chụp duy nhất 1 ảnh lưu bằng tên TestCaseID
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