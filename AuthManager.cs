using Microsoft.Live;
using System;
using System.Threading.Tasks;

namespace AutoCADNote
{
    public class AuthManager
    {
        private static readonly string ClientId = "ENTER_YOUR_MICROSOFT_LIVE_APPLICATION_CLIENT_ID";

        // Define the permission scopes
        private static readonly string[] scopes = new string[] { "wl.signin", "wl.offline_access", "office.onenote_update_by_app" };
        
        // Set up the Live variables
        private static LiveAuthClient authClient;
        
        public static string Token { get; private set; }
        public static string Code { get; private set; }

        internal static async Task<string> GetLoginUrl()
        {
            // Create authClient
            if (authClient == null)
            {
                await Initialize();
            }

            // Get the login url with the right scopes
            return authClient.GetLoginUrl(scopes);
        }

        private static async Task Initialize()
        {
            authClient = new LiveAuthClient(ClientId);
            LiveLoginResult loginResult = await authClient.InitializeAsync(scopes);            
        }

        internal static async Task<string> GetToken(Uri url)
        {
            // store the token and the code
            Code = url.Query.Split('&', '=')[1];
            var session = await authClient.ExchangeAuthCodeAsync(Code);
            Token = session.AccessToken;
            return Token;
        }
    }
}
