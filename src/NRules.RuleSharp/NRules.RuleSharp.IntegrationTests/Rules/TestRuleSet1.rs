using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule "Test Rule 1"
    description "This is Test Rule 1"
    priority 10
    when
        var fact = TestFact1(x => x.StringProperty == "Valid" && x.IntProperty >= 0, x => x.BoolProperty);
    
    then
        fact.StringProperty = "Invalid";

rule "Test Rule 2"
{
    tag "Simple", "Test"
    when
        var fact = TestFact1(x => x.StringProperty == "Valid" && x.IntProperty < 0, x => !x.BoolProperty);
    
    then
        fact.StringProperty = "Invalid";
}