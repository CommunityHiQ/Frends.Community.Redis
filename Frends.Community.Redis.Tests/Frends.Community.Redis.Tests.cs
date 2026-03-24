using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Frends.Community.Redis.Tests
{
	[TestClass]
	public class UnitTests
	{
		private static List<string> _keyValueCleanUp = [];
		private static List<string> _setValueCleanUp = [];
		private static List<string> _jsonValueCleanUp = [];
		private static List<string> _indexCleanUp = [];


		private static readonly Connection _connection = new()
		{
			ConnectionString = "", // Add your Redis connection string here, e.g. "localhost:6379,password=yourpassword"
			Timeout = 5
		};

		private static ConnectionMultiplexer _connectionMultiplexer;

		private static readonly Options _options = new()
		{
			Workers = 4,
			InputOutputCompletionPorts = 1
		};

		[TestInitialize]
		public void StartUp()
		{
		}

		[TestCleanup]
		public void CleanUp()
		{
			DeleteTestData();
		}

		[TestMethod]
		public void Test_AddKeyValue()
        {
			// Arrange
			_setValueCleanUp.AddRange(["test:key:1", "test:key:2"]);

			// Act
			AddInput input = new()
			{
				InputObjectType = ObjectType.KeyValuePair,
				KeyValuePairInput =
				[
					new() {
						Key = "test:key:1",
						Value = "TestValue1",
						TimeToLive = TimeSpan.FromMinutes(5)
					},
					new() {
						Key = "test:key:2",
						Value = "TestValue2",
						TimeToLive = TimeSpan.FromMinutes(5)
					}
				]
			};

			List<Result> results = Redis.Add(input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.AreEqual(2, results.Count);
		}

		[TestMethod]
		public void Test_AddSetValue()
		{
			// Arrange
			_setValueCleanUp.AddRange(["test:key:3", "test:key:4"]);

			// Act
			AddInput input = new()
			{
				InputObjectType = ObjectType.Set,
				SetInput =
				[
					new() {
						Key = "test:key:3",
						Value = ["Value1", "Value2"]
					},
					new() {
						Key = "test:key:4",
						Value = ["Value1", "Value2"]
					}
				]
			};

			List<Result> results = Redis.Add(input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.AreEqual(2, results.Count);
		}

		[TestMethod]
		public void Test_AddJson()
		{
			// Arrange
			_jsonValueCleanUp.AddRange(["test:foo:1", "test:foo:2", "test:foo:3", "test:foo:4"]);

			var existingUser = new
			{
				name = "Glenn John",
				email = "paul.john@example.com",
				age = 42,
				city = "London",
				parent = new
				{
					name = "Existing Parent"
				}
			};

			var db = GetRedisDatabase();
			db.JSON().Set("test:foo:1", "$", existingUser);

			// Act
			AddInput input = new()
			{
				InputObjectType = ObjectType.Json,
				JsonInput =
				[
					new() {
						Key = "test:foo:1",
						Path = "",
						Value = new
						{
							name = "Paul John",
							email = "paul.john@example.com",
							age = 42,
							city = "London"
						}
					},
					new() {
						Key = "test:foo:2",
						Value = new
						{
							name = "Eden Zamir",
							email = "eden.zamir@example.com",
							age = 29,
							city = "Tel Aviv"
						},
					},
					new() {
						Key = "test:foo:3",
						Value = new
						{
							name = "Paul Zamir",
							email = "paul.zamir@example.com",
							age = 35,
							city = "Tel Aviv"
						},
					},
					new () {
						Key = "test:foo:4",
						Value = "{\n  \"name\": \"Paul John\",\n    \"age\": 42,\n    \"city\": \"London\"\n}",
					}
				]
			};

			List<Result> results = Redis.Add(input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.AreEqual(4, results.Count);

			var u1 = GetJson("test:foo:1");
			Assert.AreEqual("Paul John", (string)u1.name);
			Assert.IsNull(u1.parent);

			var u2 = GetJson("test:foo:2");
			Assert.AreEqual("Eden Zamir", (string)u2.name);
			
			var u3 = GetJson("test:foo:3");
			Assert.AreEqual(35, (int)u3.age);
			
			var u4 = GetJson("test:foo:4");
			Assert.AreEqual("London", (string)u4.city);
		}

		[TestMethod] 
		public void Test_GetJson()
		{
			// Arrange
			_jsonValueCleanUp.AddRange(["test:user:0"]);

			var user1 = new
			{
				name = "Paul John",
				email = "paul.john@example.com",
				age = 42,
				city = "London"
			};

			var db = GetRedisDatabase();
			db.JSON().Set("test:user:0", "$", user1);

			// Act
			GetInput input = new()
			{
				ObjectType = ObjectType.Json,
				JsonKey = "test:user:0"
			};

			GetResult result = Redis.Get(input, _connection, _options, CancellationToken.None);
		
			// Assert
			Assert.AreEqual(1, result.Values.Count);

			var user = JsonConvert.DeserializeObject<dynamic>(result.Values[0].ToString());
			Assert.AreEqual("paul.john@example.com", (string)user.email);
		}

		[TestMethod]
		public void Test_QueryJson()
		{
			// Arrange
			_indexCleanUp.Add("idx:test:queryjson");
			_jsonValueCleanUp.AddRange(["test:user:1", "test:user:2", "test:user:3"]);

			var user1 = new
			{
				name = "Paul John",
				email = "paul.john@example.com",
				age = 42,
				city = "London"
			};
			var user2 = new
			{
				name = "Eden Zamir",
				email = "eden.zamir@example.com",
				age = 29,
				city = "Tel Aviv"
			};
			var user3 = new
			{
				name = "Paul Zamir",
				email = "paul.zamir@example.com",
				age = 35,
				city = "Tel Aviv"
			};
			
			var db = GetRedisDatabase();
			db.JSON().Set("test:user:1", "$", user1);
			db.JSON().Set("test:user:2", "$", user2);
			db.JSON().Set("test:user:3", "$", user3);

			var schema = new Schema();
			schema.AddTextField(new FieldName("$.name", "name"));
			schema.AddNumericField(new FieldName("$.age", "age"));

			db.FT()
				.Create(
					"idx:test:queryjson",
					new FTCreateParams()
						.On(IndexDataType.JSON)
						.Prefix("test:user"),
					schema);

			// Act
			JsonQueryInput input = new()
			{
				IndexName = "idx:test:queryjson",
				Query = "Paul @age:[30 40]"
			};

			GetResult result = Redis.Query(input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.AreEqual(1, result.Values.Count);

			var user = JsonConvert.DeserializeObject<dynamic>(result.Values[0].ToString());
			Assert.AreEqual("paul.zamir@example.com", (string)user.email);
		}

		[TestMethod]
		public void Test_QueryJson_By_Tag()
		{
			// Arrange
			_indexCleanUp.Add("idx:test:queryjsonbytag");
			_jsonValueCleanUp.AddRange(["test:user:1", "test:user:2", "test:user:3"]);

			var user1 = new
			{
				objectId = "test:user:1",
				objectDomain = "data",
				name = "Paul John",
				email = "paul.john@example.com",
				age = 42,
				city = "London"
			};
			var user2 = new
			{
				objectId = "test:user:2",
				name = "Eden Zamir",
				email = "eden.zamir@example.com",
				age = 29,
				city = "Tel Aviv"
			};
			var user3 = new
			{
				objectId = "test:user:3",
				name = "Paul Zamir",
				email = "paul.zamir@example.com",
				age = 35,
				city = "Tel Aviv"
			};

			var db = GetRedisDatabase();
			db.JSON().Set("test:user:1", "$", user1);
			db.JSON().Set("test:user:2", "$", user2);
			db.JSON().Set("test:user:3", "$", user3);

			var schema = new Schema();
			schema.AddTagField(new FieldName("$.objectId", "objectId"));
			schema.AddTagField(new FieldName("$.objectDomain", "objectDomain"));
			schema.AddTextField(new FieldName("$.name", "name"));
			schema.AddNumericField(new FieldName("$.age", "age"));

			db.FT()
				.Create(
					"idx:test:queryjsonbytag",
					new FTCreateParams()
						.On(IndexDataType.JSON)
						.Prefix("test:user"),
					schema);

			Thread.Sleep(500); // Wait for index to be ready
							   //Query query = new Query("@objectId:{\"urn:bkid:data:datadistributionexport:test-export-incomingblobtoentryscape-outgoing\"}").Dialect(2);
							   //Query query = new Query("@objectId:{\"test:user:1\"}"); //.Dialect(2);

			////	query.AddParam("objectId", "test:user:1");

			//SearchResult searchResult = db.FT().Search(
			//	"idx:test:queryjsonbytag",
			//	query
			//);


			//var foo = searchResult.Documents;
			//Assert.AreEqual(1, foo.Count);
			// Act
			JsonQueryInput input = new()
			{
				IndexName = "idx:test:queryjsonbytag",
				Query = "@objectDomain:{data}"
			};

			GetResult result = Redis.Query(input, _connection, _options, CancellationToken.None);
			Assert.AreEqual(1, result.Values.Count);

			var user = JsonConvert.DeserializeObject<dynamic>(result.Values[0].ToString());
			Assert.AreEqual("paul.john@example.com", (string)user.email);
		}


		[TestMethod]
		public void Test_MergeJson()
		{
			// Arrange
			var jsonId1 = $"test:{Guid.NewGuid()}";
			var jsonId2 = $"test:{Guid.NewGuid()}";
			var jsonId3 = $"test:{Guid.NewGuid()}";
			_jsonValueCleanUp.AddRange([jsonId1, jsonId2, jsonId3]);

			var json1 = new
			{
				a = "Value A",
				b = 2,
				c = new {
					c1 = "Value C1",
					c2 = "Value C2"
				}
			};

			var json2 = new
			{
				a = "Value A",
				b = 3,
				d = new
				{
					d1 = "Value D1",
					d2 = "Value D2"
				}
			};

			var db = GetRedisDatabase();
			db.JSON().Set(jsonId1, "$", json1);
			db.JSON().Set(jsonId2, "$", json2);

			// Act
			MergeInput addInput = new()
			{
				ObjectType = ObjectType.Json,
				JsonInput = new()
				{
					Key = jsonId3,
					Value = new
					{
						a = "Value A",
						b = 99

					}
				}
			};

			MergeInput update1Input = new()
			{
				ObjectType = ObjectType.Json,
				JsonInput = new()
				{
					Key = jsonId1,
					Path = "$",
					Value = new
					{
						c = new
						{
							c3 = "Value C3"
						}
					}
				}
			};

			MergeInput update2Input = new()
			{
				ObjectType = ObjectType.Json,
				JsonInput = new()
				{
					Key = jsonId2,
					Path = "$.d",
					Value = new
					{
						d1 = (string)null,
						d2 = (string)null,
						d3 = "Value D3"
					}
				}
			};

			Result addResult = Redis.Merge(addInput, _connection, _options, CancellationToken.None);
			Result update1Result = Redis.Merge(update1Input, _connection, _options, CancellationToken.None);
			Result update2Result = Redis.Merge(update2Input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.IsTrue(addResult.Success);
			var o1 = GetJson(jsonId3);
			Assert.AreEqual("Value A", (string)o1.a);
			Assert.AreEqual(99, (int)o1.b);

			Assert.IsTrue(update1Result.Success);
			var o2 = GetJson(jsonId1);
			Assert.AreEqual("Value A", (string)o2.a);
			Assert.AreEqual(2, (int)o2.b);
			Assert.AreEqual("Value C3", (string)o2.c.c3);

			Assert.IsTrue(update2Result.Success);
			var o3 = GetJson(jsonId2);
			Assert.AreEqual("Value A", (string)o3.a);
			Assert.AreEqual(3, (int)o3.b);
			Assert.IsNull(o3.d.d1);
			Assert.AreEqual("Value D3", (string)o3.d.d3);
		}


		[TestMethod]
		public void Test_CreateIndex()
		{
			// Arrange
			_indexCleanUp.Add("idx:test:create");

			// Act
			IndexCreateInput input = new()
			{
				Name = "idx:test:create",
				Prefix = "user:",
				Parameters =
				[
					new IndexFieldDefinition
					{
						Name = "$.name",
						Attribute = "name",
						FieldType = FieldDefinitionType.Text
					},
					new IndexFieldDefinition
					{
						Name = "$.city",
						Attribute = "city",
						FieldType = FieldDefinitionType.Text
					},
					new IndexFieldDefinition
					{
						Name = "$.age",
						Attribute = "age",
						FieldType = FieldDefinitionType.Numeric
					}
				]
			};

			Result result = Redis.CreateIndex(input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.Success);
			
			var db = GetRedisDatabase();
			var list = db.FT()._List();
			bool indexExists = list.Any(i => i.ToString() == "idx:test:create");
			
			Assert.IsTrue(indexExists);
		}

		[TestMethod]
		public void Test_DeleteIndex()
		{
			//  Arrange
			var schema = new Schema();
			schema.AddTextField(new FieldName("$.name", "name"));
			schema.AddNumericField(new FieldName("$.age", "age"));

			var db = GetRedisDatabase();
			var indexCreated = db.FT()
			.Create(
				"idx:test:delete",
				new FTCreateParams()
					.On(IndexDataType.JSON)
					.Prefix("idx:test"),
				schema);

			// Act
			IndexDeleteInput input = new()
			{
				Name = "idx:test:delete",
				DeleteDocuments = false
			};

			Result result = Redis.DeleteIndex(input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.Success);
			Thread.Sleep(500);

			var list = db.FT()._List();
			bool indexExists = list.Any(i => i.ToString() == "idx:test:delete");

			Assert.IsFalse(indexExists);
		}

		[TestMethod]
		public void Test_UpdateIndex()
		{
			// Arrange
			_indexCleanUp.Add("idx:test:update");
			var schema = new Schema();
			schema.AddTextField(new FieldName("$.name", "name"));
			schema.AddNumericField(new FieldName("$.age", "age"));

			var db = GetRedisDatabase();
			 db.FT()
				.Create(
					"idx:test:update",
					new FTCreateParams()
						.On(IndexDataType.JSON)
						.Prefix("idx:test"),
					schema);
			
			// Act
			IndexUpdateInput input = new()
			{
				Name = "idx:test:update",
				Parameters =
				[
					new IndexFieldDefinition
					{
						Name = "$.foo",
						Attribute = "foo",
						FieldType = FieldDefinitionType.Text
					}
				]
			};

			Result result = Redis.UpdateIndex(input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.Success);

			var info = db.FT().Info("idx:test:update");

			Assert.IsTrue(info.Attributes.Any(a => a.ContainsKey("identifier") && a["identifier"].ToString() == "$.name"), "$.name is missing");
			Assert.IsTrue(info.Attributes.Any(a => a.ContainsKey("identifier") && a["identifier"].ToString() == "$.age"), "$.age is missing");
			Assert.IsTrue(info.Attributes.Any(a => a.ContainsKey("identifier") && a["identifier"].ToString() == "$.foo"), "$.foo is missing");
		}

		[TestMethod]
		public void Test_CreateOrUpdateIndex()
		{
			// Arrange
			_indexCleanUp.Add("idx:test:createorupdate");

			// Act
			IndexCreateOrUpdateInput input = new()
			{
				Name = "idx:test:createorupdate",
				Prefix = "user:",
				Parameters =
				[
					new IndexFieldDefinition
					{
						Name = "$.name",
						Attribute = "name",
						FieldType = FieldDefinitionType.Text
					},
					new IndexFieldDefinition
					{
						Name = "$.city",
						Attribute = "city",
						FieldType = FieldDefinitionType.Text
					},
					new IndexFieldDefinition
					{
						Name = "$.age",
						Attribute = "age",
						FieldType = FieldDefinitionType.Numeric
					}
				]
			};

			Result result = Redis.CreateOrUpdateIndex(input, _connection, _options, CancellationToken.None);

			// Assert
			Assert.IsTrue(result.Success);
		}

		private static dynamic GetJson(string key)
		{
			var db = GetRedisDatabase();
			RedisResult result = db.JSON().Get(key);
			return JsonConvert.DeserializeObject<dynamic>(result.ToString());
		}

		private static void DeleteTestData()
		{
			var db = GetRedisDatabase();

			foreach (var key in _keyValueCleanUp)
			{
				db.KeyDelete(key, CommandFlags.FireAndForget);
			}

			foreach (var key in _setValueCleanUp)
			{
				db.StringDelete(key, When.Always);
			}

			foreach(var key in _jsonValueCleanUp)
			{
				db.JSON().Del(key);
			}

			foreach (var index in _indexCleanUp)
			{
				db.FT().DropIndex(index, false);
			}


			_indexCleanUp.Clear();


		}

		private static IDatabase GetRedisDatabase()
		{
			if (_connectionMultiplexer == null)
			{
				_connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(_connection.ConnectionString, new TimeSpan(0, 0, _connection.Timeout));
			}
			
			return _connectionMultiplexer.GetDatabase();
		}
	}
}
