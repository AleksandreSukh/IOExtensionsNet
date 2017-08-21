using System;
using System.IO;

namespace IOExtensionsNet
{
    public static class FileExt 
    {
        static void Log(string text)
        {
            //_?.WriteLine(text);
        }

        public static bool FileIsNotEmpty(string path) { return File.Exists(path) && !FileSizeIs0(path); }
        public static bool FileSizeIs0(string path) { return GetFileSize(path) == 0; }
        public static bool FileIsAbsentOrEmpty(string path) { return !FileIsNotEmpty(path); }
        public static void CreateFileIfDoesNotExist(string path) { if (!File.Exists(path)) File.Create(path).Close(); }
        public static void DeleteFileWaitFinalizers(string destPath)
        {
            try { File.Delete(destPath); }
            catch
            {
                //Without GC. methods file deletion was, sometimes, not possible
                GC.Collect();
                GC.WaitForPendingFinalizers();

                File.Delete(destPath);
            }
        }

        public static bool MoveFileWaitFinalizers(string source, string destination, bool tryOverwrite = false, bool tryRename = false)
        {
            var copied = Copy(source, destination, tryOverwrite, tryRename);
            if (copied)
                DeleteFileWaitFinalizers(source);
            return copied;
        }

        public static void Move(string source, string destination, bool tryOverwrite = false, bool tryRename = false)
        {
            Log($"--1 Moving file:\"{source}\" to \"{destination}\" overwrite:{tryOverwrite} rename:{tryRename}");

            try
            {
                //Log($"Moving file from {source} to {destination}");
                File.Move(source, destination);
                Log("--2");
                //return true;
            }
            catch (IOException ioException)
            {
                Log("--3");
                try
                {
                    Log("--4 " + ioException);
                    if (!tryOverwrite) throw;
                    Log("--5");

                    File.Delete(destination);
                    Log("--6");

                    Move(source, destination, tryRename: tryRename);
                }
                catch (Exception ex1)
                {
                    Log("--7 " + ex1);
                    if ((!(ex1 is IOException) && !(ex1 is UnauthorizedAccessException)) || !tryRename) throw;
                    Log("--8");
                    string renamedTo;
                    RenameRandomly(destination, out renamedTo);
                    Log("--9");

                    //return 
                    Move(source, destination);
                    Log("--10");
                }
            }
            //catch (Exception ex)
            //{
            //    Log(ex.ToString());

            //    //return false;
            //}
        }
        public static string MoveWithRename(string source, string destination, bool tryRename = false)
        {
            string renamedTo = null;
            Log($"Moving with rename from:\"{source}\" to \"{destination}\"");

            try
            {
                File.Move(source, destination);
            }
            catch (IOException)
            {
                RenameRandomly(destination, out renamedTo);
                Move(source, destination);
            }
            var resultMessage = renamedTo != null ? $"Moving with rename from:\"{source}\" to \"{destination}\" existing file renamed to:{renamedTo}" : $"Moving with rename from:\"{source}\" to \"{destination}\" existing file was not renamed";
            Log(resultMessage);
            return renamedTo;
        }
        public static bool Copy(string source, string destination, bool tryOverwrite = false, bool tryRename = false)
        {
            if (tryRename)
                tryOverwrite = false;

            //Log($"Copying file from {source} to {destination}");
            try
            {
                File.Copy(source, destination, tryOverwrite);
                return true;
            }
            catch (IOException)
            {
                try
                {
                    if (tryOverwrite) return Copy(source, destination);
                    throw;
                }
                catch (IOException)
                {
                    if (!tryRename) throw;
                    string renamedTo;
                    RenameRandomly(destination, out renamedTo);
                    return Copy(source, destination);
                }
            }
        }
        public static long GetFileSize(string path2File)
        {
            try { return new FileInfo(path2File).Length; }
            catch { return -1; }
        }
        public static decimal? GetFileSizeNullable(string path)
        {
            return FileSizeHelperStatic.GetFileSize(path);
        }

        static void RenameRandomly(string destination, out string destFileRenamedTo)
        {
            var randomFileName = GenerateRandomNameStartingWithOriginal(destination);
            Move(destination, randomFileName);
            destFileRenamedTo = randomFileName;
        }
        static string GenerateRandomNameStartingWithOriginal(string destination) { return destination + Guid.NewGuid(); }

    }
}