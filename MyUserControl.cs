using System;
using System.Windows.Forms;

namespace AutoCADNote
{
    public partial class MyUserControl : UserControl
    {
        public MyUserControl()
        {
            InitializeComponent();
            this.webBrowser1.ScrollBarsEnabled = false;
        }

        public WebBrowser WebBrowser
        {
            get { return this.webBrowser1; }
        }

        public void SetLoginUrl(string loginUrl)
        {
            webBrowser1.Navigate(loginUrl, "_self", null, "User-Agent: " + "OneNoteForAutoCAD");
        }

        internal void SetBrowserContent(string editUrl)
        {
            webBrowser1.Navigate(editUrl, "_self", null, "User-Agent: " + "OneNoteForAutoCAD");
        }

        internal void ShowLoadingLabel(bool show)
        {
            this.LoadingLabel.Visible = show;
        }
    }
}
