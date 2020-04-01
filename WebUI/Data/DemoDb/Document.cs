using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;
using SapphireDb.Models;
using SapphireDb.Models.SapphireApiBuilder;

namespace WebUI.Data.DemoDb
{
    public class Document : SapphireOfflineEntity
    {
        public string Name { get; set; }

        // [MergeConflictResolutionMode(MergeConflictResolutionMode.ConflictMarkers)]
        [Updatable]
        public string Content { get; set; }
    }

    public class DocumentConfiguration : ISapphireModelConfiguration<Document>
    {
        public void Configure(SapphireModelBuilder<Document> modelBuilder)
        {
            modelBuilder.Property(m => m.Content)
                .SetMergeConflictResolutionMode(MergeConflictResolutionMode.ConflictMarkers);
        }
    }

}
