// Guids.cs
// MUST match guids.h
using System;

namespace FredericTorres.CoffeeScriptRunnerVSPackage
{
    static class GuidList
    {
        public const string guidCoffeeScriptRunnerVSPackagePkgString = "7aee5fd7-53a9-45e6-b72c-667be25996e7";
        public const string guidCoffeeScriptRunnerVSPackageCmdSetString = "d84ae008-b9de-4f34-99cd-23ba4a21be8c";

        public static readonly Guid guidCoffeeScriptRunnerVSPackageCmdSet = new Guid(guidCoffeeScriptRunnerVSPackageCmdSetString);
    };
}