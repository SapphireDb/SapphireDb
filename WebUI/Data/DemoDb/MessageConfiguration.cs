using System;
using SapphireDb.Models.SapphireApiBuilder;

namespace WebUI.Data.DemoDb
{
    public class MessageConfiguration : ISapphireModelConfiguration<Message>
    {
        public void Configure(SapphireModelBuilder<Message> modelBuilder)
        {
            // modelBuilder.SetQueryFunction(information => { return message => message.Content == "test123"; });
            // modelBuilder.AddQueryAuth("requireAdmin");

            modelBuilder.AddCreateEvent(before: (message, information) =>
            {
                Console.WriteLine(message.Content.ToString());
            });

            // modelBuilder.Property(m => m.CreatedOn).AddQueryAuth("requireAdmin");
            // modelBuilder.Property(m => m.Content).MakeNonCreatable();
        }
    }
}