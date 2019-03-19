namespace Naos.Core.Common
{
    using System.Drawing;
    using System.IO;

    public static class Logo
    {
        public static Bitmap GetLogoAsBitmap()
        {
            var stream = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Core.Common.Resources.logo.png")).BaseStream;
            return new Bitmap(Image.FromStream(stream));
        }

        public static byte[] GetLogoAsBytes()
        {
            return new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Core.Common.Resources.logo.png")).ReadAllBytes();
        }

        public static Bitmap GetIconAsBitmap()
        {
            var stream = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Core.Common.Resources.favicon.ico")).BaseStream;
            return new Icon(stream).ToBitmap();
        }

        public static byte[] GetIconAsBytes()
        {
            return new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Core.Common.Resources.favicon.ico")).ReadAllBytes();
        }

        public static string GetLogoText()
        {
            return @"
    `::`     .:`      :/.       `.::::-`     .-/:/--`
    .oo+-    :o`     /:++-     -o/`   -o/`  +o:`  `o/
    .o-:o:   :o`    /:-:+::   .o+      :o/  +o-`   `
    .o- -o/` :o`   //-..+-:/` :o+      -oo`  .//+/:.`
    .o-  .++.:o`  /:`   -/`:+ .o+      -o/       `-+o.
    .o-   `/o+o` /-......+-`:+ /o-    `++`  ++     /o-
    .+.     -+/ `::::::::/+:`   `:/:-::-    -:/--:/:`
";
        }
    }
}
