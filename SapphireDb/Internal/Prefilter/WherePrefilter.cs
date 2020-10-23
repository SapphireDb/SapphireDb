using Newtonsoft.Json.Linq;
using SapphireDb.Helper;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SapphireDb.Internal.Prefilter
{
    public class WherePrefilter : IPrefilter
    {
        public JToken Conditions { get; set; }

        public Expression<Func<object, bool>> WhereExpression { get; set; }

        public Func<object, bool> WhereExpressionCompiled { get; set; }
        
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
            Expression whereConditionBody = ExpressionHelper.ConvertConditionParts(modelType, Conditions, modelExpression);

            WhereExpression = Expression.Lambda<Func<object, bool>>(whereConditionBody, parameter);
            WhereExpressionCompiled = WhereExpression.Compile();
        }
        
        public void InitializeServer<TModel>(Expression<Func<TModel, bool>> expression) where TModel : class
        {
            initialized = true;
            
            ParameterExpression parameter = Expression.Parameter(typeof(object));
            UnaryExpression modelExpression = Expression.Convert(parameter, typeof(TModel));
            SubstitutionExpressionVisitor expressionVisitor =
                new SubstitutionExpressionVisitor(expression.Parameters.Single(), modelExpression);
            Expression whereCondition = expressionVisitor.Visit(expression.Body);
            
            WhereExpression = Expression.Lambda<Func<object, bool>>(whereCondition, parameter);
            WhereExpressionCompiled = WhereExpression.Compile();
        }
        
        public void Dispose()
        {
            
        }
        
        public string Hash()
        {
            return $"WherePrefilter,{WhereExpression}";
        }
    }
}
