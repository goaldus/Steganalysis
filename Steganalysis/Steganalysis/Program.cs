using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Accord.Statistics.Testing;


namespace Stegoanalysis
{
    class Program
    {
        //Chi square blocks
        private static int csSize = 1024;

        static void Main(string[] args)
        {
            using (Stream BitmapStream = System.IO.File.Open("stegoImage.png", System.IO.FileMode.Open))
            {
                Image picture = Image.FromStream(BitmapStream);
                var mBitmap = new Bitmap(picture);


                int nBlocks = ((3 * mBitmap.Width * mBitmap.Height) / csSize) - 1;
                double[] x = new double[nBlocks];
                double[] chi = new double[nBlocks];
                ChiSquareFromTopToBottom(mBitmap, x, chi, csSize);
                double totalVal = 0;
                foreach (double chiVal in chi)
                {
                    totalVal += chiVal;
                }
                Console.WriteLine(totalVal / chi.Length);

            }

        }

        private static void ChiSquareFromTopToBottom(Bitmap image, double[] x, double[] chi, int size)
        {
            int width = image.Width;
            int height = image.Height;
            int block = 0;
            int nBytes = 1;
            int red, green, blue;
            int[] values = new int[256];
            double[] expectedValues = new double[128];
            double[] pov = new double[128];
            Color pixel = Color.Empty;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = 1;
                x[i] = i;
            }

            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    if (block < chi.Length)
                    {
                        pixel = image.GetPixel(k, j);
                        red = pixel.R;
                        values[red]++;
                        nBytes++;
                        if (nBytes > size)
                        {
                            for (int i = 0; i < expectedValues.Length; i++)
                            {
                                expectedValues[i] = (values[2 * i] + values[2 * i + 1]) / 2;
                                pov[i] = values[2 * i];
                            }
                            chi[block] = new ChiSquareTest(expectedValues, pov, 1).PValue;
                            block++;
                            nBytes = 1;
                        }
                    }

                    if (block < chi.Length)
                    {
                        green = pixel.G;
                        values[green]++;
                        nBytes++;
                        if (nBytes > size)
                        {
                            for (int i = 0; i < expectedValues.Length; i++)
                            {
                                expectedValues[i] = (values[2 * i] + values[2 * i + 1]) / 2;
                                pov[i] = values[2 * i];
                            }
                            chi[block] = new ChiSquareTest(expectedValues, pov, 1).PValue;
                            block++;
                            nBytes = 1;
                        }
                    }

                    if (block < chi.Length)
                    {
                        blue = pixel.B;
                        values[blue]++;
                        nBytes++;
                        if (nBytes > size)
                        {
                            for (int i = 0; i < expectedValues.Length; i++)
                            {
                                expectedValues[i] = (values[2 * i] + values[2 * i + 1]) / 2;
                                pov[i] = values[2 * i];
                            }
                            chi[block] = new ChiSquareTest(expectedValues, pov, 1).PValue;
                            block++;
                            nBytes = 1;
                        }
                    }
                }
            }
        }
    }
}
