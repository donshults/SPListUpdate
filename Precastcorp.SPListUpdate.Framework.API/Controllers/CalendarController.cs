using PrecastCorp.CalendarSupport.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Extensions.Configuration;

namespace Precastcorp.SPListUpdate.Framework.API.Controllers
{
    [Route("api/calendar")]
    public class CalendarController : BaseApiController
    {

        [Authorize]
        [Route("api/calendar/timezones")]
        public HttpResponseMessage GetTimeZones()
        {
            var results = TheCalendarSupport.GetTimeZoneInfo();
            return Request.CreateResponse(HttpStatusCode.OK, results);
        }
       /* [Authorize]
        public HttpResponseMessage GetTitle()
        {
            var results = TheCalendarSupport.GetCalendarTitle();
            return Request.CreateResponse(HttpStatusCode.OK, results);
        }
        */

       
        public HttpResponseMessage Get(string siteUrl, string listName, string localTimeZone)
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
    }
}
