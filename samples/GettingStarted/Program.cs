using GettingStarted.Domain;
using System;
using NRules;
using NRules.RuleSharp;

namespace GettingStarted;

public static class Program
{
    static void Main(string[] args)
    {
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
    }
}