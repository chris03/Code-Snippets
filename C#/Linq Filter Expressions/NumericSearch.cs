namespace Chris03.CodeSnippets.LinqFilterExpressions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A numeric search
    /// </summary>
    public class NumericSearch : Search
    {
        public NumericSearch()
        {
            // Default value
            this.NumericType = typeof(int);
        }

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
                    Comparators.IsGreater,
                    Comparators.IsSmaller,
                    Comparators.IsBetween,
                    Comparators.HasNone,
                    Comparators.HasAny
                };
            }
        }

        public Type NumericType { get; set; }

        protected override Expression BuildSearchExpression(Expression property, Comparators comparator, params string[] searchTerms)
        {
            ValueContainer searchTerm1;
            ValueContainer searchTerm2;

            if (!this.ParseSearchTerms(searchTerms, out searchTerm1, out searchTerm2))
            {
                return null;
            }

            Expression searchExpression = null;
            Expression valueProperty = property; // this.PropertyIsNullable ? Expression.Property(property, "Value") : property;

            switch (comparator)
            {
                case Comparators.Equals:
                    if (searchTerm1.HasValue)
                    {
                        searchExpression = Expression.Equal(valueProperty, Expression.Constant(searchTerm1.Value));
                    }

                    break;

                case Comparators.IsGreater:
                    if (searchTerm1.HasValue)
                    {
                        searchExpression = Expression.GreaterThan(valueProperty, Expression.Constant(searchTerm1.Value));
                    }

                    break;

                case Comparators.IsSmaller:
                    if (searchTerm1.HasValue)
                    {
                        searchExpression = Expression.LessThan(valueProperty, Expression.Constant(searchTerm1.Value));
                    }

                    break;

                case Comparators.HasAny:
                    searchExpression = Expression.NotEqual(property, Expression.Constant(null));
                    break;

                case Comparators.HasNone:
                    searchExpression = Expression.Equal(property, Expression.Constant(null));
                    break;

                case Comparators.IsBetween:
                    
                    Expression exp1 = null;
                    Expression exp2 = null;

                    if (searchTerm1.HasValue)
                    {
                        exp1 = Expression.GreaterThanOrEqual(valueProperty, Expression.Constant(searchTerm1.Value));
                        searchExpression = exp1;
                    }

                    if (searchTerm2.HasValue)
                    {
                        exp2 = Expression.LessThanOrEqual(valueProperty, Expression.Constant(searchTerm2.Value));
                        searchExpression = exp2;
                    }

                    if (exp1 != null && exp2 != null)
                    {
                        searchExpression = Expression.AndAlso(exp1, exp2);
                    }

                    break;
            }

            return searchExpression;
        }

        private bool ParseSearchTerms(string[] searchTerms, out ValueContainer searchTerm1, out ValueContainer searchTerm2)
        {
            var parseSuccess = false;
            searchTerm1 = new ValueContainer();
            searchTerm2 = new ValueContainer();

            if (searchTerms != null)
            {
                searchTerms = searchTerms.SelectMany(x => x.Split(',')).ToArray();

                try
                {
                    if (searchTerms.Length > 0)
                    {
                        searchTerm1 = new ValueContainer(Convert.ChangeType(searchTerms[0], this.NumericType));
                    }

                    if (searchTerms.Length > 1)
                    {
                        searchTerm2 = new ValueContainer(Convert.ChangeType(searchTerms[1], this.NumericType));
                    }

                    parseSuccess = true;
                }
                catch (Exception e)
                {
                    // Failed to parse
                }
            }

            return parseSuccess;
        }

        private class ValueContainer
        {
            public ValueContainer()
            {
            }

            public ValueContainer(object o)
            {
                this.Value = o;
                this.HasValue = true;
            }

            public bool HasValue { get; private set; }

            public object Value { get; private set; }
        }
    }
}
