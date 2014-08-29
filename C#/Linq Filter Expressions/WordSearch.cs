namespace Chris03.CodeSnippets.LinqFilterExpressions
{
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A word search
    /// </summary>
    public class WordSearch : TextSearch
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
                    Comparators.Equals,
                    Comparators.EqualsNot
                }).ToArray();
            }
        }

        protected override Expression BuildSearchExpression(Expression property, Comparators comparator, params string[] searchTerms)
        {
            if (searchTerms == null)
            {
                return null;
            }

            var toSearch = searchTerms.FirstOrDefault();

            var searchExpression = base.BuildSearchExpression(property, comparator, searchTerms);

            if (searchExpression == null)
            {
                switch (comparator)
                {
                    case Comparators.Equals:
                        searchExpression = Expression.Equal(property, toSearch.ToConstantExpression());
                        break;
                    case Comparators.EqualsNot:
                        searchExpression = Expression.NotEqual(property, toSearch.ToConstantExpression());
                        break;
                }
            }

            return searchExpression;
        }
    }
}
