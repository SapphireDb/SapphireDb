using System;
using System.Collections.Generic;
using System.Text;
using SapphireDb.Command;

namespace SapphireDb.Helper
{
    static class ResponseHelper
    {
        public static ResponseBase CreateExceptionResponse<T>(this CommandBase command, Exception exception)
            where T : ResponseBase
        {
            T response = Activator.CreateInstance<T>();

            response.ReferenceId = command.ReferenceId;
            response.Error = exception;

            return response;
        }

        public static ResponseBase CreateExceptionResponse<T>(this CommandBase command, string exceptionMessage)
            where T : ResponseBase
        {
            return command.CreateExceptionResponse<T>(new Exception(exceptionMessage));
        }
    }
}
