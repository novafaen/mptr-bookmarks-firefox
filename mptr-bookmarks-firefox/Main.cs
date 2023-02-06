using ManagedCommon;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Wox.Plugin;

namespace mptr_bookmarks_firefox
{
    public class Main : IPlugin
    {
        private string IconPath { get; set; }

        private PluginInitContext Context { get; set; }
        public string Name => "Bookmarks: Firefox";

        public string Description => "Microsoft Powertoys Run plugin for accessing Firefox bookmarks.";

        private const string firefoxSettingsPath = "Mozilla\\Firefox\\Profiles\\";
        private List<KeyValuePair<string, string>> bookmarks = new();

        public List<Result> Query(Query query)
        {
            if (query?.Search is null)
            {
                return new List<Result>(0);
            }

            var value = query.Search.Trim().ToLower();

            if (string.IsNullOrEmpty(value))
            {
                return new List<Result>(0);
            }

            List<Result> result = new();

            foreach (KeyValuePair<string, string> bookmark in bookmarks)
            {
                // filter out what user want
                if (bookmark.Key.ToLower().Contains(value) || bookmark.Value.ToLower().Contains(value)) {
                    result.Add(
                        new Result
                        {
                            Title = bookmark.Key,
                            SubTitle = bookmark.Value,
                            IcoPath = IconPath,
                            Action = e =>
                            {
                                OpenUrl(bookmark.Value);
                                return true;
                            },
                        }
                    );
                }
            }

            return result;
        }

        public void Init(PluginInitContext context)
        {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());

            string fullFirefoxSetingsPath = Environment.ExpandEnvironmentVariables("%AppData%") + "\\" + firefoxSettingsPath;
            string[] profilePaths = Directory.GetDirectories(fullFirefoxSetingsPath);

            string? pathBookmarksFile = null;
            foreach (string profilePath in profilePaths)
            {   
                string[] profileFiles = Directory.GetFiles(profilePath);
                foreach (string profileFile in profileFiles)
                {
                    string fileName = Path.GetFileName(profileFile);
                    if (String.Equals(fileName, "places.sqlite", StringComparison.OrdinalIgnoreCase)) {
                        pathBookmarksFile = profileFile;
                        break;
                    }
                }
            }

            if (pathBookmarksFile is null)
            {
                return;  // no bookmarks file found.
            }

            try
            {
                string connectionString = string.Format("Data Source={0};Version=3;Read Only=True", pathBookmarksFile);
                SqliteConnection? sqlite_conn = null;

                sqlite_conn = new SqliteConnection("Data Source = " + pathBookmarksFile);
                sqlite_conn.Open();
                
                SqliteDataReader sqlite_datareader;
                SqliteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                sqlite_cmd.CommandText = "SELECT b.title, p.url FROM moz_bookmarks as b LEFT JOIN moz_places as p WHERE b.fk = p.id AND b.title <> ''";
                sqlite_cmd.CreateParameter();
                sqlite_datareader = sqlite_cmd.ExecuteReader();
                while (sqlite_datareader.Read())
                {
                    bookmarks.Add(
                        new KeyValuePair<string, string>(sqlite_datareader.GetString(0), sqlite_datareader.GetString(1))
                    );
                }
            }
            catch (Exception exc)
            {
                // better error messages needed
                bookmarks.Add(
                    new KeyValuePair<string, string>("Error!", "Unexpected SQL error." + exc.Message)
                );
            }
        }

        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                IconPath = "images/bookmarks.light.png";
            }
            else
            {
                IconPath = "images/bookmarks.dark.png";
            }
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        // originates from https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}