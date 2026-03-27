using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace sqa_testing_report.Services
{
    // D?ch v? ch?p ?nh màn hình
    public static class ScreenshotService
    {
        // P/Invoke ?? l?y kích th??c màn hình chính trên Windows
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        /// <summary>
        /// Ch?p toàn màn hình và l?u ?nh vào th? m?c t??ng ??i trong repo (m?c ??nh: Data/Screenshots).
        /// Tr? v? ???ng d?n t??ng ??i c?a ?nh (so v?i root repo) n?u tìm ???c root, ng??c l?i tr? v? ???ng d?n tuy?t ??i.
        /// Tên file luôn duy nh?t b?ng cách g?n timestamp + GUID.
        /// L?u ý: Hi?n implementation s? d?ng API Windows (user32.dll) nên ch? ch?y trên Windows.
        /// </summary>
        /// <param name="saveDirectoryRelative">Th? m?c t??ng ??i ?? l?u, ví d? "Data/Screenshots"</param>
        /// <returns>???ng d?n t??ng ??i (ho?c tuy?t ??i n?u không tìm ???c repo root) c?a file ?nh ?ã l?u</returns>
        public static string Capture(string saveDirectoryRelative = "Data\\Screenshots")
        {
            // Xác ??nh kích th??c màn hình chính
            int width = GetSystemMetrics(SM_CXSCREEN);
            int height = GetSystemMetrics(SM_CYSCREEN);

            using var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));
            }

            // Tìm th? m?c root c?a repo (d?a trên .csproj)
            string start = AppContext.BaseDirectory;
            string repoRoot = FindRepoRoot(start);

            string targetRoot = repoRoot ?? start;
            string targetDir = Path.Combine(targetRoot, saveDirectoryRelative);
            Directory.CreateDirectory(targetDir);

            string fileName = $"screenshot_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0,8)}.png";
            string fullPath = Path.Combine(targetDir, fileName);

            // L?u ?nh
            bmp.Save(fullPath, ImageFormat.Png);

            // Tr? v? ???ng d?n t??ng ??i n?u có repoRoot
            if (repoRoot != null)
            {
                return Path.GetRelativePath(repoRoot, fullPath).Replace(Path.DirectorySeparatorChar, '/');
            }

            return fullPath;
        }

        // Tìm th? m?c cha ch?a file project .csproj (gi?ng helper trong test)
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
