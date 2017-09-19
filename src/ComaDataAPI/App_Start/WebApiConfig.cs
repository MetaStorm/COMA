using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.Restier.Providers.EntityFramework;
using Microsoft.Restier.Publishers.OData;
using Microsoft.Restier.Publishers.OData.Batch;

namespace ComaDataAPI {
  public static class WebApiConfig {
    public async static void Register(HttpConfiguration config) {
      // Web API configuration and services

      // Web API routes
      config.MapHttpAttributeRoutes();

      config.Filter().Expand().Select().OrderBy().MaxTop(null).Count();
      await config.MapRestierRoute<EntityFrameworkApi<COMA.ComaContext>>(
          "ComaData",
          "dpi/ComaData",
          new RestierBatchHandler(GlobalConfiguration.DefaultServer));
    }
  }
}
