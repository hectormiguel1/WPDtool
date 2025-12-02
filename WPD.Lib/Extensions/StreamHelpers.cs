using System;
using System.IO;

namespace WPD.Extensions;

internal static class StreamHelpers
{
    public static void CopyStreamTo(this Stream inStream, Stream outStream, long size, bool showProgress)
    {
        const int bufferSize = 81920;
        var amountRemaining = size;
        long amountCopied = 0;

        while (amountRemaining > 0)
        {
            var arraySize = Math.Min(bufferSize, amountRemaining);
            var copyArray = new byte[arraySize];

            _ = inStream.Read(copyArray, 0, (int)arraySize);
            outStream.Write(copyArray, 0, (int)arraySize);

            amountRemaining -= arraySize;

            amountCopied += arraySize;

            if (!showProgress) continue;
            var currentAmount = Math.Round(((decimal)amountCopied / size) * 100);
            Console.Write("\r{0}", "Copied " + currentAmount + "%");
        }
    }


    public static void PadNull(this Stream stream, int padAmount)
    {
        for (var p = 0; p < padAmount; p++)
        {
            stream.WriteByte(0);
        }
    }
}