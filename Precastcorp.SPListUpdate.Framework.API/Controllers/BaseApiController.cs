using Precastcorp.SPListUpdate.Framework.API.Models;
using PrecastCorp.CalendarSupport.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Precastcorp.SPListUpdate.Framework.API.Controllers
{
    public abstract class BaseApiController : ApiController
    {
        private CalendarSupport calendarSupport;
        private ModelFactory modelFactory;

        public BaseApiController()
        {
            calendarSupport = new CalendarSupport();   
        }

        protected ModelFactory TheModelFactory
        {
            get
            {
                if (modelFactory == null)
                {
                    modelFactory = new ModelFactory(this.Request);
                }
                return modelFactory;
            }
        }

        protected CalendarSupport TheCalendarSupport
        {
            get
            {
                return calendarSupport;
            }
        }
    }
}
