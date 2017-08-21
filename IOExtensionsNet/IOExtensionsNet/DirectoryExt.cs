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
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool tryOverwrite, bool tryRename)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!Directory.Exists(dir.FullName))
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = System.IO.Path.Combine(destDirName, file.Name);
                FileExt.Copy(file.FullName, temppath, tryOverwrite, tryRename);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (!copySubDirs) return;

            foreach (var subdir in dirs)
            {
                var temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, true, tryOverwrite, tryRename);
            }
        }

        public static void DeleteDirectoryTree(string target_dir)
        {
            var files = Directory.GetFiles(target_dir);
            var dirs = Directory.GetDirectories(target_dir);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in dirs)
            {
                DeleteDirectoryTree(dir);
            }

            Directory.Delete(target_dir, false);
        }

        public static void CreateNewOrClear(string destDir)
        {
            if (Directory.Exists(destDir))
            {
                try
                {
                    Directory.Delete(destDir, true);
                    Directory.CreateDirectory(destDir);
                }
                catch (IOException)
                {
                    foreach (var item in Directory.GetFiles(destDir))
                    {
                        File.Move(item, $"{item}.del.{Guid.NewGuid()}");
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(destDir);
            }
        }

        public static void CreateIfDoesNotExist(string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
        }
        public static string CreateForFile(string path)
        {
            var absolutePath = !Path.IsPathRooted(path) ? Path.Combine(Directory.GetCurrentDirectory(), path) : path;
            var destDir = Path.GetDirectoryName(absolutePath);
            CreateIfDoesNotExist(destDir);
            return destDir;
        }

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
        //TODO:Refactor take this to Utils
        public static bool ContainsIgnoreCaseInvariant(this string string2Check, string containsString)
        {
            if (string2Check == null || containsString == null)
                return false;
            return string2Check.ToLowerInvariant().Contains(containsString.ToLowerInvariant());
        }
    }
}
