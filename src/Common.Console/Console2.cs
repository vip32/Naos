namespace Naos.Core.Common
{
    using System;
    using System.Drawing;

    public static class Console2
    {
        public static void WriteBitmapLogo()
        {
            WriteBitmap(ResourcesHelper.GetLogoAsBitmap(), 45);
        }

        public static void WriteTextLogo()
        {
            Colorful.Console.WriteLine(ResourcesHelper.GetLogoAsString(), Color.Cyan);
            Colorful.Console.WriteLine(string.Empty, Color.Cyan);
        }

        public static void WriteBitmap(Bitmap image, int sMax = 39)
        {
            var foregroundColor = System.Console.ForegroundColor;
            var backgroundColor = System.Console.BackgroundColor;

            WriteBitmapInternal(image, sMax);

            System.Console.ForegroundColor = foregroundColor;
            System.Console.BackgroundColor = backgroundColor;
        }

        private static void WriteBitmapInternal(Bitmap image, int sMax = 39)
        {
            var percent = Math.Min(decimal.Divide(sMax, image.Width), decimal.Divide(sMax, image.Height));
            var resSize = new Size((int)(image.Width * percent), (int)(image.Height * percent));
            Func<Color, int> toConsoleColor = c =>
            {
                var index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
                index |= (c.R > 64) ? 4 : 0;
                index |= (c.G > 64) ? 2 : 0;
                index |= (c.B > 64) ? 1 : 0;
                return index;
            };
            var bmpMin = new Bitmap(image, resSize);
            for(var i = 0; i < resSize.Height; i++)
            {
                for(var j = 0; j < resSize.Width; j++)
                {
                    System.Console.ForegroundColor = (ConsoleColor)toConsoleColor(bmpMin.GetPixel(j, i));
                    System.Console.Write("██");
                }

                System.Console.WriteLine();
            }
        }
    }
}
