using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class UpdateRoleCommand : CommandBase
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
