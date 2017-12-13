using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule "Array Indexer Rule"
    when
        var fact = TestFact1();
    
    then
        var itemLength = fact.ArrayProperty[0].Length;
        NRules.RuleSharp.IntegrationTests.TestAssets.RuleActions.NoOp();

rule "List Indexer Rule"
    when
        var fact = TestFact1();
    
    then
        var itemLength = fact.ListProperty[0].Length;
        RuleActions.NoOp();

rule "String Indexer Rule"
    when
        var fact = TestFact1();
    
    then
        var charValue = fact.StringProperty[0];
        RuleActions.NoOp();
