using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using PrecastCorp.CalendarSupport.Service.Entities;

namespace PrecastCorp.CalendarSupport.Service
{
    public class CalendarSupport : ICalendarSupport
    {
        private string clientId;
        private string clientSecret;
        private string apiBaseUrl;

        public CalendarSupport()
        {
            clientId = ConfigurationManager.AppSettings["ida:ClientId"];
            clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            apiBaseUrl = ConfigurationManager.AppSettings["ida:ApiBaseUrl"];            
        }

        public CalendarItem AddCalendarItem(CalendarItem calItem)
        {
            throw new NotImplementedException();
        }

        public bool DeleteCalendarItemById(string siteUrl, string listName, int itemId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAccessTokenAsync()
        {
            throw new NotImplementedException();
        }

        public CalendarItem GetCalendarItemById(string siteUrl, string listName, int itemId, string timeZoneId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CalendarItem> GetCalendarItems(string siteUrl, string listName, string timeZoneId)
        {
            List<CalendarItem> calItems = new List<CalendarItem>();
            string _timeZoneId = timeZoneId;

            DateTime todayDate = DateTime.Now;
            string todayString = todayDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            DateTime maxDate = DateTime.Now.AddDays(30);
            string maxString = maxDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

            OfficeDevPnP.Core.AuthenticationManager authManager = new OfficeDevPnP.Core.AuthenticationManager();
            try
            {
                using (ClientContext context = authManager.GetAppOnlyAuthenticatedContext(siteUrl, clientId, clientSecret))
                {
                    try
                    {
                        List oList = context.Web.Lists.GetByTitle(listName);
                        CamlQuery camlQuery = new CamlQuery();
                        camlQuery.ViewXml = "<View><Query><Where><And><Geq><FieldRef Name='EventDate'/><Value IncludeTimeValue='False' Type='DateTime'>" + todayString + "</Value></Geq><Leq><FieldRef Name='EventDate'/><Value IncludeTimeValue='False' Type='DateTime'>" + maxString + "</Value></Leq></And></Where></Query></View>";
                        ListItemCollection listItems = oList.GetItems(camlQuery);
                        context.Load(listItems);
                        context.ExecuteQuery();
                        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);
                        foreach (ListItem oListItem in listItems)
                        {
                            var fields = oListItem.FieldValues;
                            var calItem = new CalendarItem();
                            foreach (var field in fields)
                            {
                                switch (field.Key)
                                {
                                    case "ID":
                                        calItem.ID = Convert.ToInt32(field.Value);
                                        break;
                                    case "Title":
                                        calItem.Title = field.Value.ToString();
                                        break;
                                    case "Description":
                                        calItem.Description = WebUtility.HtmlDecode(field.Value.ToString());
                                        break;
                                    case "FileDirRef":
                                        calItem.FileDirRef = field.Value.ToString();
                                        break;
                                    case "FileRef":
                                        calItem.FileRef = field.Value.ToString();
                                        break;
                                    case "Location":
                                        calItem.Location = field.Value.ToString();
                                        break;
                                    case "Created":
                                        calItem.Created = DateTime.Parse(field.Value.ToString());
                                        break;
                                    case "Modified":
                                        calItem.Modified = DateTime.Parse(field.Value.ToString());
                                        break;
                                    case "EndDate":
                                        calItem.EndDate = DateTime.Parse(field.Value.ToString());
                                        break;
                                    case "EventDate":
                                        calItem.EventDate = DateTime.Parse(field.Value.ToString());
                                        break;
                                    case "Author":
                                        FieldUserValue itemAuthor = field.Value as FieldUserValue;
                                        var author = new Models.UserModel();
                                        author.Email = itemAuthor.Email;
                                        author.LookupId = itemAuthor.LookupId;
                                        author.LookupValue = itemAuthor.LookupValue;
                                        calItem.Author = author;
                                        break;
                                    case "Editor":
                                        FieldUserValue itemEditor = field.Value as FieldUserValue;
                                        var editor = new Models.UserModel();
                                        editor.Email = itemEditor.Email;
                                        editor.LookupId = itemEditor.LookupId;
                                        editor.LookupValue = itemEditor.LookupValue;
                                        calItem.Editor = editor;
                                        break;
                                }
                            }
                            calItem.ListName = listName;
                            calItem.SiteUrl = siteUrl;
                            calItem.LocalTZ = tzi;
                            calItems.Add(calItem);
                        }
                        return calItems;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex1)
            {
                return null;
            }
        }

        public string GetCalendarTitle()
        {
            CalendarItem newCalendarItem = new CalendarItem();
            string siteUrl = "https://tektaniuminc.sharepoint.com/sites/spdev";
            string listName = "DemoVacationCalendar";


            OfficeDevPnP.Core.AuthenticationManager authManager = new OfficeDevPnP.Core.AuthenticationManager();
            using (ClientContext context = authManager.GetAppOnlyAuthenticatedContext(siteUrl, clientId, clientSecret))
            {
                List oList = context.Web.Lists.GetByTitle(listName);
                context.Load(oList);
                context.ExecuteQuery();
                return oList.Title;
            }
        }

        public IReadOnlyCollection<TimeZoneInfo> GetTimeZoneInfo()
        {
            DateTimeFormatInfo dateFormats = CultureInfo.CurrentCulture.DateTimeFormat;
            ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
            return timeZones;
        }

        public CalendarItem UpdateCalendarItem(CalendarItem calItem)
        {
            throw new NotImplementedException();
        }


    }
}
