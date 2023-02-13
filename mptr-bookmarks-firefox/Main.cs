using ManagedCommon;
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

            List<Bookmark> bookmarks = new List<Bookmark>();

            if (bookmarksFirefox.HaveBookmarks())
            {
                bookmarks.AddRange(bookmarksFirefox.GetBookmarks());
            }

            return generateResultList(value, bookmarks);
        }

        private List<Result> generateResultList(string value, List<Bookmark> bookmarks)
        {
            List<Result> result = new();

            foreach (Bookmark bookmark in bookmarks)
            {
                // filter out what user want
                if (bookmark.Title.ToLower().Contains(value) || bookmark.URL.ToLower().Contains(value))
                {
                    result.Add(
                        new Result
                        {
                            Title = bookmark.Title,
                            SubTitle = bookmark.URL,
                            IcoPath = IconPath,
                            Action = e =>
                            {
                                OpenUrl(bookmark.URL);
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