using System;
using System.IO;
using System.Text;
using IMGBlibrary.Repack;
using IMGBlibrary.Support;
using IMGBlibrary.Unpack;
using Native;
using WPD.Extensions;

namespace WPD;

public static class Actions
{
    public static void Repack(string inWPDExtractedDir)
        {
            var inWPDExtractedDirRoot = Path.GetDirectoryName(inWPDExtractedDir);

            var outWPDfileName = Path.GetFileName(inWPDExtractedDir);

            if (outWPDfileName.StartsWith('_'))
            {
                outWPDfileName = outWPDfileName.Remove(0, 1);
            }

            var outWPDfile = Path.Combine(inWPDExtractedDirRoot, outWPDfileName);
            var outWPDImgbFile = Path.Combine(inWPDExtractedDirRoot, Path.GetFileNameWithoutExtension(outWPDfileName) + ".imgb");
            var inWPDExtractedIMGBDir = Path.Combine(inWPDExtractedDirRoot, "_" + Path.GetFileNameWithoutExtension(outWPDfileName) + ".imgb");

            var recordsListFile = Path.Combine(inWPDExtractedDir, SharedMethods.RecordsList);

            if (!File.Exists(recordsListFile))
            {
                SharedMethods.ErrorExit($"Error: Missing file '{SharedMethods.RecordsList}' in extracted directory. Please ensure that the wpd file is unpacked properly with this tool.");
            }

            if (Directory.Exists(inWPDExtractedIMGBDir))
            {
                if (Directory.GetFiles(inWPDExtractedIMGBDir).Length != 0)
                {
                    if (outWPDImgbFile.EndsWith("ps3.imgb") || outWPDImgbFile.EndsWith("x360.imgb"))
                    {
                        SharedMethods.ErrorExit("Error: Detected PS3 or Xbox 360 version's extracted IMGB directory. repacking is not supported for these two versions.");
                    }

                    if (!File.Exists(outWPDImgbFile))
                    {
                        SharedMethods.ErrorExit($"Error: Paired imgb file for the extracted IMGB directory, is missing.\nPlease ensure that the paired imgb file is present next to the extracted imgb directory.");
                    }
                }
            }

            if (File.Exists(outWPDfile))
            {
                IfFileExistsDel(outWPDfile + ".old");

                File.Move(outWPDfile, outWPDfile + ".old");
            }

            if (File.Exists(outWPDImgbFile))
            {
                IfFileExistsDel(outWPDImgbFile + ".old");

                File.Copy(outWPDImgbFile, outWPDImgbFile + ".old");
            }

            var platform = IMGBEnums.Platforms.win32;

            if (outWPDfileName.EndsWith("ps3.xgr"))
            {
                platform = IMGBEnums.Platforms.ps3;
            }
            else if (outWPDfileName.EndsWith("x360.xgr"))
            {
                platform = IMGBEnums.Platforms.x360;
            }


            using (var recordListReader = new StreamReader(recordsListFile))
            {
                var isValidNum = uint.TryParse(recordListReader.ReadLine(), out uint totalRecords);

                if (!isValidNum)
                {
                    SharedMethods.ErrorExit($"Specified record count is invalid in the {SharedMethods.RecordsList} file");
                }
                
                // Write all record names and extensions
                // into the new wpd file
                using (var outWpdRecordsWriter = new StreamWriter(outWPDfile, true, new UTF8Encoding(false)))
                {
                    outWpdRecordsWriter.Write("WPD");
                    PadNullBytes(outWpdRecordsWriter, 13);

                    for (int r = 0; r < totalRecords; r++)
                    {
                        var currentRecordLineData = recordListReader.ReadLine().Split(SharedMethods.DataSplitChar, StringSplitOptions.None);

                        var currentRecordNameArray = Encoding.UTF8.GetBytes(currentRecordLineData[0]);

                        outWpdRecordsWriter.Write(Encoding.UTF8.GetString(currentRecordNameArray));
                        PadNullBytes(outWpdRecordsWriter, (16 - (uint)currentRecordNameArray.Length) + 8);

                        if (currentRecordLineData[1] == "null")
                        {
                            PadNullBytes(outWpdRecordsWriter, 8);
                        }
                        else
                        {
                            outWpdRecordsWriter.Write(currentRecordLineData[1]);
                            PadNullBytes(outWpdRecordsWriter, 8 - (uint)currentRecordLineData[1].Length);
                        }
                    }
                }


                // Copy in all record's data into the file
                // and update the offsets

                using var outWPDdataStream = new FileStream(outWPDfile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var outWPDoffsetStream = new FileStream(outWPDfile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                using var outWPDoffsetReader = new BinaryReader(outWPDoffsetStream);
                using var outWPDoffsetWriter = new BinaryWriter(outWPDoffsetStream);
                
                outWPDoffsetWriter.BaseStream.Position = 4;
                outWPDoffsetWriter.WriteBytesUInt32(totalRecords, true);


                uint readStartPos = 16;
                uint writeStartPos = 32;
                for (var o = 0; o < totalRecords; o++)
                {
                    outWPDoffsetReader.BaseStream.Position = readStartPos;
                    var currentRecordNameArray = outWPDoffsetReader.ReadBytesTillNull().ToArray();
                    var currentRecordName = Encoding.UTF8.GetString(currentRecordNameArray);

                    var recordNameAdjusted = SharedMethods.RemoveIllegalChars(currentRecordName);

                    outWPDoffsetReader.BaseStream.Position = readStartPos + 24;
                    var currentRecordExtn = "." + outWPDoffsetReader.ReadStringTillNull();

                    if (currentRecordExtn.Equals("."))
                    {
                        currentRecordExtn = "";
                    }

                    var recordDataStartPos = (uint)outWPDdataStream.Length;
                    outWPDoffsetWriter.BaseStream.Position = writeStartPos;
                    outWPDoffsetWriter.WriteBytesUInt32(recordDataStartPos, true);

                    var currentFile = Path.Combine(inWPDExtractedDir, recordNameAdjusted + currentRecordExtn);

                    if (Enum.TryParse(currentRecordExtn.Replace(".", ""), false, out IMGBEnums.FileExtensions fileExtension) == true)
                    {
                        if (Directory.Exists(inWPDExtractedIMGBDir))
                        {
                            IMGBRepack1.RepackIMGBType1(currentFile, outWPDImgbFile, inWPDExtractedIMGBDir, platform, true);
                        }
                    }

                    var currentFileSize = (uint)new FileInfo(currentFile).Length;

                    outWPDoffsetWriter.BaseStream.Position = writeStartPos + 4;
                    outWPDoffsetWriter.WriteBytesUInt32(currentFileSize, true);

                    using (var currentFileStream = new FileStream(currentFile, FileMode.Open, FileAccess.Read))
                    {
                        currentFileStream.Position = 0;
                        currentFileStream.CopyStreamTo(outWPDdataStream, currentFileSize, false);
                    }

                    // Pad null bytes to make the next
                    // start position divisible by a 
                    // pad value
                    var currentPos = outWPDdataStream.Length;
                    const int padValue = 4;
                    if (currentPos % padValue != 0)
                    {
                        var remainder = currentPos % padValue;
                        var increaseBytes = padValue - remainder;
                        var newPos = currentPos + increaseBytes;
                        var nullBytesAmount = newPos - currentPos;

                        outWPDdataStream.Seek(currentPos, SeekOrigin.Begin);
                        outWPDdataStream.PadNull((int)nullBytesAmount);
                    }

                    Log.Fine($"Repacked {currentFile}");

                    recordDataStartPos += currentFileSize;
                    readStartPos += 32;
                    writeStartPos += 32;
                }
            }

            Log.Info($"Finished repacking record files to \"{outWPDfile}\"");
        }


        private static void PadNullBytes(StreamWriter streamName, uint padding)
        {
            for (var b = 0; b < padding; b++)
            {
                streamName.Write("\0");
            }
        }


        private static void IfFileExistsDel(string fileToDelete)
        {
            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
            }
        }
        
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
                        Log.Fatal("Not a valid WPD file");
                        throw new InvalidDataException("Not a valid WPD file");
                    }

                    wpdReader.BaseStream.Position = 4;
                    var totalRecords = wpdReader.ReadBytesUInt32(true);
                    uint readStartPos = 16;

                    Log.Info("Writing record list....");
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
                        Log.Fine("Unpacking " + currentOutFile);

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

            Log.Info($"Finished unpacking file \"{Path.GetFileName(inWPDfile)}\"");
        }


        private static void DeleteDirIfExists(string directoryName)
        {
            if (Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName, true);
            }
        }
}