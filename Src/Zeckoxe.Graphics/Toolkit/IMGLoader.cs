﻿// Copyright (c) 2019-2020 Faber Leonardo. All Rights Reserved.

/*=============================================================================
	IMGLoader.cs
=============================================================================*/




using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;


namespace Zeckoxe.Graphics.Toolkit
{

    public unsafe class IMGLoader : IDisposable
    {
        private Image<Rgba32> _image;

        public IMGLoader(string filename)
        {
            _image = Image.Load<Rgba32>(filename);
            Span<Rgba32> pixels = _image.GetPixelSpan();

            //for (int i = 0; i < pixels.Length; i++)
            //{
            //    ref Rgba32 pixel = ref pixels[i];
            //    byte a = pixel.A;
            //
            //    if (a is 0)
            //    {
            //        pixel.PackedValue = 0;
            //    }
            //    else
            //    {
            //        pixel.R = (byte)((pixel.R * a) >> 8);
            //        pixel.G = (byte)((pixel.G * a) >> 8);
            //        pixel.B = (byte)((pixel.B * a) >> 8);
            //    }
            //}

            TextureData data = new TextureData()
            {
                Width = _image.Width,
                Height = _image.Height,
                Format = PixelFormat.R8G8B8A8UNorm,
                Size = 4,
                Depth = 1,
                IsCubeMap = false,
                MipMaps = 1, // TODO: MipMaps 
                Data = MemoryMarshal.AsBytes(pixels).ToArray(),
            };


            TextureData = data;

        }


        public TextureData TextureData { get; private set; }

        public int Width => _image.Width;

        public int Height => _image.Height;

        public int MipMaps => 1; // TODO: MipMaps 

        public int Size => 4;

        public byte[] Data => GetAllTextureData();

        public bool IsCubeMap => false;




        public static TextureData LoadFromFile(string filename)
        {
            return new IMGLoader(filename).TextureData;
        }

        public void Dispose()
        {
            _image.Dispose();
        }

        private byte[] GetAllTextureData()
        {
            Span<Rgba32> pixels = _image.GetPixelSpan();

            return MemoryMarshal.AsBytes(pixels).ToArray();
        }
    }
}
