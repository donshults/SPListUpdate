using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PrecastCorp.SPListUpdate.DemoConsole
{
    static class Program
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:aadInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:tenant"];
        private static string serviceResourceId = ConfigurationManager.AppSettings["ida:serviceResourceID"];
        private static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);



        [STAThread]
        static void Main()
        {
            string serviceBaseAddress = "https://localhost:44355/";
            Console.WriteLine("Press enter to start..");
            Console.ReadLine();

            string webApiUrl = serviceBaseAddress + "api/values";
            CallWebApiProtectedAsync(webApiUrl, authority, clientId, appKey).Wait();
            Console.WriteLine("Press enter to End..");
            Console.ReadLine();

        }

        static async Task CallWebApiProtectedAsync(string webApiUrl, string authority, string clientId, string clientSecret)
        {
            var parameters = new PlatformParameters(PromptBehavior.Always);
             authority = authority + "/oauth2/authorize?resource="+serviceResourceId; // Authorization endpoint
            authority = "https://login.windows.net/common/oauth2/authorize?resource=https://tektanium.com/Precastcorp.SPListUpdate.Framework.API";
            //string authority = "https://login.windows.net/b29343ba-***/oauth2/token"; //token endpoint
            string resource = "f2cc175f-6c6e-4acf-829b-9a44721b9751";  //Web API Client Id (mgmtADApp)
            string redirectUri = "https://localhost:44443/.auth/login/done";
            ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);

            try
            {
                AuthenticationContext authContext = new AuthenticationContext(authority);
                //var token = await authContext.AcquireTokenAsync(resource, clientId, new Uri(redirectUri), parameters);
                var token = await authContext.AcquireTokenAsync(resource, clientCredential);
                var authHeader = token.CreateAuthorizationHeader();
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token.AccessTokenType, token.AccessToken);
                Uri requestURI = new Uri(webApiUrl);
                Console.WriteLine($"Reading values from '{requestURI}'.");
                HttpResponseMessage httpResponse = await client.GetAsync(requestURI);
                Console.WriteLine($"HTTP Status Code: '{httpResponse.StatusCode.ToString()}'");
                Console.WriteLine($"HTTP Response: '{httpResponse.ToString()}'");
                string responseString = await httpResponse.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject(responseString);
                Console.WriteLine($"JSON Response: {json}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in CallWebApiUnprotectedAsync(): " + ex.Message);
            }
        }

      /*  private static async Task GetAuthResult()
        {
            Task<AuthenticationResult> result = null;
            AuthenticationContext authContext = new AuthenticationContext(authority);
            ClientCredential clientCredential = new ClientCredential(clientId, appKey);
            int retryCount = 0;
            bool retry = false;
            do
            {
                retry = false;
                try
                {
                    Task<AuthenticationResult> result = await authContext.AcquireTokenAsync(serviceResourceId, clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }
                }
            } while ((retry == true) && (retryCount < 3));

            if (result == null)
            {
                Console.WriteLine("Cancelling attempt ..");
                return  ;
            }

            return Task<AuthenticationResult>result;
        }
        */

 /*       private static async Task MakeHttpsCall(AuthenticationResult result)
        {
            string serviceBaseAddress = "https://localhost:44355/";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            HttpResponseMessage response = await httpClient.GetAsync(serviceBaseAddress + "api/calendar/timezones");

            if (response.IsSuccessStatusCode)
            {
                string r = await response.Content.ReadAsStringAsync();
                Console.WriteLine(r);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    authContext.TokenCache.Clear();
                }
                Console.WriteLine("Access Denied!");
            }
        }
        */
    }
}
