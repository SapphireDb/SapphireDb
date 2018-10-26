using RealtimeDatabase.Models.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Internal
{
    class ActionHandlerAccesor
    {
        private readonly IServiceProvider serviceProvider;

        public ActionHandlerAccesor(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
        }

        public ActionHandlerBase GetActionHandler(Type type)
        {
            return (ActionHandlerBase)serviceProvider.GetService(type);
        }
    }
}
