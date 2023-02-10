using System;
using System.IO;

namespace mptr_bookmarks_firefox
{
    public class FileFinder
    {
        public static List<string> FindFilesInDirectory(string fileName, string path)
        {
            List<string> filesFound = new();
            try
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (string.Equals(Path.GetFileName(file), fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(file);
                        filesFound.Add(file);
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing, will return empty list.
            }

            try
            {
                string[] directories = Directory.GetDirectories(path);

                foreach (string directory in directories)
                {
                    filesFound.AddRange(FindFilesInDirectory(fileName, directory));
                }
            }
            catch (Exception)
            {
                // Do nothing, will return empty list.
            }

            return filesFound;
        }
        public static DateTime LastUpdated(string file)
        {
            return new FileInfo(file).LastWriteTime;
        }
    }
}