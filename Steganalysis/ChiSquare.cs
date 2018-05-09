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

        public double analyze()
        {
            int nBlocks = ((3 * Width * Height) / chiSquareBlocks) - 1;
            double[] chi = new double[nBlocks];

            int block = 0;
            int nbBytes = 1;
            int[] values = new int[256];
            double[] expected = new double[128];
            double[] observed = new double[128];
            Color color;

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = 1;
            }

            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    color = image.GetPixel(i, j);

                    if (block < chi.Length)
                    {
                        values[color.R]++;
                        nbBytes++;

                        if (nbBytes > chiSquareBlocks)
                        {
                            for (int k = 0; k < expected.Length; k++)
                            {
                                expected[k] = ((values[2 * k] + values[2 * k + 1]) / 2);
                                observed[k] = values[2 * k];
                            }
                            chi[block] = new ChiSquareTest(expected, observed, expected.Length - 1).PValue;
                            block++;
                            nbBytes = 1;
                        }
                    }

                    if (block < chi.Length)
                    {
                        values[color.G]++;
                        nbBytes++;

                        if (nbBytes > chiSquareBlocks)
                        {
                            for (int k = 0; k < expected.Length; k++)
                            {
                                expected[k] = ((values[2 * k] + values[2 * k + 1]) / 2);
                                observed[k] = values[2 * k];
                            }
                            chi[block] = new ChiSquareTest(expected, observed, expected.Length - 1).PValue;
                            block++;
                            nbBytes = 1;
                        }
                    }

                    if (block < chi.Length)
                    {
                        values[color.B]++;
                        nbBytes++;

                        if (nbBytes > chiSquareBlocks)
                        {
                            for (int k = 0; k < expected.Length; k++)
                            {
                                expected[k] = ((values[2 * k] + values[2 * k + 1]) / 2);
                                observed[k] = values[2 * k];
                            }
                            chi[block] = new ChiSquareTest(expected, observed, expected.Length - 1).PValue;
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

            return csQuant/chi.Length;
        }
    }
}
