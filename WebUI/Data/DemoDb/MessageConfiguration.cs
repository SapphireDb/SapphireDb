using SapphireDb.Models.SapphireApiBuilder;

namespace WebUI.Data.DemoDb
{
    public class MessageConfiguration : ISapphireModelConfiguration<Message>
    {
        public void Configure(SapphireModelBuilder<Message> modelBuilder)
        {
            modelBuilder.SetQueryFunction(information => { return message => message.Content == "test123"; });
        }
    }
}