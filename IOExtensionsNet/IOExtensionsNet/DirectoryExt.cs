using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IOExtensionsNet
{
    public static class DirectoryExt
    {
        public static string CreateTempDir(string tempDirName)
        {
            var tempDirPath = Path.Combine(Path.GetTempPath(), tempDirName);
            if (Directory.Exists(tempDirPath))
                Directory.Delete(tempDirPath, true);
            Directory.CreateDirectory(tempDirPath);
            return tempDirPath;
        }
        //TODO:Documentation needed
        public static void CopyAllFiles(string sourceDir, string destDir, string[] excludedFileTypes = null, string[] excludedFileNames = null)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDir);

            var dirs = dir.GetDirectories();

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            var files = dir.GetFiles();
            var filesFilteredByName = excludedFileNames == null
                ? files
                : files.Where(f => !excludedFileNames.Any(ex => f.Name.ContainsIgnoreCaseInvariant(ex)));

            var filesFilteredByNameAndType = excludedFileTypes == null ? filesFilteredByName : filesFilteredByName.Where(f => !excludedFileTypes.Any(
                ext => Path.GetExtension(f.FullName).ToLower().Equals(ext, StringComparison.InvariantCultureIgnoreCase)));
            foreach (var file in filesFilteredByNameAndType)
            {
                var destFileName = Path.Combine(destDir, file.Name);
                file.CopyTo(destFileName, true);
            }

            foreach (var subdir in dirs)
                CopyAllFiles(subdir.FullName, Path.Combine(destDir, subdir.Name), excludedFileTypes, excludedFileNames);
        }
        public static bool ContainsIgnoreCaseInvariant(this string string2Check, string containsString)
        {
            if (string2Check == null || containsString == null)
                return false;
            return string2Check.ToLowerInvariant().Contains(containsString.ToLowerInvariant());
        }
    }
}
