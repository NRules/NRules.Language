using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule "Metadata Rule 1"
    description "Rule with description and priority"
    priority 10
    when
        var fact = TestFact1();
    
    then
        RuleActions.NoOp();

rule "Metadata Rule 2"
{
    tag "Metadata", "Test"
    when
        var fact = TestFact1();
    
    then
        RuleActions.NoOp();
}