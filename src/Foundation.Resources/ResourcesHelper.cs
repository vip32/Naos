namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

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

        public static string GetHtmlHeaderAsString(string logo = null, string title = null, IDictionary<string, string> menuItems = null)
        {
            logo ??= GetLogoAsString();
            menuItems ??= new Dictionary<string, string>
            {
                { "/api", "api" },
                { "/api/operations/health/dashboard", "health" },
                { "/api/operations/logevents/dashboard", "logevents" },
                { "/api/operations/logtraces/dashboard", "traces" },
                { "/api/operations/logevents/dashboard?q=TrackType=journal", "journal" },
                { "/swagger/index.html", "swagger" }
            };

            //<link href='css/naos/bootstrap.min.css' rel ='stylesheet' />
            //<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css' integrity='sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T' crossorigin='anonymous'>
            //<script src='https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js' integrity='sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM' crossorigin='anonymous'></script>
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width' />
    <title>Naos</title>
    <base href='/' />
    <link rel='stylesheet' href='https://use.fontawesome.com/releases/v5.0.10/css/all.css' integrity='sha384-+d0P83n9kaQMCwj8F4RJB66tzIwOKmrdb46+porD/OvrJ+37WqIM7UoBtwHO6Nlg' crossorigin='anonymous'>
    <link href='css/naos/styles.css' rel ='stylesheet' />
</head>
<body>
    <span style='/*display: inline-block;*/'>
        <pre style='color: cyan;font-size: xx-small;'>
        {logo}
        </pre>
    </span>
    <span style='color: grey;font-size: xx-small;'>
        &nbsp;&nbsp;&nbsp;&nbsp;
        {title}
    </span>
    <hr />
    <div>
      &nbsp;&nbsp;&nbsp;{menuItems.Safe().Select(m => $"<a href='{m.Key}'>{m.Value}</a>").ToString("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;")}
    </div>
";
        }

        public static string GetHtmlFooterAsString()
        {
            return @"
</body>
</html>
";
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
