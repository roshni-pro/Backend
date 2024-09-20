using AngularJSAuthentication.Model;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using System.Web.Http;

namespace AngularJSAuthentication.API.App_Start
{
    public static class OdataConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes(); //This has to be called before the following OData mapping, so also before 
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<WarehouseCategory>("CategoryOdata");
            builder.EntitySet<SubsubCategory>("SubsubCategoryOdata");
            config.MapODataServiceRoute(
                  routeName: "ODataRoute",
                  routePrefix: "oapi",
                  model: builder.GetEdmModel());

        }
    }
}