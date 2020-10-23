using System.Linq.Expressions;

namespace SapphireDb.Helper
{
    public class SubstitutionExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression beforeExpression;
        private readonly Expression afterExpression;

        public SubstitutionExpressionVisitor(Expression beforeExpression, Expression afterExpression)
        {
            this.beforeExpression = beforeExpression;
            this.afterExpression = afterExpression;
        }
        
        public override Expression Visit(Expression node)
        {
            return node == beforeExpression ? afterExpression : base.Visit(node);
        }
    }
}