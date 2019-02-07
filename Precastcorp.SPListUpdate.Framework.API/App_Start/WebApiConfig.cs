using PrecastCorp.CalendarSupport.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Unity;
using Unity.Lifetime;

namespace Precastcorp.SPListUpdate.Framework.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var container = new UnityContainer();
            container.RegisterType<ICalendarSupport, CalendarSupport>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);


            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
