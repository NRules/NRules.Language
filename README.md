# Rule# - Language for NRules

Rule# (Rule Sharp) is a business rules language for NRules rules engine.

> :warning: This project is currently in early development

[![Build status](https://img.shields.io/appveyor/ci/nrules/nrules-language.svg)](https://ci.appveyor.com/project/NRules/nrules-language) [![NuGet](https://img.shields.io/nuget/v/NRules.RuleSharp.svg)](https://nuget.org/packages/NRules.RuleSharp) [![NRules on Stack Overflow](https://img.shields.io/badge/stack%20overflow-nrules-orange.svg)](http://stackoverflow.com/questions/tagged/nrules) [![Join the chat](https://img.shields.io/gitter/room/nrules/nrules.language.svg)](https://gitter.im/nrules/nrules.language)

## Installing Rule#

To compile Rule# rules to the canonical form, get [NRules.RuleSharp](https://www.nuget.org/packages/NRules.RuleSharp) from nuget via the package manager
```console
> Install-Package NRules.RuleSharp
```

To compile rules in the canonical form to the runtime model and to be able to execute rules, install [NRules.Runtime](https://www.nuget.org/packages/NRules.Runtime)
```console
> Install-Package NRules.Runtime
```
    
## Getting Started

In NRules, rules are expressed against a domain model.
Given the following domain model (located in the ```Domain``` namespace):

```c#
namespace Domain;

public class Customer
{
    public string Name { get; set; }
    public bool IsPreferred { get; set; }
}

public class Order
{
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double PercentDiscount { get; set; }
    public bool IsDiscounted => PercentDiscount > 0;
    public double Price => UnitPrice*Quantity*(1.0 - PercentDiscount/100.0);
}
```

And given the following rule ```Discount.rul```:

```
using Domain;

rule "Order Discount"
when
    var customer = Customer(x => x.IsPreferred);
    var order = Order(x => !x.IsDiscounted, x => x.Quantity > 10);
    
then
    order.PercentDiscount = 5.0;
    Console.WriteLine("Applied discount. Customer={0}, Discount={1}", customer.Name, order.PercentDiscount);
```

The following code will compile the ```Discount.rul```, insert facts into the rules session and fire the activated rules.

```c#
var repository = new RuleRepository();
repository.AddNamespace("System");
repository.AddReference(typeof(Console).Assembly);
repository.AddReference(typeof(Order).Assembly);

repository.Load(@"Discount.rul");

var factory = repository.Compile();
var session = factory.CreateSession();

var customer = new Customer {Name = "John Doe", IsPreferred = true};
var order1 = new Order {Quantity = 12, UnitPrice = 10.0};
var order2 = new Order {Quantity = 5, UnitPrice = 15.0};

session.Insert(customer);
session.InsertAll(new[] {order1, order2});

session.Fire();
```

## Getting Help

Use the following discussion and Q&A platforms to get help with NRules Rule#

- [Discussions](https://github.com/NRules/NRules/discussions)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/nrules)
- [Gitter Chat](https://gitter.im/NRules/NRules.Language)

## Contributing

See [Contributor Guide](https://github.com/NRules/NRules/blob/main/CONTRIBUTING.md) for the guidelines on how to contribute to the project.

---
Copyright &copy; 2012-2023 [Sergiy Nikolayev](https://github.com/snikolayev) under the [MIT license](LICENSE.txt).
