/*
 *    Digital Invisible Ink Toolkit
 *    Copyright (C) 2005  K. Hempstalk	
 *    Original C++ Code is copyright to Zhe Wang et al, McMaster University.
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
 *
 *		@author Kathryn Hempstalk
 *
 */

using System;
using System.Drawing;

/**
* Sample pairs analysis for an image.
* <P>
* Sample pairs analysis is a technique for detecting
* steganography in an image.  More information can be found
* in the paper "Detection of LSB steganography via Sample Pair analysis".
* This implementation is based off some C++ code kindly provided by 
* the authors of the paper.
*
* @author Kathryn Hempstalk
* 
* Modified by Ondrej Molnar
* Rewrote in c# with little modifications like checking for odd size (Arguments of getPixel function has to be less than size height or width)
* or making absolute value of result
* Original code here: https://github.com/b3dk7/StegExpose/blob/master/SamplePairs.java
*/

namespace Steganalysis
{
    public class SamplePairs : HelperFunctions ,IDetector
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SamplePairs(int width, int height, Bitmap bitmap)
        {
            if (width % 2 == 1)
                this.Width = width - 1;
            else
                this.Width = width;

            if (height % 2 == 1)
                this.Height = height - 1;
            else
                this.Height = height;

            this.image = bitmap;
        }

        public double analyze()
        {
            double average = 0.0;
            average = analyze(Colors.Red);
            average += analyze(Colors.Green);
            average += analyze(Colors.Blue);

            average = average / 3.0;
            average = Math.Abs(average);
            if (average > 1)
                return 1;
            else
                return average;
        }

        public double analyze(Colors color)
        {
            int u, v;
            long P = 0, X = 0, Y = 0, Z = 0, W = 0;

            // pairs across the image
            for (int starty = 0; starty < Height; starty++)
            {
                for (int startx = 0; startx < Width; startx += 2)
                {
                    u = getColorFromPixel(startx, starty, color);
                    v = getColorFromPixel(startx + 1, starty, color);

                    //if the 7 msb are the same, but the 1 lsb are different
                    if ((u >> 1 == v >> 1) && ((v & 0x1) != (u & 0x1)))
                        W++;
                    //if the pixels are the same
                    if (u == v)
                        Z++;
                    //if lsb(v) = 0 & u < v OR lsb(v) = 1 & u > v
                    if ((v == (v >> 1) << 1) && (u < v) || (v != (v >> 1) << 1) && (u > v))
                        X++;
                    //vice versa
                    if ((v == (v >> 1) << 1) && (u > v) || (v != (v >> 1) << 1) && (u < v))
                        Y++;
                    P++;
                }
            }

            //pairs down the image
            for (int starty = 0; starty < Height; starty += 2)
            {
                for (int startx = 0; startx < Width; startx++)
                {
                    u = getColorFromPixel(startx, starty, color);
                    v = getColorFromPixel(startx, starty + 1, color);

                    //if the 7 msb are the same, but the 1 lsb are different
                    if ((u >> 1 == v >> 1) && ((v & 0x1) != (u & 0x1)))
                        W++;
                    //the pixels are the same
                    if (u == v)
                        Z++;
                    //if lsb(v) = 0 & u < v OR lsb(v) = 1 & u > v
                    if ((v == (v >> 1) << 1) && (u < v) || (v != (v >> 1) << 1) && (u > v))
                        X++;
                    //vice versa
                    if ((v == (v >> 1) << 1) && (u > v) || (v != (v >> 1) << 1) && (u < v))
                        Y++;
                    P++;
                }
            }

            //solve the quadratic equation
            //in the form ax^2 + bx + c = 0
            double a = 0.5 * (W + Z);
            double b = 2 * X - P;
            double c = Y - X;

            //the result
            double x;

            //straight line
            if (a == 0)
                x = c / b;

            //curve
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
                x = c / b;
            }

            if (x == 0)
            {
                //let's assume straight lines again, something is probably wrong
                x = c / b;
            }

            return x;

        }
    }
}
