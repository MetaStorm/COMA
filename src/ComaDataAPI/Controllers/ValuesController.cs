using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using CommonExtensions;
using Swashbuckle.Swagger.Annotations;

namespace ComaDataAPI.Controllers {
  [RoutePrefix("api/test")]
  public class TestController : ApiController {
    class Configer : Foundation.Config<Configer> {
      [Foundation.CustomConfig.appSettings]
      public static string[] ExcludeTests => KeyValue<string>().Split(',');

      public static IList<Assembly> LoadAssemblies() {
        var dlls = Helpers.FindFiles("*.dll");
        var codeBase = Helpers.ToFunc((Assembly a) => new Uri(a.CodeBase.IfEmpty(a.Location)));
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToArray();
        var dllsToLoad = (from dll in dlls
                          join a in assemblies.Select(a => new { a, cb = codeBase(a) }) on new Uri(dll) equals a.cb into g
                          where g.IsEmpty()
                          where !dll.Contains("\\Microsoft.") && !dll.StartsWith("\\System.")
                          select dll
                          ).ToArray();
        var loadedAssembleys = assemblies.Concat(dllsToLoad.Select(dll => {
          try {
            return Assembly.LoadFile(dll);
          }catch(Exception exc) {
            throw;
          }
        }));
        //Func<Assembly, Type[]> loadTypes = a => {
        //  try {
        //    return a.GetTypes();
        //  }
        //  catch (ReflectionTypeLoadException exc) {
        //    if (errorHandler != null) {
        //      exc.LoaderExceptions.ForEach(e => errorHandler(e));
        //      return new Type[0];
        //    }
        //    throw;
        //  }
        //};
        return loadedAssembleys.ToList();
      }

    }
    [Route("RunTest")]
    [HttpGet]
    public async Task<IHttpActionResult> RunTest() {
      Configer.LoadAssemblies().Count();
      return await RunTestImpl(false);
    }
    async Task<IHttpActionResult> RunTestImpl(bool fast) {
      Foundation.Testable.UnTest().Count();
      Func<Foundation.TestableException, bool> isExcluded = te => Configer.ExcludeTests.Contains(te.TestedType.FullName.ToLower());
      var errors = new List<Foundation.TestableException>();
      var testOutput = (await Foundation.Testable.RunTestsAsync(fast, exc => { if(!isExcluded(exc)) errors.Add(exc); return true; }, Configer.ExcludeTests)).OrderBy(t => t.Key.FullName).ToArray();
      var tests = new List<object>();
      Func<Exception, IEnumerable<Exception>> check = exc
        => exc is AggregatedException ? (exc as AggregatedException).InnerExceptions : new[] { exc };
      if(errors.Any())
        tests.Add("***** Errors *****");
      tests.AddRange(errors
        .SelectMany(error => new[] {
        "*** {0}[{1}]".Formatter(error.TestedType.FullName,error.TestedType.Assembly.CodeBase) }
        .Concat(error.Inners()
        .SelectMany(check)
        .SelectMany(exc => new[] {
          exc.Message.IsEmpty()?null:"+++ "+exc.Message,
          exc.Message.IsEmpty() || exc is Foundation.Core.ConfigurationErrorException?"": exc.StackTrace }))));
      tests.Add($"*************** Tests Outputs ({testOutput.Length}) *******************");
      tests.AddRange(testOutput.SelectMany(e => new object[] { /*header.Formatter(e.Key.FullName),*/ e.Value }));
      var output = tests
        .SelectMany(o => o is string ? (o + "").Split('\n') : new[] { o })
        .Where(t => t != null && !string.IsNullOrWhiteSpace(t + ""));
      var statusCode = errors.Any() ? HttpStatusCode.InternalServerError : HttpStatusCode.OK;
      return Content(statusCode, output);
    }
    // GET api/values/5
    [SwaggerOperation("GetById")]
    [SwaggerResponse(HttpStatusCode.OK)]
    [SwaggerResponse(HttpStatusCode.NotFound)]
    public string Get(int id) {
      return "value";
    }

    // POST api/values
    [SwaggerOperation("Create")]
    [SwaggerResponse(HttpStatusCode.Created)]
    public void Post([FromBody]string value) {
    }

    // PUT api/values/5
    [SwaggerOperation("Update")]
    [SwaggerResponse(HttpStatusCode.OK)]
    [SwaggerResponse(HttpStatusCode.NotFound)]
    public void Put(int id, [FromBody]string value) {
    }

    // DELETE api/values/5
    [SwaggerOperation("Delete")]
    [SwaggerResponse(HttpStatusCode.OK)]
    [SwaggerResponse(HttpStatusCode.NotFound)]
    public void Delete(int id) {
    }
  }
}
