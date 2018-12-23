using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class CreateRoleCommand : CommandBase
    {
        public string Name { get; set; }
    }
}
