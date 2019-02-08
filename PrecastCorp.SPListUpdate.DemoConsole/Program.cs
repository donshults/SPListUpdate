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
using System.Net;
using PrecastCorp.CalendarSupport.Service.Models;

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

        private static AuthenticationContext authContext = new AuthenticationContext(authority);
        private static ClientCredential clientCredential = new ClientCredential(clientId, appKey);

        [STAThread]
        static void Main()
        {
            Console.WriteLine("Press enter to start..");
            Console.ReadLine();

            var result = GetAuthResult(authContext);
            // HttpStatusCode code =  MakeHttpsGetCall(result, "api/calendar?siteUrl=https://tektaniuminc.sharepoint.com/sites/spdev&listName=DemoVacationCalendar").Result;
            CalendarItemModel calItem = new CalendarItemModel();
            calItem.Category = "Special";
            calItem.Description = "Demo Vacation Schedule";
            calItem.EventDate = DateTime.Parse("02/10/2019 08:00:00AM");
            calItem.EndDate = DateTime.Parse("02/10/2019 11:59:00PM");
            calItem.Location = "Portland";
            calItem.ID = 0;
            calItem.ListName = "DemoVacationCalendar";
            calItem.SiteUrl = "https://tektaniuminc.sharepoint.com/sites/spdev";
            calItem.Title = "Vacation ID " + DateTime.Now.Minute.ToString();

            HttpStatusCode code = MakeHttpsPostCall(result, "api/calendar?siteUrl=https://tektaniuminc.sharepoint.com/sites/spdev&listName=DemoVacationCalendar", calItem).Result;


            Console.WriteLine("Press enter to End..");
            Console.ReadLine();

        }

        private static AuthenticationResult GetAuthResult(AuthenticationContext authContext)
        {
            AuthenticationResult result = null;

            int retryCount = 0;
            bool retry = false;
            do
            {
                retry = false;
                try
                {
                    result = authContext.AcquireTokenAsync(serviceResourceId, clientCredential).Result;         
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
                return  null;
            }
            Console.WriteLine("Authentication Successful... making HTTPS call");
            return result;
        }
        

        private static async Task<HttpStatusCode> MakeHttpsGetCall(AuthenticationResult result, string url)
        {
            string serviceBaseAddress = "https://localhost:44355/";
            string webApiUrl = serviceBaseAddress + url;
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            HttpResponseMessage response = await httpClient.GetAsync(webApiUrl);

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
            return response.StatusCode;   
        }


        private static async Task<HttpStatusCode> MakeHttpsPostCall(AuthenticationResult result, string url, CalendarItemModel callItem )
        {
            string serviceBaseAddress = "https://localhost:44355/";
            string webApiUrl = serviceBaseAddress + url;

            using (var client = new HttpClient())
                using(var request = new HttpRequestMessage(HttpMethod.Post,webApiUrl))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                var json = JsonConvert.SerializeObject(callItem);
                using(var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;
                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        return response.StatusCode;
                    }
                }

            }
        }

    }
}
