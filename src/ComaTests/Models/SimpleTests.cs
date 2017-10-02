using ComaModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using SimpleExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComaModel.Tests {
  [TestClass()]
  public class SimpleTests {
    [TestMethod()]
    public void ReadToDoMLab() {
      String uri = "mongodb://dimok:1Aaaaaaa@ds040017.mlab.com:40017/forex";
      var database = "forex";
      var collection = "ToDo";
      var todo = new { _id = ObjectId.Empty, What = "", When = "" };
      var json = HedgeHog.MongoExtensions.ReadCollectionAnon(todo, uri, database, collection).ToArray();
      Console.WriteLine(json.ToJson());
      Assert.AreEqual("Code", json[0].What);
    }
  }
}

namespace SimpleExample.Tests {
  [TestClass()]
  public class SimpleTests {
    [TestMethod()]
    public async Task CreateSeedDataTest() {
      BsonDocument[] seedData = Simple. CreateSeedData();
      await Simple.AsyncCrud(seedData);
    }
  }
}