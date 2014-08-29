namespace Chris03.CodeSnippets.LinqFilterExpressions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Base class for a search type
    /// </summary>
    public abstract class Search
    {
        public enum Comparators
        {
            Unkown,
            Equals,
            EqualsNot,
            HasNone,
            HasAny,
            IsGreater,
            IsSmaller,
            IsBetween,
            Contains,
            ContainsNot,
            DateToday,
            DateThisWeek,
            DateThisMonth,
            DateThisYear
        }

        /// <summary>
        /// Gets the supported comparators.
        /// </summary>
        public abstract Comparators[] SupportedComparators { get; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>An expression or null</returns>
        public Expression<Func<T, bool>> GetExpression<T>(Expression<Func<T, object>> property, Comparators comparator, params string[] searchTerms)
        {
            ParameterExpression parameterExpression;
            string[] pathParts;
            var searchExpression = this.BuildExpression(property, comparator, searchTerms, out pathParts, out parameterExpression);

            var exp = this.WrapWithNullCheck<T>(pathParts, searchExpression, parameterExpression, null);

            return exp;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>An expression or null</returns>
        public Expression<Func<T, bool>> GetExpressionWithoutNullChecks<T>(Expression<Func<T, object>> property, Comparators comparator, params string[] searchTerms)
        {
            ParameterExpression parameterExpression;
            string[] pathParts;
            var searchExpression = this.BuildExpression(property, comparator, searchTerms, out pathParts, out parameterExpression);

            // Generate lambda with parameter
            var exp = Expression.Lambda<Func<T, bool>>(searchExpression, parameterExpression);

            return exp;
        }

        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="comparator">The comparator</param>
        /// <param name="searchTerms">Terms for the search</param>
        /// <returns>The expression</returns>
        protected abstract Expression BuildSearchExpression(Expression property, Comparators comparator, params string[] searchTerms);

        /// <summary>
        /// Gets a value indicating whether the property is nullable.
        /// </summary>
        protected bool IsNullableProperty(MemberExpression propertyExpression)
        {
            var prop = propertyExpression.Member as PropertyInfo;

            return prop != null && prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private Expression BuildExpression<T>(Expression<Func<T, object>> property, Comparators comparator, string[] searchTerms, out string[] pathParts, out ParameterExpression parameterExpression)
        {
            if (!this.SupportedComparators.Contains(comparator))
            {
                throw new InvalidOperationException(string.Format("Comparator {0} is not supported by filter.", comparator.ToString("F")));
            }

            var unaryExpression = property.Body as UnaryExpression;
            var memberExpression = unaryExpression == null ? property.Body as MemberExpression : unaryExpression.Operand as MemberExpression;

            if (memberExpression == null)
            {
                throw new InvalidOperationException("Invalid property selector.");
            }

            // Parameter for expression
            parameterExpression = Expression.Parameter(typeof(T), "p");

            // Get property path
            var path = memberExpression.ToString();
            pathParts = path.Split('.');

            // Rebuild expression
            var propertyExpression = pathParts.Skip(1).Aggregate(parameterExpression as Expression, Expression.Property);

            // Add search terms to property expression
            var searchExpression = this.BuildSearchExpression(propertyExpression, comparator, searchTerms);

            return searchExpression;
        }

        private Expression<Func<T, bool>> WrapWithNullCheck<T>(string[] propertyPathParts, Expression searchExpression, ParameterExpression arg, MemberExpression targetProperty)
        {
            Expression nullCheckExpression = null;

            if (propertyPathParts.Length > 2)
            {
                var property = Expression.Property(arg, propertyPathParts[1]);
                nullCheckExpression = Expression.NotEqual(property, Expression.Constant(null));

                for (var i = 2; i < propertyPathParts.Length - 1; i++)
                {
                    property = Expression.Property(property, propertyPathParts[i]);
                    Expression innerNullCheckExpression = Expression.NotEqual(property, Expression.Constant(null));

                    nullCheckExpression = Expression.AndAlso(nullCheckExpression, innerNullCheckExpression);
                }
            }

            /*
            if (!this.IgnoreNulls && (!targetProperty.Type.IsValueType || (targetProperty.Type.IsGenericType && targetProperty.Type.GetGenericTypeDefinition() == typeof(Nullable<>))))
            {
                var innerNullCheckExpression = Expression.NotEqual(targetProperty, Expression.Constant(null));

                nullCheckExpression = nullCheckExpression == null ? innerNullCheckExpression : Expression.AndAlso(nullCheckExpression, innerNullCheckExpression);
            }*/

            if (nullCheckExpression != null)
            {
                searchExpression = Expression.AndAlso(nullCheckExpression, searchExpression);
            }

            return Expression.Lambda<Func<T, bool>>(searchExpression, arg);
        }
    }
}
