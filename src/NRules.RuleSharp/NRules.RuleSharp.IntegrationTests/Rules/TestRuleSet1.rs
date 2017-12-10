using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule TestRule1 "Test Rule 1"
{
	when
		var fact = TestFact1(x => x.StringProperty == "Valid" && x.IntProperty >= 0, x => x.BoolProperty);

	then
		fact.StringProperty = "Invalid";
}
