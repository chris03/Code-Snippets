namespace Chris03.CodeSnippets.LinqFilterExpressions
{
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A display item search
    /// </summary>
    public class ByIdOptionalSearch : ByIdSearch
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
                    Comparators.HasAny,
                    Comparators.HasNone
                }).ToArray();
            }
        }

        protected override Expression BuildSearchExpression(Expression property, Comparators comparator, params string[] searchTerms)
        {
            var searchExpression = base.BuildSearchExpression(property, comparator, searchTerms);

            if (searchExpression == null)
            {
                switch (comparator)
                {
                    case Comparators.HasAny:
                        // Must not be null
                        searchExpression = Expression.NotEqual(property, Expression.Constant(null));
                        break;

                    case Comparators.HasNone:
                        // Must be null 
                        searchExpression = Expression.Equal(property, Expression.Constant(null));
                        break;
                }
            }

            return searchExpression;
        }
    }
}
