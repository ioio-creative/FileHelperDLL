using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileHelperDLL
{
    public class FileHelper
    {
        public static long GetFileSize(string filePath)
        {
            return GetFileSize(new FileInfo(filePath));
        }

        public static long GetFileSize(FileInfo file)
        {
            return file.Length;
        }

        public static bool IsFileSizeZero(string filePath)
        {
            return IsFileSizeZero(new FileInfo(filePath));
        }

        public static bool IsFileSizeZero(FileInfo file)
        {
            return GetFileSize(file) == 0;
        }

        public static IEnumerable<FileInfo> GetFilesInDirectory(string targetDirectory, string fileSearchPattern = "*")
        {
            return new DirectoryInfo(targetDirectory).EnumerateFiles(fileSearchPattern, SearchOption.TopDirectoryOnly);            
        }

        // https://stackoverflow.com/questions/3527203/getfiles-with-multiple-extensions
        // it does not matter whether the extensions contain the "." dot
        public static IEnumerable<FileInfo> GetFilesInDirectoryByExtensions(string targetDirectory, params string[] extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException("No extension arguments provided.");
            }

            IEnumerable<FileInfo> files = new DirectoryInfo(targetDirectory).EnumerateFiles();

            return files.Where(f =>
                extensions.Select(ext => RemoveDotFromExtensionIfExists(ext).ToLower())
                .Contains(RemoveDotFromExtensionIfExists(f.Extension).ToLower()));
        }

        public static FileInfo GetFileWithEarliestLastWriteTimeInDirectory(string targetDirectory, string fileSearchPattern = "")
        {
            return GetFileWithEarliestLastWriteTime(GetFilesInDirectory(targetDirectory, fileSearchPattern));
        }

        public static FileInfo GetFileWithEarliestLastWriteTimeInDirectoryByExtensions(string targetDirectory, params string[] extensions)
        {
            return GetFileWithEarliestLastWriteTime(GetFilesInDirectoryByExtensions(targetDirectory, extensions));
        }

        private static FileInfo GetFileWithEarliestLastWriteTime(IEnumerable<FileInfo> files)
        {
            return files.OrderBy(f => f.LastWriteTime).FirstOrDefault();  // FirstOrDefault() can return null and don't raise an exception
        }

        public static FileInfo GetFileWithLatestLastWriteTimeInDirectory(string targetDirectory, string fileSearchPattern = "")
        {
            return GetFileWithLatestLastWriteTime(GetFilesInDirectory(targetDirectory, fileSearchPattern));
        }

        public static FileInfo GetFileWithLatestLastWriteTimeInDirectoryByExtensions(string targetDirectory, params string[] extensions)
        {
            return GetFileWithLatestLastWriteTime(GetFilesInDirectoryByExtensions(targetDirectory, extensions));
        }

        private static FileInfo GetFileWithLatestLastWriteTime(IEnumerable<FileInfo> files)
        {
            return files.OrderBy(f => f.LastWriteTime).LastOrDefault();  // LastOrDefault() can return null and don't raise an exception
        }

        public static void MoveFilesFromOneDirectoryToAnother(string oldFileDirectory,
            string newFileDirectory, string fileSearchPattern = "")
        {
            IEnumerable<FileInfo> oldFiles = GetFilesInDirectory(oldFileDirectory, fileSearchPattern);
            MoveFilesFromOneDirectoryToAnother(oldFiles, newFileDirectory);
        }

        public static void MoveFilesFromOneDirectoryToAnotherByExtensions(string oldFileDirectory,
            string newFileDirectory, params string[] extensions)
        {
            IEnumerable<FileInfo> oldFiles = GetFilesInDirectoryByExtensions(oldFileDirectory, extensions);
            MoveFilesFromOneDirectoryToAnother(oldFiles, newFileDirectory);
        }

        public static void MoveFilesFromOneDirectoryToAnother(IEnumerable<FileInfo> oldFiles, string newFileDirectory)
        {
            foreach (FileInfo oldFile in oldFiles)
            {
                MoveFileFromOneDirectoryToAnother(oldFile, newFileDirectory);
            }
        }

        public static bool MoveFileFromOneDirectoryToAnother(FileInfo oldFile, string newFileDirectory)
        {
            return MoveFileFromOneDirectoryToAnother(oldFile.FullName, newFileDirectory);
        }

        public static bool MoveFileFromOneDirectoryToAnother(string oldFilePath, string newFileDirectory)
        {
            bool isMoved = false;
            string newFilePath = ChangeDirectoryInFilePathString(oldFilePath, newFileDirectory);

            // guard because File.Move() will raise exception if newFilePath already existss.
            if (!File.Exists(newFilePath))
            {
                File.Move(oldFilePath, newFilePath);
                isMoved = true;
            }

            return isMoved;
        }

        public static void CopyFileFromOneDirectoryToAnother(FileInfo oldFile, string newFileDirectory)
        {
            CopyFileFromOneDirectoryToAnother(oldFile.FullName, newFileDirectory);
        }

        public static void CopyFileFromOneDirectoryToAnother(string oldFilePath, string newFileDirectory)
        {
            string newFilePath = ChangeDirectoryInFilePathString(oldFilePath, newFileDirectory);
            File.Copy(oldFilePath, newFilePath, true);  // overwrite
        }

        public static string ChangeDirectoryInFilePathString(FileInfo oldFile, string newFileDirectory)
        {
            string fileName = Path.GetFileName(oldFile.FullName);
            return newFileDirectory + fileName;
        }

        public static string ChangeDirectoryInFilePathString(string oldFilePath, string newFileDirectory)
        {
            string fileName = Path.GetFileName(oldFilePath);
            return newFileDirectory + fileName;
        }

        public static void DeleteAllFilesInDirectory(string targetDirectory)
        {
            foreach (string filePath in Directory.EnumerateFiles(targetDirectory))
            {
                File.Delete(filePath);
            }
        }

        public static void DeleteAllFilesInDirectorySafe(string targetDirectory)
        {
            foreach (string filePath in Directory.EnumerateFiles(targetDirectory))
            {
                DeleteFileSafe(filePath);
            }
        }

        public static void DeleteFileSafe(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static bool FileExistsWithoutSpecifyingExtension(string path)
        {
            // https://stackoverflow.com/questions/22641503/check-if-file-exists-not-knowing-the-extension
            string directoryPath = Path.GetDirectoryName(path);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            IEnumerable<string> matchedFiles = Directory.EnumerateFiles(directoryPath,
                fileNameWithoutExtension + ".*", SearchOption.TopDirectoryOnly);
            return matchedFiles.Count() > 0;
        }

        // https://stackoverflow.com/questions/802541/creating-an-empty-file-in-c-sharp
        public static void CreateEmptyFile(string fileName)
        {
            //File.Create(fileName).Dispose();
            using (File.Create(fileName)) {}
        }

        public static string RemoveDotFromExtensionIfExists(string extension)
        {
            if (extension[0] == '.')
            {
                return extension.Substring(1);
            }
            else
            {
                return extension;
            }
        }
        
        public static bool MoveAndRenameFile(string oldFilePath, string newFilePath)
        {
            bool isMoved = false;

            // create directories in newFilePath if they do not exist
            // https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.createdirectory?view=netframework-4.7#System_IO_Directory_CreateDirectory_System_String_
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));

            // guard because File.Move() will raise exception if newFilePath already existss.
            if (!File.Exists(newFilePath))
            {
                File.Move(oldFilePath, newFilePath);
                isMoved = true;
            }

            return isMoved;
        }
    }
}
