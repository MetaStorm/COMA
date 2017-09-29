using Microsoft.Restier.Providers.EntityFramework;
using Microsoft.Restier.Publishers.OData;
using Microsoft.Restier.Publishers.OData.Batch;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Extensions;

namespace ComaDataAPI {
  public static class WebApiConfig {
    public async static void Register(HttpConfiguration config) {
      config.MapHttpAttributeRoutes();
      // https://stackoverflow.com/questions/9847564/how-do-i-get-asp-net-web-api-to-return-json-instead-of-xml-using-chrome
      config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
      // Redirect root to Swagger UI      //config.Routes.MapHttpRoute(name: "Swagger UI", routeTemplate: "", defaults: null, constraints: null, handler: new RedirectHandler(SwaggerDocsConfig.DefaultRootUrlResolver, "swagger/ui/index"));
      // Enable CORS
      var cors = new EnableCorsAttribute("*", "*", "*");
      cors.SupportsCredentials = true;
      config.EnableCors(cors);
      // Restier
      config.Filter().Expand().Select().OrderBy().MaxTop(null).Count();
      await config.MapRestierRoute<EntityFrameworkApi<COMA.ComaContext>>(
          "ComaData",
          "dpi/ComaData",
          new RestierBatchHandler(GlobalConfiguration.DefaultServer));
    }
  }
}
