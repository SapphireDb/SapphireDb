using System;
using System.ComponentModel.DataAnnotations;
using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    [CreateEvent(Before = nameof(OnCreate), After = nameof(OnCreated))]
    [UpdateEvent(Before = nameof(OnUpdate), After = nameof(OnUpdated))]
    [DeleteEvent(After = nameof(OnRemoved))]
    public class EventDemo
    {
        [Key]
        public Guid Id { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        [Updateable]
        public string Content { get; set; }

        private void OnCreate()
        {
            CreatedOn = DateTimeOffset.Now;
        }
        
        private void OnCreated(DemoContext demoContext)
        {
            demoContext.Logs.Add(new Log()
            {
                Content = $"Created {Content}"
            });
            demoContext.SaveChanges();
        }

        private void OnUpdate()
        {
            UpdatedOn = DateTimeOffset.Now;
        }
        
        private void OnUpdated(DemoContext demoContext)
        {
            demoContext.Logs.Add(new Log()
            {
                Content = $"Updated {Content}"
            });
            demoContext.SaveChanges();
        }
        
        private void OnRemoved(DemoContext demoContext)
        {
            demoContext.Logs.Add(new Log()
            {
                Content = $"Removed {Content}"
            });
            demoContext.SaveChanges();
        }
    }
}