using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using RealtimeDatabase.Attributes;

namespace WebUI.Data.Models
{
    [CreateEvent("OnCreate")]
    public class Log : Base
    {
        public DateTime CreatedOn { get; set; }

        public string Message { get; set; }

        public string UserId { get; set; }

        public void OnCreate()
        {
            CreatedOn = DateTime.UtcNow;
        }
    }
}
