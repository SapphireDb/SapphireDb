using System;
using System.Linq;
using System.Reflection;
using SapphireDb.Attributes;
using SapphireDb.Helper;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphirePropertyBuilder<TModel, TProperty> where TModel : class
    {
        private readonly PropertyAttributesInfo attributesInfo;

        public SapphirePropertyBuilder(PropertyInfo propertyInfo)
        {
            attributesInfo = typeof(TModel).GetPropertyAttributesInfos()
                .FirstOrDefault(property => property.PropertyInfo == propertyInfo);
        }

        public SapphirePropertyBuilder<TModel, TProperty> MakeUpdateable()
        {
            attributesInfo.UpdateableAttribute = new UpdateableAttribute();
            return this;
        }

        public SapphirePropertyBuilder<TModel, TProperty> MakeNonCreatable()
        {
            attributesInfo.NonCreatableAttribute = new NonCreatableAttribute();
            return this;
        }

        public SapphirePropertyBuilder<TModel, TProperty> AddQueryAuth(string policies = null,
            Func<HttpInformation, TModel, bool> function = null)
        {
            attributesInfo.QueryAuthAttributes.Add(CreateAuthAttribute<QueryAuthAttribute>(policies, function));
            return this;
        }

        public SapphirePropertyBuilder<TModel, TProperty> AddUpdateAuth(string policies = null,
            Func<HttpInformation, TModel, bool> function = null)
        {
            attributesInfo.UpdateAuthAttributes.Add(CreateAuthAttribute<UpdateAuthAttribute>(policies, function));
            return this;
        }

        public SapphirePropertyBuilder<TModel, TProperty> SetMergeConflictResolutionMode(
            MergeConflictResolutionMode mergeConflictResolutionConflictResolutionMode)
        {
            attributesInfo.MergeConflictResolutionModeAttribute =
                new MergeConflictResolutionModeAttribute(mergeConflictResolutionConflictResolutionMode);
            return this;
        }

        private TAttributeType CreateAuthAttribute<TAttributeType>(string policies,
            Func<HttpInformation, TModel, bool> function) where TAttributeType : AuthAttributeBase
        {
            TAttributeType attribute =
                (TAttributeType) Activator.CreateInstance(typeof(TAttributeType), policies, null);

            if (function != null)
            {
                attribute.FunctionLambda = (information, model) => function(information, (TModel) model);
            }

            return attribute;
        }
    }
}