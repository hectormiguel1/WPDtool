using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WPD.Native;

public static class Exports
{

    private enum Status : int
    {
        InvalidArgs = -1,
        Success = 0,
        Exception = 2
    }

    [UnmanagedCallersOnly(EntryPoint = "wpd_repack", CallConvs = [typeof(CallConvCdecl)])]
    public static int Repack(IntPtr inWpdDirPtr)
    {
        var inWpdDir = Marshal.PtrToStringUTF8(inWpdDirPtr);
        if (inWpdDir == null)
        {
            Log.Error("inWpdDir is null!");
            return (int)Status.InvalidArgs;
        }
        Log.Debug($"Repacking directory {inWpdDir}...");
        try
        {
            Actions.Repack(inWpdDir);
            Log.Info($"Successfully repacked {inWpdDir}!"); 
            return (int)Status.Success;
        }
        catch (Exception e)
        {
            Log.Error($"Encountered error while repacking directory: {inWpdDir}. Error: {e.Message}");
            return (int)Status.Exception;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "wpd_unpack", CallConvs = [typeof(CallConvCdecl)])]
    public static int Unpack(IntPtr inWpdFilePtr)
    {
        var inWpdFile = Marshal.PtrToStringUTF8(inWpdFilePtr);
        if (inWpdFile == null)
        {
            Log.Error("inWpdFile is null!");
            return (int)Status.InvalidArgs;
        }
        Log.Debug($"Unpacking WPD File: {inWpdFile}...");
        try
        {
            Actions.Unpack(inWpdFile);
            Log.Info($"Successfully unpacked WPD file: {inWpdFile}!"); 
            return (int)Status.Success;
        }
        catch (Exception e)
        {
            Log.Error($"Encountered error while unpacking wpd file: {inWpdFile}. Error: {e.Message}");
            return (int)Status.Exception;
        } 
    }
}