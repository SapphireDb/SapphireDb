using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SapphireDb.Helper;

namespace SapphireDb.Internal.Prefilter
{
    public class SelectPrefilter : IAfterQueryPrefilter
    {
        public List<string> Properties { get; set; }

        public Expression<Func<object, object>> SelectExpression { get; set; }

        public object Execute(IQueryable<object> array)
        {
            return array.Select(SelectExpression);
        }

        private bool initialized;

        public void Initialize(Type modelType)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            ParameterExpression parameter = Expression.Parameter(typeof(object));
            UnaryExpression modelExpression = Expression.Convert(parameter, modelType);

            Expression body;

            if (Properties.Count == 1)
            {
                string propertyName = modelType.GetProperty(Properties[0],
                    BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase |
                    BindingFlags.Instance)?.Name;

                if (string.IsNullOrEmpty(propertyName))
                {
                    body = Expression.Constant(null);
                }
                else
                {
                    body = Expression.PropertyOrField(modelExpression, propertyName);
                }
            }
            else
            {
                IEnumerable<UnaryExpression> propertyExpressions = Properties
                    .Select(propertyName => modelType.GetProperty(propertyName,
                        BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase |
                        BindingFlags.Instance))
                    .Where(property => property != null)
                    .Select((property, index) =>
                    {
                        MemberExpression propertyExpression = Expression.PropertyOrField(modelExpression, property.Name);
                        return Expression.Convert(propertyExpression, typeof(object));
                    });

                body = Expression.NewArrayInit(typeof(object), propertyExpressions);
            }

            SelectExpression = Expression.Lambda<Func<object, object>>(body, parameter);
        }
        
        public void InitializeServer<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression) where TModel : class
        {
            initialized = true;
            
            ParameterExpression parameter = Expression.Parameter(typeof(object));
            UnaryExpression modelExpression = Expression.Convert(parameter, typeof(TModel));
            SubstitutionExpressionVisitor expressionVisitor =
                new SubstitutionExpressionVisitor(expression.Parameters.Single(), modelExpression);
            Expression selectExpression = Expression.Convert(expressionVisitor.Visit(expression.Body), typeof(object));
            
            SelectExpression = Expression.Lambda<Func<object, object>>(selectExpression, parameter);
        }

        public void Dispose()
        {
            
        }
        
        public string Hash()
        {
            return $"SelectPrefilter,{SelectExpression}";
        }
    }
}
