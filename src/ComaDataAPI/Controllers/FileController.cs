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
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

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
      } else if(!hello.IsNullOrWhiteSpace() && world.IsNullOrWhiteSpace()) {
        var filePath = Helpers.FindFiles("Store.txt").SingleOrDefault();
        if(filePath == null) throw new Exception("Store.txt not found");
        File.AppendAllLines(filePath, new[] { hello });
        return Content(HttpStatusCode.NoContent, "");
      } else if(hello.IsNullOrWhiteSpace() && !world.IsNullOrWhiteSpace()) {
        var gwPath = Helpers.FindFiles("App_Data\\GW.txt").Counter(1, new Exception("GW.txt not found"), new Exception("GW.txt two many")).SingleOrDefault();
        var gws = ReadUsers(gwPath);
        var filePath = Helpers.FindFiles("App_Data\\Store.txt").Counter(1, new Exception("Store.txt not found"), new Exception("Store.txt two many")).SingleOrDefault();
        var lines = File.ReadAllLines(filePath).Select(l => new { k = Getit(l, world), v = l }).Distinct(x => x.k).ToArray();
        File.WriteAllLines(filePath, lines.Select(x => x.v));

        var info = (from l in lines
                    join gw in gws on l.k.Split(':')[0] equals gw.login
                    select gw.name + ":" + l.k);
        return HttpTextResult.Ok(info.Flatten("<br />"), HttpTextResult.ContentType.html);
      }
      return Content(HttpStatusCode.NoContent, "");
      string Getit(string text, string pass) {
        var x = Helpers.Encryptor.AESThenHMAC.SimpleDecryptWithPassword(text, pass);
        try {
          var y = Undo(x, false);
          return string.Join(":", y.Split(':').Select((s, i) => Undo(s, i == 0)));
        } catch(Exception exc) {
          throw new Exception(new { DidntGetTt = text, x } + "", exc);
        }
      }
      string Reverse(string str) => new string(str.Reverse().ToArray());
      string Undo(string y2, bool toUpper) {
        try {
          var s = System.Text.ASCIIEncoding.ASCII.GetString(System.Convert.FromBase64String(Reverse(y2)));
          return toUpper ? s.ToUpper() : s;
        } catch(Exception exc) {
          throw new Exception(new { y2 } + "", exc);
        }
      }
      (string dir, string login, string name)[] ReadUsers(string gwPath) => File.ReadAllLines(gwPath).Select(l => {
        var fields = CsvToJson.GetFields(l, "\t").ToArray();
        return (dir: fields[0], login: fields[1].ToUpper(), name: fields[2]);
      }).ToArray();
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

  public static class CsvToJson {
    private static string ReadFile(string filePath, string delimiter) {
      string payload = "";
      try {
        if(!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath) && Path.GetExtension(filePath).Equals(".csv", StringComparison.InvariantCultureIgnoreCase)) {
          string[] lines = File.ReadAllLines(filePath);

          if(lines != null && lines.Length > 1) {
            var headers = GetHeaders(lines.First(), delimiter);
            payload = GetPayload(headers, lines.Skip(1), delimiter);
          }
        }
      } catch(Exception exp) {
      }
      return payload;
    }

    private static IEnumerable<string> GetHeaders(string data, string delimiter) {
      IEnumerable<string> headers = null;

      if(!string.IsNullOrWhiteSpace(data) && data.Contains(delimiter)) {
        headers = GetFields(data, delimiter).Select(x => x.Replace(" ", ""));
      }
      return headers;
    }

    private static string GetPayload(IEnumerable<string> headers, IEnumerable<string> fields, string delimiter) {
      string jsonObject = "";
      try {
        var dictionaryList = fields.Select(x => GetField(headers, x, delimiter));
        jsonObject = JsonConvert.SerializeObject(dictionaryList);
      } catch(Exception ex) {
      }
      return jsonObject;
    }

    private static Dictionary<string, string> GetField(IEnumerable<string> headers, string fields, string delimiter) {
      Dictionary<string, string> dictionary = null;

      if(!string.IsNullOrWhiteSpace(fields)) {
        var columns = GetFields(fields, delimiter);

        if(columns != null && headers != null && columns.Count() == headers.Count()) {
          dictionary = headers.Zip(columns, (x, y) => new { x, y }).ToDictionary(item => item.x, item => item.y);
        }
      }
      return dictionary;
    }

    public static IEnumerable<string> GetFields(string line, string delimiter) {
      IEnumerable<string> fields = null;
      using(TextReader reader = new StringReader(line)) {
        using(TextFieldParser parser = new TextFieldParser(reader)) {
          parser.TextFieldType = FieldType.Delimited; parser.SetDelimiters(delimiter); fields = parser.ReadFields();
        }
      }
      return fields;
    }
  }
}
