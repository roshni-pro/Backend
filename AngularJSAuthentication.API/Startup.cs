using AngularJSAuthentication.API.App_Start;
using AngularJSAuthentication.API.Handler;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Providers;
using AngularJSAuthentication.Infrastructure;
using AngularJSAuthentication.Providers;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Mongo;
using Hangfire.RecurringDateRange.Server;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Jwt;

using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel.Security.Tokens;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Optimization;
using System.Web.Routing;
using Elastic;
using Elastic.Apm;
using Elastic.Apm.Logging;
using NLog;
using AngularJSAuthentication.Common.Helpers;
using Elastic.Apm.SqlClient;
using MongoDB.Driver;

[assembly: OwinStartup(typeof(AngularJSAuthentication.API.Startup))]

namespace AngularJSAuthentication.API
{
    public class Startup
    {
        string Url = ConfigurationManager.AppSettings["BaseURL"];
        public static string MLExePath = ConfigurationManager.AppSettings["MLExePath"];
        public static string CloudName = ConfigurationManager.AppSettings["CloudName"];
        public static string APIKey = ConfigurationManager.AppSettings["APIKey"];
        public static string APISecret = ConfigurationManager.AppSettings["APISecret"];
        public static string smsauthKey = ConfigurationManager.AppSettings["SMSKey"];
        public static string FreezedAsignmentCopyFilePath = ConfigurationManager.AppSettings["FreezedAsignmentCopyFilePath"];

        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        public static GoogleOAuth2AuthenticationOptions googleAuthOptions { get; private set; }
        public static FacebookAuthenticationOptions facebookAuthOptions { get; private set; }
        public void Configuration(IAppBuilder app)
        {

            app.Map("/signalr", map =>
            {
                // Setup the CORS middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {

                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    // EnableJSONP = true
                };
                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch already runs under the "/signalr"
                // path.
                map.RunSignalR(hubConfiguration);
            });

            HttpConfiguration config = new HttpConfiguration();
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            //if (ConfigurationManager.AppSettings["EnableTrace"]=="true")
            //{
            config.MessageHandlers.Add(new MessageLoggingHandler());
            config.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());
            //}
            ConfigureOAuth(app);
            OdataConfig.Register(config);
            WebApiConfig.Register(config);

            if (Agent.IsConfigured)
                Agent.Subscribe(new SqlClientDiagnosticSubscriber());

            //app.UseEastic(config);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigureOAuthTokenConsumption(app);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);

            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<AuthContext, AngularJSAuthentication.API.Migrations.Configuration>());

            var migrationOptions = new MongoMigrationOptions
            {
                Strategy = MongoMigrationStrategy.Drop,
                BackupStrategy = MongoBackupStrategy.None,
            };
            var storageOptions = new MongoStorageOptions
            {
                MigrationOptions = migrationOptions,
            };

#if !DEBUG
            var useHangefire = Convert.ToBoolean(ConfigurationManager.AppSettings["UseHangFire"]);
            if (useHangefire)
            {
                // Hangfire.GlobalConfiguration.Configuration.UseMongoStorage(ConfigurationManager.AppSettings["mongoConnStr"], ConfigurationManager.AppSettings["mongoHangFireDbName"], storageOptions);

                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(ConfigurationManager.AppSettings["mongoConnStr"]));
                //settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                settings.ConnectTimeout = TimeSpan.FromMinutes(1);
                settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(10);

                var mongoStorage = new MongoStorage(settings, ConfigurationManager.AppSettings["mongoHangFireDbName"], storageOptions);
                JobStorage.Current = mongoStorage;

                //Hangfire.GlobalConfiguration.Configuration.UseMongoStorage(mongoStorage)

                app.UseHangfireServer(new RecurringDateRangeJobScheduler());

                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new MyAuthorizationFilter() },
                    AppPath = VirtualPathUtility.ToAbsolute("~"),

                });
                OrderMasterChangeDetectManager.StartDetect();
            }
#endif
            //OrderMasterChangeDetectManager.GenerateConsent();
            //app.UseHangfireDashboard("/hangfire", dashboardoptions);
        }
        public void ConfigureOAuth(IAppBuilder app)
        {
            //use a cookie to temporarily store information about a user logging in with a third party login provider
            app.CreatePerOwinContext(AuthContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            app.UseExternalSignInCookie(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie);
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),

                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(500),
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleRefreshTokenProvider(),
                AccessTokenFormat = new CustomJwtFormat(Url)
            };

            // Token Generation           
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);

            //Configure Google External Login
            googleAuthOptions = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "xxxxxx",
                ClientSecret = "xxxxxx",
                Provider = new GoogleAuthProvider()
            };
            app.UseGoogleAuthentication(googleAuthOptions);

            //Configure Facebook External Login
            facebookAuthOptions = new FacebookAuthenticationOptions()
            {
                AppId = "xxxxxx",
                AppSecret = "xxxxxx",
                Provider = new FacebookAuthProvider()
            };
            app.UseFacebookAuthentication(facebookAuthOptions);

        }
        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            var issuer = Url;
            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    //IssuerSecurityTokenProviders = new IssuedSecurityTokenProvider[]
                    //{
                    //    new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    //}
                    //IssuerSecurityKeyProviders=new List<IssuedSecurityTokenProvider>
                    //{


                    //}
                    IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[]
                     {
                         new SymmetricKeyIssuerSecurityKeyProvider(issuer,audienceSecret)
                     }
                });
        }
    }

    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In case you need an OWIN context, use the next line, `OwinContext` class
            // is the part of the `Microsoft.Owin` package.
            //var owinContext = new OwinContext(context.GetOwinEnvironment());

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return true;//owinContext.Authentication.User.Identity.IsAuthenticated;
        }
    }


    internal class ApmLoggerBridge : IApmLogger
    {
        private readonly Lazy<Logger> _logger;

        public bool IsEnabled(Elastic.Apm.Logging.LogLevel level)
        {
            return true;
        }

        public void Log<TState>(Elastic.Apm.Logging.LogLevel level, TState state, Exception e, Func<TState, Exception, string> formatter)
        {
            TextFileLogHelper.TraceLog(e.ToString());
        }
    }

}