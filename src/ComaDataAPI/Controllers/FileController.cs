using System;
using System.Linq;
using System.Web.Http;
using Foundation;
using Foundation.CustomConfig;
using System.Threading.Tasks;
using System.Dynamic;
using CommonExtensions;
using System.IO;

namespace ComaDataAPI.Controllers {
  [RoutePrefix("api/files")]
  public class FileController :ApiController {
    [Route("HelloWorld")]
    [HttpGet]
    public IHttpActionResult HelloWorld() {
      return Ok(from path in CommonExtensions.Helpers.FindFiles("*.txt", "App_Data")
                select new { path, text = System.IO.File.ReadAllText(path) });
    }
    [Route("RunTest")]
    [HttpGet]
    public async Task<IHttpActionResult> RunTest(bool unTest = false) {
      if(unTest) Configer.UnTest();
      return Ok(await Configer.RunTestAsync(null));
    }

    #region Configer
    class Configer :EventLogger<Configer> {
      [appSettings]
      public string DataPath => KeyValue();
      #region RunTest
      protected override async Task<ExpandoObject> _RunTestAsync(ExpandoObject parameters, params Func<ExpandoObject, ExpandoObject>[] merge) {
        return await TestHostAsync(parameters, async (p, m) => {
          var e = await base._RunTestAsync(parameters, merge);
          try {
            var appDataPath = Path.Combine(DataPath, Path.GetRandomFileName());
            var testFile = Helpers.MapExecPath(appDataPath);
            using(var s = File.CreateText(testFile))
              s.WriteLine("xxx");
            File.Delete(testFile);
            return e.AddOrMerge("DataPath Access",  new { testFile, access = "Read/Write" });
          } catch(Exception exc) {
            throw new Exception(new { ConfigValue = new { DataPath } } + "", exc);
          }
        }, _LogError, merge);
      }
      #endregion
    }
    #endregion
  }
}
