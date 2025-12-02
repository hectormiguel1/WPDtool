using System.Runtime.CompilerServices;
using Native;

namespace WPD;

internal static class Init
{
    [ModuleInitializer]
    internal static void Initialize()
    {   
        NativeLogger.ModuleName = "WPDLIB";
    }
}