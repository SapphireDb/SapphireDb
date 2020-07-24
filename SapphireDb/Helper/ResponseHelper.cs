using System;
using SapphireDb.Command;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Helper
{
    static class ResponseHelper
    {
        public static ResponseBase CreateExceptionResponse<T>(this CommandBase command, SapphireDbException exception)
            where T : ResponseBase
        {
            T response = Activator.CreateInstance<T>();
            
            response.ReferenceId = command.ReferenceId;
            response.Error = new SapphireDbErrorResponse(exception);
            
            return response;
        }
    }
}
