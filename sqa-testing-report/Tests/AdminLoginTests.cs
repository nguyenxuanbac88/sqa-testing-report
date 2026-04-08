using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class AdminLoginTests
    {
        private IWebDriver _driver;
        private AdminLoginPage _loginPage;
        private ExcelTestCaseHelper _excelHelper;

        // Cập nhật tên Sheet trong file DataTest.xlsx của bạn
        private readonly string _sheetName = "Kha_automationTC";

        [SetUp]
        public void Setup()
        {
            _driver = DriverFactory.InitDriver();
            _loginPage = new AdminLoginPage(_driver);

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

        [Test] public void TC_ADLOGIN_01_DangNhapThanhCong() => ExecuteAdminLoginTest("TC_ADLOGIN_01");
        [Test] public void TC_ADLOGIN_02_EmailSaiDinhDang() => ExecuteAdminLoginTest("TC_ADLOGIN_02");
        [Test] public void TC_ADLOGIN_05_BoTrongEmail() => ExecuteAdminLoginTest("TC_ADLOGIN_05");
        [Test] public void TC_ADLOGIN_07_EmailChuaDangKy() => ExecuteAdminLoginTest("TC_ADLOGIN_07");
        [Test] public void TC_ADLOGIN_08_SaiMatKhau() => ExecuteAdminLoginTest("TC_ADLOGIN_08");
        [Test] public void TC_ADLOGIN_09_BoTrongMatKhau() => ExecuteAdminLoginTest("TC_ADLOGIN_09");
        [Test] public void TC_ADLOGIN_10_KhoaTaiKhoanSau5LanSai() => ExecuteAdminLoginTest("TC_ADLOGIN_10");

        private void ExecuteAdminLoginTest(string tcId)
        {
            var steps = _excelHelper.ReadTestCaseById(_sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId} trong file Excel");

            try
            {
                // ==============================
                // PRE-CONDITIONS & ACTIONS
                // ==============================
                _loginPage.GoToPage();
                Thread.Sleep(1000);

                if (tcId == "TC_ADLOGIN_01")
                {
                    _loginPage.Login("khoa992005@gmail.com", "Khoa@123");
                }
                else if (tcId == "TC_ADLOGIN_02")
                {
                    _loginPage.Login("khoa123", "Khoa@123"); // Sai định dạng email
                }
                else if (tcId == "TC_ADLOGIN_05")
                {
                    _loginPage.Login("", "Khoa@123"); // Bỏ trống email
                }
                else if (tcId == "TC_ADLOGIN_07")
                {
                    _loginPage.Login("emailchuadangky@gmail.com", "Khoa@123"); // Email chưa tồn tại
                }
                else if (tcId == "TC_ADLOGIN_08")
                {
                    _loginPage.Login("khoa992005@gmail.com", "MatKhauSai123"); // Đúng email, sai pass
                }
                else if (tcId == "TC_ADLOGIN_09")
                {
                    _loginPage.Login("khoa992005@gmail.com", ""); // Bỏ trống mật khẩu
                }
                else if (tcId == "TC_ADLOGIN_10")
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        _loginPage.Login("khoa992005@gmail.com", "MatKhauSai" + i);
                        Thread.Sleep(1000);
                    }
                    _loginPage.Login("khoa992005@gmail.com", "Khoa@123");
                }

                // ==============================
                // EVALUATE STEPS (ASSERTIONS)
                // ==============================
                bool isPreviousStepFailed = false;

                foreach (var step in steps)
                {
                    if (isPreviousStepFailed) { step.Status = "Fail"; continue; }

                    try
                    {
                        string action = step.StepAction?.ToLower() ?? "";
                        string actualMsg = "Đã thực hiện thao tác nhập liệu.";

                        if (action.Contains("kiểm tra") || action.Contains("nhấn nút") || action.Contains("đăng nhập") || step == steps.Last())
                        {
                            bool isSuccess = _loginPage.IsLoginSuccessful();

                            if (tcId == "TC_ADLOGIN_01")
                            {
                                Assert.IsTrue(isSuccess, "Lỗi: Không thể đăng nhập với thông tin hợp lệ.");
                                actualMsg = "Đăng nhập thành công, chuyển hướng vào trang admin.";
                            }
                            else if (tcId == "TC_ADLOGIN_10")
                            {
                                Assert.IsFalse(isSuccess, "Lỗi: Hệ thống KHÔNG khóa tài khoản sau 5 lần nhập sai.");
                                actualMsg = "Đăng nhập bị chặn. Hệ thống ĐÃ khóa tài khoản đúng như mong đợi.";
                            }
                            else
                            {
                                // SỬA Ở ĐÂY: Nếu login thất bại, lấy y hệt message lỗi trên UI ghi vào Actual
                                Assert.IsFalse(isSuccess, "Lỗi: Hệ thống cho phép đăng nhập dù thông tin không hợp lệ.");
                                actualMsg = _loginPage.GetErrorMessage();
                            }
                        }
                        else if (action.Contains("sai 5 lần") || action.Contains("liên tục"))
                        {
                            actualMsg = "Đã thử đăng nhập sai 5 lần liên tiếp.";
                        }

                        step.ActualResult = actualMsg;
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
                _excelHelper.WriteTestCaseSteps(_sheetName, steps);
                if (steps.Any(s => s.Status == "Fail")) Assert.Fail($"{tcId} thất bại.");
            }
        }
    }
}