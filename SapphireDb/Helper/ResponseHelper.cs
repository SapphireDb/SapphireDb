using System;
using System.Collections.Generic;
using System.Text;
using SapphireDb.Command;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    static class ResponseHelper
    {
        public static ResponseBase CreateExceptionResponse<T>(this CommandBase command, Exception exception)
            where T : ResponseBase
        {
            T response = Activator.CreateInstance<T>();
            
            response.ReferenceId = command.ReferenceId;
            response.Error = new SapphireDbError(exception);
            
            return response;
        }
    }
}
