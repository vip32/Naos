namespace Naos.Core.Authentication.App.Web
{
    using System;
    using System.Threading.Tasks;

    public static class ValidationFunctions
    {
        public static Func<ValidationContext, Task> StaticApiKeyValidation()
        {
            return context =>
            {
                //byte[] headerValueBytes = Convert.FromBase64String(context.ApiKey);
                // etc

                //if (context.ApiKey == "1")
                //{
                    context.Success();
                //}

                return Task.CompletedTask;
            };
        }
    }
}
