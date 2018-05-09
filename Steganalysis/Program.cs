using System.IO;
using System.Drawing;
using Steganalysis;
using System;

namespace Stegoanalysis
{
    class Program
    {
        static readonly double threshold = 0.2;
        static void Main(string[] args)
        {
            DirectoryInfo d = new DirectoryInfo(@"scn\");
            FileInfo[] files = d.GetFiles("*.png");

            foreach (var file in files)
            {

                //kontrola TODO

                using (Stream BitmapStream = System.IO.File.Open(file.FullName, System.IO.FileMode.Open))
                {
                    Image picture = Image.FromStream(BitmapStream);
                    var mBitmap = new Bitmap(picture);

                    double avg = 0;

                    var ChiSquareAnalysis = new ChiSquare(picture.Width, picture.Height, mBitmap);
                    var cs = ChiSquareAnalysis.analyze();
                    Console.WriteLine(file.Name + " CS: " + cs);

                    var SamplePairsAnalysis = new SamplePairs(picture.Width, picture.Height, mBitmap);
                    var sp = SamplePairsAnalysis.analyze();
                    Console.WriteLine("SP: " + sp);

                    var RSAnalysis = new RSAnalysis(picture.Width, picture.Height, mBitmap, 2, 2);
                    var rs = RSAnalysis.analyze();

                    avg = (cs + sp + rs) / 3;
                    Console.WriteLine("Prumer: " + avg);

                    if (avg > threshold)
                        Console.WriteLine("Podezrely obrazek");
                    else
                        Console.WriteLine("Cisty obrazek");
                }
            }

        }

        public static int getRed(int pixel)
        {
            return ((pixel >> 16) & 0xff);
        }

        /**
         * Gets the green content of a pixel.
         *
         * @param pixel The pixel to get the green content of.
         * @return The green content of the pixel.
         */
        public static int getGreen(int pixel)
        {
            return ((pixel >> 8) & 0xff);
        }

        /**
         * Gets the blue content of a pixel.
         *
         * @param pixel The pixel to get the blue content of.
         * @return The blue content of the pixel.
         */
        public static int getBlue(int pixel)
        {
            return (pixel & 0xff);
        }

    }
}
