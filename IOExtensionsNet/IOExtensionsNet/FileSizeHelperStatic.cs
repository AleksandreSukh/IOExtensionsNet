using System;
using System.Globalization;
using System.IO;

namespace IOExtensionsNet
{
    public static class FileSizeHelperStatic
    {
        public static string FormatFilePathPlusSizeIfAvailable(string path)
        {
            decimal? sth;
            return FormatFilePathPlusSizeIfAvailable(path, out sth);
        }

        public static string FormatFilePathPlusSizeIfAvailable(string path, out decimal? size)
        {
            var fileSize = GetFileSize(path);
            size = fileSize;

            return FormatFilePathPlusKbs(path, ConvertBytesToKbs(fileSize ?? 0));
        }

        public static string FormatFilePathPlusKbs(string path, decimal? fileSize)
        {
            return
                $"{(fileSize.HasValue ? fileSize.Value.ToString(CultureInfo.InvariantCulture) : "")}kb : \t{path}";
        }

        public static decimal ConvertBytesToKbs(decimal fileSize)
        {
            return Math.Round(fileSize / 1024, 2);
        }

        public static decimal? GetFileSize(string path)
        {
            try
            { return new FileInfo(path).Length; }
            catch { return null; }
        }


    }
}