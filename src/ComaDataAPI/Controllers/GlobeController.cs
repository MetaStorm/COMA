using CommonExtensions;
using Foundation;
using Foundation.CustomConfig;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ComaDataAPI.Controllers {
  [RoutePrefix("api/globe")]
  public class GlobeController :ApiController {

    [Route("HelloWold"), HttpGet]
    public IHttpActionResult HelloWold(string language = "en") => Ok(EMAIL.LANGS.GREETINGS.Get(language));

    [Route("FarewellWold"), HttpGet]
    public IHttpActionResult FarewellWold(string language = "en") => Ok(EMAIL.LANGS.Farewells.Get(language));

    [Route("RunTest"), HttpGet]
    public IHttpActionResult RunTest() => Ok(EMAIL.RunTests());

    #region Configer
    class Configer :EventLogger<Configer> {
      [appSettings]
      public string DataPath => KeyValue();
      #region RunTest
      protected override async Task<ExpandoObject> _RunTestAsync(ExpandoObject parameters, params Func<ExpandoObject, ExpandoObject>[] merge) {
        return await TestHostAsync(parameters, async (p, m) => {
          var e = await base._RunTestAsync(parameters, merge);
            return e.AddOrMerge("langs", EMAIL.RunTests());
        }, _LogError, merge);
      }
      #endregion
    }
    #endregion

    public class EMAIL :RunTestable<EMAIL> {
      public static class LANGS {

        public class GREETINGS :Helpers.Languages<GREETINGS> {
          static GREETINGS() => Set = _ => LoadFromPath("txt");
        }

        public class Farewells :Helpers.Languages<Farewells> {
          static Farewells() => Set = _ => LoadFromPath("txt");
        }

      }
    }
  }
}