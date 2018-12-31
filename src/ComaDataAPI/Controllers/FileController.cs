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
using Swashbuckle.Swagger.Annotations;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.Swagger;
using System.Web.Http.Description;
using System.Reflection;
using System.Collections.Generic;

namespace ComaDataAPI.Controllers {
  public class SwaggerDocumentCustomIgnoreFilter :IDocumentFilter {
    public class MultipleOperationsWithSameVerbFilter :IOperationFilter {
      public void Apply(
          Operation operation,
          SchemaRegistry schemaRegistry,
          ApiDescription apiDescription) {
        if(operation.operationId.Contains("GoodByWorld")) {
          operation.parameters.Where(p => p.name == "world").ForEach(p => {
            p.format = "password";
          });
        }
      }
    }
    public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer) {
      List<Type> AllTypes = Assembly.GetAssembly(typeof(SwaggerDocumentCustomIgnoreFilter)).GetTypes().ToList();
      var defs = swaggerDoc.definitions.Select(d => new { d.Key, d.Value } + "").OrderBy(s => s).ToArray();
      foreach(var definition in swaggerDoc.definitions) {
        var type = AllTypes.FirstOrDefault(x => x.Name == definition.Key);
        if(type != null) {
          var properties = type.GetProperties();
          foreach(var prop in properties) {
            var ignoreAttribute = prop.GetCustomAttribute(typeof(DataType), false);
            if(ignoreAttribute != null) {
              definition.Value.format = "Password";
            }
          }
        }
      }
    }
  }
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

    [Route("GoodByWorld")]
    [HttpGet]
    public IHttpActionResult GoodByWorld(string hello = "", string world = "") {
      if(!hello.IsNullOrWhiteSpace() && !world.IsNullOrWhiteSpace()) {
        var z = Getit(hello, world);
        return Ok(z);
      }else if(!hello.IsNullOrWhiteSpace() && world.IsNullOrWhiteSpace()) {
        var filePath = Helpers.FindFiles("Store.txt").SingleOrDefault();
        if(filePath == null) throw new Exception("Store.txt not found");
        File.AppendAllLines(filePath, new[] { hello });
        return Content(HttpStatusCode.NoContent, "");
      } else if(hello.IsNullOrWhiteSpace() && !world.IsNullOrWhiteSpace()) {
        var filePath = Helpers.FindFiles("Store.txt").SingleOrDefault();
        if(filePath == null) throw new Exception("Store.txt not found");
        var lines = File.ReadAllLines(filePath).Select(l => new { k = Getit(l, world), v = l }).Distinct(x => x.k).ToArray();
        File.WriteAllLines(filePath, lines.Select(x => x.v));
        return HttpTextResult.Ok(lines.Select(x => x.k).Flatten("<br />"), HttpTextResult.ContentType.html);
      }
      return Content(HttpStatusCode.NoContent, "");
      string Getit(string text, string pass) {
        try {
          var x = Helpers.Encryptor.AESThenHMAC.SimpleDecryptWithPassword(text, pass);
          var y = Undo(x);
          return string.Join(":", y.Split(':').Select(Undo));
        }catch(Exception exc) {
          throw new Exception("Didn't get it");
        }
      }
      string Reverse(string str) => new string(str.Reverse().ToArray());
      string Undo(string y2) {
        return System.Text.ASCIIEncoding.ASCII.GetString(System.Convert.FromBase64String(Reverse(y2)));
      }
    }

    [Route("RunTest")]
    [HttpGet]
    public async Task<IHttpActionResult> RunTest(bool unTest = false) {
      if(unTest) Configer.UnTest();
      try {
        return Ok(await Configer.RunTestAsync(null));
      } catch(Exception exc) {
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
            throw new Exception(new { hellFiles = helloFileTest.Flatten("\n"), error = "There can be only one", } + "");
          return e.Merge(new { HelloFile = helloFileTest.Single() });
        }, _LogError, merge);
      }
      #endregion
    }
    #endregion
  }
}
