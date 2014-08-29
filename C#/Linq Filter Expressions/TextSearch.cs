namespace Chris03.CodeSnippets.LinqFilterExpressions
{
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A text search
    /// </summary>
    public class TextSearch : Search
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
                    Comparators.Contains,
                    Comparators.ContainsNot,
                    Comparators.HasAny,
                    Comparators.HasNone
                };
            }
        }

        protected override Expression BuildSearchExpression(Expression property, Comparators comparator, params string[] searchTerms)
        {
            if (searchTerms == null)
            {
                return null;
            }

            var toSearch = searchTerms.FirstOrDefault();

            Expression searchExpression;

            switch (comparator)
            {
                case Comparators.Contains:
                    searchExpression = Contains(property, toSearch);
                    break;

                case Comparators.ContainsNot:
                    searchExpression = Expression.Not(Contains(property, toSearch));
                    break;

                case Comparators.HasAny:
                    // Must not be null
                    searchExpression = Expression.NotEqual(property, Expression.Constant(null));
                    break;

                case Comparators.HasNone:
                    // Must be null 
                    searchExpression = Expression.Equal(property, Expression.Constant(null));
                    break;

                default:
                    searchExpression = null;
                    break;
            }

            return searchExpression;
        }

        private static Expression Contains(Expression property, string value)
        {
            var stringType = typeof(string);
            var containsMethodInfo = stringType.GetMethod("Contains", new[] { stringType });

            return Expression.Call(property, containsMethodInfo, value.ToConstantExpression());
        }
    }
}
