using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JavaScriptEngineSwitcher.Core;
using Newtonsoft.Json.Linq;
using SapphireDb.Helper;

namespace SapphireDb.Internal.Prefilter
{
    public class WherePrefilter : IPrefilter
    {
        public JToken Conditions { get; set; }

        public Expression<Func<object, bool>> WhereExpression { get; set; }

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return array.Where(WhereExpression);
        }

        private bool initialized = false;

        public void Initialize(Type modelType)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            ParameterExpression parameter = Expression.Parameter(typeof(object));
            UnaryExpression modelExpression = Expression.Convert(parameter, modelType);

            //Expression t = ExpressionHelper.CreateCompareExpression(modelType, Conditions.First().Value<JArray>(), modelExpression);
            Expression t = ConvertConditionParts(modelType, Conditions, modelExpression);

            WhereExpression = Expression.Lambda<Func<object, bool>>(t, parameter);
        }

        private Expression ConvertConditionParts(Type modelType, JToken conditionParts, Expression modelExpression)
        {
            if (conditionParts.Type == JTokenType.Array)
            {
                if (conditionParts.First().Type == JTokenType.Array)
                {
                    Expression completeExpression = null;
                    Expression prevExpression = null;

                    foreach (JToken combineOperator in conditionParts.Where(t => t.Type == JTokenType.String))
                    {
                        if (prevExpression == null)
                        {
                            prevExpression = ConvertConditionParts(modelType, combineOperator.Previous, modelExpression);
                        }
                        else
                        {
                            prevExpression = completeExpression;
                        }

                        Expression nextExpression = ConvertConditionParts(modelType, combineOperator.Next, modelExpression);

                        string operatorValue = combineOperator.Value<string>();

                        if (operatorValue == "and")
                        {
                            completeExpression = Expression.AndAlso(prevExpression, nextExpression);
                        }
                        else
                        {
                            completeExpression = Expression.OrElse(prevExpression, nextExpression);
                        }
                    }

                    return completeExpression;
                }
                else
                {
                    return ExpressionHelper.CreateCompareExpression(modelType, conditionParts.Value<JArray>(), modelExpression);
                }
            }

            throw new Exception("Wrong order of conditions");
        }

        public void Dispose()
        {
            
        }
    }
}
