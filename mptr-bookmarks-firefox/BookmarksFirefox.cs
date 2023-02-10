using Microsoft.Data.Sqlite;
using System;

namespace mptr_bookmarks_firefox
{
    public class BookmarksFirefox
    {
        private List<string> bookmarkFiles = new();

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

        public List<string> getBookmarkFilesPaths() => bookmarkFiles;

        public List<KeyValuePair<string, string>> GetBookmarks()
        {
            // todo: check if file is changed since last time and re-read, use cached results otherwise

            List<KeyValuePair<string, string>>? bookmarks = new();

            foreach (string bookmarkFile in bookmarkFiles)
            {
                SqliteConnection sqlite_conn = new(string.Format("Data Source={0};Mode=ReadOnly", bookmarkFile));
                sqlite_conn.Open();

                SqliteCommand sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = "SELECT b.title, p.url FROM moz_bookmarks as b LEFT JOIN moz_places as p WHERE b.fk = p.id AND b.title <> '' ORDER BY LENGTH(b.title)";

                SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
                while (sqlite_datareader.Read())
                {
                    bookmarks.Add(
                        new KeyValuePair<string, string>(sqlite_datareader.GetString(0), sqlite_datareader.GetString(1))
                    );
                }
            }

            return bookmarks;
        }
    }
}