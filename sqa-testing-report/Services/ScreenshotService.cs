using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace sqa_testing_report.Services
{
    // Dịch vụ chụp ảnh màn hình - Đã nâng cấp tính năng đặt tên cố định và tự xóa ảnh cũ
    public static class ScreenshotService
    {
        // P/Invoke để lấy kích thước màn hình chính trên Windows
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        /// <summary>
        /// Chụp toàn màn hình và lưu ảnh với tên cố định theo TestCaseID (vd: TC_CINEMA_01.png).
        /// Nếu file ảnh đã tồn tại, code sẽ tự động xóa ảnh cũ trước khi lưu ảnh mới.
        /// </summary>
        /// <param name="testCaseId">TestCaseID dùng để đặt tên file ảnh (không bao gồm đuôi file)</param>
        /// <param name="saveDirectoryRelative">Thư mục tương đối để lưu, ví dụ "Data/Screenshots"</param>
        /// <returns>Đường dẫn tương đối của file ảnh đã lưu</returns>
        [SupportedOSPlatform("windows")]
        public static string Capture(string testCaseId, string saveDirectoryRelative = "Data/Screenshots")
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("ScreenshotService.Capture is supported only on Windows platforms.");

            // Xác định kích thước màn hình chính
            int width = GetSystemMetrics(SM_CXSCREEN);
            int height = GetSystemMetrics(SM_CYSCREEN);

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));
                }

                // Tìm thư mục root của repo (dựa trên .csproj)
                string start = AppContext.BaseDirectory;
                string repoRoot = FindRepoRoot(start);

                string targetRoot = repoRoot ?? start;
                string targetDir = Path.Combine(targetRoot, saveDirectoryRelative);

                // Đảm bảo thư mục tồn tại
                Directory.CreateDirectory(targetDir);

                // Đặt tên file cố định theo TestCaseID.png
                string fileName = $"{testCaseId}.png";
                string fullPath = Path.Combine(targetDir, fileName);

                // --- FIX: Logic tự xóa ảnh cũ khi chạy lại test ---
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        // Ghi log nếu không xóa được (ví dụ file đang bị mở) nhưng không làm crash test
                        TestContext.WriteLine($"Cảnh báo: Không thể xóa ảnh cũ {fileName}: {ex.Message}");
                    }
                }

                // Lưu ảnh mới
                bmp.Save(fullPath, ImageFormat.Png);

                // Trả về đường dẫn tương đối nếu có repoRoot
                if (repoRoot != null)
                {
                    return Path.Combine(saveDirectoryRelative, fileName).Replace(Path.DirectorySeparatorChar, '/');
                }

                return fullPath;
            }
        }

        // Tìm thư mục cha chứa file project .csproj
        private static string FindRepoRoot(string start)
        {
            var di = new DirectoryInfo(start);
            while (di != null)
            {
                var csproj = Directory.GetFiles(di.FullName, "*.csproj");
                if (csproj != null && csproj.Length > 0)
                    return di.FullName;
                di = di.Parent;
            }
            return null;
        }
    }
}