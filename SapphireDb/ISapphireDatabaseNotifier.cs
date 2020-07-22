using SapphireDb.Command.Subscribe;
using System;
using System.Collections.Generic;

namespace SapphireDb
{
    public interface ISapphireDatabaseNotifier
    {
        void HandleChanges(List<ChangeResponse> changes, Type dbContextType);
    }
}
