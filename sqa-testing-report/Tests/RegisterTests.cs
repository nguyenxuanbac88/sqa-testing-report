using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class RegisterTests
    {
        private IWebDriver driver;
        private string _excelPath;
        private ExcelTestCaseHelper excelHelper;
        private RegisterPage registerPage;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            registerPage = new RegisterPage(driver);

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
        public void TC_REG_01_KiemTraDangKiHopLe()
        {
            string sheetName = "Bac_automationTC";
            string tcId = "TC_REG_01";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            registerPage.GoToHomePage();
            registerPage.SwitchToVietnamese();

            bool isPreviousStepFailed = false;

            // TẠO DỮ LIỆU ĐỘNG DỰA TRÊN THỜI GIAN THỰC ĐỂ TRÁNH TRÙNG LẶP
            string dynamicEmail = $"testuser{DateTime.Now:yyyyMMddHHmmss}@gmail.com";
            string dynamicPhone = $"09{DateTime.Now:HHmmssff}"; // 09 + 8 số random từ Giờ, Phút, Giây, MiliGiây

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
                    string stepNum = step.StepNumber?.Trim();

                    switch (stepNum)
                    {
                        case "1":
                            registerPage.OpenRegisterModal();
                            System.Threading.Thread.Sleep(500);
                            step.ActualResult = "Đã mở form đăng ký thành công.";
                            break;
                        case "2":
                            registerPage.EnterFullName("Nguyễn Văn A");
                            step.ActualResult = "Đã nhập họ và tên: Nguyễn Văn A";
                            break;
                        case "3":
                            registerPage.EnterEmail(dynamicEmail); // Dùng email động
                            step.ActualResult = $"Đã nhập email: {dynamicEmail}";
                            break;
                        case "4":
                            registerPage.EnterPhone(dynamicPhone); // Dùng SĐT động
                            step.ActualResult = $"Đã nhập SĐT: {dynamicPhone}";
                            break;
                        case "5":
                            registerPage.SelectGender("nam");
                            step.ActualResult = "Đã chọn giới tính: nam";
                            break;
                        case "6":
                            registerPage.SelectDOB("10/01/2005");
                            step.ActualResult = "Đã chọn ngày sinh: 10/01/2005";
                            break;
                        case "7":
                            registerPage.EnterPassword("Kho@09092005");
                            step.ActualResult = "Đã nhập mật khẩu.";
                            break;
                        case "8":
                            registerPage.EnterConfirmPassword("Kho@09092005");
                            step.ActualResult = "Đã nhập lại mật khẩu.";
                            break;
                        case "9":
                            registerPage.SubmitRegistration();
                            System.Threading.Thread.Sleep(1000);
                            Assert.IsTrue(registerPage.IsRegistrationSuccessful(), "Không thấy thông báo đăng kí thành công.");
                            step.ActualResult = "Hiển thị thông báo Đăng kí thành công.";
                            break;
                    }

                    step.Status = "Pass";
                    step.Screenshots = "";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi Exception: " + ex.Message;

                    if (OperatingSystem.IsWindows())
                        step.Screenshots = ScreenshotHelper.Capture(tcId);

                    isPreviousStepFailed = true;
                }
            }

            excelHelper.WriteTestCaseSteps(sheetName, steps);

            if (steps.Any(s => s.Status == "Fail"))
                Assert.Fail($"Test Case {tcId} thất bại. Xem ảnh lỗi tại: Data/Screenshots/{tcId}.png");
        }

        [Test]
        public void TC_REG_02_KiemTraLoiKhiDeTrongHoTen()
        {
            string sheetName = "Bac_automationTC";
            string tcId = "TC_REG_02";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            registerPage.GoToHomePage();
            registerPage.SwitchToVietnamese();

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
                    string stepNum = step.StepNumber?.Trim();

                    // Lấy dữ liệu từ Excel và dọn dẹp khoảng trắng 2 đầu
                    string testData = step.TestData?.Trim() ?? "";

                    switch (stepNum)
                    {
                        case "1":
                            registerPage.OpenRegisterModal();
                            System.Threading.Thread.Sleep(500); // Đợi modal xổ xuống hoàn toàn
                            step.ActualResult = "Đã mở form đăng ký thành công.";
                            break;
                        case "2":
                            // Trong Excel ô này để trống, testData sẽ là "" (chuỗi rỗng)
                            registerPage.EnterFullName(testData);
                            step.ActualResult = "Đã để trống họ và tên.";
                            break;
                        case "3":
                            registerPage.EnterEmail(testData);
                            step.ActualResult = $"Đã nhập email: {testData}";
                            break;
                        case "4":
                            registerPage.EnterPhone(testData);
                            step.ActualResult = $"Đã nhập SĐT: {testData}";
                            break;
                        case "5":
                            registerPage.SelectGender(testData);
                            step.ActualResult = $"Đã chọn giới tính: {testData}";
                            break;
                        case "6":
                            // Safeguard: Lọc bỏ phần giờ phút nếu Excel tự convert Ngày sinh thành DateTime
                            string dob = testData.Contains(" ") ? testData.Split(' ')[0] : testData;
                            registerPage.SelectDOB(dob);
                            step.ActualResult = $"Đã chọn ngày sinh: {dob}";
                            break;
                        case "7":
                            registerPage.EnterPassword(testData);
                            step.ActualResult = "Đã nhập mật khẩu.";
                            break;
                        case "8":
                            registerPage.EnterConfirmPassword(testData);
                            step.ActualResult = "Đã nhập lại mật khẩu.";
                            break;
                        case "9":
                            registerPage.SubmitRegistration();
                            System.Threading.Thread.Sleep(500); // Đợi trình duyệt văng ra cái popup báo lỗi HTML5
                            Assert.IsTrue(registerPage.IsHtml5ValidationTriggered("name"), "Không trigger lỗi validation HTML5 cho trường Tên.");
                            step.ActualResult = "Hệ thống chặn submit và yêu cầu nhập họ tên.";
                            break;
                    }

                    step.Status = "Pass";
                    step.Screenshots = "";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi Exception: " + ex.Message;

                    if (OperatingSystem.IsWindows())
                        step.Screenshots = ScreenshotHelper.Capture(tcId);

                    isPreviousStepFailed = true;
                }
            }

            excelHelper.WriteTestCaseSteps(sheetName, steps);

            if (steps.Any(s => s.Status == "Fail"))
                Assert.Fail($"Test Case {tcId} thất bại. Xem file Excel cột Actual Result để biết gãy ở dòng nào.");
        }

        [Test]
        public void TC_REG_03_KiemTraLoiEmailKhongHopLe()
        {
            string sheetName = "Bac_automationTC";
            string tcId = "TC_REG_03";

            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            registerPage.GoToHomePage();
            registerPage.SwitchToVietnamese();

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
                    string stepNum = step.StepNumber?.Trim();

                    // Lấy dữ liệu từ Excel và dọn dẹp khoảng trắng 2 đầu
                    string testData = step.TestData?.Trim() ?? "";

                    switch (stepNum)
                    {
                        case "1":
                            registerPage.OpenRegisterModal();
                            System.Threading.Thread.Sleep(500); // Đợi modal xổ xuống hoàn toàn
                            step.ActualResult = "Đã mở form đăng ký thành công.";
                            break;
                        case "2":
                            registerPage.EnterFullName(testData);
                            step.ActualResult = $"Đã nhập họ và tên: {testData}";
                            break;
                        case "3":
                            registerPage.EnterEmail(testData); // Sẽ bốc giá trị "abc123" từ Excel
                            step.ActualResult = $"Đã nhập email: {testData}";
                            break;
                        case "4":
                            registerPage.EnterPhone(testData);
                            step.ActualResult = $"Đã nhập SĐT: {testData}";
                            break;
                        case "5":
                            registerPage.SelectGender(testData);
                            step.ActualResult = $"Đã chọn giới tính: {testData}";
                            break;
                        case "6":
                            // Safeguard: Lọc bỏ phần giờ phút nếu Excel tự convert Ngày sinh thành DateTime
                            string dob = testData.Contains(" ") ? testData.Split(' ')[0] : testData;
                            registerPage.SelectDOB(dob);
                            step.ActualResult = $"Đã chọn ngày sinh: {dob}";
                            break;
                        case "7":
                            registerPage.EnterPassword(testData);
                            step.ActualResult = "Đã nhập mật khẩu.";
                            break;
                        case "8":
                            registerPage.EnterConfirmPassword(testData);
                            step.ActualResult = "Đã nhập lại mật khẩu.";
                            break;
                        case "9":
                            registerPage.SubmitRegistration();
                            System.Threading.Thread.Sleep(500); // Đợi trình duyệt văng ra cái popup báo lỗi HTML5

                            // Kiểm tra xem trường Email có bị HTML5 validation chặn lại không (do abc123 thiếu @)
                            Assert.IsTrue(registerPage.IsHtml5ValidationTriggered("email"), "Không trigger lỗi validation HTML5 cho trường Email.");
                            step.ActualResult = "Hệ thống chặn submit và báo lỗi định dạng email không hợp lệ.";
                            break;
                    }

                    step.Status = "Pass";
                    step.Screenshots = "";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi Exception: " + ex.Message;

                    if (OperatingSystem.IsWindows())
                        step.Screenshots = ScreenshotHelper.Capture(tcId);

                    isPreviousStepFailed = true;
                }
            }

            excelHelper.WriteTestCaseSteps(sheetName, steps);

            if (steps.Any(s => s.Status == "Fail"))
                Assert.Fail($"Test Case {tcId} thất bại. Xem file Excel cột Actual Result để biết gãy ở dòng nào.");
        }
    }
}