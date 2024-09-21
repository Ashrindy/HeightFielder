using AshDumpLib.HedgehogEngine;
using Hexa.NET.DirectXTex;
using System.Runtime.InteropServices;

public class Program
{
    public unsafe static void Main(string[] args)
    {
        string filepath = "";
        if(args.Length > 0)
            filepath = args[0];
        else
        {
            Console.WriteLine("What's the file?");
            filepath = Console.ReadLine();
        }
        
        if(Path.GetExtension(filepath) == ".heightfield")
        {
            HeightField hfd = new(filepath);
            TexMetadata metadata = new()
            {
                Width = (ulong)hfd.TerrainSize.X,
                Height = (ulong)hfd.TerrainSize.Y,
                Depth = 1,
                ArraySize = 1,
                MipLevels = 1,
                Format = 56,
                Dimension = TexDimension.Texture2D
            };

            ScratchImage img = DirectXTex.CreateScratchImage();
            img.Initialize2D(metadata.Format, metadata.Width, metadata.Height, metadata.ArraySize, metadata.MipLevels, CPFlags.None);
            IntPtr pixelPtr = (IntPtr)img.GetImages()->Pixels;
            for(int y = 0; y < (int)metadata.Height; y++)
            {
                for(int x = 0; x < (int)metadata.Width; x++)
                {
                    int index = (y * (int)metadata.Width + x) * 2;
                    ushort red = (ushort)(hfd.CollisionData[(y * (int)metadata.Width + x)]);

                    Marshal.WriteInt16(pixelPtr, index, (short)red);
                }
            }
            string outputPath = filepath.Replace(".heightfield", ".dds");
            DirectXTex.SaveToDDSFile(img.GetImages(), DDSFlags.None, outputPath);

            //img.Initialize2D(61, metadata.Width, metadata.Height, metadata.ArraySize, metadata.MipLevels, CPFlags.None);
            //pixelPtr = (IntPtr)img.GetImages()->Pixels;
            //for (int y = 0; y < (int)metadata.Height - 2; y++)
            //{
            //    for (int x = 0; x < (int)metadata.Width; x++)
            //    {
            //        int index = (y * (int)metadata.Width + x);
            //        byte red = (byte)(hfd.MaterialData[(y * (int)metadata.Width + x)] * hfd.Materials.Count);

            //        Marshal.WriteByte(pixelPtr, index, red);
            //    }
            //}
            //outputPath = filepath.Replace(".heightfield", "_material.dds");
            //DirectXTex.SaveToDDSFile(img.GetImages(), DDSFlags.None, outputPath);
        }
        if(Path.GetExtension(filepath) == ".dds")
        {
            HeightField hfd = new(filepath.Replace(".dds", ""));
            ScratchImage img = DirectXTex.CreateScratchImage();
            TexMetadata metadata = default;
            DirectXTex.LoadFromDDSFile(filepath, DDSFlags.None, ref metadata, ref img);
            IntPtr pixelPtr = (IntPtr)img.GetImages()->Pixels;
            for (int y = 0; y < (int)metadata.Height; y++)
            {
                for (int x = 0; x < (int)metadata.Width; x++)
                {
                    int index = (y * (int)metadata.Width + x) * 2;
                    hfd.CollisionData[(y * (int)metadata.Width + x)] = (ushort)(Marshal.ReadInt16(pixelPtr, index));
                }
            }
            hfd.SaveToFile(filepath.Replace(".dds", ".heightfield"));
        }
    }
}