using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealtimeDatabase.Helper
{
    static class ValidationHelper
    {
        public static bool ValidateModel(object model, IServiceProvider serviceProvider, out Dictionary<string, List<string>> validationResults)
        {
            validationResults = new Dictionary<string, List<string>>();

            ValidationContext validationContext = new ValidationContext(model, serviceProvider, null);
            List<ValidationResult> validationResultsTemp = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, validationContext, validationResultsTemp, true))
            {
                foreach (ValidationResult vr in validationResultsTemp)
                {
                    foreach (string memberName in vr.MemberNames)
                    {
                        string memberNameCamel = memberName.ToCamelCase();

                        if (validationResults.ContainsKey(memberNameCamel))
                        {
                            validationResults[memberNameCamel].Add(vr.ErrorMessage);
                        } else
                        {
                            validationResults.Add(memberNameCamel, new List<string> { vr.ErrorMessage });
                        }
                    }
                }

                return false;
            }

            return true;
        }
    }
}
