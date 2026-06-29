using System.Linq.Expressions;

namespace MyNotes.Common.Expressions;

public sealed class ParameterExpressionVisitor(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
{
  protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : base.VisitParameter(node);
}