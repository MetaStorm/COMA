using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgeHog {
  public class MongoExtensions {
    public static IEnumerable<T> ReadCollectionAnon<T>(T anon, string uri, string database, string collection) =>
      ReadCollection<T>(uri, database, collection);

    public static IEnumerable<T> ReadCollection<T>(string uri, string database, string collection) {
      return new MongoClient(uri)
        .GetDatabase(database)
        .GetCollection<T>(collection)
        .Find(Builders<T>.Filter.Empty)
        .ToEnumerable();
    }
  }
}
