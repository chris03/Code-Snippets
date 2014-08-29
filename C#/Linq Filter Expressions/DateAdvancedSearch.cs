namespace Chris03.CodeSnippets.LinqFilterExpressions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Search a date
    /// </summary>
    public class DateAdvancedSearch : DateSearch
    {
        /// <summary>
        /// Gets the supported comparators.
        /// </summary>
        public override Comparators[] SupportedComparators
        {
            get
            {
                return base.SupportedComparators.Concat(new[] 
                {
                    Comparators.DateToday,
                    Comparators.DateThisWeek,
                    Comparators.DateThisMonth,
                    Comparators.DateThisYear
                }).ToArray();
            }
        }

        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="comparator"></param>
        /// <param name="searchTerms"></param>
        /// <returns>The expression</returns>
        protected override Expression BuildSearchExpression(Expression property, Comparators comparator, params string[] searchTerms)
        {
          /*  Expression valueProperty = null;

            if (this.PropertyIsNullable)
            {
                valueProperty = Expression.Property(property, "Value");
            }

            // DbFunctions.TruncateTime();
            var methodInfo = typeof(DbFunctions).GetMethod("TruncateTime", new[] { typeof(DateTime?) });

            var truncatedDate = Expression.Call(methodInfo, property);


            var constSearchValue = Expression.Constant(this.SearchTerm, typeof(DateTime?));
            var constNullValue = Expression.Constant(null);
            */
            switch (comparator)
            {
               // Dates
                case Comparators.DateToday:
                   // return Expression.Equal(truncatedDate, Expression.Constant(DateTime.Today, typeof(DateTime?)));

                case Comparators.DateThisWeek:
                case Comparators.DateThisMonth:
                case Comparators.DateThisYear:
                    // TODO:
                    return null;

                default:
                    throw new InvalidOperationException("Comparator not supported.");
            }
        }
    }
}
