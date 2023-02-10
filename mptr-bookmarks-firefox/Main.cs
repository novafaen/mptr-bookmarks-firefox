using ManagedCommon;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private BookmarksFirefox bookmarksFirefox = new BookmarksFirefox();

        public List<Result> Query(Query query)
        {
            if (query?.Search is null)
            {
                return new List<Result>(0);
            }

            string value = query.Search.Trim().ToLower();

            if (string.IsNullOrEmpty(value))
            {
                return new List<Result>(0);
            }

            // for debugging purposes
            if (String.Equals(value, "!!status"))
            {

            }

            List<KeyValuePair<string, string>> bookmarks = new List<KeyValuePair<string, string>>();

            if (bookmarksFirefox.HaveBookmarks())
            {
                bookmarks.AddRange(bookmarksFirefox.GetBookmarks());
            }

            return generateResultList(value, bookmarks);
        }

        private List<Result> generateResultList(string value, List<KeyValuePair<string, string>> bookmarks)
        {
            List<Result> result = new();

            foreach (KeyValuePair<string, string> bookmark in bookmarks)
            {
                // filter out what user want
                if (bookmark.Key.ToLower().Contains(value) || bookmark.Value.ToLower().Contains(value))
                {
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
        private static void OpenUrl(string url)
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