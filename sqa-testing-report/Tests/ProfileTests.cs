using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class ProfileTests
    {
        private IWebDriver driver;
        private ExcelTestCaseHelper excelHelper;
        private LoginPage loginPage;
        private ProfilePage profilePage;
        private readonly string sheetName = "Bac_automationTC";

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.InitDriver();
            loginPage = new LoginPage(driver);
            profilePage = new ProfilePage(driver);

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
        public void TC_PROFILE_08_KiemTraLoiMatKhauCuKhongChinhXac()
        {
            string tcId = "TC_PROFILE_08";
            var steps = excelHelper.ReadTestCaseById(sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            // TIỀN ĐỀ: Đăng nhập thành công để vào trang cá nhân
            loginPage.GoToHomePage();
            loginPage.SwitchToVietnamese();
            loginPage.OpenLoginModal();
            loginPage.EnterEmail("nguyendamkha@gmail.com");
            loginPage.EnterPassword("Vs2022.NET15.0");
            loginPage.SubmitLogin();
            System.Threading.Thread.Sleep(2000);

            profilePage.GoToProfilePage();

            bool isPreviousStepFailed = false;

            foreach (var step in steps)
            {
                if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                try
                {
                    string action = step.StepAction?.ToLower() ?? "";
                    string data = step.TestData?.Trim() ?? "";

                    if (action.Contains("thay đổi"))
                    {
                        profilePage.OpenChangePasswordModal();
                        step.ActualResult = "Đã mở modal đổi mật khẩu.";
                    }
                    else if (action.Contains("nhập lại mật khẩu mới"))
                    {
                        profilePage.EnterConfirmNewPassword(data);
                        step.ActualResult = "Đã nhập lại mật khẩu mới.";
                    }
                    else if (action.Contains("mật khẩu mới"))
                    {
                        profilePage.EnterNewPassword(data);
                        step.ActualResult = "Đã nhập mật khẩu mới.";
                    }
                    else if (action.Contains("mật khẩu cũ"))
                    {
                        profilePage.EnterOldPassword(data);
                        step.ActualResult = $"Đã nhập mật khẩu cũ: {data}";
                    }
                    else if (action.Contains("xác nhận"))
                    {
                        profilePage.SubmitChangePassword();
                        // Chờ trang xử lý POST và load lại thông tin
                        System.Threading.Thread.Sleep(2000);

                        // CẬP NHẬT THEO PHÁT HIỆN MỚI: Phải mở lại form mới thấy Alert lỗi
                        profilePage.OpenChangePasswordModal();

                        string error = profilePage.GetErrorMessage();

                        // Kiểm tra xem có đúng lỗi "Mật khẩu cũ không chính xác" không
                        Assert.IsNotNull(error, "Hệ thống KHÔNG hiển thị thông báo lỗi ngay cả khi đã mở lại modal.");
                        Assert.IsTrue(error.Contains("không chính xác"), $"Lỗi thực tế nhận được: {error}");

                        step.ActualResult = "Hệ thống báo lỗi chính xác sau khi mở lại modal: " + error;
                    }

                    step.Status = "Pass";
                    // TUÂN THỦ QUY TẮC: Pass thì xóa link ảnh để file Excel sạch sẽ
                    step.Screenshots = "";
                }
                catch (Exception ex)
                {
                    step.Status = "Fail";
                    step.ActualResult = "Lỗi: " + ex.Message;
                    // TUÂN THỦ QUY TẮC: Fail thì chụp ảnh làm bằng chứng
                    if (OperatingSystem.IsWindows()) step.Screenshots = ScreenshotHelper.Capture(tcId);
                    isPreviousStepFailed = true;
                }
            }

            excelHelper.WriteTestCaseSteps(sheetName, steps);
            if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"{tcId} thất bại.");
        }
    }
}