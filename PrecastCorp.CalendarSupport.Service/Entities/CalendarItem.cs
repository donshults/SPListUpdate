using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecastCorp.CalendarSupport.Service.Entities
{
    public class CalendarItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public Models.UserModel Author { get; set; }
        public Models.UserModel Editor { get; set; }
        public string FileRef { get; set; }
        public string FileDirRef { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string SiteUrl { get; set; }
        public string ListName { get; set; }
        public string Category { get; set; }
        public TimeZoneInfo LocalTZ { get; set; }
    }
}
