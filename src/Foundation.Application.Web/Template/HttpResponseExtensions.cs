namespace Naos.Foundation.Application
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public static class HttpResponseExtensions
    {
        public static async Task WriteNaosTemplate(this HttpResponse source, string header, string subHeader, string[] menuItems, string css = "css/naos/styles.css")
        {
            await source.WriteAsync(@"
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
        " + header + @"
        </pre>
    </span>
    <span style='color: grey;font-size: xx-small;'>
        &nbsp;&nbsp;&nbsp;&nbsp;"
+ subHeader + @"
    </span>
    <hr />
    &nbsp;&nbsp;&nbsp;<a href='/api'>infos</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/health'>health</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard'>logs</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logtraces/dashboard'>traces</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='/api/operations/logevents/dashboard?q=TrackType=journal'>journal</a></br>
</body>
</html>
").AnyContext();
        }
    }
}
