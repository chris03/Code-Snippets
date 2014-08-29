namespace Chris03.CodeSnippets.LinqFilterExpressions
{
    using System;
    using System.Data.Entity;
    using System.Linq.Expressions;

    /// <summary>
    /// Search a date
    /// </summary>
    public class DateSearch : Search
    {
        /// <summary>
        /// Gets the supported comparators.
        /// </summary>
        public override Comparators[] SupportedComparators
        {
            get
            {
                return new[]
                {
                  Comparators.Equals,
                  Comparators.EqualsNot,
                  Comparators.HasAny,
                  Comparators.HasNone,
                  Comparators.IsGreater,
                  Comparators.IsSmaller,
                  Comparators.IsBetween
                };
            }
        }

        protected override Expression BuildSearchExpression(Expression property, Comparators comparator, params string[] searchTerms)
        {
            DateTime? searchTerm1 = null;
            DateTime? searchTerm2 = null;

            if (searchTerms != null)
            {
                DateTime date1;
                DateTime date2;

                searchTerm1 = searchTerms.Length > 0 && DateTime.TryParse(searchTerms[0], out date1) ? (DateTime?)date1 : null;
                searchTerm2 = searchTerms.Length > 1 && DateTime.TryParse(searchTerms[1], out date2) ? (DateTime?)date2 : null;
            }


            Expression searchExpression = null;

            // DbFunctions.TruncateTime();
            var truncateTimeMethodInfo = typeof(DbFunctions).GetMethod("TruncateTime", new[] { typeof(DateTime?) });
            var truncatedTimeProperty = Expression.Call(truncateTimeMethodInfo, property);

            var constSearchValue = Expression.Constant(searchTerm1, typeof(DateTime?));

            switch (comparator)
            {
                // Smaller, Greater
                case Comparators.IsSmaller:
                    searchExpression = Expression.LessThan(truncatedTimeProperty, constSearchValue);
                    break;
                case Comparators.IsGreater:
                    searchExpression = Expression.GreaterThan(truncatedTimeProperty, constSearchValue);
                    break;

                // Equals
                case Comparators.Equals:
                    searchExpression = Expression.Equal(truncatedTimeProperty, constSearchValue);
                    break;
                case Comparators.EqualsNot:
                    searchExpression = Expression.NotEqual(truncatedTimeProperty, constSearchValue);
                    break;

                // Has
                case Comparators.HasAny:
                    searchExpression = Expression.NotEqual(property, Expression.Constant(null));
                    break;
                case Comparators.HasNone:
                    searchExpression = Expression.Equal(property, Expression.Constant(null));
                    break;

                // Between
                case Comparators.IsBetween:
                    searchExpression = truncatedTimeProperty.Between(searchTerm1, searchTerm2);

                    break;

                // Dates
                case Comparators.DateToday:
                    searchExpression = Expression.Equal(truncatedTimeProperty, Expression.Constant(DateTime.Today, typeof(DateTime?)));
                    break;

                case Comparators.DateThisWeek:
                case Comparators.DateThisMonth:
                case Comparators.DateThisYear:
                    break;
            }

            return searchExpression;
        }
    }
}
