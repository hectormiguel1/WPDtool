using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Native.Common;

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
    public static unsafe NativeResult.Result<int> Repack(byte* inWpdDirPtr)
    {
        var inWpdDir = NativeResult.StringFromPtr(inWpdDirPtr);
        if (string.IsNullOrEmpty(inWpdDir))
        {   const string msg = "inWpdDir is null!";
            Log.Fatal(msg);
            return NativeResult.CreateError<int>(msg, (int)Status.InvalidArgs);
        }
        Log.Fine($"Repacking directory {inWpdDir}...");
        try
        {
            Actions.Repack(inWpdDir);
            Log.Info($"Successfully repacked {inWpdDir}!"); 
            return NativeResult.CreateInlineSuccess((int)Status.Success);
        }
        catch (Exception e)
        {
            Log.Fatal($"Encountered error while repacking directory: {inWpdDir}. Error: {e.Message}");
            return NativeResult.CreateError<int>(e.Message, (int)Status.Exception);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "wpd_unpack", CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe NativeResult.Result<int> Unpack(byte* inWpdFilePtr)
    {
        var inWpdFile = NativeResult.StringFromPtr(inWpdFilePtr);
        if (string.IsNullOrEmpty(inWpdFile))
        {
            const string msg = "inWpdFile is null!";
            Log.Fatal(msg);
            return NativeResult.CreateError<int>(msg, (int)Status.InvalidArgs);
        }
        Log.Fine($"Unpacking WPD File: {inWpdFile}...");
        try
        {
            Actions.Unpack(inWpdFile);
            Log.Info($"Successfully unpacked WPD file: {inWpdFile}!");
            return NativeResult.CreateInlineSuccess((int)Status.Success);
        }
        catch (Exception e)
        {
            Log.Fatal($"Encountered error while unpacking wpd file: {inWpdFile}. Error: {e.Message}");
            return NativeResult.CreateError<int>(e.Message, (int)Status.Exception);
        } 
    }
}