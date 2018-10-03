using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Data.ViewModels.Auth
{
    public class RenewViewModel
    {
        public string UserId { get; set; }

        public string RefreshToken { get; set; }
    }
}
