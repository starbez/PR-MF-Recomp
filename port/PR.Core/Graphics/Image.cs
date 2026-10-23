using System;
using System.IO;
using StbImageSharp;
using StbImageWriteSharp;

namespace PR.Core.Graphics;

public sealed class Image
{
    public int Width { get; }
    public int Height { get; }
    public uint[] Pixels { get; }

    public Image(int width, int height)
    {
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        Pixels = new uint[Width * Height];
    }

    public Image(int width, int height, uint[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels ?? throw new ArgumentNullException(nameof(pixels));
    }

    public static Image CreateImage(int width, int height)
    {
        return new Image(width, height);
    }

    public static Image CreateImage(byte[] imageData, int imageOffset, int imageLength)
    {
        try
        {
            byte[] sub = new byte[imageLength];
            Array.Copy(imageData, imageOffset, sub, 0, imageLength);
            var result = ImageResult.FromMemory(sub, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            if (result == null || result.Width <= 0 || result.Height <= 0)
            {
                return new Image(1, 1);
            }

            int w = result.Width;
            int h = result.Height;
            uint[] pixels = new uint[w * h];
            byte[] data = result.Data;

            for (int i = 0; i < pixels.Length; i++)
            {
                int bi = i * 4;
                byte r = data[bi];
                byte g = data[bi + 1];
                byte b = data[bi + 2];
                byte a = data[bi + 3];
                pixels[i] = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
            }

            return new Image(w, h, pixels);
        }
        catch
        {
            return new Image(1, 1);
        }
    }

    public static Image CreateImage(Stream stream)
    {
        try
        {
            var result = ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            if (result == null || result.Width <= 0 || result.Height <= 0)
            {
                return new Image(1, 1);
            }

            int w = result.Width;
            int h = result.Height;
            uint[] pixels = new uint[w * h];
            byte[] data = result.Data;

            for (int i = 0; i < pixels.Length; i++)
            {
                int bi = i * 4;
                byte r = data[bi];
                byte g = data[bi + 1];
                byte b = data[bi + 2];
                byte a = data[bi + 3];
                pixels[i] = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
            }

            return new Image(w, h, pixels);
        }
        catch
        {
            return new Image(1, 1);
        }
    }

    public static Image CreateImage(Image image, int x, int y, int width, int height, int transform)
    {
        var dest = new Image(width, height);
        var g = dest.GetGraphics();
        g.DrawRegion(image, x, y, width, height, transform, 0, 0, Graphics.TOP | Graphics.LEFT);
        return dest;
    }

    public Graphics GetGraphics()
    {
        return new Graphics(this);
    }

    public void SavePng(string filePath)
    {
        string? dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        byte[] rgba = new byte[Width * Height * 4];
        for (int i = 0; i < Pixels.Length; i++)
        {
            uint p = Pixels[i];
            rgba[i * 4 + 0] = (byte)((p >> 16) & 0xFF); // R
            rgba[i * 4 + 1] = (byte)((p >> 8) & 0xFF);  // G
            rgba[i * 4 + 2] = (byte)(p & 0xFF);         // B
            rgba[i * 4 + 3] = (byte)((p >> 24) & 0xFF); // A
        }

        using var stream = File.OpenWrite(filePath);
        var writer = new ImageWriter();
        writer.WritePng(rgba, Width, Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
    }
}
