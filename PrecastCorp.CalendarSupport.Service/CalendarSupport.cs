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
using PrecastCorp.CalendarSupport.Service.Models;

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
            string realm = ConfigurationManager.AppSettings["ida:Audience"];
            string appId = ConfigurationManager.AppSettings["ida:ClientId"];
            string appSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            OfficeDevPnP.Core.AuthenticationManager authManager = new OfficeDevPnP.Core.AuthenticationManager();
            var siteUrl = calItem.SiteUrl;
            var listName = calItem.ListName;
            if (listName == null || siteUrl == null)
            {
                return null;
            }
            using (ClientContext context = authManager.GetAppOnlyAuthenticatedContext(siteUrl, appId, appSecret))
            {
                try
                {
                    List oList = context.Web.Lists.GetByTitle(listName);
                    ListItemCreationInformation oItemCreateInfo = new ListItemCreationInformation();
                    ListItem oItem = oList.AddItem(oItemCreateInfo);
                    oItem["Description"] = calItem.Description;
                    oItem["Location"] = calItem.Location;
                    oItem["EventDate"] = calItem.EventDate;
                    oItem["EndDate"] = calItem.EndDate;
                    oItem["Title"] = calItem.Title;
                    oItem.Update();
                    context.ExecuteQuery();
                    calItem.ID = oItem.Id;
                    return calItem;
                }
                catch (Exception ex)
                {

                    return null;

                }
            }

        }

        public bool DeleteCalendarItemById(string siteUrl, string listName, int itemId)
        {
            string realm = ConfigurationManager.AppSettings["ida:Audience"];
            string appId = ConfigurationManager.AppSettings["ida:ClientId"];
            string appSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            bool success = false;

            OfficeDevPnP.Core.AuthenticationManager authManager = new OfficeDevPnP.Core.AuthenticationManager();
            using (ClientContext context = authManager.GetAppOnlyAuthenticatedContext(siteUrl, appId, appSecret))
            {
                try
                {
                    List oList = context.Web.Lists.GetByTitle(listName);
                    ListItem oItem = oList.GetItemById(itemId);
                    oItem.DeleteObject();
                    context.ExecuteQuery();
                    success = true;
                    return success;
                }
                catch (Exception ex)
                {
                    return success;
                }
            }
        }

        public Task<string> GetAccessTokenAsync()
        {
            throw new NotImplementedException();
        }

        public CalendarItem GetCalendarItemById(string siteUrl, string listName, int itemId)
        {
            string realm = ConfigurationManager.AppSettings["ida:Audience"];
            string appId = ConfigurationManager.AppSettings["ida:ClientId"];
            string appSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            CalendarItem calItem = new CalendarItem();

            OfficeDevPnP.Core.AuthenticationManager authManager = new OfficeDevPnP.Core.AuthenticationManager();
            using (ClientContext context = authManager.GetAppOnlyAuthenticatedContext(siteUrl, appId, appSecret))
            {
                try
                {
                    List oList = context.Web.Lists.GetByTitle(listName);
                    ListItem oItem = oList.GetItemById(itemId);
                    context.Load(oItem);
                    context.ExecuteQuery();

                    calItem.Title = oItem["Title"].ToString();
                    calItem.ID = oItem.Id;
                    calItem.Description = oItem["Description"].ToString();
                    calItem.Created = DateTime.Parse(oItem["Created"].ToString());
                    calItem.EndDate = DateTime.Parse(oItem["EndDate"].ToString());
                    calItem.EventDate = DateTime.Parse(oItem["EventDate"].ToString());
                    calItem.FileDirRef = oItem["FileDirRef"].ToString();
                    calItem.FileRef = oItem["FileRef"].ToString();
                    calItem.Location = oItem["Location"].ToString();
                    calItem.Modified = DateTime.Parse(oItem["Modified"].ToString());
                    FieldUserValue itemAuthor = oItem["Author"] as FieldUserValue;
                    var author = new Models.UserModel();
                    author.Email = itemAuthor.Email;
                    author.LookupId = itemAuthor.LookupId;
                    author.LookupValue = itemAuthor.LookupValue;
                    calItem.Author = author;
                    FieldUserValue itemEditor = oItem["Editor"] as FieldUserValue;
                    var editor = new Models.UserModel();
                    editor.Email = itemEditor.Email;
                    editor.LookupId = itemEditor.LookupId;
                    editor.LookupValue = itemEditor.LookupValue;
                    calItem.Editor = editor;
                    calItem.SiteUrl = siteUrl;
                    calItem.ListName = listName;
                    return calItem;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
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
            string realm = ConfigurationManager.AppSettings["ida:Audience"];
            string appId = ConfigurationManager.AppSettings["ida:ClientId"];
            string appSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            OfficeDevPnP.Core.AuthenticationManager authManager = new OfficeDevPnP.Core.AuthenticationManager();
            if (calItem.SiteUrl == null || calItem.ListName == null)
            {
                return null;
            }
            var listName = calItem.ListName;
            var siteUrl = calItem.SiteUrl;

            using (ClientContext context = authManager.GetAppOnlyAuthenticatedContext(siteUrl, appId, appSecret))
            {
                try
                {
                    List oList = context.Web.Lists.GetByTitle(listName);
                    ListItemCreationInformation oItemCreateInfo = new ListItemCreationInformation();
                    ListItem oItem = oList.GetItemById(calItem.ID);

                    oItem["Description"] = calItem.Description;
                    oItem["Location"] = calItem.Location;
                    oItem["EventDate"] = calItem.EventDate;
                    oItem["EndDate"] = calItem.EndDate;
                    oItem["Title"] = calItem.Title;
                    oItem["Author"] = calItem.Author;
                    oItem["Editor"] = calItem.Editor;
                    oItem.Update();
                    context.ExecuteQuery();
                    return calItem;
                }
                catch (Exception)
                {
                    return null;

                }
            }
        }


    }
}
