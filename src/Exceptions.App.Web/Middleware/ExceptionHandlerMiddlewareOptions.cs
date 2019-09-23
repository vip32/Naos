namespace Naos.ServiceExceptions.App.Web
{
    public class ExceptionHandlerMiddlewareOptions
    {
        public bool HideDetails { get; set; }

        public bool JsonResponse { get; set; } = true;
    }
}