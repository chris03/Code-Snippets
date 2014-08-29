Linq Filter Expressions
======

Classes to generate Linq Expressions that can be used to filter objets, e.g. in the the Where() clause of a queryable object.

Usage:

```C#
var filter = new TextSearch().GetExpression<Individual>(x => x.Name.FirstName, Search.Comparators.Contains, "Bob");

var individuals = this.individualRepository.Where(filter);
```

The variable filter will contain the following expressions: _p => p.Individual.Name.FirstName.Contains("Bob")_
