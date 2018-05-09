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
    }
}
