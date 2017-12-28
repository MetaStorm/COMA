using Jira;
using Microsoft.Restier.Providers.EntityFramework;
using Microsoft.Restier.Publishers.OData;
using Microsoft.Restier.Publishers.OData.Batch;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Extensions;
using Wcf.ProxyMonads;

namespace ComaDataAPI {
  public static class WebApiConfig {
    static WebApiConfig() {
      var _workflowSchemeIds = new RestMonad().GetWorlflowSchemeIdsAsync().GetAwaiter().GetResult().Value.Select(int.Parse).ToArray();
      RestConfiger.WorkflowSchemaIdsProvider = () => _workflowSchemeIds;
      //RestConfiger.WorkflowSchemaIdsProvider = tSQLt.tSQLtEntities.WorkflowSchemaIds;
      RestConfiger.ProjectIssueTypeWorkflowProvider = RestConfiger.GetProjectIssueTypeWorkflowAsync;
      //tSQLt.tSQLtEntities.GetProjectIssueTypeWorkflow;
    }
    public async static void Register(HttpConfiguration config) {
      config.MapHttpAttributeRoutes();
      // https://stackoverflow.com/questions/9847564/how-do-i-get-asp-net-web-api-to-return-json-instead-of-xml-using-chrome
      //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

      var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
      config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
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
