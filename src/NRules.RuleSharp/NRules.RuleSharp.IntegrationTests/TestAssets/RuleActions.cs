using System;

namespace NRules.RuleSharp.IntegrationTests.TestAssets;

public static class RuleActions
{
    public static void NoOp()
    {
    }
        
    public static void NoOp(TestFact1 fact1)
    {
    }
        
    public static void NoOp(TestFact1 fact1, TestFact2 fact2)
    {
    }

    public static void Add(object value)
    {
    }

    public static Action<string> GetAction()
    {
        return s => { };
    }

    public static void MethodWithOptionalParam(TestFact1 fact1, string optionalParam = null)
    {
    }
}