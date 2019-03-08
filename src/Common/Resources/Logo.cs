namespace Naos.Core.Common
{
    using System.Drawing;
    using System.IO;

    public static class Logo
    {
        public static Bitmap GetAsBitmap()
        {
            var stream = new BinaryReader(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Naos.Core.Common.Resources.logo.png")).BaseStream;
            return new Bitmap(Image.FromStream(stream));
        }

        public static string GetAsText()
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
