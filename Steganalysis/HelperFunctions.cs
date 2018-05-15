using System.Drawing;

namespace Steganalysis
{
    public abstract class HelperFunctions
    {
        protected Bitmap image;
        public enum Colors
        {
            Red,
            Green,
            Blue
        }

        /// <summary>
        /// Returns specified color channel from directions x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        protected int getColorFromPixel(int x, int y, Colors color)
        {
            if (color == Colors.Red)
                return image.GetPixel(x, y).R;
            else if (color == Colors.Green)
                return image.GetPixel(x, y).G;
            else
                return image.GetPixel(x, y).B;
        }

        /// <summary>
        /// Returns specified color channel from pixel
        /// </summary>
        /// <param name="block"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        protected int getColorFromPixel(Color block, Colors color)
        {
            if (color == Colors.Red)
                return block.R;
            else if (color == Colors.Green)
                return block.G;
            else
                return block.B;
        }
    }
}
