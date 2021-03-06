﻿using PrecastCorp.CalendarSupport.Service;
using PrecastCorp.CalendarSupport.Service.Entities;
using PrecastCorp.CalendarSupport.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;

namespace Precastcorp.SPListUpdate.Framework.API.Models
{
    public class ModelFactory
    {
        private UrlHelper _urlHelper;
        private ICalendarSupport calendarSupport;

        public ModelFactory(HttpRequestMessage request, ICalendarSupport repo)
        {
            _urlHelper = new UrlHelper(request);
            calendarSupport = repo;
        }

        public CalendarItemModel Create(CalendarItem calendarItem)
        {
            return new CalendarItemModel
            {
                Url = _urlHelper.Link("Calendar", new { siteUrl = calendarItem.SiteUrl, listName = calendarItem.ListName, id = calendarItem.ID }),
                //ID = calendarItem.ID,
                Title = calendarItem.Title,
                Description = calendarItem.Description,
                EventDate = ConvertToLocalTime(calendarItem.EventDate, calendarItem.LocalTZ),
                EndDate = ConvertToLocalTime(calendarItem.EndDate, calendarItem.LocalTZ),
                FileRef = calendarItem.FileRef,
                FileDirRef = calendarItem.FileDirRef,
                Location = calendarItem.Location,
                Created = System.TimeZone.CurrentTimeZone.ToLocalTime(calendarItem.Created),
                Author = CreateUser(calendarItem.Author),
                Editor = CreateUser(calendarItem.Editor),
                SiteUrl = calendarItem.SiteUrl,
                ListName = calendarItem.ListName,
                Category = calendarItem.Category,
                //LocalTZ = calendarItem.LocalTZ

            };
        }
        private DateTime ConvertToLocalTime(DateTime eventDate, TimeZoneInfo localTZ)
        {
            DateTime localTime = DateTime.MinValue;
            try
            {
                localTime = TimeZoneInfo.ConvertTimeFromUtc(eventDate, localTZ);
                return localTime;
            }
            catch
            {
                return localTime;
            }


        }

        public UserModel CreateUser(UserModel user)
        {
            return new UserModel
            {
                Email = user.Email,
                LookupId = user.LookupId,
                LookupValue = user.LookupValue,
                LoginName = user.LoginName,
                IsAdmin = user.IsAdmin
            };

        }

        public CalendarItem Parse(CalendarItemModel calEntry)
        {
            CalendarItem entry = new CalendarItem();
            entry.Author = new UserModel();
            entry.Editor = new UserModel();
            try
            {
                entry.ID = calEntry.ID;
                if (calEntry.Author != null)
                {
                    //entry.Author.LookupId = calEntry.Author.LookupId;
                    entry.Author = calEntry.Author;
                }
                entry.Description = calEntry.Description;
                if(calEntry.Editor != null)
                {
                    //entry.Editor.LookupId = calEntry.Editor.LookupId;
                    entry.Editor = calEntry.Editor;
                }
                entry.EndDate = TimeZoneInfo.ConvertTimeToUtc(calEntry.EndDate);
                entry.EventDate = TimeZoneInfo.ConvertTimeToUtc(calEntry.EventDate);
                entry.Location = calEntry.Location;
                entry.Title = calEntry.Title;
                entry.Category = calEntry.Category;    
                entry.SiteUrl = calEntry.SiteUrl;
                entry.ListName = calEntry.ListName;
                return entry;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}