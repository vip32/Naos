https://jwt.io/introduction/
https://jasonwatmore.com/post/2018/08/14/aspnet-core-21-jwt-authentication-tutorial-with-example-api
https://jasonwatmore.com/post/2019/01/08/aspnet-core-22-role-based-authorization-tutorial-with-example-api
https://jasonwatmore.com/post/2018/06/26/aspnet-core-21-simple-api-for-authentication-registration-and-user-management

https://github.com/sodalom/simple-transaction/blob/master/src/Services/Identity/Controllers/UserController.cs


services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })




https://developer.okta.com/blog/2018/03/23/token-authentication-aspnetcore-complete-guide
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.Authority = "{yourAuthorizationServerAddress}";
		options.Audience = "{yourAudience}";
	});

--------------------------------------------------------------------------------
POLICY BASED AUTH + JWT (AAD)
https://joonasw.net/view/azure-ad-authentication-aspnet-core-api-part-1
https://joonasw.net/view/azure-ad-authentication-aspnet-core-api-part-2
https://github.com/juunas11/Joonasw.AzureAdApiSample