namespace Naos.Core.App.ExceptionHandling.Web
{
    public class ExceptionHandlerMiddlewareOptions
    {
        public bool HideDetails { get; set; }

        public bool JsonResponse { get; set; } = true;
    }
}