using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Extensions;
using Microsoft.Restier.Providers.EntityFramework;
using Microsoft.Restier.Publishers.OData;
using Microsoft.Restier.Publishers.OData.Batch;
using Swashbuckle.Application;

namespace ComaDataAPI {
  public static class WebApiConfig {
    public async static void Register(HttpConfiguration config) {
      // Web API configuration and services

      // Web API routes
      config.MapHttpAttributeRoutes();

      // Redirect root to Swagger UI
      //config.Routes.MapHttpRoute(name: "Swagger UI", routeTemplate: "", defaults: null, constraints: null, handler: new RedirectHandler(SwaggerDocsConfig.DefaultRootUrlResolver, "swagger/ui/index"));

      // Enable CORS
      var cors = new EnableCorsAttribute("*", "*", "*");
      cors.SupportsCredentials = true;
      config.EnableCors(cors);

      config.Filter().Expand().Select().OrderBy().MaxTop(null).Count();
      await config.MapRestierRoute<EntityFrameworkApi<COMA.ComaContext>>(
          "ComaData",
          "dpi/ComaData",
          new RestierBatchHandler(GlobalConfiguration.DefaultServer));
    }
  }
}
