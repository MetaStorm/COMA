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
    #region RunTest
    string testRoot { get { return GetType().FullName; } }
    //          var url = JiraMonad.JiraServiceBaseAddress();
    protected override async Task<ExpandoObject> _RunTestFastAsync(ExpandoObject parameters, params Func<ExpandoObject, ExpandoObject>[] merge) {
      return await TestHostAsync(parameters, (p, m) =>
        (from rf in base._RunTestFastAsync(parameters, merge)
         from ut in Task.Run(() => { JiraMonad.UnTest(); return new ExpandoObject(); })
         from jm in JiraMonad.RunTestFastAsync(parameters, merge)
         select new ExpandoObject().Merge(testRoot, jm.Merge(rf).Merge(new { DataPath }))
        )
      , _LogError, merge);
    }

    protected override async Task<ExpandoObject> _RunTestAsync(ExpandoObject parameters, params Func<ExpandoObject, ExpandoObject>[] merge) {
      return await TestHostAsync(parameters, async (p, m) => {
        var e = await base._RunTestAsync(parameters, merge);
        var self = await RestMonad.Empty().GetMySelfAsync();
        var l = new { self }.ToExpando();
        if (string.IsNullOrWhiteSpace(DataPath))
          l = l.Merge(new { DataPath });
        else {
          try {
            var appDataPath = Path.Combine(DataPath, Path.GetRandomFileName());
            var testFile = JiraController.ServerPath(appDataPath) ?? Helpers.MapExecPath(appDataPath);
            using (var s = File.CreateText(testFile))
              s.WriteLine("xxx");
            File.Delete(testFile);
            l = l.Merge(new { dataPath = new { testFile, access = "Read/Write" } });
          }
          catch (Exception exc) {
            throw new Exception(new { ConfigValue = new { DataPath } } + "", exc);
          }
        }
        return e.AddOrMerge(testRoot, l);
      }, _LogError, merge);
    }
    #endregion
    }
    [Route("HelloWorld")]
    public IHttpActionResult Get() {
      return Ok(from path in CommonExtensions.Helpers.FindFiles("*.txt","App_Data")
              select new { path, text = System.IO.File.ReadAllText(path) });
    }
  }
}
