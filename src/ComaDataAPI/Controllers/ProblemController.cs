using CommonExtensions;
using Foundation;
using Foundation.CustomConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ComaDataAPI.Controllers {
  [RoutePrefix("api/problem")]
  public class ProblemController :ApiController {

    [HttpGet, Route("RunTestLang")]
    public IHttpActionResult RunTestLang() {
      try {
        return Ok(RunTestable<EMAIL>.RunTests(null));
      } catch(Exception exc) {
        return Content(HttpStatusCode.InternalServerError, exc.Messages());
      }
    }

    public class EMAIL :RunTestable<EMAIL> {
      public static class LANGS {
        public class PROBLEM :Helpers.Languages<PROBLEM> {
          static PROBLEM() => Set = _ => LoadFromPath("txt");
        }
        public class Farewells :Helpers.Languages<Farewells> {
          static Farewells() => Set = _ => LoadFromPath("txt");
        }
      }
    }
    [ProblemConfig]
    private class ConfigerKey :EventLogger<ConfigerKey> {
      private class ProblemConfig :ConfigFileAttribute { }
      private class ProblemSettings :ConfigSectionAttribute { }
      [ProblemSettings]
      public string DataPathProblem => KeyValue(null, "DataPathProblem");
    }
  }
}