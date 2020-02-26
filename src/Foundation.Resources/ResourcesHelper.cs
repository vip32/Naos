namespace Naos.Foundation
{
    using System.Drawing;
    using System.IO;

    public static class ResourcesHelper
    {
        public static Bitmap GetLogoAsBitmap()
        {
            using (var reader = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Foundation.Resources.logo.png")))
            {
                return new Bitmap(Image.FromStream(reader.BaseStream));
            }
        }

        public static byte[] GetLogoAsBytes()
        {
            using (var reader = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Foundation.Resources.logo.png")))
            {
                return reader?.ReadAllBytes();
            }
        }

        public static Bitmap GetIconAsBitmap()
        {
            using var reader = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Foundation.Resources.favicon.ico"));
            using (var icon = new Icon(reader.BaseStream))
            {
                return icon.ToBitmap();
            }
        }

        public static byte[] GetIconAsBytes()
        {
            using (var reader = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Foundation.Resources.favicon.ico")))
            {
                return reader?.ReadAllBytes();
            }
        }

        public static byte[] GetStylesAsBytes()
        {
            using (var reader = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Foundation.Resources.styles.css")))
            {
                return reader?.ReadAllBytes();
            }
        }

        public static string GetStylesAsString()
        {
            using (var reader = new StreamReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Foundation.Resources.styles.css")))
            {
                return reader?.ReadToEnd();
            }
        }

        public static string GetSwaggerStylesAsString()
        {
            using (var reader = new StreamReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Foundation.Resources.swagger.css")))
            {
                return reader?.ReadToEnd();
            }
        }

        public static string GetLogoAsString()
        {
            // generated: https://www.text-image.com/convert/pic2ascii.cgi
            return @"
    `::`     .:`      :/.       `.::::-`     .-/:/--`
    .oo+-    :o`     /:++-     -o/`   -o/`  +o:`  `o/
    .o-:o:   :o`    /:-:+::   .o+      :o/  +o-`   `
    .o- -o/` :o`   //-..+-:/` :o+      -oo`  .//+/:.`
    .o-  .++.:o`  /:`   -/`:+ .o+      -o/       `-+o.
    .o-   `/o+o` /-......+-`:+ /o-    `++`  ++     /o-
    .+.     -+/ `::::::::/+:`   `:/:-::-    -:/--:/:`";
        }
    }
}
