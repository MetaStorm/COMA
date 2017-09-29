using System;
using System.Linq;
using System.Web.Http;
using Foundation;
using Foundation.CustomConfig;
using System.Threading.Tasks;
using System.Dynamic;
using CommonExtensions;
using System.IO;
using System.Net;

namespace ComaDataAPI.Controllers {
  [RoutePrefix("api/files")]
  public class FileController :ApiController {
    static string _helloFile = "Hello*.txt";
    [Route("HelloWorld")]
    [HttpGet]
    public IHttpActionResult HelloWorld() {
      return base.Ok(from path in FetchHellFiles()
                     select new { path, text = File.ReadAllText(path) });
    }
    static System.Collections.Generic.IEnumerable<string> FetchHellFiles() => CommonExtensions.Helpers.FindFiles(_helloFile, "App_Data");

    [Route("RunTest")]
    [HttpGet]
    public async Task<IHttpActionResult> RunTest(bool unTest = false) {
      if(unTest) Configer.UnTest();
      try {
        return Ok(await Configer.RunTestAsync(null));
      }catch(Exception exc) {
        return Content(HttpStatusCode.InternalServerError, exc.ToMessages());
      }
    }

    #region Configer
    class Configer :EventLogger<Configer> {
      [appSettings]
      public string DataPath => KeyValue();
      #region RunTest
      protected override async Task<ExpandoObject> _RunTestAsync(ExpandoObject parameters, params Func<ExpandoObject, ExpandoObject>[] merge) {
        return await TestHostAsync(parameters, async (p, m) => {
          var e = await base._RunTestAsync(parameters, merge);
          // Test DataPath Access
          try {
            var appDataPath = Path.Combine(DataPath, Path.GetRandomFileName());
            var testFile = Helpers.MapExecPath(appDataPath);
            using(var s = File.CreateText(testFile))
              s.WriteLine("xxx");
            File.Delete(testFile);
            e = e.AddOrMerge("DataPath Access", new { testFile, access = "Read/Write" });
          } catch(Exception exc) {
            throw new Exception(new { ConfigValue = new { DataPath } } + "", exc);
          }
          // Test Hello File
          var helloFileTest = FetchHellFiles().ToArray();
          if(helloFileTest.Length != 1)
            throw new Exception(new {hellFiles=helloFileTest.Flatten("\n"),error= "There can be only one", } + "");
          return e.Merge(new { HelloFile = helloFileTest.Single() });
        }, _LogError, merge);
      }
      #endregion
    }
    #endregion
  }
}
