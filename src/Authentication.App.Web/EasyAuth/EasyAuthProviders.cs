namespace Naos.Authentication.App.Web
{
    public struct EasyAuthProviders // https://docs.microsoft.com/en-us/azure/app-service/app-service-authentication-how-to?WT.mc_id=easyauth-github-marouill
    {
        public const string AzureActiveDirectory = "aad";

        public const string Google = "google";

        public const string Facebook = "facebook";

        public const string Twitter = "twitter";

        public const string MicrosoftAccount = "microsoftaccount";
    }
}
