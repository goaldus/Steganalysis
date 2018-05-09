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

        protected int getColorFromPixel(int x, int y, Colors color)
        {
            if (color == Colors.Red)
                return image.GetPixel(x, y).R;
            else if (color == Colors.Green)
                return image.GetPixel(x, y).G;
            else
                return image.GetPixel(x, y).B;
        }

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
