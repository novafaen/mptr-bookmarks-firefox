using Microsoft.Data.Sqlite;

namespace mptr_bookmarks_firefox
{
    public readonly record struct Bookmark(string Title, string URL);

    public class BookmarksFirefox
    {
        private List<BookmarkFile> bookmarkFiles = new();
        private List<Bookmark>? bookmarks = new();

        public BookmarksFirefox()
        {
            // Bookmarks if Firefox is installed with Windows Store.
            try
            {
                string[] pathPackages = Directory.GetDirectories(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Packages\\"
                );

                foreach (string pathPackage in pathPackages)
                {
                    if (pathPackage.Contains("firefox", StringComparison.OrdinalIgnoreCase))
                    {
                        bookmarkFiles.AddRange(FileFinder.FindFilesInDirectory("places.sqlite", pathPackage));
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing, just that no file found.
            }

            // Bookmarks if Firefox is installed with installer.
            try
            {
                string[] pathPackages = Directory.GetDirectories(
                    Environment.ExpandEnvironmentVariables("%AppData%") + "\\Mozilla\\Firefox\\"
                );

                foreach (string pathPackage in pathPackages)
                {
                    if (pathPackage.Contains("firefox", StringComparison.OrdinalIgnoreCase))
                    {
                        bookmarkFiles.AddRange(FileFinder.FindFilesInDirectory("places.sqlite", pathPackage));
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing, just that no file found.
            }
        }

        public bool HaveBookmarks() => bookmarkFiles.Count > 0;

        public List<Bookmark> GetBookmarks()
        {
            // check if bookmark file have been updated since last read.
            bool bookmarksUpdated = false;
            foreach (BookmarkFile bookmarkFile in bookmarkFiles)
            {
                if (bookmarkFile.LastUpdated >= FileFinder.LastUpdated(bookmarkFile.Path)) {
                    // bookmarks have not been updated
                    bookmarksUpdated = true;
                }
            }
            
            if (!bookmarksUpdated) {
                return bookmarks;
            }

            foreach (BookmarkFile bookmarkFile in bookmarkFiles)
            {
                SqliteConnection sqlite_conn = new(string.Format("Data Source={0};Mode=ReadOnly", bookmarkFile));
                sqlite_conn.Open();

                SqliteCommand sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = "SELECT b.title, p.url FROM moz_bookmarks as b LEFT JOIN moz_places as p WHERE b.fk = p.id AND b.title <> '' ORDER BY LENGTH(b.title)";

                SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
                while (sqlite_datareader.Read())
                {
                    bookmarks.Add(
                        new Bookmark(sqlite_datareader.GetString(0), sqlite_datareader.GetString(1))
                    );
                }
            }

            return bookmarks;
        }
    }
}