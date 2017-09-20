using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Foundation;
using Foundation.CustomConfig;

namespace ComaDataAPI.Controllers {
  [RoutePrefix("api/files")]
  public class FileController : ApiController {
    class Configer : Config<Configer> {
      [appSettings]
      public string DataPath => KeyValue();
    }
    [Route("HelloWorld")]
    public IHttpActionResult Get() {
      return Ok(from path in CommonExtensions.Helpers.FindFiles("*.txt","App_Data")
              select new { path, text = System.IO.File.ReadAllText(path) });
    }
  }
}