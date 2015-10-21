using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AutoCADNote
{
    public static class OneNoteManager
    {
        private static readonly Uri NotebookEndPoint = new Uri("https://www.onenote.com/api/v1.0/notebooks");

        private static string pageName;
        private static string pageEditUrl;
        private static string AutoCADNotebook = "AutoCAD_OneNote";
        private static string AutoCADNotebookSection = "Drawings";

        public static string SectionsEndPoint { get; set; }

        public static string PagesEndPoint { get; set; }

        public static string PageEditUrl { get { return pageEditUrl; } }

        public static string PageName { get { return pageName; } }

        private static Dictionary<string, string> PageNameToEditUrls = new Dictionary<string, string>();

        public static async Task CreatePageIfNotExists(string name)
        {
            // Check if a page with this name exists in the AutoCAD notebook, if not create it.
            // Store the sectionsUrl and pagesurl of created notebook and section for the page
            name = SanitizeName(name);

            if (String.IsNullOrEmpty(SectionsEndPoint) || String.IsNullOrEmpty(PagesEndPoint)) // We dont have context of section yet, create or get if it exists
            {
                // Check if notebook exists, if not create it
                await CreateNotebookIfNotExists();

                // Check if section exists, if not create it
                await CreateSectionIfNotExists();

                // Now SectionsUrl and PagesUrl are populated
            }

            if (!String.IsNullOrEmpty(PageName) && PageName == name)
            {
                // This page is the one in context, so no need to create anything, PageEditUrl in context is the right one.
            }
            else // First time run when there is not PageName populated, or if the page name is different
            {
                // Notebook and section are created and verified, just the pagename is different, do only page level processing

                // Check if page exists, if not create it
                // First check if in-memory dictionary has this page, if so use it from there
                if (PageNameToEditUrls.ContainsKey(name))
                {
                    pageEditUrl = PageNameToEditUrls[name];
                    pageName = name;
                }
                else
                {
                    List<PageObject> existingPages = await GetPagesInSection(AuthManager.Token);
                    var sameNamedPages = existingPages.Where(e => e.Title == name);
                    if (sameNamedPages.Any())
                    {
                        pageEditUrl = sameNamedPages.First().EditUrl;
                    }
                    else
                    {
                        pageEditUrl = await CreatePageInOneNote(name, AuthManager.Token);
                    }

                    pageName = name; // Set the name so that we can detect when it changed(new file opened),
                                     //and decide to open that page, or create a new page

                    // Add to in-memory dictionary for faster retrieval in case of switching between open documents.
                    if (!PageNameToEditUrls.ContainsKey(pageName))
                    {
                        PageNameToEditUrls.Add(pageName, pageEditUrl);
                    }
                }
            }
        }

        // Create a simple HTML page and send it to the OneNote API
        private static async Task<string> CreatePageInOneNote(string name, string token)
        {
            var client = new HttpClient();

            // Note: API only supports JSON return type.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string date = DateTime.Now.ToString("o");
            string simpleHtml = "<html>" +
                           "<head>" +
                           "<title>" + name + "</title>" +
                           "<meta name=\"created\" content=\"" + date + "\" />" +
                           "</head>" +
                           "</html>";

            var createMessage = new HttpRequestMessage(HttpMethod.Post, PagesEndPoint)
            {
                Content = new StringContent(simpleHtml, System.Text.Encoding.UTF8, "text/html")
            };

            HttpResponseMessage response = await client.SendAsync(createMessage);

            dynamic responseObject = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            var editUrl = responseObject["links"]["oneNoteWebUrl"]["href"].ToString();

            return editUrl;
        }

        private static string SanitizeName(string name)
        {
            name = name.Replace("&", String.Empty); // Strip unsafe html characters.
            var parts = name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return String.Join(" ", parts);
        }

        private static async Task<List<PageObject>> GetPagesInSection(string token)
        {
            var pages = new List<PageObject>();
            var client = new HttpClient();

            // Note: API only supports JSON return type.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(PagesEndPoint);
            dynamic responseObject = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            foreach (var pageObject in responseObject.value)
            {
                pages.Add(new PageObject()
                {
                    Id = (string)pageObject["id"],
                    Title = (string)pageObject["title"],
                    EditUrl = (string)pageObject["links"]["oneNoteWebUrl"]["href"]
                });
            }

            return pages;
        }

        private static async Task CreateNotebookIfNotExists()
        {
            string autoCadNotebookSectionsUrl = await GetAutoCadNotebookSectionsUrlForUser(AuthManager.Token);
            if (autoCadNotebookSectionsUrl != null)
            {
                SectionsEndPoint = autoCadNotebookSectionsUrl;
            }
            else
            {
                string createdNotebookSectionsUrl = await CreateNotebookInOneNote(AuthManager.Token);
                if (!string.IsNullOrEmpty(createdNotebookSectionsUrl))
                {
                    SectionsEndPoint = createdNotebookSectionsUrl;
                }
            }
        }

        private static async Task<string> CreateNotebookInOneNote(string token)
        {
            var client = new HttpClient();

            // Note: API only supports JSON return type.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = String.Format("{{ name: \"{0}\"}}", AutoCADNotebook);
            var createMessage = new HttpRequestMessage(HttpMethod.Post, NotebookEndPoint)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(createMessage);

            var notebookResponseJson = JToken.Parse(await response.Content.ReadAsStringAsync());

            // Get the SectionsUrl property of created notebook that has notebook id within it.
            return (string)notebookResponseJson["sectionsUrl"];
        }

        private static async Task<string> GetAutoCadNotebookSectionsUrlForUser(string token)
        {
            var client = new HttpClient();

            // Note: API only supports JSON return type.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(NotebookEndPoint);
            dynamic responseObject = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            string autoCadNotebookSectionsUrl = null;
            foreach (var notebookObject in responseObject.value)
            {
                if (((string)notebookObject["name"]).Equals(AutoCADNotebook, StringComparison.InvariantCultureIgnoreCase))
                {
                    autoCadNotebookSectionsUrl = (string)notebookObject["sectionsUrl"];
                }
            }

            return autoCadNotebookSectionsUrl;
        }

        public static async Task CreateSectionIfNotExists()
        {
            string autocadSectionPagesUrl = await GetAutoCadSectionPagesUrlForNotebook(AuthManager.Token);
            if (!string.IsNullOrEmpty(autocadSectionPagesUrl))
            {
                PagesEndPoint = autocadSectionPagesUrl;
            }
            else
            {
                string createdSectionPagesUrl = await CreateSectionInOneNote(AuthManager.Token);
                if (!string.IsNullOrEmpty(createdSectionPagesUrl))
                {
                    PagesEndPoint = createdSectionPagesUrl;
                }
            }
        }

        private static async Task<string> CreateSectionInOneNote(string token)
        {
            var client = new HttpClient();

            // Note: API only supports JSON return type.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = String.Format("{{ name: \"{0}\"}}", AutoCADNotebookSection);
            var createMessage = new HttpRequestMessage(HttpMethod.Post, SectionsEndPoint)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(createMessage);

            var notebookResponseJson = JToken.Parse(await response.Content.ReadAsStringAsync());

            // Get the PagesUrl property of created section that has section id within it.
            return (string)notebookResponseJson["pagesUrl"];
        }

        public static async Task<string> GetAutoCadSectionPagesUrlForNotebook(string token)
        {
            var client = new HttpClient();

            // Note: API only supports JSON return type.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(SectionsEndPoint);
            dynamic responseObject = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            string autoCadSectionPagesUrl = null;
            foreach (var sectionObject in responseObject.value)
            {
                if (((string)sectionObject["name"]).Equals(AutoCADNotebookSection, StringComparison.InvariantCultureIgnoreCase))
                {
                    autoCadSectionPagesUrl = (string)sectionObject["pagesUrl"];
                }
            }

            return autoCadSectionPagesUrl;
        }
    }

    public class PageObject
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public string EditUrl { get; set; }
    }
}
