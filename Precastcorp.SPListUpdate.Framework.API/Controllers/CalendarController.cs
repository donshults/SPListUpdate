using PrecastCorp.CalendarSupport.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Extensions.Configuration;
using PrecastCorp.CalendarSupport.Service.Models;

namespace Precastcorp.SPListUpdate.Framework.API.Controllers
{
    [Authorize]
    [Route("api/calendar")]
    public class CalendarController : BaseApiController
    {

        public CalendarController() : base()
        {

        }
        //[HttpGet]
        [Route("api/calendar/timezones")]
        public HttpResponseMessage GetTimeZones()
        {
            var results = TheCalendarSupport.GetTimeZoneInfo();
            return Request.CreateResponse(HttpStatusCode.OK, results);
        }
        
        [HttpGet]
        [Route("api/calendar")]
        public HttpResponseMessage Get(string localTimeZone, string listName, string siteUrl)
        {
                     
            var results = TheCalendarSupport.GetCalendarItems(siteUrl, listName, localTimeZone);

            if (results != null)
            {
                results.ToList().Select(c => TheModelFactory.Create(c));
                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Can't find calendar list");
            }
        }

        [HttpPost]
        [Route("api/calendar")]
        public HttpResponseMessage Post([FromBody] CalendarItemModel model)
        {
            try
            {
                var entity = TheModelFactory.Parse(model);
                if (entity == null)
                    Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not read Calendar Entry");
                //var calendarItem = TheCalendarSupport.GetCalendarItemById(calendarEntryModel.SiteUrl, calendarEntryModel.ListName, entity.Id);
                //if (calendarItem == null) Request.CreateResponse(HttpStatusCode.NotFound);
                var results = TheCalendarSupport.AddCalendarItem(entity);
                if (results == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Data to Post");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Created, results);
                }

            }
            catch (Exception ex)
            {

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        
        public HttpResponseMessage Delete([FromBody] CalendarItemModel model)
        {
            try
            {
                var exist = TheCalendarSupport.GetCalendarItemById(model.SiteUrl, model.ListName, model.ID);
                var deleteRequest = false;
                if (exist == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                deleteRequest = TheCalendarSupport.DeleteCalendarItemById(model.SiteUrl, model.ListName, model.ID);

                if (deleteRequest)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }

        }
        
        public HttpResponseMessage Put([FromBody] CalendarItemModel model)
        {
            try
            {
                var entity = TheModelFactory.Parse(model);
                if (entity == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Data to Post");
                }
                else
                {
                    var results = TheCalendarSupport.UpdateCalendarItem(entity);
                    if (results == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Data to Post");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Created, results);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        } 
    }


    public class RequestItem
    {
        public string SiteUrl { get; set; }
        public string ListName { get; set; }
        public string LocalTimeZone { get; set; }
    }
}
