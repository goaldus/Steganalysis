/* 
 *
 * This code is based on Benedikt Boehm's work
 * https://github.com/b3dk7/StegExpose/blob/master/ChiSquare.java
 *
 * Slightly modified(computing algorithm unchanged) and rewrote in C# by Ondrej Molnar
 */

using Accord.Statistics.Testing;
using System.Drawing;

namespace Steganalysis
{
    public class ChiSquare : IDetector
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Bitmap image;
        private static int chiSquareBlocks = 1024;

        public ChiSquare(int width, int height, Bitmap bitmap)
        {
            this.Width = width;
            this.Height = height;
            this.image = bitmap;
        }

        /// <summary>
        /// Computes mean value of all chi blocks
        /// </summary>
        /// <returns></returns>
        public double analyze()
        {
            int nBlocks = ((3 * Width * Height) / chiSquareBlocks) - 1;
            double[] chi = new double[nBlocks];

            int block = 0;
            int nbBytes = 1;
            int[] histogram = new int[256];
            double[] expected = new double[128];
            double[] observed = new double[128];
            Color pixel;

            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] = 1;
            }

            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    pixel = image.GetPixel(i, j);

                    if (block < chi.Length)
                    {
                        histogram[pixel.R]++;
                        nbBytes++;

                        if (nbBytes > chiSquareBlocks)
                        {
                            chi[block] = getChiSquarePValue(histogram, expected.Length, expected, observed);
                            block++;
                            nbBytes = 1;
                        }
                    }

                    if (block < chi.Length)
                    {
                        histogram[pixel.G]++;
                        nbBytes++;

                        if (nbBytes > chiSquareBlocks)
                        {
                            chi[block] = getChiSquarePValue(histogram, expected.Length, expected, observed);
                            block++;
                            nbBytes = 1;
                        }
                    }

                    if (block < chi.Length)
                    {
                        histogram[pixel.B]++;
                        nbBytes++;

                        if (nbBytes > chiSquareBlocks)
                        {
                            chi[block] = getChiSquarePValue(histogram, expected.Length, expected, observed);
                            block++;
                            nbBytes = 1;
                        }
                    }
                }
            }

            double csQuant = 0.0;
            foreach (var val in chi)
            {
                csQuant += val;
            }

            return csQuant / chi.Length;
        }

        /// <summary>
        /// Returns p(probability) value of hidden message presence - greater value means greater probability
        /// </summary>
        /// <param name="histogram"></param>
        /// <param name="pairCount"></param>
        /// <param name="expected"></param>
        /// <param name="observed"></param>
        /// <returns></returns>
        public double getChiSquarePValue(int[] histogram, int pairCount, double[] expected, double[] observed)
        {
            for (int k = 0; k < expected.Length; k++)
            {
                expected[k] = ((histogram[2 * k] + histogram[2 * k + 1]) / 2);
                observed[k] = histogram[2 * k];
            }
            return new ChiSquareTest(expected, observed, expected.Length - 1).PValue;
        }
    }
}
