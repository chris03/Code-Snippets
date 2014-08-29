Linq Filter Expressions
======

Usage:

```C#
var filter = new TextSearch().GetExpression<Individual>(x => x.Name.FirstName, Search.Comparators.Contains, "Bob");

var individuals = this.individualRepository.Where(filter);
```