using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;
using SapphireDb.Models;

namespace WebUI.Data.DemoDb
{
    public class Document : SapphireOfflineEntity
    {
        public string Name { get; set; }

        [Updatable]
        public string Content { get; set; }
    }
}
