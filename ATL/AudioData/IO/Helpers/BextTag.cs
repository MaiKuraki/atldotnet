﻿using Commons;
using System.Collections.Generic;
using System.IO;
using ATL.Logging;
using static ATL.AudioData.IO.MetaDataIO;
using System;
using System.Text;
using System.Globalization;

namespace ATL.AudioData.IO
{
    /// <summary>
    /// Represents a Broadcast Wave Format ("bext"; see EBU – TECH 3285) metadata set
    /// </summary>
    internal static class BextTag
    {
        /// <summary>
        /// Identifier of a bext chunk
        /// </summary>
        public const string CHUNK_BEXT = "bext";


        /// <summary>
        /// Read a bext chunk from the given source into the given Metadata I/O, using the given read parameters
        /// </summary>
        /// <param name="source">Stream to read data from</param>
        /// <param name="meta">Metadata I/O to copy metadata to</param>
        /// <param name="readTagParams">Read parameters to use</param>
        public static void FromStream(Stream source, MetaDataIO meta, ReadTagParams readTagParams)
        {
            byte[] data = new byte[256];

            // Description
            if (source.Read(data, 0, 256) < 256) return;
            var str = Utils.StripEndingZeroChars(Encoding.UTF8.GetString(data).Trim());
            if (str.Length > 0) meta.SetMetaField("bext.description", str, readTagParams.ReadAllMetaFrames);

            // Originator
            if (source.Read(data, 0, 32) < 32) return;
            str = Utils.StripEndingZeroChars(Encoding.UTF8.GetString(data, 0, 32).Trim());
            if (str.Length > 0) meta.SetMetaField("bext.originator", str, readTagParams.ReadAllMetaFrames);

            // OriginatorReference
            if (source.Read(data, 0, 32) < 32) return;
            str = Utils.StripEndingZeroChars(Encoding.UTF8.GetString(data, 0, 32).Trim());
            if (str.Length > 0) meta.SetMetaField("bext.originatorReference", str, readTagParams.ReadAllMetaFrames);

            // OriginationDate
            if (source.Read(data, 0, 10) < 10) return;
            str = Utils.StripEndingZeroChars(Encoding.UTF8.GetString(data, 0, 10).Trim());
            if (str.Length > 0) meta.SetMetaField("bext.originationDate", str, readTagParams.ReadAllMetaFrames);

            // OriginationTime
            if (source.Read(data, 0, 8) < 8) return;
            str = Utils.StripEndingZeroChars(Encoding.UTF8.GetString(data, 0, 8).Trim());
            if (str.Length > 0) meta.SetMetaField("bext.originationTime", str, readTagParams.ReadAllMetaFrames);

            // TimeReference
            if (source.Read(data, 0, 8) < 8) return;
            ulong timeReference = StreamUtils.DecodeUInt64(data);
            meta.SetMetaField("bext.timeReference", timeReference.ToString(), readTagParams.ReadAllMetaFrames);

            // BEXT version
            if (source.Read(data, 0, 2) < 2) return;
            int intData = StreamUtils.DecodeUInt16(data);
            meta.SetMetaField("bext.version", intData.ToString(), readTagParams.ReadAllMetaFrames);

            // UMID
            if (source.Read(data, 0, 64) < 64) return;
            int usefulLength = 32; // "basic" UMID
            if (data[12] > 19) usefulLength = 64; // data[12] gives the size of remaining UMID
            StringBuilder sbr = new StringBuilder();
            for (int i = 0; i < usefulLength; i++) sbr.Append(data[i].ToString("X2"));

            meta.SetMetaField("bext.UMID", sbr.ToString(), readTagParams.ReadAllMetaFrames);

            // LoudnessValue
            if (source.Read(data, 0, 2) < 2) return;
            intData = StreamUtils.DecodeInt16(data);
            meta.SetMetaField("bext.loudnessValue", (intData / 100.0).ToString(), readTagParams.ReadAllMetaFrames);

            // LoudnessRange
            if (source.Read(data, 0, 2) < 2) return;
            intData = StreamUtils.DecodeInt16(data);
            meta.SetMetaField("bext.loudnessRange", (intData / 100.0).ToString(), readTagParams.ReadAllMetaFrames);

            // MaxTruePeakLevel
            if (source.Read(data, 0, 2) < 2) return;
            intData = StreamUtils.DecodeInt16(data);
            meta.SetMetaField("bext.maxTruePeakLevel", (intData / 100.0).ToString(), readTagParams.ReadAllMetaFrames);

            // MaxMomentaryLoudness
            if (source.Read(data, 0, 2) < 2) return;
            intData = StreamUtils.DecodeInt16(data);
            meta.SetMetaField("bext.maxMomentaryLoudness", (intData / 100.0).ToString(), readTagParams.ReadAllMetaFrames);

            // MaxShortTermLoudness
            if (source.Read(data, 0, 2) < 2) return;
            intData = StreamUtils.DecodeInt16(data);
            meta.SetMetaField("bext.maxShortTermLoudness", (intData / 100.0).ToString(), readTagParams.ReadAllMetaFrames);

            // Reserved
            source.Seek(180, SeekOrigin.Current);

            // CodingHistory
            long initialPos = source.Position;
            if (StreamUtils.FindSequence(source, Utils.CR_LF))
            {
                long endPos = source.Position - 2;
                source.Seek(initialPos, SeekOrigin.Begin);

                int size = (int)(endPos - initialPos);
                if (data.Length < size) data = new byte[size];
                if (source.Read(data, 0, size) < size) return;

                str = Utils.StripEndingZeroChars(Utils.Latin1Encoding.GetString(data, 0, (int)(endPos - initialPos)).Trim());
                if (str.Length > 0) meta.SetMetaField("bext.codingHistory", str, readTagParams.ReadAllMetaFrames);
            }
        }

        /// <summary>
        /// Indicate whether the given Metadata I/O contains metadata relevant to the Bext format
        /// </summary>
        /// <param name="meta">Metadata I/O to test with</param>
        /// <returns>True if the given Metadata I/O contains data relevant to the Bext format; false if it doesn't</returns>
        public static bool IsDataEligible(MetaDataHolder meta)
        {
            if (meta.GeneralDescription.Length > 0) return true;

            return WavHelper.IsDataEligible(meta, "bext.");
        }

        /// <summary>
        /// Write Bext metadata from the given Metadata I/O to the given writer, using the given endianness for the size headers
        /// </summary>
        /// <param name="w">Writer to write data to</param>
        /// <param name="isLittleEndian">Endianness to write the size headers with</param>
        /// <param name="meta">Metadata to write</param>
        /// <returns>The number of written fields</returns>
        public static int ToStream(BinaryWriter w, bool isLittleEndian, MetaDataHolder meta)
        {
            IDictionary<string, string> additionalFields = meta.AdditionalFields;
            w.Write(Utils.Latin1Encoding.GetBytes(CHUNK_BEXT));

            long sizePos = w.BaseStream.Position;
            w.Write(0); // Placeholder for chunk size that will be rewritten at the end of the method

            // Text values
            string description = Utils.ProtectValue(meta.GeneralDescription);
            if (0 == description.Length && additionalFields.TryGetValue("bext.description", out var field)) description = field;

            WavHelper.WriteFixedTextValue(description, 256, w);
            WavHelper.WriteFixedFieldTextValue("bext.originator", additionalFields, 32, w);
            WavHelper.WriteFixedFieldTextValue("bext.originatorReference", additionalFields, 32, w);
            WavHelper.WriteFixedFieldTextValue("bext.originationDate", additionalFields, 10, w);
            WavHelper.WriteFixedFieldTextValue("bext.originationTime", additionalFields, 8, w);

            // Int values
            WavHelper.WriteFieldIntValue("bext.timeReference", additionalFields, w, (ulong)0);
            WavHelper.WriteFieldIntValue("bext.version", additionalFields, w, (ushort)0);

            // UMID
            if (additionalFields.ContainsKey("bext.UMID"))
            {
                if (Utils.IsHex(additionalFields["bext.UMID"]))
                {
                    int usedValues = (int)Math.Floor(additionalFields["bext.UMID"].Length / 2.0);
                    for (int i = 0; i < usedValues; i++)
                    {
                        w.Write(Convert.ToByte(additionalFields["bext.UMID"].Substring(i * 2, 2), 16));
                    }
                    // Complete the field to 64 bytes
                    for (int i = 0; i < 64 - usedValues; i++) w.Write((byte)0);
                }
                else
                {
                    LogDelegator.GetLogDelegate()(Log.LV_WARNING, "'bext.UMID' : error writing field - hexadecimal notation required; " + additionalFields["bext.UMID"] + " found");
                    for (int i = 0; i < 64; i++) w.Write((byte)0);
                }
            }
            else
            {
                for (int i = 0; i < 64; i++) w.Write((byte)0);
            }


            // Float values
            WavHelper.WriteField100DecimalValue("bext.loudnessValue", additionalFields, w, (short)0);
            WavHelper.WriteField100DecimalValue("bext.loudnessRange", additionalFields, w, (short)0);
            WavHelper.WriteField100DecimalValue("bext.maxTruePeakLevel", additionalFields, w, (short)0);
            WavHelper.WriteField100DecimalValue("bext.maxMomentaryLoudness", additionalFields, w, (short)0);
            WavHelper.WriteField100DecimalValue("bext.maxShortTermLoudness", additionalFields, w, (short)0);

            // Reserved
            for (int i = 0; i < 180; i++) w.Write((byte)0);

            // CodingHistory
            byte[] textData = Array.Empty<byte>();
            if (additionalFields.TryGetValue("bext.codingHistory", out var additionalField))
            {
                textData = Utils.Latin1Encoding.GetBytes(additionalField);
                w.Write(textData);
            }
            w.Write(Utils.CR_LF);

            // Emulation of the BWFMetaEdit padding behaviour (256 characters)
            for (int i = 0; i < 256 - (textData.Length + 2) % 256; i++) w.Write((byte)0);

            // Add the extra padding byte if needed
            long finalPos = w.BaseStream.Position;
            long paddingSize = (finalPos - sizePos) % 2;
            if (paddingSize > 0) w.BaseStream.WriteByte(0);

            // Write actual tag size
            w.BaseStream.Seek(sizePos, SeekOrigin.Begin);
            if (isLittleEndian)
            {
                w.Write((int)(finalPos - sizePos - 4));
            }
            else
            {
                w.Write(StreamUtils.EncodeBEInt32((int)(finalPos - sizePos - 4)));
            }

            return 14;
        }
    }
}
