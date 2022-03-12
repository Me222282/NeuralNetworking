using System;
using System.Collections.Generic;
using System.IO;
using Zene.Structs;
using Zene.Windowing;

namespace GenFileLoader
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            // No file to load
            if (args.Length == 0)
            {
                Console.WriteLine("No file loaded.");
                Console.WriteLine("Enter path to file or nothing to exit");

                List<string> paths = new List<string>();
                while (true)
                {
                    string path = Console.ReadLine();

                    // Nothing means exit
                    if (path == null || path == "" && paths.Count == 0) { return; }
                    else if (path == null || path == "")
                    {
                        args = paths.ToArray();
                        break;
                    }

                    if (!File.Exists(path))
                    {
                        Console.WriteLine("Path doesn't exist.");
                        continue;
                    }

                    paths.Add(path);
                    Console.WriteLine("Enter path to file or nothing to execute entered paths");
                }
            }

            Core.Init();

            Window(args);

            Core.Terminate();
        }

        public static void Window(string[] paths)
        {
            byte[][] data = new byte[paths.Length][];

            string text = "";

            for (int i = 0; i < paths.Length; i++)
            {
                text += paths[i] + " - ";

                data[i] = File.ReadAllBytes(paths[i]);
            }

            text = text.Remove(text.Length - 3);

            WindowW window = new WindowW(128 * 6, 128 * 6, text, data);

            window.Run();
        }

        public struct FramePart
        {
            public FramePart(byte r, byte g, byte b, int x, int y)
            {
                Colour = new Colour(r, g, b);
                Position = new Vector2I(x, y);
            }

            public Colour Colour { get; }
            public Vector2I Position { get; }
        }

        public static unsafe FramePart[,] ImportFrames(byte[] data, out int frameCount, out int lifeCount, out int worldSize)
        {
            int readOffset = 0;

            worldSize = BitConverter.ToInt32(data, readOffset);
            readOffset += 4;
            frameCount = BitConverter.ToInt32(data, readOffset);
            readOffset += 4;
            lifeCount = BitConverter.ToInt32(data, readOffset);
            readOffset += 4;

            FramePart[,] frames = new FramePart[frameCount, lifeCount];

            for (int f = 0; f < frameCount; f++)
            {
                for (int l = 0; l < lifeCount; l++)
                {
                    byte r = data[readOffset];
                    readOffset++;
                    byte g = data[readOffset];
                    readOffset++;
                    byte b = data[readOffset];
                    readOffset++;

                    int x = BitConverter.ToInt32(data, readOffset);
                    readOffset += 4;
                    int y = BitConverter.ToInt32(data, readOffset);
                    readOffset += 4;

                    frames[f, l] = new FramePart(r, g, b, x, y);
                }
            }

            return frames;
        }
    }
}
