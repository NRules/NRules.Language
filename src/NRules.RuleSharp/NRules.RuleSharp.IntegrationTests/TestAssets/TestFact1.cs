using System.Collections.Generic;

namespace NRules.RuleSharp.IntegrationTests.TestAssets;

public class TestFact1
{
    public string StringProperty { get; set; }
    public int IntProperty { get; set; }
    public int? NullableIntProperty { get; set; }
    public bool BoolProperty { get; set; }
    public string[] ArrayProperty { get; set; }
    public List<string> ListProperty { get; set; }
    public TestDto ObjectProperty { get; set; }
}