using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    // [QueryFunction(nameof(QueryFunction))]
    public class QueryFunctionDemo
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Content { get; set; }
        
        // private static Expression<Func<QueryFunctionDemo, bool>> QueryFunction()
        // {
        //     return d => d.Content.StartsWith("test");
        // }
    }
}