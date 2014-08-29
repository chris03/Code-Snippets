namespace Unidac.Civil.Infrastructure.SearchFilters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A display item search
    /// </summary>
    public class ByIdSearch : Search
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
                    Comparators.EqualsNot
                };
            }
        }

        protected override Expression BuildSearchExpression(Expression property, Comparators comparator, params string[] searchTerms)
        {
            var ids = new List<int>();

            if (searchTerms != null)
            {
                // Parse values
                searchTerms.ToList().ForEach(x =>
                {
                    int val;
                    if (int.TryParse(x, out val))
                    {
                        ids.Add(val);
                    }
                });
            }

            if (!ids.Any())
            {
                return null;
            }

            // Use .Id
            var propertyId = Expression.Property(property, "Id");

            var values = ids.Select(x => Expression.Constant(x));

            switch (comparator)
            {
                case Comparators.Equals:
                    // Values must equal, combined by OrElse
                    return property.IsNotNullExpression().AndAlso(values.Select(x => Expression.Equal(propertyId, x)).AggregateOrElse());

                case Comparators.EqualsNot:
                    // Values must not equal, combined by AndAlso
                    return property.IsNotNullExpression().AndAlso(values.Select(x => Expression.NotEqual(propertyId, x)).AggregateAndAlso());
            }

            return null;
        }
    }
}
