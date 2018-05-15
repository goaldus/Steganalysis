using System.Drawing;

namespace Steganalysis
{
    public static class LSBEnhancement
    {
        /// <summary>
        /// Creates an image with enhanced LSB
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Bitmap createLSBEnhancementImage(Bitmap image)
        {
            Bitmap newImage = new Bitmap(image);
            int width = newImage.Width;
            int height = newImage.Height;
            Color pixel;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pixel = newImage.GetPixel(x, y);
                    newImage.SetPixel(x, y, enhanceLSBInPixel(pixel));
                }
            }

            return newImage;
        }


        private static Color enhanceLSBInPixel(Color pixel)
        {
            int redColor = pixel.R;
            int greenColor = pixel.G;
            int blueColor = pixel.B;

            if ((redColor & 0x01) == 1)
                redColor = 255;
            else
                redColor = 0;

            if ((greenColor & 0x01) == 1)
                greenColor = 255;
            else
                greenColor = 0;

            if ((blueColor & 0x01) == 1)
                blueColor = 255;
            else
                blueColor = 0;

            return Color.FromArgb(redColor, greenColor, blueColor);
        }


    }
}
