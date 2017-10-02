using CommonExtensions;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ComaDataAPI.Controllers {
  [RoutePrefix("api/mongo")]
  public class MongoController :ApiController {
    class Configer :Foundation.EventLogger<Configer> {
      [Foundation.CustomConfig.ConfigValue]
      public static string MongoUri => KeyValue();
      public static string MongoPassword => KeyValue();
      public static string MongoUriFull => string.Format(MongoUri, MongoPassword);

      #region RunTest
      protected override async Task<ExpandoObject> _RunTestAsync(ExpandoObject parameters, params Func<ExpandoObject, ExpandoObject>[] merge) {
        return await TestHostAsync(parameters, async (p, m) => {
          var e = await base._RunTestAsync(parameters, merge);
          var database = "forex";
          var collection = "ToDo";
          try {
            var todos = HedgeHog.MongoExtensions.ReadCollectionAnon(new ExpandoObject(),  MongoUriFull, database, collection)
            .Take(1)
            .ToArray();
            return e.Merge(new { todos });
          } catch(Exception exc) {
            throw new Exception(new { database, collection } + "", exc);
          }
        }, _LogError, merge);
      }
      #endregion

    }
    [HttpGet, Route("Todos")]
    public IHttpActionResult Todos() {
      var database = "forex";
      var collection = "ToDo";
      var todo = new { _id = ObjectId.Empty, What = "", When = "" };
      return Ok(HedgeHog.MongoExtensions.ReadCollectionAnon(todo, Configer.MongoUriFull, database, collection).ToArray());
    }
    [HttpGet, Route("RunTest")]
    public async Task<IHttpActionResult> RunTest(bool unTest=true) {
      if(unTest) Configer.UnTest();
      return Ok(await Configer.RunTestAsync(null));
    }
  }
}
