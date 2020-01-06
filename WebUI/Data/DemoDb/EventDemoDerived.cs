using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    [CreateEvent(After = nameof(OnCreated))]
    public class EventDemoDerived : EventDemo
    {
        public string Content2 { get; set; }
        
        private void OnCreated(DemoContext demoContext)
        {
            demoContext.Logs.Add(new Log()
            {
                Content = $"Created Event Demo Derived {Content}"
            });
            demoContext.SaveChanges();
        }
    }
}