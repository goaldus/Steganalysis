/*
 *    Digital Invisible Ink Toolkit
 *    Copyright (C) 2005  K. Hempstalk	
 *
 *    This program is free software; you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation; either version 2 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program; if not, write to the Free Software
 *    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *		@author Kathryn Hempstalk
 */

using System;
using System.Drawing;

/**
 * RS analysis for a stego-image.
 * <P>
 * RS analysis is a system for detecting LSB steganography proposed by
 * Dr. Fridrich at Binghamton University, NY.  You can visit her
 * webpage for more information - 
 * {@link http://www.ws.binghamton.edu/fridrich/} <BR>
 * Implemented as described in "Reliable detection of LSB steganography
 * in color and grayscale images" by J. Fridrich, M. Goljan and R. Du. 
 * <BR>
 *
 * @author Kathryn Hempstalk
 * 
 * This code is based on Benedikt Boehm's work
 * https://github.com/b3dk7/StegExpose/blob/master/RSAnalysis.java
 * 
 * Slightly modified and rewrote in C# by Ondrej Molnar
 * Original code here: https://github.com/b3dk7/StegExpose/blob/master/RSAnalysis.java
 */

namespace Steganalysis
{
    public class RSAnalysis : HelperFunctions, IDetector
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private int[][] mask;
        private int maskSizeX, maskSizeY;

        public RSAnalysis(int width, int height, Bitmap bitmap, int m, int n)
        {
            this.Width = width;
            this.Height = height;
            this.image = bitmap;

            mask = new int[2][];
            mask[0] = new int[m * n];
            mask[1] = new int[m * n];

            //iterate through them and set alternating bits
            int k = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (((j % 2) == 0 && (i % 2) == 0)
                            || ((j % 2) == 1 && (i % 2) == 1))
                    {
                        mask[0][k] = 1;
                        mask[1][k] = 0;
                    }
                    else
                    {
                        mask[0][k] = 0;
                        mask[1][k] = 1;
                    }
                    k++;
                }
            }

            //set up the mask size.
            maskSizeX = m;
            maskSizeY = n;
        }

        public double analyze()
        {
            //RS analysis for overlapping groups
            double averageOverlappingValue = (analyze(Colors.Red, true)[26] + analyze(Colors.Green, true)[26] +
                                             analyze(Colors.Blue, true)[26]) / 3;
            //RS analysis for non-overlapping groups
            double averageNonOverlappingValue = (analyze(Colors.Red, false)[26] + analyze(Colors.Green, false)[26] +
                                                analyze(Colors.Blue, false)[26]) / 3;

            return (averageNonOverlappingValue + averageOverlappingValue) / 2;
        }

        private double[] analyze(Colors color, bool overlap)
        {
            int startx = 0, starty = 0;
            Color[] block = new Color[maskSizeX * maskSizeY];
            double numregular = 0, numsingular = 0, numnegregular = 0, numnegsingular = 0, numunusable = 0, numnegunusable = 0;
            double variationB, variationP, variationN;

            while (startx < Width && starty < Height)
            {
                for (int m = 0; m < 2; m++)
                {
                    int k = 0;
                    for (int i = 0; i < maskSizeY; i++)
                    {
                        for (int j = 0; j < maskSizeX; j++)
                        {
                            block[k] = image.GetPixel(startx + j, starty + i);
                            k++;
                        }
                    }

                    //get the variation the block
                    variationB = getVariation(block, color);

                    //now flip according to the mask
                    block = flipBlock(block, mask[m]);
                    variationP = getVariation(block, color);

                    //flip it back
                    block = flipBlock(block, mask[m]);

                    //invert the mask
                    mask[m] = invertMask(mask[m]);
                    variationN = getNegativeVariation(block, color, mask[m]);
                    mask[m] = invertMask(mask[m]);

                    //now we need to work out which group each belongs to

                    //positive groupings
                    if (variationP > variationB)
                        numregular++;
                    if (variationP < variationB)
                        numsingular++;
                    if (variationP == variationB)
                        numunusable++;

                    //negative mask groupings
                    if (variationN > variationB)
                        numnegregular++;
                    if (variationN < variationB)
                        numnegsingular++;
                    if (variationN == variationB)
                        numnegunusable++;

                    //now we keep going...
                }

                if (overlap)
                    startx += 1;
                else
                    startx += maskSizeX;

                if (startx >= (Width - 1))
                {
                    startx = 0;
                    if (overlap)
                        starty += 1;
                    else
                        starty += maskSizeY;
                }

                if (starty >= (Height - 1))
                    break;
            }

            //get all the details needed to derive x...
            double totalgroups = numregular + numsingular + numunusable;
            double[] allpixels = this.getAllPixelFlips(color, overlap);
            double x = getX(numregular, numnegregular, allpixels[0], allpixels[2],
                            numsingular, numnegsingular, allpixels[1], allpixels[3]);
            //calculate the estimated percent of flipped pixels and message length
            double epf, ml;
            if (2 * (x - 1) == 0)
                epf = 0;
            else
                epf = Math.Abs(x / (2 * (x - 1)));

            if (x - 0.5 == 0)
                ml = 0;
            else
                ml = Math.Abs(x / (x - 0.5));

            //now we have the number of regular and singular groups...
            double[] results = new double[28];

            //save them all...

            //these results
            results[0] = numregular;                                // Number of regular groups (positive)
            results[1] = numsingular;                               // Number of singular groups (positive)
            results[2] = numnegregular;                             // Number of regular groups (negative)
            results[3] = numnegsingular;                            // Number of singular groups (negative)
            results[4] = Math.Abs(numregular - numnegregular);      // Difference for regular groups
            results[5] = Math.Abs(numsingular - numnegsingular);    // Difference for singular groups
            results[6] = (numregular / totalgroups) * 100;          // Percentage of regular groups (positive)
            results[7] = (numsingular / totalgroups) * 100;         // Percentage of singular groups (positive)
            results[8] = (numnegregular / totalgroups) * 100;       // Percentage of regular groups (negative)
            results[9] = (numnegsingular / totalgroups) * 100;      // Percentage of singular groups (negative)
            results[10] = (results[4] / totalgroups) * 100;         // Difference for regular groups %
            results[11] = (results[5] / totalgroups) * 100;         // Difference for singular groups %

            //all pixel results
            results[12] = allpixels[0];                             // Number of regular groups (positive for all flipped)
            results[13] = allpixels[1];                             // Number of singular groups (positive for all flipped)
            results[14] = allpixels[2];                             // Number of regular groups (negative for all flipped)
            results[15] = allpixels[3];                             // Number of singular groups (negative for all flipped)
            results[16] = Math.Abs(allpixels[0] - allpixels[1]);    // Difference for regular groups (all flipped)
            results[17] = Math.Abs(allpixels[2] - allpixels[3]);    // Difference for singular groups (all flipped)
            results[18] = (allpixels[0] / totalgroups) * 100;       // Percentage of regular groups (positive for all flipped)
            results[19] = (allpixels[1] / totalgroups) * 100;       // Percentage of singular groups (positive for all flipped)
            results[20] = (allpixels[2] / totalgroups) * 100;       // Percentage of regular groups (negative for all flipped)
            results[21] = (allpixels[3] / totalgroups) * 100;       // Percentage of singular groups (negative for all flipped)
            results[22] = (results[16] / totalgroups) * 100;        // Difference for regular groups (all flipped) %
            results[23] = (results[17] / totalgroups) * 100;        // Difference for singular groups (all flipped) %

            //overall results
            results[24] = totalgroups;                              // Total number of groups
            results[25] = epf;                                      // Estimated percent of flipped pixels
            results[26] = ml;                                       // Estimated message length (in percent of pixels)(p)
            results[27] = ((Width * Height * 3) * ml) / 8;          // Estimated message length (in bytes)

            return results;
        }


        /**
	     * Gets the variation of the blocks of data. Uses
	     * the formula f(x) = |x0 - x1| + |x1 - x3| + |x3 - x2| + |x2 - x0|;
	     * However, if the block is not in the shape 2x2 or 4x1, this will be
	     * applied as many times as the block can be broken up into 4 (without
	     * overlaps).
	     *
         * @param block The block of data (in 24 bit color).
         * @param color The color to get the variation of.
	     * @return The variation in the block.
	     */
        private double getVariation(Color[] block, Colors color)
        {
            double variation = 0;
            int val1, val2;

            for (int i = 0; i < block.Length; i += 4)
            {
                val1 = getColorFromPixel(block[0 + i], color);
                val2 = getColorFromPixel(block[1 + i], color);
                variation += Math.Abs(val1 - val2);

                val1 = getColorFromPixel(block[1 + i], color);
                val2 = getColorFromPixel(block[3 + i], color);
                variation += Math.Abs(val1 - val2);

                val1 = getColorFromPixel(block[3 + i], color);
                val2 = getColorFromPixel(block[2 + i], color);
                variation += Math.Abs(val1 - val2);

                val1 = getColorFromPixel(block[2 + i], color);
                val2 = getColorFromPixel(block[0 + i], color);
                variation += Math.Abs(val1 - val2);
            }

            return variation;
        }

        /**
             * Gets the negative variation of the blocks of data. Uses
             * the formula f(x) = |x0 - x1| + |x1 - x3| + |x3 - x2| + |x2 - x0|;
             * However, if the block is not in the shape 2x2 or 4x1, this will be
             * applied as many times as the block can be broken up into 4 (without
             * overlaps).
             *
             * @param block The block of data (in 24 bit color).
             * @param color The color to get the variation of.
             * @param mask The negative mask.
             * @return The variation in the block.
             */
        private double getNegativeVariation(Color[] block, Colors color, int[] mask)
        {
            double variation = 0;
            int val1, val2;
            for (int i = 0; i < block.Length; i = i + 4)
            {
                val1 = getColorFromPixel(block[0 + i], color);
                val2 = getColorFromPixel(block[1 + i], color);

                if (mask[0 + i] == -1)
                    val1 = invertLSB(val1);
                if (mask[1 + i] == -1)
                    val2 = invertLSB(val2);
                variation += Math.Abs(val1 - val2);

                val1 = getColorFromPixel(block[1 + i], color);
                val2 = getColorFromPixel(block[3 + i], color);
                if (mask[1 + i] == -1)
                    val1 = invertLSB(val1);
                if (mask[3 + i] == -1)
                    val2 = invertLSB(val2);
                variation += Math.Abs(val1 - val2);

                val1 = getColorFromPixel(block[3 + i], color);
                val2 = getColorFromPixel(block[2 + i], color);
                if (mask[3 + i] == -1)
                    val1 = invertLSB(val1);
                if (mask[2 + i] == -1)
                    val2 = invertLSB(val2);
                variation += Math.Abs(val1 - val2);

                val1 = getColorFromPixel(block[2 + i], color);
                val2 = getColorFromPixel(block[0 + i], color);
                if (mask[2 + i] == -1)
                    val1 = invertLSB(val1);
                if (mask[0 + i] == -1)
                    val2 = invertLSB(val2);
                variation += Math.Abs(val1 - val2);
            }
            return variation;
        }

        /**
        * Flips a block of pixels.
        *
        * @param block The block to flip.
        * @param mask The mask to use for flipping.
        * @return The flipped block.
        */
        private Color[] flipBlock(Color[] block, int[] mask)
        {
            //if the mask is true, negate every LSB
            for (int i = 0; i < block.Length; i++)
            {
                if (mask[i] == 1)
                {
                    //get the color
                    int red = getColorFromPixel(block[i], Colors.Red);
                    int green = getColorFromPixel(block[i], Colors.Green);
                    int blue = getColorFromPixel(block[i], Colors.Blue);

                    //negate their LSBs
                    red = negateLSB(red);
                    green = negateLSB(green);
                    blue = negateLSB(blue);

                    //build a new pixel
                    Color newPixel = Color.FromArgb(red, green, blue);

                    //change the block pixel
                    block[i] = newPixel;
                }
                else if (mask[i] == -1)
                {
                    //get the color
                    int red = getColorFromPixel(block[i], Colors.Red);
                    int green = getColorFromPixel(block[i], Colors.Green);
                    int blue = getColorFromPixel(block[i],Colors.Blue);

                    //invert their LSBs
                    red = invertLSB(red);
                    green = invertLSB(green);
                    blue = invertLSB(blue);

                    //build a new pixel
                    Color newPixel = Color.FromArgb(red, green, blue);

                    //change the block pixel
                    block[i] = newPixel;
                }
            }

            return block;
        }

        /**
	     * Gets the RS analysis results for flipping performed on all
	     * pixels.
	     *
	     * @param image The image to analyse.
	     * @param colour The color to analyse.
	     * @param overlap Whether the blocks should overlap.
	     * @return The analysis information for all flipped pixels.
	     */
        private double[] getAllPixelFlips(Colors color, bool overlap)
        {
            //setup the mask for everything...
            int[] allmask = new int[maskSizeX * maskSizeY];
            for (int i = 0; i < allmask.Length; i++)
            {
                allmask[i] = 1;
            }

            int startx = 0, starty = 0;
            Color[] block = new Color[maskSizeX * maskSizeY];
            double numregular = 0, numsingular = 0, numnegregular = 0, numnegsingular = 0, numunusable = 0, numnegunusable = 0;
            double variationB, variationP, variationN;

            while (startx < Width && starty < Height)
            {
                for (int m = 0; m < 2; m++)
                {
                    int k = 0;
                    for (int i = 0; i < maskSizeY; i++)
                    {
                        for (int j = 0; j < maskSizeX; j++)
                        {
                            block[k] = image.GetPixel(startx + j, starty + i);
                            k++;
                        }
                    }

                    //flip all the pixels in the block (NOTE: THIS IS WHAT'S DIFFERENT
                    //TO THE OTHER doAnalysis() METHOD)
                    block = flipBlock(block, allmask);

                    //get the variation the block
                    variationB = getVariation(block, color);

                    //now flip according to the mask
                    block = flipBlock(block, mask[m]);
                    variationP = getVariation(block, color);

                    //flip it back
                    block = flipBlock(block, mask[m]);

                    //invert the mask
                    mask[m] = invertMask(mask[m]);
                    variationN = getNegativeVariation(block, color, mask[m]);
                    mask[m] = invertMask(mask[m]);

                    //now we need to work out which group each belongs to

                    //positive groupings
                    if (variationP > variationB)
                        numregular++;
                    if (variationP < variationB)
                        numsingular++;
                    if (variationP == variationB)
                        numunusable++;

                    //negative mask groupings
                    if (variationN > variationB)
                        numnegregular++;
                    if (variationN < variationB)
                        numnegsingular++;
                    if (variationN == variationB)
                        numnegunusable++;

                    //now we keep going...
                }

                if (overlap)
                    startx += 1;
                else
                    startx += maskSizeX;

                if (startx >= (Width - 1))
                {
                    startx = 0;
                    if (overlap)
                        starty += 1;
                    else
                        starty += maskSizeY;
                }

                if (starty >= (Height - 1))
                    break;
            }

            //save all the results (same order as before)
            double[] results = new double[4];

            results[0] = numregular;
            results[1] = numsingular;
            results[2] = numnegregular;
            results[3] = numnegsingular;

            return results;
        }

        /**
         * Gets the x value for the p=x(x/2) RS equation. See the paper for
         * more details.
         *
         * @param r The value of Rm(p/2).
         * @param rm The value of R-m(p/2).
         * @param r1 The value of Rm(1-p/2).
         * @param rm1 The value of R-m(1-p/2).
         * @param s The value of Sm(p/2).
         * @param sm The value of S-m(p/2).
         * @param s1 The value of Sm(1-p/2).
         * @param sm1 The value of S-m(1-p/2).
         * @return The value of x.
         */
        private double getX(double r, double rm, double r1, double rm1,
                double s, double sm, double s1, double sm1)
        {

            double x = 0; //the cross point.
            double dzero = r - s; // d0 = Rm(p/2) - Sm(p/2)
            double dminuszero = rm - sm; // d-0 = R-m(p/2) - S-m(p/2)
            double done = r1 - s1; // d1 = Rm(1-p/2) - Sm(1-p/2)
            double dminusone = rm1 - sm1; // d-1 = R-m(1-p/2) - S-m(1-p/2)

            //get x as the root of the equation 
            //2(d1 + d0)x^2 + (d-0 - d-1 - d1 - 3d0)x + d0 - d-0 = 0
            //x = (-b +or- sqrt(b^2-4ac))/2a
            //where ax^2 + bx + c = 0 and this is the form of the equation

            //thanks to a good friend in Dunedin, NZ for helping with maths
            //and to Miroslav Goljan's fantastic Matlab code

            double a = 2 * (done + dzero);
            double b = dminuszero - dminusone - done - (3 * dzero);
            double c = dzero - dminuszero;

            if (a == 0)
                //take it as a straight line
                x = c / b;

            //take it as a curve
            double discriminant = Math.Pow(b, 2) - (4 * a * c);

            if (discriminant >= 0)
            {
                double rootpos = ((-1 * b) + Math.Sqrt(discriminant)) / (2 * a);
                double rootneg = ((-1 * b) - Math.Sqrt(discriminant)) / (2 * a);

                //return the root with the smallest absolute value (as per paper)
                if (Math.Abs(rootpos) <= Math.Abs(rootneg))
                    x = rootpos;
                else
                    x = rootneg;
            }
            else
            {
                //maybe it's not the curve we think (straight line)
                double cr = (rm - r) / (r1 - r + rm - rm1);
                double cs = (sm - s) / (s1 - s + sm - sm1);
                x = (cr + cs) / 2;
            }

            if (x == 0)
            {
                double ar = ((rm1 - r1 + r - rm) + (rm - r) / x) / (x - 1);
                double @as = ((sm1 - s1 + s - sm) + (sm - s) / x) / (x - 1);
                if (@as > 0 | ar < 0)
                {
                    //let's assume straight lines again...
                    double cr = (rm - r) / (r1 - r + rm - rm1);
                    double cs = (sm - s) / (s1 - s + sm - sm1);
                    x = (cr + cs) / 2;
                }
            }
            return x;
        }

        /**
        * Negates the LSB of a given byte (stored in an int).
        *
        * @param colorValue The value to negate the LSB.
        * @return The value with negated LSB.
        */
        private int negateLSB(int colorValue)
        {
            int tmp = colorValue & 0xfe;
            if (tmp == colorValue)
                return colorValue | 0x1;
            else
                return tmp;

        }

        /**
        * Inverts the LSB of a given byte (stored in an int).
        * 
        * @param colorValue The value to flip.
        * @return The value with the flipped LSB.
        */
        private int invertLSB(int colorValue)
        {
            if (colorValue == 255)
                return 256;
            if (colorValue == 256)
                return 255;
            return (negateLSB(colorValue + 1) - 1);
        }


        /**
         * Inverts a mask.
         *
         * @param mask The mask to invert.
         * @return The flipped mask.
         */
        private int[] invertMask(int[] mask)
        {
            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] = mask[i] * -1;
            }
            return mask;
        }


    }
}