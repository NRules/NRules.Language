using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule "Fact Condition Rule 1"
    when
        var fact = TestFact1(x => x.StringProperty == "Valid" && x.IntProperty >= 0, x => x.BoolProperty);
    
    then
        RuleActions.NoOp();

rule "Fact Condition Rule 2"
{
    when
        var fact = TestFact1(x => x.StringProperty != "Valid" || x.IntProperty < 0, x => !x.BoolProperty);
    
    then
        RuleActions.NoOp();
}