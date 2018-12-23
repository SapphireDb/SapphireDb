using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class DeleteUserCommand : CommandBase
    {
        public string Id { get; set; }
    }
}
