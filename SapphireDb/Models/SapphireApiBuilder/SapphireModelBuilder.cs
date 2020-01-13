using System;
using System.Linq.Expressions;
using SapphireDb.Attributes;
using SapphireDb.Helper;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireModelBuilder<T>
    {
        private readonly ModelAttributesInfo attributesInfo;

        public SapphireModelBuilder()
        {
            attributesInfo = typeof(T).GetModelAttributesInfo();
        }

        public SapphireModelBuilder<T> SetQueryFunction(Func<HttpInformation, Expression<Func<T, bool>>> expressionBuilder)
        {
            if (attributesInfo.QueryFunctionAttribute == null)
            {
                attributesInfo.QueryFunctionAttribute = new QueryFunctionAttribute(null);
            }
            
            attributesInfo.QueryFunctionAttribute.FunctionBuilder = expressionBuilder;
            
            return this;
        }
    }
}