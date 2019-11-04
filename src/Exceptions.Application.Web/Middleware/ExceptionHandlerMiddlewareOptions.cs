namespace Naos.ServiceExceptions.Application.Web
{
    public class ExceptionHandlerMiddlewareOptions
    {
        public bool HideDetails { get; set; }

        public bool JsonResponse { get; set; } = true;
    }
}