using System;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
using SapphireDb.Connection;
using SapphireDb.Helper;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireModelBuilder<T> where T : class
    {
        private readonly ModelAttributesInfo attributesInfo;

        public SapphireModelBuilder()
        {
            attributesInfo = typeof(T).GetModelAttributesInfo();
        }

        public SapphireModelBuilder<T> AddQueryAuth(string policies = null,
            Func<IConnectionInformation, bool> function = null)
        {
            attributesInfo.QueryAuthAttributes.Add(CreateAuthAttribute<QueryAuthAttribute>(policies, 
                (information, model) => function(information)));
            return this;
        }

        public SapphireModelBuilder<T> AddQueryEntryAuth(string policies = null,
            Func<IConnectionInformation, T, bool> function = null)
        {
            attributesInfo.QueryEntryAuthAttributes.Add(CreateAuthAttribute<QueryEntryAuthAttribute>(policies, function));
            return this;
        }

        public SapphireModelBuilder<T> AddCreateAuth(string policies = null,
            Func<IConnectionInformation, T, bool> function = null)
        {
            attributesInfo.CreateAuthAttributes.Add(CreateAuthAttribute<CreateAuthAttribute>(policies, function));
            return this;
        }

        public SapphireModelBuilder<T> AddUpdateAuth(string policies = null,
            Func<IConnectionInformation, T, bool> function = null)
        {
            attributesInfo.UpdateAuthAttributes.Add(CreateAuthAttribute<UpdateAuthAttribute>(policies, function));
            return this;
        }

        public SapphireModelBuilder<T> AddDeleteAuth(string policies = null,
            Func<IConnectionInformation, T, bool> function = null)
        {
            attributesInfo.DeleteAuthAttributes.Add(CreateAuthAttribute<DeleteAuthAttribute>(policies, function));
            return this;
        }

        private TAttributeType CreateAuthAttribute<TAttributeType>(string policies,
            Func<IConnectionInformation, T, bool> function) where TAttributeType : AuthAttributeBase
        {
            TAttributeType attribute = (TAttributeType) Activator.CreateInstance(typeof(TAttributeType), policies, null);
            
            if (function != null)
            {
                attribute.FunctionLambda = (information, model) => function(information, (T) model);
            }

            return attribute;
        }

        public SapphireModelBuilder<T> AddCreateEvent(Action<T, IConnectionInformation> before = null,
            Action<T, IConnectionInformation> beforeSave = null, Action<T, IConnectionInformation> after = null,
            Action<T, IConnectionInformation> insteadOf = null)
        {
            attributesInfo.CreateEventAttributes.Add(CreateEventAttribute<CreateEventAttribute>(before, beforeSave, after, insteadOf));
            return this;
        }

        public SapphireModelBuilder<T> AddUpdateEvent(Action<T, IConnectionInformation> before = null,
            Action<T, IConnectionInformation> beforeSave = null, Action<T, IConnectionInformation> after = null,
            Action<T, IConnectionInformation> insteadOf = null)
        {
            attributesInfo.UpdateEventAttributes.Add(
                CreateEventAttribute<UpdateEventAttribute>(before, beforeSave, after, insteadOf));
            return this;
        }
        
        public SapphireModelBuilder<T> AddUpdateEvent(Action<T, object, IConnectionInformation> before = null,
            Action<T, object, IConnectionInformation> beforeSave = null, Action<T, object, IConnectionInformation> after = null,
            Action<T, object, IConnectionInformation> insteadOf = null)
        {
            attributesInfo.UpdateEventAttributes.Add(
                CreateEventAttribute<UpdateEventAttribute>(before, beforeSave, after, insteadOf));
            return this;
        }

        public SapphireModelBuilder<T> AddDeleteEvent(Action<T, IConnectionInformation> before = null,
            Action<T, IConnectionInformation> beforeSave = null, Action<T, IConnectionInformation> after = null,
            Action<T, IConnectionInformation> insteadOf = null)
        {
            attributesInfo.DeleteEventAttributes.Add(
                CreateEventAttribute<DeleteEventAttribute>(before, beforeSave, after, insteadOf));
            return this;
        }

        private TAttributeType CreateEventAttribute<TAttributeType>(
            Action<T, object, IConnectionInformation> before, Action<T, object, IConnectionInformation> beforeSave,
            Action<T, object, IConnectionInformation> after, Action<T, object, IConnectionInformation> insteadOf)
            where TAttributeType : ModelStoreEventAttributeBase
        {
            TAttributeType attribute = (TAttributeType)Activator.CreateInstance(typeof(TAttributeType), null, null, null, null);

            if (before != null)
            {
                attribute.BeforeLambda = (oldValue, newValue, IConnectionInformation) => before((T)oldValue, newValue, IConnectionInformation);
            }
            
            if (beforeSave != null)
            {
                attribute.BeforeSaveLambda = (oldValue, newValue, IConnectionInformation) => beforeSave((T)oldValue, newValue, IConnectionInformation);
            }
            
            if (after != null)
            {
                attribute.AfterLambda = (oldValue, newValue, IConnectionInformation) => after((T)oldValue, newValue, IConnectionInformation);
            }
            
            if (insteadOf != null)
            {
                attribute.InsteadOfLambda = (oldValue, newValue, IConnectionInformation) => insteadOf((T)oldValue, newValue, IConnectionInformation);
            }

            return attribute;
        }
        
        private TAttributeType CreateEventAttribute<TAttributeType>(
            Action<T, IConnectionInformation> before, Action<T, IConnectionInformation> beforeSave,
            Action<T, IConnectionInformation> after, Action<T, IConnectionInformation> insteadOf)
            where TAttributeType : ModelStoreEventAttributeBase
        {
            TAttributeType attribute = (TAttributeType)Activator.CreateInstance(typeof(TAttributeType), null, null, null, null);

            if (before != null)
            {
                attribute.BeforeLambda = (oldValue, _, IConnectionInformation) => before((T)oldValue, IConnectionInformation);
            }
            
            if (beforeSave != null)
            {
                attribute.BeforeSaveLambda = (oldValue, _, IConnectionInformation) => beforeSave((T)oldValue, IConnectionInformation);
            }
            
            if (after != null)
            {
                attribute.AfterLambda = (oldValue, _, IConnectionInformation) => after((T)oldValue, IConnectionInformation);
            }
            
            if (insteadOf != null)
            {
                attribute.InsteadOfLambda = (oldValue, _, IConnectionInformation) => insteadOf((T)oldValue, IConnectionInformation);
            }

            return attribute;
        }

        public SapphireModelBuilder<T> MakeUpdateable()
        {
            attributesInfo.UpdateableAttribute = new UpdateableAttribute();
            return this;
        }
        
        public SapphireModelBuilder<T> DisableAutoMerge()
        {
            attributesInfo.DisableAutoMergeAttribute = new DisableAutoMergeAttribute();
            return this;
        }

        public SapphireModelBuilder<T> CreateQuery(string queryName, Func<SapphireQueryBuilder<T>, IConnectionInformation, SapphireQueryBuilder<T>> builder)
        {
            attributesInfo.QueryAttributes.Add(new QueryAttribute(queryName, null)
            {
                FunctionLambda = (queryBuilder, information, _) => builder(queryBuilder, information)
            });
            return this;
        }
        
        public SapphireModelBuilder<T> CreateQuery(string queryName, Func<SapphireQueryBuilder<T>, IConnectionInformation, JToken[], SapphireQueryBuilder<T>> builder)
        {
            attributesInfo.QueryAttributes.Add(new QueryAttribute(queryName, null)
            {
                FunctionLambda = (queryBuilder, information, parameters) => builder(queryBuilder, information, parameters)
            });
            return this;
        }
        
        public SapphirePropertyBuilder<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            PropertyInfo property = (PropertyInfo)((MemberExpression) selector.Body).Member;
            return new SapphirePropertyBuilder<T, TProperty>(property);
        }
    }
}