using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyNotes.Common.Expressions;

public static class ExpressionExtensions
{
  extension<T>(IReadOnlyList<Expression<Func<T, bool>>> expressions)
  {
    public Expression<Func<T, bool>> AndAll()
    {
      if(expressions.Count == 0)
      {
        return _ => true;
      }

      var parameter = Expression.Parameter(typeof(T), "T");
      Expression? combinedExpression = null;

      foreach(var expression in expressions)
      {
        var replacedExpression = new ParameterExpressionVisitor(expression.Parameters[0], parameter).Visit(expression.Body)
          ?? throw new InvalidOperationException("조건식 파라미터 치환에 실패했습니다.");

        combinedExpression = combinedExpression is null
          ? replacedExpression
          : Expression.AndAlso(combinedExpression, replacedExpression);
      }

      return Expression.Lambda<Func<T, bool>>(combinedExpression ?? Expression.Constant(true), parameter);
    }

    public Expression<Func<T, bool>> OrAll()
    {
      if (expressions.Count == 0)
      {
        return _ => true;
      }

      var parameter = Expression.Parameter(typeof(T), "T");
      Expression? combinedExpression = null;

      foreach (var expression in expressions)
      {
        var replacedExpression = new ParameterExpressionVisitor(expression.Parameters[0], parameter).Visit(expression.Body)
          ?? throw new InvalidOperationException("조건식 파라미터 치환에 실패했습니다.");

        combinedExpression = combinedExpression is null
          ? replacedExpression
          : Expression.OrElse(combinedExpression, replacedExpression);
      }

      return Expression.Lambda<Func<T, bool>>(combinedExpression ?? Expression.Constant(true), parameter);
    }
  }
}