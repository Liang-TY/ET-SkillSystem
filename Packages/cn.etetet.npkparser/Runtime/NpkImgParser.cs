using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ET
{
    public struct NpkSprite
    {
        public int Index;
        public int Width, Height;
        public int X, Y;
        public int FrameWidth, FrameHeight;
        public int[] ArgbData;
    }

    public static class NpkImgParser
    {
        private const int TypeArgb1555 = 0x0E;
        private const int TypeArgb4444 = 0x0F;
        private const int TypeArgb8888 = 0x10;
        private const int TypeReference = 0x11;
        private const int FlagCompressed = 0x06;

        public static NpkSprite[] Parse(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            // Header: magic(16) + tableLength(4) + skip(4) + version(4) + frameCount(4)
            byte[] magic = reader.ReadBytes(16);
            string magicStr = System.Text.Encoding.ASCII.GetString(magic).TrimEnd('\0');
            if (magicStr != "Neople Img File")
            {
                throw new Exception($"Invalid IMG magic: {magicStr}");
            }

            int tableLength = reader.ReadInt32();
            reader.ReadInt32(); // skip
            int version = reader.ReadInt32();
            int frameCount = reader.ReadInt32();

            if (version != 2 && version != 4)
            {
                throw new Exception($"Unsupported IMG version: {version}, only v2 and v4 are supported");
            }

            // v4: read palette
            int[] palette = null;
            if (version == 4)
            {
                int colorNum = reader.ReadInt32();
                palette = new int[colorNum];
                for (int i = 0; i < colorNum; i++)
                {
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();
                    byte a = reader.ReadByte();
                    palette[i] = (a << 24) | (r << 16) | (g << 8) | b;
                }
            }

            // Read frame directory
            var frameInfos = new List<FrameInfo>(frameCount);
            for (int i = 0; i < frameCount; i++)
            {
                int type = reader.ReadInt32();
                if (type == TypeReference)
                {
                    int refIndex = reader.ReadInt32();
                    frameInfos.Add(new FrameInfo { Type = type, RefIndex = refIndex });
                }
                else
                {
                    int compressed = reader.ReadInt32();
                    int width = reader.ReadInt32();
                    int height = reader.ReadInt32();
                    int length = reader.ReadInt32();
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    int frameWidth = reader.ReadInt32();
                    int frameHeight = reader.ReadInt32();
                    frameInfos.Add(new FrameInfo
                    {
                        Type = type, Compressed = compressed,
                        Width = width, Height = height, Length = length,
                        X = x, Y = y, FrameWidth = frameWidth, FrameHeight = frameHeight
                    });
                }
            }

            // Read frame data sequentially
            var sprites = new NpkSprite[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                FrameInfo info = frameInfos[i];
                if (info.Type == TypeReference)
                {
                    if (info.RefIndex < sprites.Length && sprites[info.RefIndex].ArgbData != null)
                    {
                        sprites[i] = sprites[info.RefIndex];
                    }
                    continue;
                }

                byte[] frameData = reader.ReadBytes(info.Length);
                if (info.Compressed == FlagCompressed)
                {
                    frameData = DecompressZlib(frameData);
                }

                int[] argb;
                if (version == 4 && palette != null)
                {
                    argb = DecodeIndexed(frameData, palette);
                }
                else
                {
                    argb = DecodeV2(frameData, info.Type);
                }

                sprites[i] = new NpkSprite
                {
                    Index = i,
                    Width = info.Width,
                    Height = info.Height,
                    X = info.X,
                    Y = info.Y,
                    FrameWidth = info.FrameWidth,
                    FrameHeight = info.FrameHeight,
                    ArgbData = argb
                };
            }

            return sprites;
        }

        private static byte[] DecompressZlib(byte[] data)
        {
            using var input = new MemoryStream(data, 2, data.Length - 2);
            using var output = new MemoryStream();
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            deflate.CopyTo(output);
            return output.ToArray();
        }

        private static int[] DecodeV2(byte[] data, int type)
        {
            int bytesPerPixel = type == TypeArgb8888 ? 4 : 2;
            int pixelCount = data.Length / bytesPerPixel;
            int[] result = new int[pixelCount];
            int offset = 0;

            for (int i = 0; i < pixelCount; i++)
            {
                switch (type)
                {
                    case TypeArgb1555:
                    {
                        ushort pixel = (ushort)(data[offset] | (data[offset + 1] << 8));
                        int a = ((pixel >> 15) & 1) * 255;
                        int r = Scale5To8((pixel >> 10) & 0x1F);
                        int g = Scale5To8((pixel >> 5) & 0x1F);
                        int b = Scale5To8(pixel & 0x1F);
                        result[i] = (a << 24) | (r << 16) | (g << 8) | b;
                        break;
                    }
                    case TypeArgb4444:
                    {
                        ushort pixel = (ushort)(data[offset] | (data[offset + 1] << 8));
                        int a = Scale4To8((pixel >> 12) & 0xF);
                        int r = Scale4To8((pixel >> 8) & 0xF);
                        int g = Scale4To8((pixel >> 4) & 0xF);
                        int b = Scale4To8(pixel & 0xF);
                        result[i] = (a << 24) | (r << 16) | (g << 8) | b;
                        break;
                    }
                    case TypeArgb8888:
                    {
                        // 存储序 = 小端 ARGB 整型：字节 B, G, R, A（与 1555/4444 的小端读法一致）。
                        // 实证 2026-08-21：bloodboom 8888 特效按 R,G,B,A 读会红蓝互换（血红变蓝），
                        // 用 wpf-img-ani 查看器 + 逐字节通道统计确证真实序为 B,G,R,A。
                        int b = data[offset];
                        int g = data[offset + 1];
                        int r = data[offset + 2];
                        int a = data[offset + 3];
                        result[i] = (a << 24) | (r << 16) | (g << 8) | b;
                        break;
                    }
                    default:
                        throw new Exception($"Unknown pixel type: 0x{type:X2}");
                }

                offset += bytesPerPixel;
            }

            return result;
        }

        private static int[] DecodeIndexed(byte[] data, int[] palette)
        {
            int[] result = new int[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = palette[data[i]];
            }
            return result;
        }

        private static int Scale5To8(int v) => (v << 3) | (v >> 2);
        private static int Scale4To8(int v) => (v << 4) | v;

        private struct FrameInfo
        {
            public int Type;
            public int Compressed;
            public int Width, Height, Length;
            public int X, Y;
            public int FrameWidth, FrameHeight;
            public int RefIndex;
        }
    }
}
