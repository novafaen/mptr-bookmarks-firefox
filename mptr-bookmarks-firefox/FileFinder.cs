namespace mptr_bookmarks_firefox
{
    public readonly record struct BookmarkFile(string Path, DateTime LastUpdated);

    public class FileFinder
    {
        public static List<BookmarkFile> FindFilesInDirectory(string fileName, string path)
        {
            List<BookmarkFile> filesFound = new();
            try
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (string.Equals(Path.GetFileName(file), fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        filesFound.Add(new BookmarkFile(file, new FileInfo(file).LastWriteTime));
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