using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class UpdateUserCommand : CreateUserCommand
    {
        public string Id { get; set; }
    }
}
