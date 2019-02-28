namespace Naos.Core.Common
{
    using System;
    using System.Drawing;
    using System.IO;

    public static class CommonConsole
    {
        public static void WriteNaosBitmapLogo()
        {
            WriteImage("Naos.Core.Common.Resources.logo.png", 45);
        }

        public static void WriteNaosTextLogo()
        {
            // generated: https://www.text-image.com/convert/pic2ascii.cgi
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
    `::`     .:`      :/.       `.::::-`     .-/:/--`
    .oo+-    :o`     /:++-     -o/`   -o/`  +o:`  `o/
    .o-:o:   :o`    /:-:+::   .o+      :o/  +o-`   `
    .o- -o/` :o`   //-..+-:/` :o+      -oo`  .//+/:.`
    .o-  .++.:o`  /:`   -/`:+ .o+      -o/       `-+o.
    .o-   `/o+o` /-......+-`:+ /o-    `++`  ++     /o-
    .+.     -+/ `::::::::/+:`   `:/:-::-    -:/--:/:`
");
            Console.ForegroundColor = foregroundColor;
        }

        public static void WriteImage(string resource, int sMax = 39)
        {
            var foregroundColor = Console.ForegroundColor;
            var backgroundColor = Console.BackgroundColor;

            var str = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)).BaseStream;
            var bm = new Bitmap(Image.FromStream(str));
            WriteImage(bm, sMax);

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
        }

        public static void WriteImage(Bitmap bmpSrc, int sMax = 39)
        {
            decimal percent = Math.Min(decimal.Divide(sMax, bmpSrc.Width), decimal.Divide(sMax, bmpSrc.Height));
            Size resSize = new Size((int)(bmpSrc.Width * percent), (int)(bmpSrc.Height * percent));
            Func<Color, int> toConsoleColor = c =>
            {
                int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
                index |= (c.R > 64) ? 4 : 0;
                index |= (c.G > 64) ? 2 : 0;
                index |= (c.B > 64) ? 1 : 0;
                return index;
            };
            Bitmap bmpMin = new Bitmap(bmpSrc, resSize);
            for (int i = 0; i < resSize.Height; i++)
            {
                for (int j = 0; j < resSize.Width; j++)
                {
                    Console.ForegroundColor = (ConsoleColor)toConsoleColor(bmpMin.GetPixel(j, i));
                    Console.Write("██");
                }

                Console.WriteLine();
            }
        }
    }
}
