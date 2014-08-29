Linq Filter Expressions
======

Classes to generate Linq Expressions that can be used to filter objets, e.g. in the the Where() clause of a queryable object.

Text Search Example
------
```C#
var filter = new TextSearch().GetExpression<Individual>(x => x.Name.FirstName, Search.Comparators.Contains, "Bob");

var individuals = this.individualRepository.Where(filter);
```
Filter is:  _p => p.Individual.Name.FirstName.Contains("Bob")_



Numeric Search Example
------
```C#
var data = new[]
{
    new Money(-10, MoneyCurrency.CAD),
    new Money(12, MoneyCurrency.CAD),
    new Money(42, MoneyCurrency.CAD),
    new Money(50, MoneyCurrency.CAD),
    new Money(200, MoneyCurrency.CAD)
}.AsQueryable();

var numericSearch = new NumericSearch { NumericType = typeof(decimal) }.GetExpression<Money>(x => x.Amount, Search.Comparators.IsBetween, "13", "49");

var result = data.Where(numericSearch).ToList();
```
Filter is: _p => ((p.Amount >= 13) AndAlso (p.Amount <= 49))_
