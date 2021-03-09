using System;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;
using SapphireDb.Attributes;
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
            Func<HttpInformation, bool> function = null)
        {
            attributesInfo.QueryAuthAttributes.Add(CreateAuthAttribute<QueryAuthAttribute>(policies, 
                (information, model) => function(information)));
            return this;
        }

        public SapphireModelBuilder<T> AddQueryEntryAuth(string policies = null,
            Func<HttpInformation, T, bool> function = null)
        {
            attributesInfo.QueryEntryAuthAttributes.Add(CreateAuthAttribute<QueryEntryAuthAttribute>(policies, function));
            return this;
        }

        public SapphireModelBuilder<T> AddCreateAuth(string policies = null,
            Func<HttpInformation, T, bool> function = null)
        {
            attributesInfo.CreateAuthAttributes.Add(CreateAuthAttribute<CreateAuthAttribute>(policies, function));
            return this;
        }

        public SapphireModelBuilder<T> AddUpdateAuth(string policies = null,
            Func<HttpInformation, T, bool> function = null)
        {
            attributesInfo.UpdateAuthAttributes.Add(CreateAuthAttribute<UpdateAuthAttribute>(policies, function));
            return this;
        }

        public SapphireModelBuilder<T> AddDeleteAuth(string policies = null,
            Func<HttpInformation, T, bool> function = null)
        {
            attributesInfo.DeleteAuthAttributes.Add(CreateAuthAttribute<DeleteAuthAttribute>(policies, function));
            return this;
        }

        private TAttributeType CreateAuthAttribute<TAttributeType>(string policies,
            Func<HttpInformation, T, bool> function) where TAttributeType : AuthAttributeBase
        {
            TAttributeType attribute = (TAttributeType) Activator.CreateInstance(typeof(TAttributeType), policies, null);
            
            if (function != null)
            {
                attribute.FunctionLambda = (information, model) => function(information, (T) model);
            }

            return attribute;
        }

        public SapphireModelBuilder<T> AddCreateEvent(Action<T, HttpInformation> before = null,
            Action<T, HttpInformation> beforeSave = null, Action<T, HttpInformation> after = null,
            Action<T, HttpInformation> insteadOf = null)
        {
            attributesInfo.CreateEventAttributes.Add(CreateEventAttribute<CreateEventAttribute>(before, beforeSave, after, insteadOf));
            return this;
        }

        public SapphireModelBuilder<T> AddUpdateEvent(Action<T, HttpInformation> before = null,
            Action<T, HttpInformation> beforeSave = null, Action<T, HttpInformation> after = null,
            Action<T, HttpInformation> insteadOf = null)
        {
            attributesInfo.UpdateEventAttributes.Add(
                CreateEventAttribute<UpdateEventAttribute>(before, beforeSave, after, insteadOf));
            return this;
        }
        
        public SapphireModelBuilder<T> AddUpdateEvent(Action<T, object, HttpInformation> before = null,
            Action<T, object, HttpInformation> beforeSave = null, Action<T, object, HttpInformation> after = null,
            Action<T, object, HttpInformation> insteadOf = null)
        {
            attributesInfo.UpdateEventAttributes.Add(
                CreateEventAttribute<UpdateEventAttribute>(before, beforeSave, after, insteadOf));
            return this;
        }

        public SapphireModelBuilder<T> AddDeleteEvent(Action<T, HttpInformation> before = null,
            Action<T, HttpInformation> beforeSave = null, Action<T, HttpInformation> after = null,
            Action<T, HttpInformation> insteadOf = null)
        {
            attributesInfo.DeleteEventAttributes.Add(
                CreateEventAttribute<DeleteEventAttribute>(before, beforeSave, after, insteadOf));
            return this;
        }

        private TAttributeType CreateEventAttribute<TAttributeType>(
            Action<T, object, HttpInformation> before, Action<T, object, HttpInformation> beforeSave,
            Action<T, object, HttpInformation> after, Action<T, object, HttpInformation> insteadOf)
            where TAttributeType : ModelStoreEventAttributeBase
        {
            TAttributeType attribute = (TAttributeType)Activator.CreateInstance(typeof(TAttributeType), null, null, null, null);

            if (before != null)
            {
                attribute.BeforeLambda = (oldValue, newValue, httpInformation) => before((T)oldValue, newValue, httpInformation);
            }
            
            if (beforeSave != null)
            {
                attribute.BeforeSaveLambda = (oldValue, newValue, httpInformation) => beforeSave((T)oldValue, newValue, httpInformation);
            }
            
            if (after != null)
            {
                attribute.AfterLambda = (oldValue, newValue, httpInformation) => after((T)oldValue, newValue, httpInformation);
            }
            
            if (insteadOf != null)
            {
                attribute.InsteadOfLambda = (oldValue, newValue, httpInformation) => insteadOf((T)oldValue, newValue, httpInformation);
            }

            return attribute;
        }
        
        private TAttributeType CreateEventAttribute<TAttributeType>(
            Action<T, HttpInformation> before, Action<T, HttpInformation> beforeSave,
            Action<T, HttpInformation> after, Action<T, HttpInformation> insteadOf)
            where TAttributeType : ModelStoreEventAttributeBase
        {
            TAttributeType attribute = (TAttributeType)Activator.CreateInstance(typeof(TAttributeType), null, null, null, null);

            if (before != null)
            {
                attribute.BeforeLambda = (oldValue, _, httpInformation) => before((T)oldValue, httpInformation);
            }
            
            if (beforeSave != null)
            {
                attribute.BeforeSaveLambda = (oldValue, _, httpInformation) => beforeSave((T)oldValue, httpInformation);
            }
            
            if (after != null)
            {
                attribute.AfterLambda = (oldValue, _, httpInformation) => after((T)oldValue, httpInformation);
            }
            
            if (insteadOf != null)
            {
                attribute.InsteadOfLambda = (oldValue, _, httpInformation) => insteadOf((T)oldValue, httpInformation);
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

        public SapphireModelBuilder<T> CreateQuery(string queryName, Func<SapphireQueryBuilder<T>, HttpInformation, SapphireQueryBuilder<T>> builder)
        {
            attributesInfo.QueryAttributes.Add(new QueryAttribute(queryName, null)
            {
                FunctionLambda = (queryBuilder, information, _) => builder(queryBuilder, information)
            });
            return this;
        }
        
        public SapphireModelBuilder<T> CreateQuery(string queryName, Func<SapphireQueryBuilder<T>, HttpInformation, JToken[], SapphireQueryBuilder<T>> builder)
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