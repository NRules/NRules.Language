using System;

namespace NRules.RuleSharp.IntegrationTests.TestAssets
{
    public static class RuleActions
    {
        public static void NoOp()
        {
        }

        public static Action<string> GetAction()
        {
            return s => { };
        }
    }
}
