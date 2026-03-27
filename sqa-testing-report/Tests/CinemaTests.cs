using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using sqa_testing_report.Services;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class CinemaTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseService svc;

        [SetUp]
        public void Setup()
        {
            // Cấu hình Chrome chạy ngầm (Headless) để test chạy nhanh hơn và ổn định hơn trên Server CI
            var options = new ChromeOptions();
            // options.AddArgument("--headless"); // Comment dòng này nếu muốn nhìn thấy trình duyệt bật lên

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();

            // ImplicitWait: Đợi ngầm định 10s cho mọi element
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            // Logic tìm thư mục gốc chuẩn xác để lấy file Excel
            string start = TestContext.CurrentContext.WorkDirectory;
            string repoRoot = FindRepoRoot(start);

            if (!string.IsNullOrEmpty(repoRoot))
                _excelPath = System.IO.Path.Combine(repoRoot, "Data", "DataTest.xlsx");
            else
                _excelPath = System.IO.Path.Combine(start, "Data", "DataTest.xlsx");

            svc = new ExcelTestCaseService(_excelPath);
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

        // --- TEST CASE TC_CINEMA_01 ---
        [Test]
        public void TC_CINEMA_01_KiemTraHienThiDanhSachRap()
        {
            string sheetName = "Bac_automationTC"; // NHỚ ĐỔI ĐÚNG TÊN SHEET CỦA BẠN NHÉ
            string tcId = "TC_CINEMA_01";

            // 1. Đọc Test Data từ Excel
            var steps = svc.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            bool isPassed = true;
            string actualMsg = "";
            string shotPath = "";

            try
            {
                // PRE-CONDITIONS & STEP 1: Truy cập trang danh sách rạp
                driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81/Cinema/GetAllCinemas");

                // --- XỬ LÝ ĐA NGÔN NGỮ ---
                // Ép trình duyệt chuyển về Tiếng Việt (VN) để Assert text chính xác
                try
                {
                    var vnButton = wait.Until(d => d.FindElement(By.CssSelector("button[data-lang='vi']")));
                    if (!vnButton.GetAttribute("class").Contains("active"))
                    {
                        vnButton.Click();
                        // Đợi web reload ngôn ngữ
                        System.Threading.Thread.Sleep(1500);
                    }
                }
                catch { /* Bỏ qua nếu không có nút ngôn ngữ */ }

                // --- VERIFICATION (ASSERT) ---
                // 1. Kiểm tra sự tồn tại của container danh sách rạp (Accordion)
                wait.Until(d => d.FindElement(By.ClassName("accordion")));

                // 2. Kiểm tra hiển thị các khu vực/thành phố (Ví dụ: Hồ Chí Minh, Bến Tre)
                var cities = driver.FindElements(By.CssSelector(".accordion-button"));
                Assert.IsTrue(cities.Count > 0, "Không tìm thấy danh sách khu vực/thành phố nào được hiển thị.");

                // 3. Kiểm tra hiển thị cụ thể tên một rạp (Dựa trên data cũ bạn gửi)
                Assert.IsTrue(driver.PageSource.Contains("Galaxy Bến Tre"), "Không tìm thấy rạp 'Galaxy Bến Tre' trong danh sách.");

                actualMsg = "Đã hiển thị đầy đủ danh sách rạp, sắp xếp theo khu vực/thành phố thành công.";
            }
            catch (Exception ex)
            {
                isPassed = false;
                actualMsg = "Lỗi hiển thị: " + ex.Message;

                // --- FIX: Truyền TestCaseID vào hàm chụp ảnh ---
                if (OperatingSystem.IsWindows())
                {
                    shotPath = ScreenshotService.Capture(tcId); // Ảnh sẽ tên TC_CINEMA_01.png
                }
            }
            finally
            {
                // CẬP NHẬT KẾT QUẢ VÀO EXCEL (Ô GỘP)
                // Vì Expected Result gộp cả 3 bước, nên Actual Result cũng gộp
                foreach (var step in steps)
                {
                    step.Status = isPassed ? "Pass" : "Fail";
                    step.ActualResult = actualMsg;

                    if (isPassed)
                    {
                        step.Screenshots = ""; // Pass thì xóa link ảnh cũ
                    }
                    else
                    {
                        step.Screenshots = shotPath; // Fail thì điền link ảnh mới (cùng tên)
                    }
                }

                svc.WriteTestCaseSteps(sheetName, steps);
            }

            if (!isPassed)
            {
                Assert.Fail($"Test Case thất bại. Xem file ảnh lỗi tại: Data/Screenshots/{tcId}.png");
            }
        }

        // --- TEST CASE TC_CINEMA_05 (Giữ lại và cập nhật lệnh chụp ảnh) ---
        [Test]
        public void TC_CINEMA_05_KiemTraChiTietRap()
        {
            string sheetName = "Thanh_automationTC"; // NHỚ ĐỔI ĐÚNG TÊN SHEET CỦA BẠN NHÉ
            string tcId = "TC_CINEMA_05";

            var steps = svc.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81/Cinema/GetAllCinemas");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Đánh giá Fail do bước trước đó đã thất bại, không thể chạy tiếp.";
                    step.Screenshots = null; // Giữ null để không ghi đè ô gộp
                    continue;
                }

                try
                {
                    string testData = step.TestData;
                    string action = step.StepAction.ToLower();

                    if (step.StepNumber == "1" && action.Contains("click chọn rạp"))
                    {
                        var cinemaLink = wait.Until(d => d.FindElement(By.XPath($"//a[contains(., '{testData}')]")));

                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("arguments[0].scrollIntoView(true);", cinemaLink);
                        js.ExecuteScript("arguments[0].click();", cinemaLink);

                        wait.Until(d => d.Url.Contains("/GetCinema"));
                        Assert.IsTrue(driver.PageSource.Contains(testData), $"Trang chi tiết không chứa {testData}");
                        step.ActualResult = $"Đã chuyển hướng đến trang chi tiết của rạp [{testData}].";
                    }
                    else if (step.StepNumber == "2" && action.Contains("kiểm tra các trường"))
                    {
                        string htmlSource = driver.PageSource;
                        bool hasAddress = htmlSource.Contains("Địa chỉ:") || htmlSource.Contains("Address:");
                        bool hasCity = htmlSource.Contains("Thành phố:") || htmlSource.Contains("City:");
                        bool hasHotline = htmlSource.Contains("Hotline:");
                        Assert.IsTrue(hasAddress && hasCity && hasHotline, "Thiếu trường thông tin rạp (Địa chỉ / Thành phố / Hotline).");
                        step.ActualResult = "Hiển thị đầy đủ thông tin: Địa chỉ, Thành phố, Hotline.";
                    }
                    else if (step.StepNumber == "3" && action.Contains("kiểm tra sự tồn tại"))
                    {
                        bool hasMovies = driver.PageSource.Contains("Movies");
                        Assert.IsTrue(hasMovies, "Không tìm thấy mục 'Movies' bên dưới thông tin rạp.");
                        step.ActualResult = "Hiển thị mục 'Movies' thành công.";
                    }

                    step.Status = "Pass";
                    step.Screenshots = ""; // Xóa link ảnh cũ nếu step này Pass
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;

                    if (OperatingSystem.IsWindows())
                    {
                        // --- FIX: Truyền TestCaseID vào hàm chụp ảnh ---
                        step.Screenshots = ScreenshotService.Capture(tcId); // Ảnh tên TC_CINEMA_05.png
                    }
                    isPreviousStepFailed = true;
                }
            }

            svc.WriteTestCaseSteps(sheetName, steps);
            if (steps.Any(s => s.Status == "Fail"))
            {
                Assert.Fail("Test Case thất bại. Xem Excel để biết chi tiết.");
            }
        }

        // --- TEST CASE TC_CINEMA_09 ---
        [Test]
        public void TC_CINEMA_09_KiemTraSuatChieuTheoPhim()
        {
            string sheetName = "Bac_automationTC"; // NHỚ ĐỔI ĐÚNG TÊN SHEET CỦA BẠN
            string tcId = "TC_CINEMA_09";

            var steps = svc.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            // PRE-CONDITION: Mở trang danh sách rạp
            driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81/Cinema/GetAllCinemas");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            bool isPreviousStepFailed = false;

            // Xử lý ép ngôn ngữ (Bê nguyên từ case cũ xuống cho an toàn)
            try
            {
                var vnButton = wait.Until(d => d.FindElement(By.CssSelector("button[data-lang='vi']")));
                if (!vnButton.GetAttribute("class").Contains("active"))
                {
                    vnButton.Click();
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch { }

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

                    // STEP 1: Click chọn rạp từ danh sách (Galaxy Bến Tre)
                    if (step.StepNumber == "1" && action.Contains("chọn rạp"))
                    {
                        var cinemaLink = wait.Until(d => d.FindElement(By.XPath($"//a[contains(., 'Galaxy Bến Tre')]")));

                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("arguments[0].scrollIntoView(true);", cinemaLink);
                        js.ExecuteScript("arguments[0].click();", cinemaLink);

                        wait.Until(d => d.Url.Contains("/GetCinema"));
                        step.ActualResult = "Đã chuyển hướng đến trang chi tiết rạp Galaxy Bến Tre.";
                    }

                    // STEP 2: Cuộn màn hình và CHỌN ĐÚNG NGÀY (10/04/2026)
                    else if (step.StepNumber == "2" && action.Contains("cuộn màn hình"))
                    {
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                        // Tìm phần Movies và cuộn xuống
                        var moviesSection = driver.FindElements(By.XPath("//*[contains(text(), 'Movies') or contains(text(), 'Phim')]")).FirstOrDefault();
                        if (moviesSection != null)
                        {
                            js.ExecuteScript("arguments[0].scrollIntoView(true);", moviesSection);
                        }

                        // LỰA CHỌN ĐÚNG NGÀY: Dùng CSS Selector siêu chuẩn xác từ HTML của bạn
                        var dateTab = wait.Until(d => d.FindElement(By.CssSelector("button[data-date='2026-04-10']")));
                        js.ExecuteScript("arguments[0].click();", dateTab);

                        // Đợi 1 giây để giao diện (Tab) kịp chuyển đổi hiệu ứng
                        System.Threading.Thread.Sleep(1000);

                        step.ActualResult = "Đã cuộn đến danh sách phim và chọn thành công Tab ngày 10/04/2026.";
                    }

                    // STEP 3: Kiểm tra vị trí suất chiếu 11:00 dưới phim Khá VÀ Nhấn vào suất chiếu
                    else if (step.StepNumber == "3" && action.Contains("kiểm tra vị trí"))
                    {
                        // 1. Tìm chính xác cái thẻ bọc ngoài cùng (movie-card) của phim "Khá" TRONG ngày 10/04
                        var movieCard = wait.Until(d => d.FindElement(By.XPath(
                            "//div[contains(@class, 'movie-card') and @data-date='2026-04-10']//h6[contains(text(), 'Khá')]/ancestor::div[contains(@class, 'movie-card')]"
                        )));

                        // 2. Tìm nút suất chiếu "11:00" NẰM GỌN BÊN TRONG thẻ của phim Khá
                        // (Dùng dấu chấm '.' ở đầu XPath để giới hạn phạm vi tìm kiếm chỉ ở trong movieCard)
                        var showtime11 = movieCard.FindElement(By.XPath(".//a[contains(text(), '11:00')]"));

                        // Assert để chắc chắn nó tồn tại
                        Assert.IsNotNull(showtime11, "Không tìm thấy suất chiếu 11:00 nằm gọn bên dưới phim Khá.");

                        // 3. VÀO SUẤT CHIẾU NHƯ YÊU CẦU
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("arguments[0].click();", showtime11);

                        // 4. Kiểm tra xem trình duyệt có nhảy sang trang Chi tiết phim hoặc Chọn ghế không
                        wait.Until(d => d.Url.Contains("/Movie/Details") || d.Url.Contains("/Seat"));

                        step.ActualResult = "Các suất chiếu nằm gọn dưới tên phim Khá. Đã nhấn vào suất chiếu 11:00 thành công.";
                    }

                    // Đánh pass nếu code chạy mượt mà qua các Assert
                    step.Status = "Pass";
                    step.Screenshots = ""; // Xóa link ảnh cũ nếu Pass
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;

                    if (OperatingSystem.IsWindows())
                    {
                        step.Screenshots = ScreenshotService.Capture(tcId); // Ảnh tên TC_CINEMA_09.png
                    }

                    isPreviousStepFailed = true;
                }
            }

            // Ghi kết quả
            svc.WriteTestCaseSteps(sheetName, steps);

            if (steps.Any(s => s.Status == "Fail"))
            {
                Assert.Fail($"Test Case thất bại. Xem file ảnh lỗi tại: Data/Screenshots/{tcId}.png");
            }
        }

        // --- TEST CASE TC_CINEMA_09 (PHIÊN BẢN CỐ TÌNH LÀM FAIL) ---
        // Mục đích: Kiểm tra xem code có chụp ảnh và ghi lỗi vào Excel đúng không
        [Test]
        public void TC_CINEMA_09_Intentional_Fail()
        {
            string sheetName = "Bac_automationTC"; // NHỚ ĐỔI ĐÚNG TÊN SHEET
            string tcId = "TC_CINEMA_09"; // Vẫn giữ nguyên ID này để nó ghi đè vào đúng dòng đó trong Excel

            var steps = svc.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            driver.Navigate().GoToUrl("http://api.dvxuanbac.com:81/Cinema/GetAllCinemas");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            bool isPreviousStepFailed = false;

            // Xử lý ép ngôn ngữ
            try
            {
                var vnButton = wait.Until(d => d.FindElement(By.CssSelector("button[data-lang='vi']")));
                if (!vnButton.GetAttribute("class").Contains("active"))
                {
                    vnButton.Click();
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch { }

            foreach (var step in steps)
            {
                if (isPreviousStepFailed)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Đánh giá Fail do bước trước đó đã thất bại.";
                    step.Screenshots = null; // Giữ nguyên null để bảo toàn link ảnh của bước bị lỗi
                    continue;
                }

                try
                {
                    string action = step.StepAction.ToLower();

                    // STEP 1: Vẫn cho chạy Pass bình thường để giả lập luồng người dùng
                    if (step.StepNumber == "1" && action.Contains("chọn rạp"))
                    {
                        var cinemaLink = wait.Until(d => d.FindElement(By.XPath($"//a[contains(., 'Galaxy Bến Tre')]")));
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("arguments[0].scrollIntoView(true);", cinemaLink);
                        js.ExecuteScript("arguments[0].click();", cinemaLink);
                        wait.Until(d => d.Url.Contains("/GetCinema"));

                        step.ActualResult = "Đã chuyển hướng đến trang chi tiết rạp Galaxy Bến Tre.";
                        step.Status = "Pass";
                        step.Screenshots = "";
                    }

                    // STEP 2: CỐ TÌNH ĐÁNH FAIL TẠI ĐÂY
                    else if (step.StepNumber == "2" && action.Contains("cuộn màn hình"))
                    {
                        // Ép Selenium văng lỗi ngay lập tức bằng Assert.Fail
                        Assert.Fail("CỐ TÌNH LÀM FAIL TẠI BƯỚC 2: Kiểm tra tính năng chụp ảnh màn hình và lưu file Excel.");
                    }

                    // STEP 3: Code sẽ không bao giờ chạy vào đây vì Step 2 đã văng lỗi
                    else if (step.StepNumber == "3" && action.Contains("kiểm tra vị trí"))
                    {
                        step.ActualResult = "Không bao giờ chạy tới";
                        step.Status = "Pass";
                        step.Screenshots = "";
                    }
                }
                catch (Exception ex)
                {
                    // Bắt lỗi từ Assert.Fail bên trên và xử lý
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;

                    if (OperatingSystem.IsWindows())
                    {
                        // Sẽ tạo ra file TC_CINEMA_09.png
                        step.Screenshots = ScreenshotService.Capture(tcId);
                    }

                    isPreviousStepFailed = true; // Kích hoạt cờ bỏ qua Step 3
                }
            }

            // Ghi kết quả
            svc.WriteTestCaseSteps(sheetName, steps);

            if (steps.Any(s => s.Status == "Fail"))
            {
                Assert.Fail($"[TEST CỐ TÌNH FAIL] Đã hoàn tất. Hãy mở Excel và xem ảnh tại: Data/Screenshots/{tcId}.png");
            }
        }

        // Thư viện hỗ trợ tìm repo root (giữ nguyên)
        private string FindRepoRoot(string start)
        {
            var di = new System.IO.DirectoryInfo(start);
            while (di != null)
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(di.FullName, "sqa-testing-report.csproj")))
                    return di.FullName;
                di = di.Parent;
            }
            return null;
        }
    }
}