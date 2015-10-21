using Autodesk.AutoCAD.Windows;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCADNote
{
    public class Manager
    {
        public static PaletteSet myPaletteSet;
        public static MyUserControl userControl;
        private static bool initialized;

        public static async Task Init()
        {
            // if not initialized, then initialize
            if (!initialized)
            {
                await Initialize();
            }

            // Open palette -- now there is already a page that is created.
            myPaletteSet.Visible = true;
        }

        private static async Task Initialize()
        {
            // Create paletteSet
            myPaletteSet = PaletteManager.CreatePaletteSet();

            // Create usercontrol
            userControl = new MyUserControl();

            // Wire up palette to user control
            myPaletteSet.Add("Palette1", userControl);

            // Wire up events
            userControl.WebBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            AutoCADManager.WireUpDocumentReloadEvent();
            AutoCADManager.DocumentChanged += AutoCADManager_DocumentChanged;

            // login and store code and token
            var loginUrl = await AuthManager.GetLoginUrl();

            // Ask user to login, and eventually accept permissions
            userControl.SetLoginUrl(loginUrl);

            // Show the palette
            myPaletteSet.Visible = true;
            myPaletteSet.Dock = DockSides.Right; // This needs to be set here due to a bug in AutoCad 2016 that Dock should be set after visible.

            // Set initialized
            initialized = true;
        }

        private static async Task ShowPageAsPerDocument()
        {
            var autoCadName = AutoCADManager.GetDrawingName();
            if (!String.IsNullOrEmpty(OneNoteManager.PageName)
                && !String.Equals(autoCadName, OneNoteManager.PageName))
            {
                await ShowPage(autoCadName);
            }
        }

        private static async void AutoCADManager_DocumentChanged(object sender, EventArgs e)
        {
            await ShowPageAsPerDocument();
        }

        private static async void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Check if the url incoming has a code, if so pass on to the AuthManager to get token
            if (e.Url.Query.Contains("code="))
            {
                var token = await AuthManager.GetToken(e.Url);
                await ShowPage(AutoCADManager.GetDrawingName());
            }
            else if (e.Url.ToString().StartsWith("https://onedrive.live.com"))
            {
                userControl.ShowLoadingLabel(false);
            }
            else if (e.Url.OriginalString.Contains("error=access_denied"))
            {
                // User refused to accept the permission scopes
                myPaletteSet.Visible = false;
                initialized = false; // Set initialize to false so that it tries to create again if opened next time.

                // Remove the wire-ups of events else they will happen twice
                AutoCADManager.UnWireUpDocumentReloadEvent();
                AutoCADManager.DocumentChanged -= AutoCADManager_DocumentChanged;
            }
        }

        private static async Task ShowPage(string autoCadName)
        {
            userControl.ShowLoadingLabel(true);

            // create page if not already created.
            await OneNoteManager.CreatePageIfNotExists(autoCadName);

            userControl.SetBrowserContent(OneNoteManager.PageEditUrl);
        }
    }
}
