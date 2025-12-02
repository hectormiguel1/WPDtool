using IMGBlibrary.Support;
using IMGBlibrary.Unpack;
using System;
using System.IO;
using System.Text;
using WPD.Extensions;
using Native;
namespace WPD
{
    public static class Unpacker
    {
        public static void Unpack(string inWPDfile)
        {
            var wpdFileName = Path.GetFileName(inWPDfile);
            var wpdFileDir = Path.GetDirectoryName(inWPDfile);
            var inWPDimgbFile = Path.Combine(wpdFileDir, Path.GetFileNameWithoutExtension(inWPDfile) + ".imgb");

            var extractWPDdir = Path.Combine(wpdFileDir, "_" + wpdFileName);
            var extractIMGBdir = Path.Combine(Path.GetDirectoryName(inWPDfile), "_" + Path.GetFileName(inWPDimgbFile));

            DeleteDirIfExists(extractWPDdir);
            Directory.CreateDirectory(extractWPDdir);

            var platform = IMGBEnums.Platforms.win32;

            if (File.Exists(inWPDimgbFile))
            {
                DeleteDirIfExists(extractIMGBdir);
                Directory.CreateDirectory(extractIMGBdir);

                if (inWPDimgbFile.EndsWith("ps3.imgb"))
                {
                    platform = IMGBEnums.Platforms.ps3;
                }
                else if (wpdFileName.EndsWith("x360.imgb"))
                {
                    platform = IMGBEnums.Platforms.x360;
                }
            }
            
            using (var wpdStream = new FileStream(inWPDfile, FileMode.Open, FileAccess.Read))
            {
                using (var wpdReader = new BinaryReader(wpdStream))
                {
                    wpdReader.BaseStream.Position = 0;
                    var wpdHeader = wpdReader.ReadBytesString(4, false);

                    if (!wpdHeader.Equals("WPD"))
                    {
                        NativeLogger.Error("Not a valid WPD file");
                        throw new InvalidDataException("Not a valid WPD file");
                    }

                    wpdReader.BaseStream.Position = 4;
                    var totalRecords = wpdReader.ReadBytesUInt32(true);
                    uint readStartPos = 16;

                    NativeLogger.Info("Writing record list....");
                    using (var recordListWriter = new StreamWriter(Path.Combine(extractWPDdir, SharedMethods.RecordsList), true, Encoding.UTF8))
                    {
                        recordListWriter.WriteLine(totalRecords);

                        for (int r = 0; r < totalRecords; r++)
                        {
                            wpdReader.BaseStream.Position = readStartPos;

                            var currentRecordNameArray = wpdReader.ReadBytesTillNull().ToArray();
                            recordListWriter.Write(Encoding.UTF8.GetString(currentRecordNameArray));

                            wpdReader.BaseStream.Position = readStartPos + 24;
                            var extn = wpdReader.ReadStringTillNull();

                            if (extn == "")
                            {
                                recordListWriter.WriteLine(SharedMethods.DataSplitChar[0] + "null");
                            }
                            else
                            {
                                recordListWriter.WriteLine(SharedMethods.DataSplitChar[0] + extn);
                            }

                            readStartPos += 32;
                        }
                    }
                    
                    readStartPos = 16;
                    for (var f = 0; f < totalRecords; f++)
                    {
                        wpdReader.BaseStream.Position = readStartPos;
                        var currentRecordNameArray = wpdReader.ReadBytesTillNull().ToArray();
                        var currentRecordName = Encoding.UTF8.GetString(currentRecordNameArray);

                        var recordNameAdjusted = SharedMethods.RemoveIllegalChars(currentRecordName);

                        wpdReader.BaseStream.Position = readStartPos + 16;
                        var currentRecordStart = wpdReader.ReadBytesUInt32(true);

                        wpdReader.BaseStream.Position = readStartPos + 20;
                        var currentRecordSize = wpdReader.ReadBytesUInt32(true);

                        wpdReader.BaseStream.Position = readStartPos + 24;
                        var currentRecordExtension = "." + wpdReader.ReadStringTillNull();
                        currentRecordExtension = currentRecordExtension == "." ? "" : currentRecordExtension;

                        var currentOutFile = Path.Combine(extractWPDdir, recordNameAdjusted + currentRecordExtension);
                        NativeLogger.Debug("Unpacking " + currentOutFile);

                        using (var ofs = new FileStream(currentOutFile, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            wpdStream.Position = currentRecordStart;
                            wpdStream.CopyStreamTo(ofs, currentRecordSize, false);
                        }

                        if (Enum.TryParse(currentRecordExtension.Replace(".", ""), false, out IMGBEnums.FileExtensions fileExtension) == true)
                        {
                            if (File.Exists(inWPDimgbFile))
                            {
                                IMGBUnpack.UnpackIMGB(currentOutFile, inWPDimgbFile, extractIMGBdir, platform, true);
                            }
                        }

                        readStartPos += 32;
                    }
                }
            }

            NativeLogger.Info($"Finished unpacking file \"{Path.GetFileName(inWPDfile)}\"");
        }


        private static void DeleteDirIfExists(string directoryName)
        {
            if (Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName, true);
            }
        }
    }
}