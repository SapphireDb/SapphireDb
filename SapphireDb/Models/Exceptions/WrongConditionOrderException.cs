using Newtonsoft.Json.Linq;

namespace SapphireDb.Models.Exceptions
{
    public class WrongConditionOrderException : SapphireDbException
    {
        public JToken ConditionParts { get; }

        public WrongConditionOrderException(JToken conditionParts) : base("Wrong order of conditions")
        {
            ConditionParts = conditionParts;
        }
    }
}