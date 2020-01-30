namespace Naos.Foundation.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public static class HttpResponseExtensions
    {
        public static async Task WriteNaosDashboard(
            this HttpResponse source,
            string header = null,
            string title = null,
            IEnumerable<DashboardMenuItem> menuItems = null,
            string styles = "css/naos/styles.css",
            Action<HttpResponse> action = null)
        {
            header ??= ResourcesHelper.GetLogoAsString();
            menuItems ??= new List<DashboardMenuItem>
            {
                // TODO: make this more discoverable
                new DashboardMenuItem("api", "/api"),
                new DashboardMenuItem("health", "/api/operations/health/dashboard"),
                new DashboardMenuItem("logevents", "/api/operations/logevents/dashboard"),
                new DashboardMenuItem("traces", "/api/operations/logtraces/dashboard"),
                new DashboardMenuItem("journal", "/api/operations/logevents/dashboard?q=TrackType=journal"),
                new DashboardMenuItem("swagger", "/swagger/index.html"),
            };

            source.ContentType = ContentType.HTML.ToValue();
            await source.WriteAsync($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width' />
    <title>Naos</title>
    <base href='/' />
    <link rel='stylesheet' href='https://use.fontawesome.com/releases/v5.0.10/css/all.css' integrity='sha384-+d0P83n9kaQMCwj8F4RJB66tzIwOKmrdb46+porD/OvrJ+37WqIM7UoBtwHO6Nlg' crossorigin='anonymous'>
    <link href='{styles}' rel ='stylesheet' />
</head>
<body>
    <span style='/*display: inline-block;*/'>
        <pre style='color: cyan;font-size: xx-small;margin-left: -15px'>
        {header}
        </pre>
    </span>
    <span style='color: grey;font-size: xx-small;'>
        {title}
    </span>
    <hr />
    <div style='padding-bottom: 10px;'>
      {menuItems.Safe().Select(m => $"<a href='{m.Url}'>{m.Name}</a>").ToString("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;")}
    </div>
    <script type='text/javascript'>
        (function(anchors, url, i, a) {{ // highlight current menuitem
          while ((a = anchors[i++]) && a.classList)
            a.href === url && a.classList.add('current');
        }}(document.getElementsByTagName('a'), location.href, 0));
    </script>
").AnyContext();

            try
            {
                action?.Invoke(source);
            }
            catch
            {
                // TODO: log?
            }
            finally
            {
                // TODO: render some default footer

                try
                {
                    await source.WriteAsync($@"
<hr/>
{DateTime.UtcNow:o}
</body>
</html>
").AnyContext();
                }
                catch
                {
                    // TODO: log?
                }
            }
        }
    }
}
