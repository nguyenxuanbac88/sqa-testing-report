namespace sqa_testing_report.Utilities
{
    public static class PathHelper
    {
        public static string GetRepoRoot(string startPath)
        {
            var di = new DirectoryInfo(startPath);
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