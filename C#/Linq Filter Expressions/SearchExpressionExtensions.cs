
namespace Chris03.CodeSnippets.LinqFilterExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class SearchExpressionExtensions
    {
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(new SwapVisitor(expr1.Parameters[0], expr2.Parameters[0]).Visit(expr1.Body), expr2.Body), expr2.Parameters);
        }

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(new SwapVisitor(expr1.Parameters[0], expr2.Parameters[0]).Visit(expr1.Body), expr2.Body), expr2.Parameters);
        }

        public static Expression<Func<T, bool>> AggregateAndAlso<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            return expressions != null && expressions.Any() ? expressions.Aggregate((l, r) => l.AndAlso(r)) : null;
        }

        public static Expression AggregateAndAlso(this IEnumerable<Expression> expressions)
        {
            return expressions != null && expressions.Any() ? expressions.Aggregate(Expression.AndAlso) : null;
        }

        public static Expression AggregateOrElse(this IEnumerable<Expression> expressions)
        {
            return expressions != null && expressions.Any() ? expressions.Aggregate(Expression.OrElse) : null;
        }

        public static Expression<Func<T, bool>> AggregateOrElse<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            return expressions != null && expressions.Any() ? expressions.Aggregate((l, r) => l.OrElse(r)) : null;
        }

        public static Expression IsNotNullExpression(this Expression expression)
        {
            return Expression.NotEqual(expression, Expression.Constant(null));
        }

        public static Expression AndAlso(this Expression left, Expression right)
        {
            return Expression.AndAlso(left, right);
        }

        public static Expression ToConstantExpression(this object value)
        {
            return Expression.Constant(value);
        }

        public static Expression Between<T>(this Expression property, T? value1, T? value2)
             where T : struct
        {
            Expression expression = null;
            Expression exp1 = null;
            Expression exp2 = null;

            if (value1.HasValue)
            {
                exp1 = Expression.GreaterThanOrEqual(property, Expression.Constant(value1, typeof(T?)));
                expression = exp1;
            }

            if (value2.HasValue)
            {
                exp2 = Expression.LessThanOrEqual(property, Expression.Constant(value2, typeof(T?)));
                expression = exp2;
            }

            if (exp1 != null && exp2 != null)
            {
                expression = Expression.AndAlso(exp1, exp2);
            }

            return expression;
        }

        class SwapVisitor : ExpressionVisitor
        {
            private readonly Expression from, to;

            public SwapVisitor(Expression from, Expression to)
            {
                this.from = from;
                this.to = to;
            }

            public override Expression Visit(Expression node)
            {
                return node == this.@from ? this.to : base.Visit(node);
            }
        }
    }
}
