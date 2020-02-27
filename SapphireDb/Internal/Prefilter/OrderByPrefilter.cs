using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SapphireDb.Internal.Prefilter
{
    public class OrderByPrefilter : IPrefilter
    {
        public string Property { get; set; }

        protected Expression<Func<object, object>> PropertySelectExpression { get; set; }

        public bool Descending { get; set; }

        public virtual IQueryable<object> Execute(IQueryable<object> array)
        {
            return Descending
                ? array.OrderByDescending(PropertySelectExpression)
                : array.OrderBy(PropertySelectExpression);
        }

        private bool initialized = false;

        public void Initialize(Type modelType)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            string propertyName = modelType.GetProperty(Property, BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance)?.Name;

            if (!string.IsNullOrEmpty(propertyName))
            {
                ParameterExpression parameter = Expression.Parameter(typeof(object));
                UnaryExpression convertExpression = Expression.Convert(parameter, modelType);

                MemberExpression body = Expression.PropertyOrField(convertExpression, propertyName);
                UnaryExpression bodyConverted = Expression.Convert(body, typeof(object));
                PropertySelectExpression = Expression.Lambda<Func<object, object>>(bodyConverted, parameter);
            }
        }

        public void Dispose()
        {
            
        }
        
        public string Hash()
        {
            return $"OrderByPrefilter,{Property},{Descending}";
        }
    }
}
