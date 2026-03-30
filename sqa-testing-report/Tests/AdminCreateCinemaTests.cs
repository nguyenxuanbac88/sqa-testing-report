using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class AdminCreateCinemaTests
    {
        private IWebDriver _driver;
        private AdminLoginPage _loginPage;
        private AdminCinemaPage _cinemaPage;
        private ExcelTestCaseHelper _excelHelper;
        private readonly string _sheetName = "Kha_automationTC";

        [SetUp]
        public void Setup()
        {
            _driver = DriverFactory.InitDriver();
            _loginPage = new AdminLoginPage(_driver);
            _cinemaPage = new AdminCinemaPage(_driver);

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

        // ==========================================
        // KHAI BÁO CÁC TEST CASES PHẦN CREATE
        // ==========================================
        [Test] public void TC_ADCREATECNM_01_TaoRapThanhCong() => ExecuteCreateCinemaTest("TC_ADCREATECNM_01");
        [Test] public void TC_ADCREATECNM_02_TaoRapTrungTen() => ExecuteCreateCinemaTest("TC_ADCREATECNM_02");
        [Test] public void TC_ADCREATECNM_03_BoTrongTenRap() => ExecuteCreateCinemaTest("TC_ADCREATECNM_03");
        [Test] public void TC_ADCREATECNM_04_BoTrongDiaChi() => ExecuteCreateCinemaTest("TC_ADCREATECNM_04");
        [Test] public void TC_ADCREATECNM_05_BoTrongThanhPho() => ExecuteCreateCinemaTest("TC_ADCREATECNM_05");

        // ==========================================
        // HÀM XỬ LÝ CHUNG
        // ==========================================
        private void ExecuteCreateCinemaTest(string tcId)
        {
            var steps = _excelHelper.ReadTestCaseById(_sheetName, tcId);
            if (steps.Count == 0) Assert.Ignore($"Không tìm thấy {tcId}");

            try
            {
                // ==============================
                // PRE-CONDITIONS & ACTIONS
                // ==============================
                _loginPage.GoToPage();
                _loginPage.Login("khoa992005@gmail.com", "Khoa@123");
                Thread.Sleep(2000); // Đợi login xong

                // Click Menu Danh sách rạp phim
                _cinemaPage.GoToListPage();

                // Chuẩn bị dữ liệu từ Excel
                string rawName = steps.FirstOrDefault(s => s.StepAction.Contains("tên") || s.StepAction.Contains("Tên"))?.TestData ?? "TestCinema";
                string rawAddress = steps.FirstOrDefault(s => s.StepAction.Contains("địa chỉ") || s.StepAction.Contains("Địa chỉ"))?.TestData ?? "123 Test Street";
                string rawCity = steps.FirstOrDefault(s => s.StepAction.Contains("thành phố") || s.StepAction.Contains("Thành phố"))?.TestData ?? "HCM";

                string submitName = rawName;
                string submitAddress = rawAddress;
                string submitCity = rawCity;

                if (tcId == "TC_ADCREATECNM_01")
                {
                    submitName = rawName + "_" + DateTime.Now.Ticks;
                    if (_cinemaPage.IsCinemaExist(submitName)) throw new Exception("Lỗi tiền điều kiện: Tên rạp vừa sinh ra đã tồn tại.");
                }
                else if (tcId == "TC_ADCREATECNM_02")
                {
                    submitName = _cinemaPage.GetFirstCinemaName();
                }
                else if (tcId == "TC_ADCREATECNM_03") submitName = "";
                else if (tcId == "TC_ADCREATECNM_04") submitAddress = "";
                else if (tcId == "TC_ADCREATECNM_05") submitCity = "";

                // Mở form và điền dữ liệu
                _cinemaPage.ClickCreateNew();
                _cinemaPage.EnterCinemaInfo(submitName, submitAddress, submitCity);
                _cinemaPage.ClickSubmit();

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
                        string actualMsg = "Đã thực hiện thao tác.";

                        if (action.Contains("kiểm tra") || action.Contains("nhấn nút") || action.Contains("lưu") || step == steps.Last())
                        {
                            if (tcId == "TC_ADCREATECNM_01")
                            {
                                // Click thẳng vào dòng rạp mới nhất (trên cùng)
                                _cinemaPage.ClickFirstCinemaRow();

                                // Đọc câu thông báo xanh
                                string msg = _cinemaPage.GetSuccessMessage();

                                if (!msg.Contains("thành công"))
                                    throw new Exception($"Không có thông báo thành công. Actual: {msg}");

                                actualMsg = msg;
                            }
                            else if (tcId == "TC_ADCREATECNM_02")
                            {
                                bool isBugPresent = !_driver.Url.Contains("CreateCinema");
                                if (isBugPresent)
                                    throw new Exception("Lỗi hệ thống: Không chặn tạo rạp trùng tên, vẫn lưu thành công.");

                                actualMsg = _cinemaPage.GetValidationErrorMessage();
                            }
                            else if (tcId == "TC_ADCREATECNM_03" || tcId == "TC_ADCREATECNM_04" || tcId == "TC_ADCREATECNM_05")
                            {
                                Thread.Sleep(1000);

                                if (!_driver.Url.Contains("CreateCinema"))
                                {
                                    throw new Exception("Lỗi hệ thống: Bỏ trống dữ liệu nhưng web vẫn cho tạo thành công (thiếu Validation).");
                                }

                                string errMsg = _cinemaPage.GetValidationErrorMessage();
                                if (errMsg == "Không có lỗi hiển thị")
                                    throw new Exception("Lỗi: Hệ thống chặn tạo rạp nhưng KHÔNG hiển thị text đỏ báo lỗi.");

                                actualMsg = errMsg;
                            }
                        }

                        step.ActualResult = actualMsg;
                        step.Status = "Pass";
                        step.Screenshots = "";
                    }
                    catch (Exception ex)
                    {
                        step.Status = "Fail";
                        step.ActualResult = ex.Message;
                        if (OperatingSystem.IsWindows()) step.Screenshots = ScreenshotHelper.Capture(tcId);
                        isPreviousStepFailed = true;
                    }
                }
            }
            catch (Exception globalEx)
            {
                if (steps.Any())
                {
                    var firstStep = steps.First();
                    firstStep.Status = "Fail";
                    firstStep.ActualResult = "Lỗi Exception Code: " + globalEx.Message;
                    if (OperatingSystem.IsWindows()) firstStep.Screenshots = ScreenshotHelper.Capture(tcId);
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