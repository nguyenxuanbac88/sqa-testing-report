using OpenQA.Selenium;
using sqa_testing_report.Pages;
using sqa_testing_report.Services;
using sqa_testing_report.Utilities;

namespace sqa_testing_report.Tests
{
    [TestFixture]
    public class AdminUpdateCinemaTests
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
        // KHAI BÁO CÁC TEST CASES PHẦN UPDATE (ĐÚNG 5 CASE TRONG EXCEL)
        // ==========================================
        [Test] public void TC_ADUPDATECNM_01_CapNhatRapThanhCong() => ExecuteUpdateCinemaTest("TC_ADUPDATECNM_01");
        [Test] public void TC_ADUPDATECNM_02_BoTrongTenRap() => ExecuteUpdateCinemaTest("TC_ADUPDATECNM_02");
        [Test] public void TC_ADUPDATECNM_03_BoTrongDiaChi() => ExecuteUpdateCinemaTest("TC_ADUPDATECNM_03");
        [Test] public void TC_ADUPDATECNM_05_BoTrongThanhPho() => ExecuteUpdateCinemaTest("TC_ADUPDATECNM_05");

        // ==========================================
        // HÀM XỬ LÝ CHUNG
        // ==========================================
        private void ExecuteUpdateCinemaTest(string tcId)
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
                Thread.Sleep(2000);

                _cinemaPage.GoToListPage();

                // Lấy tên rạp thứ 2 làm mồi nhử cho TC_04 (Trùng lặp)
                string duplicateName = _cinemaPage.GetSecondCinemaName();

                // Click dòng đầu tiên để vào trang Edit
                _cinemaPage.ClickFirstCinemaRow();

                // Trích xuất Data từ Excel. Nếu không có cột tương ứng, lấy dữ liệu đang hiển thị trên web
                var nameStep = steps.FirstOrDefault(s => s.StepAction.Contains("tên") || s.StepAction.Contains("Tên"));
                var addressStep = steps.FirstOrDefault(s => s.StepAction.Contains("địa chỉ") || s.StepAction.Contains("Địa chỉ"));
                var cityStep = steps.FirstOrDefault(s => s.StepAction.Contains("thành phố") || s.StepAction.Contains("Thành phố"));

                string submitName = nameStep != null ? nameStep.TestData : _driver.FindElement(By.CssSelector("#CinemaName")).GetAttribute("value");
                string submitAddress = addressStep != null ? addressStep.TestData : _driver.FindElement(By.CssSelector("#Address")).GetAttribute("value");
                string submitCity = cityStep != null ? cityStep.TestData : _driver.FindElement(By.CssSelector("#City")).GetAttribute("value");

                // MAPPING ĐÚNG ID VỚI LOGIC
                if (tcId == "TC_ADUPDATECNM_01")
                {
                    submitName = (nameStep?.TestData ?? "UpdateCinema") + "_" + DateTime.Now.Ticks;
                }
                else if (tcId == "TC_ADUPDATECNM_02")
                {
                    submitName = ""; // TC 02: Bỏ trống Tên
                }
                else if (tcId == "TC_ADUPDATECNM_03")
                {
                    submitAddress = ""; // TC 03: Bỏ trống Địa chỉ
                }
                else if (tcId == "TC_ADUPDATECNM_05")
                {
                    submitCity = ""; // TC 05: Bỏ trống Thành phố
                }

                // Điền form và bấm Lưu
                _cinemaPage.EnterCinemaInfo(submitName, submitAddress, submitCity);
                _cinemaPage.ClickUpdateSubmit();

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
                            if (tcId == "TC_ADUPDATECNM_01")
                            {
                                // KỊCH BẢN THÀNH CÔNG
                                if (!_driver.Url.Contains("LoadListCinema"))
                                    throw new Exception("Lỗi: Cập nhật xong nhưng không tự động quay về trang danh sách.");

                                _cinemaPage.ClickFirstCinemaRow();
                                string msg = _cinemaPage.GetSuccessMessage();
                                if (!msg.Contains("thành công"))
                                    throw new Exception($"Không có thông báo cập nhật thành công. Actual: {msg}");

                                actualMsg = msg;
                            }

                            else if (tcId == "TC_ADUPDATECNM_02" || tcId == "TC_ADUPDATECNM_03" || tcId == "TC_ADUPDATECNM_05")
                            {
                                // CHECK BỎ TRỐNG DỮ LIỆU (Case 02, 03, 05)
                                Thread.Sleep(1000);

                                if (_driver.Url.Contains("LoadListCinema"))
                                {
                                    throw new Exception("Lỗi hệ thống: Bỏ trống dữ liệu nhưng web vẫn cho cập nhật thành công (thiếu Validation).");
                                }

                                string errMsg = _cinemaPage.GetValidationErrorMessage();
                                if (errMsg == "Không có lỗi hiển thị")
                                    throw new Exception("Lỗi: Hệ thống chặn cập nhật nhưng KHÔNG hiển thị text đỏ báo lỗi.");

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