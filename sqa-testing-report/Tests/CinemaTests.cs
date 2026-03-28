using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class CinemaTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private CinemaPage cinemaPage;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            cinemaPage = new CinemaPage(driver); // Khởi tạo POM

            string start = TestContext.CurrentContext.WorkDirectory;
            string repoRoot = PathHelper.GetRepoRoot(start);
            _excelPath = repoRoot != null ? Path.Combine(repoRoot, "Data", "DataTest.xlsx") : Path.Combine(start, "Data", "DataTest.xlsx");

            excelHelper = new ExcelTestCaseHelper(_excelPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }

        [Test]
        public void TC_CINEMA_09_KiemTraSuatChieuTheoPhim()
        {
            string sheetName = "Bac_automationTC"; // NHỚ ĐỔI ĐÚNG TÊN SHEET CỦA BẠN
            string tcId = "TC_CINEMA_09";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            // GỌI HÀM TỪ POM - Rất dễ đọc
            cinemaPage.GoToGetAllCinemas();
            cinemaPage.SwitchToVietnamese();

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Đánh giá Fail do bước trước đó đã thất bại.";
                    step.Screenshots = null;
                    continue;
                }

                try
                {
                    string action = step.StepAction.ToLower();

                    if (step.StepNumber == "1" && action.Contains("chọn rạp"))
                    {
                        cinemaPage.SelectCinema("Galaxy Bến Tre");
                        step.ActualResult = "Đã chuyển hướng đến trang chi tiết rạp Galaxy Bến Tre.";
                    }
                    else if (step.StepNumber == "2" && action.Contains("cuộn màn hình"))
                    {
                        cinemaPage.ScrollToMoviesSection();
                        cinemaPage.SelectDateTab("2026-04-10");
                        step.ActualResult = "Đã cuộn đến danh sách phim và chọn thành công Tab ngày 10/04/2026.";
                    }
                    else if (step.StepNumber == "3" && action.Contains("kiểm tra vị trí"))
                    {
                        cinemaPage.ClickShowtime("2026-04-10", "Khá", "11:00");
                        step.ActualResult = "Các suất chiếu nằm gọn dưới tên phim Khá. Đã nhấn vào suất chiếu thành công.";
                    }

                    step.Status = "Pass";
                    step.Screenshots = "";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;
                    if (OperatingSystem.IsWindows())
                        step.Screenshots = ScreenshotHelper.Capture(tcId);
                    isPreviousStepFailed = true;
                }
            }

            excelHelper.WriteTestCaseSteps(sheetName, steps);

            if (steps.Any(s => s.Status == "Fail"))
                Assert.Fail($"Test Case thất bại. Xem file ảnh lỗi tại: Data/Screenshots/{tcId}.png");
        }

        // ----------------------------------------------------
        // TC_CINEMA_01: Kiểm tra hiển thị danh sách rạp theo khu vực
        // ----------------------------------------------------
        [Test]
        public void TC_CINEMA_01_KiemTraHienThiDanhSachRap()
        {
            string sheetName = "Bac_automationTC"; // NHỚ ĐỔI ĐÚNG TÊN SHEET NHÉ
            string tcId = "TC_CINEMA_01";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Đánh giá Fail do bước trước đó thất bại.";
                    step.Screenshots = null;
                    continue;
                }

                try
                {
                    string action = step.StepAction.ToLower();
                    string testData = step.TestData;

                    // STEP 1: Mở trang "Tất cả các rạp"
                    if (step.StepNumber == "1" && action.Contains("mở trang"))
                    {
                        cinemaPage.GoToGetAllCinemas();
                        cinemaPage.SwitchToVietnamese();

                        cinemaPage.VerifyCinemasListDisplayed();

                        step.ActualResult = "Hệ thống đã hiển thị đầy đủ danh sách khu vực.";
                    }

                    // STEP 2: Click vào khu vực [Bến Tre]
                    else if (step.StepNumber == "2" && action.Contains("click vào khu vực"))
                    {
                        cinemaPage.ClickRegion(testData); // Truyền chữ "Bến Tre" vào

                        Assert.IsTrue(cinemaPage.IsCinemaInList("Galaxy Bến Tre"), $"Không tìm thấy rạp Galaxy Bến Tre trong khu vực {testData}");

                        step.ActualResult = $"Đã hiển thị danh sách rạp thuộc khu vực [{testData}].";
                    }

                    step.Status = "Pass";
                    step.Screenshots = ""; // Xóa link ảnh cũ nếu Pass
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

            if (steps.Any(s => s.Status == "Fail"))
                Assert.Fail($"Test Case thất bại. Xem file ảnh lỗi tại: Data/Screenshots/{tcId}.png");
        }

        // ----------------------------------------------------
        // TC_CINEMA_05: Kiểm tra hiển thị chi tiết rạp
        // ----------------------------------------------------
        [Test]
        public void TC_CINEMA_05_KiemTraChiTietRap()
        {
            string sheetName = "Bac_automationTC"; // NHỚ ĐỔI ĐÚNG TÊN SHEET NHÉ
            string tcId = "TC_CINEMA_05";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            // Pre-condition: Người dùng truy cập trang "Tất cả các rạp"
            cinemaPage.GoToGetAllCinemas();
            cinemaPage.SwitchToVietnamese();

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Đánh giá Fail do bước trước đó thất bại.";
                    step.Screenshots = null;
                    continue;
                }

                try
                {
                    string action = step.StepAction.ToLower();
                    string testData = step.TestData;

                    // STEP 1: Click chọn rạp từ danh sách
                    if (step.StepNumber == "1" && action.Contains("click chọn rạp"))
                    {
                        cinemaPage.SelectCinema("Galaxy Bến Tre"); // Bạn có thể thay bằng testData nếu muốn linh động
                        step.ActualResult = $"Đã chuyển hướng đến trang chi tiết của rạp [Galaxy Bến Tre].";
                    }

                    // STEP 2: Kiểm tra các trường thông tin rạp
                    else if (step.StepNumber == "2" && action.Contains("kiểm tra các trường"))
                    {
                        Assert.IsTrue(cinemaPage.CheckCinemaInfoDisplay(), "Thiếu trường thông tin rạp (Địa chỉ / Thành phố / Hotline).");
                        step.ActualResult = "Hiển thị đầy đủ thông tin: Địa chỉ, Thành phố, Hotline.";
                    }

                    // STEP 3: Kiểm tra sự tồn tại của danh sách phim
                    else if (step.StepNumber == "3" && action.Contains("kiểm tra sự tồn tại"))
                    {
                        Assert.IsTrue(cinemaPage.CheckMoviesSectionDisplay(), "Không tìm thấy mục 'Movies' bên dưới thông tin rạp.");
                        step.ActualResult = "Hiển thị mục 'Movies' thành công.";
                    }

                    step.Status = "Pass";
                    step.Screenshots = ""; // Xóa link ảnh cũ nếu Pass
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

            if (steps.Any(s => s.Status == "Fail"))
                Assert.Fail($"Test Case thất bại. Xem file ảnh lỗi tại: Data/Screenshots/{tcId}.png");
        }

        // ----------------------------------------------------
        // TC_CINEMA_03: Kiểm tra truy cập rạp không tồn tại qua URL
        // ----------------------------------------------------
        [Test]
        public void TC_CINEMA_03_TruyCapRapKhongTonTai()
        {
            string sheetName = "Bac_automationTC";
            string tcId = "TC_CINEMA_03";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Đánh giá Fail do bước trước đó thất bại.";
                    step.Screenshots = null;
                    continue;
                }

                try
                {
                    string action = step.StepAction.ToLower();
                    string testData = step.TestData?.Trim() ?? "";

                    if (step.StepNumber == "1" && action.Contains("nhập đường dẫn"))
                    {
                        // Lấy nguyên link từ Excel (vd: http://api.dvxuanbac.com:81/Cinema/GetCinema/666)
                        cinemaPage.NavigateToUrl(testData);

                        string bodyText = cinemaPage.GetBodyText();

                        // Kiểm tra xem có văng thông báo "Cinema not found" như hình bạn gửi không
                        bool isError = bodyText.Contains("Cinema not found") || bodyText.Contains("404") || bodyText.ToLower().Contains("không tồn tại");

                        Assert.IsTrue(isError, "Hệ thống không hiển thị thông báo lỗi khi truy cập URL rạp không tồn tại.");
                        step.ActualResult = "Hệ thống đã hiển thị lỗi: " + bodyText.Trim();
                    }

                    step.Status = "Pass";
                    step.Screenshots = ""; // Xóa link ảnh cũ nếu Pass
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;
                    if (OperatingSystem.IsWindows()) step.Screenshots = ScreenshotHelper.Capture(tcId);
                    isPreviousStepFailed = true;
                }
            }

            // ĐÃ SỬA LỖI Ở ĐÂY: Truyền steps vào thay vì tcId
            excelHelper.WriteTestCaseSteps(sheetName, steps);

            if (steps.Any(s => s.Status == "Fail"))
                Assert.Fail($"Test Case thất bại. Xem file ảnh lỗi tại: Data/Screenshots/{tcId}.png");
        }

        // ----------------------------------------------------
        // TC_CINEMA_06: Kiểm tra hiển thị lịch chiếu theo từng ngày cụ thể
        // ----------------------------------------------------
        [Test]
        public void TC_CINEMA_06_KiemTraLichChieuTheoNgay()
        {
            string sheetName = "Bac_automationTC";
            string tcId = "TC_CINEMA_06";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            // Pre-condition: Mở trang Tất cả các rạp
            cinemaPage.GoToGetAllCinemas();
            cinemaPage.SwitchToVietnamese();

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                try
                {
                    string action = step.StepAction.ToLower();
                    string testData = step.TestData?.Trim() ?? "";

                    if (step.StepNumber == "1" && action.Contains("click chọn rạp"))
                    {
                        cinemaPage.SelectCinema(testData); // Click "Galaxy Bến Tre"
                        step.ActualResult = $"Đã chuyển hướng đến trang chi tiết rạp [{testData}].";
                    }
                    else if (step.StepNumber == "2" && action.Contains("click chọn một ngày"))
                    {
                        cinemaPage.ScrollToMoviesSection();

                        // Click vào Tab có chữ giống trong Excel (VD: "28-Thg3")
                        cinemaPage.SelectDateTabByText(testData);

                        // Kiểm tra xem phim có hiện ra không
                        Assert.IsTrue(cinemaPage.CheckMoviesSectionDisplay(), "Không hiển thị danh sách phim/suất chiếu cho ngày đã chọn.");
                        step.ActualResult = $"Đã hiển thị danh sách phim và suất chiếu của ngày [{testData}].";
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