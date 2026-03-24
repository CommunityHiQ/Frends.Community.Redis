using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;


#pragma warning disable 1591

namespace Frends.Community.Redis
{
    public static class Redis
    {
		/// <summary>
		/// A Frends task for adding and updating key-value pairs, Sets or Json to Redis.
		/// Returns a List of Result-objects that have properties Success and Value.
		/// See: https://github.com/CommunityHiQ/Frends.Community.Redis#Add
		/// </summary>
		/// <param name="input">Key-Value pairs or a Set</param>
		/// <param name="connection">Connection-options</param>
		/// <param name="options">Additional options for the task</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>List [ Object { bool Success, object Value } ]</returns>
		public static List<Result> Add([PropertyTab] AddInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
        {

            ConnectionMultiplexer connectionMultiplexer = null;
            List<Result> results = new List<Result>();
            setMinValue(options);
            cancellationToken.ThrowIfCancellationRequested();

            if (connection.UseCachedConnection)
            {
                connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
            }
            else
            {
                connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
            }

            IDatabase database = connectionMultiplexer.GetDatabase();
		
			if (input.InputObjectType == ObjectType.Set)
            {
                foreach (var item in input.SetInput)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var _result = database.SetAdd(ObjectToRedisKey(item.Key), item.Value.Select(x => ObjectToRedisValue(x)).ToArray());
                    results.Add(new Result((long)_result > -1, item.Key));
                }
            }
            else if (input.InputObjectType == ObjectType.KeyValuePair)
			{
                foreach (var item in input.KeyValuePairInput)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var _result = database.StringSet(ObjectToRedisKey(item.Key), ObjectToRedisValue(item.Value), item.TimeToLive, item.GetWhen());
                    results.Add(new Result((bool)_result, item.Key));
                }
            }
			else if (input.InputObjectType == ObjectType.Json)
			{
				foreach (var item in input.JsonInput)
				{
					cancellationToken.ThrowIfCancellationRequested();

					bool result = false;
					if (item.Value is string)
					{
						var json = JsonConvert.DeserializeObject<dynamic>(item.Value.ToString());
						result = database.JSON().Set(ObjectToRedisKey(item.Key), string.IsNullOrEmpty(item.Path?.ToString()) ? "$" : item.Path.ToString(), JsonConvert.SerializeObject(json));
					}
					else
					{
						result = database.JSON().Set(ObjectToRedisKey(item.Key), string.IsNullOrEmpty(item.Path?.ToString()) ? "$" : item.Path.ToString(), item.Value);
					}

					results.Add(new Result(result, item.Key));
				}
			}
			else
			{
				throw new ArgumentException("InputObjectType is of invalid type. Accepted types are KeyValuePair, Set or Json");
			}

            return results;

        }

        /// <summary>
        /// A Frends task for getting Key-Values, Sets or Json from Redis.
        /// See: https://github.com/CommunityHiQ/Frends.Community.Redis#Get
        /// </summary>
        /// <param name="input">An array of keys for key-value pairs or a key to a set</param>
        /// <param name="connection">Connection-options</param>
        /// <param name="options">Additional options for the task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object { List&lt;object&gt; Values, JToken ToJToken() } ]</returns>
        public static GetResult Get([PropertyTab] GetInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            ConnectionMultiplexer connectionMultiplexer = null;
            List<object> results = new List<object>();
            setMinValue(options);
            cancellationToken.ThrowIfCancellationRequested();

            if (connection.UseCachedConnection)
            {
                connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
            }
            else
            {
                connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
            }

            IDatabase database = connectionMultiplexer.GetDatabase();

            RedisValue[] redisValues;

            if (input.ObjectType == ObjectType.KeyValuePair)
            {
                redisValues = database.StringGet(input.Key.Select(key => ObjectToRedisKey(key)).ToArray());
            }
			else if (input.ObjectType == ObjectType.Set)
			{
                redisValues = database.SetMembers(ObjectToRedisKey(input.SetKey));
            }
			else if (input.ObjectType == ObjectType.Json)
			{
				RedisResult result = database.JSON().Get(ObjectToRedisKey(input.JsonKey));
				redisValues = [(RedisValue)result];
			}
			else
			{
				throw new ArgumentException("InputObjectType is of invalid type. Accepted types are KeyValuePair, Set or Json");
			}

			foreach (RedisValue value in redisValues)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (value.HasValue)
                {
                    results.Add(value);
                }
            }

            return new GetResult(results);
        }

        /// <summary>
        /// A Frends task for removing Key-Value pairs, Set or Json values from Redis. 
        /// Returns a Result-object with properties Success and Value. 
        /// Value tells how many cache objects were removed.
        /// See: https://github.com/CommunityHiQ/Frends.Community.Redis#Remove
        /// </summary>
        /// <param name="input">Keys or set values to be removed. For removal of a whole set, use key-value and the set key</param>
        /// <param name="connection">Connection-options</param>
        /// <param name="options">Additional options for the task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object { bool Success, object Value }</returns>
        public static Result Remove([PropertyTab] DeleteInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            ConnectionMultiplexer connectionMultiplexer = null;
            setMinValue(options);
            cancellationToken.ThrowIfCancellationRequested();

            if (connection.UseCachedConnection)
            {
                connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
            }
            else
            {
                connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
            }

            IDatabase database = connectionMultiplexer.GetDatabase();

            if (input.ObjectType == ObjectType.KeyValuePair)
            {
                var _result = database.KeyDelete(input.Key.Select(key => ObjectToRedisKey(key)).ToArray());
                return new Result(_result > -1, _result);
            }
			else if (input.ObjectType == ObjectType.Set)
			{
                var _result = database.SetRemove(ObjectToRedisKey(input.SetInput.Key), input.SetInput.Value.Select(value => ObjectToRedisValue(value)).ToArray());
                return new Result(_result > -1, _result);
            }
			else if (input.ObjectType == ObjectType.Json)
			{
				var _result = database.JSON().Del(ObjectToRedisKey(input.JsonInput.Key), string.IsNullOrEmpty(input.JsonInput.Path?.ToString()) ? "$" : input.JsonInput.Path.ToString());
				return new Result(_result > -1, _result);
			}
			else
			{
				throw new ArgumentException("InputObjectType is of invalid type. Accepted types are KeyValuePair, Set or Json");
			}
		}

        /// <summary>
        /// A Frends task for executing commands on Redis. Returns a list that has the results.
        /// See: https://github.com/CommunityHiQ/Frends.Community.Redis#Command
        /// </summary>
        /// <param name="input">The command and parameters for the task</param>
        /// <param name="connection">Connection-options</param>
        /// <param name="options">Additional options for the task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IEnumerable &lt;string&gt;</returns>
        public static IEnumerable<string> Command([PropertyTab] CommandInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            ConnectionMultiplexer connectionMultiplexer = null;
            setMinValue(options);
            cancellationToken.ThrowIfCancellationRequested();

            if (connection.UseCachedConnection)
            {
                connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
            }
            else
            {
                connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
            }

            IDatabase database = connectionMultiplexer.GetDatabase();

            RedisResult redisResult = database.Execute(input.Command, input.Parameters);

            if (redisResult.IsNull || redisResult.Resp2Type == ResultType.None)
            {
                return new List<string>();
            }

            switch (redisResult.Resp2Type)
            {
                case ResultType.BulkString:
                    return (IEnumerable<string>)redisResult.ToString().Split('\n');
                case ResultType.SimpleString:
                case ResultType.Integer:
                    return new List<string>() { redisResult.ToString() };
                default:
                    return new List<string>(redisResult.ToDictionary().SelectMany(keyVal => new List<string>() { keyVal.Key.ToString(), keyVal.Value.ToString() }));
            }
        }

		/// <summary>
		/// A Frends task for merging Json values in Redis.
		/// Returns a Result-object with properties Success and Value.
		/// </summary>
		/// <param name="input">The command and parameters for the task</param>
		/// <param name="connection">Connection-options</param>
		/// <param name="options">Additional options for the task</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Object { bool Success, object Value }</returns>
		public static Result Merge([PropertyTab] MergeInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
		{
			ConnectionMultiplexer connectionMultiplexer = null;
			setMinValue(options);
			cancellationToken.ThrowIfCancellationRequested();

			if (connection.UseCachedConnection)
			{
				connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}
			else
			{
				connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}

			IDatabase database = connectionMultiplexer.GetDatabase();
			
			if (input.ObjectType == ObjectType.Json)
			{
				bool result;
				if (input.JsonInput.Value is string)
				{
					var json = JsonConvert.DeserializeObject<dynamic>(input.JsonInput.Value.ToString());
					result = database.JSON().Merge(ObjectToRedisKey(input.JsonInput.Key), string.IsNullOrEmpty(input.JsonInput.Path?.ToString()) ? "$" : input.JsonInput.Path.ToString(), JsonConvert.SerializeObject(json));
				}
				else
				{
					result = database.JSON().Merge(ObjectToRedisKey(input.JsonInput.Key), string.IsNullOrEmpty(input.JsonInput.Path?.ToString()) ? "$" : input.JsonInput.Path.ToString(), input.JsonInput.Value);
				}

				return new Result(result, null);
			}
			else
			{
				throw new ArgumentException("InputObjectType is of invalid type. Accepted types Json");
			}
		}

		/// <summary>
		/// A Frends task for executing RediSearch queries on Redis.
		/// Returns a Result-object with properties Values.
		/// </summary>
		/// <param name="input">The command and parameters for the task</param>
		/// <param name="connection">Connection-options</param>
		/// <param name="options">Additional options for the task</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns> { object[] Values }</returns>
		public static GetResult Query([PropertyTab] JsonQueryInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
		{
			ConnectionMultiplexer connectionMultiplexer = null;
			List<object> results = new List<object>();
			setMinValue(options);
			cancellationToken.ThrowIfCancellationRequested();

			if (connection.UseCachedConnection)
			{
				connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}
			else
			{
				connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}

			IDatabase database = connectionMultiplexer.GetDatabase();

			SearchResult searchResult = database.FT().Search(
				input.IndexName,
				new(input.Query)
			);

			var documents = searchResult.Documents.Select(x => x["json"]);

			foreach (RedisValue value in documents)
			{
				cancellationToken.ThrowIfCancellationRequested();
				results.Add(value);
			}

			return new GetResult(results);
		}

		/// <summary>
		/// A Frends task for creating a RediSearch index on Redis. 
		/// Returns a Result-object with properties Success and Value.
		/// </summary>
		/// <param name="input">The command and parameters for the task</param>
		/// <param name="connection">Connection-options</param>
		/// <param name="options">Additional options for the task</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns> { bool Success, object Value }</returns>
		public static Result CreateIndex([PropertyTab] IndexCreateInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
		{
			ConnectionMultiplexer connectionMultiplexer = null;
			setMinValue(options);
			cancellationToken.ThrowIfCancellationRequested();

			if (connection.UseCachedConnection)
			{
				connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}
			else
			{
				connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}

			IDatabase database = connectionMultiplexer.GetDatabase();
			var schema = new Schema();

			foreach (var field in input.Parameters)
			{
				if (field.FieldType == FieldDefinitionType.Text)
				{
					schema.AddTextField(new FieldName(field.Name, field.Attribute));
				}
				else if (field.FieldType == FieldDefinitionType.Tag)
				{
					schema.AddTagField(new FieldName(field.Name, field.Attribute));
				}
				else if (field.FieldType == FieldDefinitionType.Numeric)
				{
					schema.AddNumericField(new FieldName(field.Name, field.Attribute));
				}
			}

			var indexCreated = database.FT()
				.Create(
					input.Name,
					new FTCreateParams()
						.On(IndexDataType.JSON)
						.Prefix(input.Prefix),
					schema);

			return new Result(indexCreated, null);
		}

		/// <summary>
		/// A Frends task for deleting a RediSearch index on Redis.
		/// </summary>
		/// <param name="input">The command and parameters for the task</param>
		/// <param name="connection">Connection-options</param>
		/// <param name="options">Additional options for the task</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns> { bool Success, object Value }</returns>
		public static Result DeleteIndex([PropertyTab] IndexDeleteInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
		{
			ConnectionMultiplexer connectionMultiplexer = null;
			setMinValue(options);
			cancellationToken.ThrowIfCancellationRequested();

			if (connection.UseCachedConnection)
			{
				connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}
			else
			{
				connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}

			IDatabase database = connectionMultiplexer.GetDatabase();
			var indexDeleted = database.FT()
				.DropIndex(
					input.Name,
					input.DeleteDocuments);

			return new Result(indexDeleted, null);
		}

		/// <summary>
		/// A Frends task for updating a RediSearch index on Redis.
		/// </summary>
		/// <param name="input">The command and parameters for the task</param>
		/// <param name="connection">Connection-options</param>
		/// <param name="options">Additional options for the task</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns> { bool Success, object Value }</returns>
		public static Result UpdateIndex([PropertyTab] IndexUpdateInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
		{
			ConnectionMultiplexer connectionMultiplexer = null;
			setMinValue(options);
			cancellationToken.ThrowIfCancellationRequested();

			if (connection.UseCachedConnection)
			{
				connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}
			else
			{
				connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}

			IDatabase database = connectionMultiplexer.GetDatabase();
			var schema = new Schema();
			var info = database.FT().Info(input.Name);

			foreach (var field in input.Parameters)
			{
				bool fieldExist = info.Attributes.Any(a => a.ContainsKey("identifier") && a["identifier"].ToString() == field.Name);

				if (!fieldExist)
				{
					if (field.FieldType == FieldDefinitionType.Text)
					{
						schema.AddTextField(new FieldName(field.Name, field.Attribute));
					}
					else if (field.FieldType == FieldDefinitionType.Tag)
					{
						schema.AddTagField(new FieldName(field.Name, field.Attribute));
					}
					else if (field.FieldType == FieldDefinitionType.Numeric)
					{
						schema.AddNumericField(new FieldName(field.Name, field.Attribute));
					}
				}
			}

			if (schema.Fields.Count == 0)
			{
				return new Result(true, "No new fields to add.");
			}

			var indexUpdated = database.FT()
				.Alter(
					input.Name,
					schema);

			return new Result(indexUpdated, null);
		}

		/// <summary>
		/// A Frends task for creating or updating a RediSearch index on Redis.
		/// If the index does not exist, it will be created. If it exists, it will be updated with new fields.
		/// </summary>
		/// <param name="input">The command and parameters for the task</param>
		/// <param name="connection">Connection-options</param>
		/// <param name="options">Additional options for the task</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns> { bool Success, object Value }</returns>
		public static Result CreateOrUpdateIndex([PropertyTab] IndexCreateOrUpdateInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
		{
			ConnectionMultiplexer connectionMultiplexer = null;
			setMinValue(options);
			cancellationToken.ThrowIfCancellationRequested();

			if (connection.UseCachedConnection)
			{
				connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}
			else
			{
				connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
			}

			IDatabase database = connectionMultiplexer.GetDatabase();
			var list = database.FT()._List();
			bool exists = list.Any(i => i.ToString() == input.Name);

			// Create index if it does not exist
			if (!exists)
			{
				return CreateIndex(new IndexCreateInput
				{
					Name = input.Name,
					Prefix = input.Prefix,
					Parameters = input.Parameters
				}, connection, options, cancellationToken);
			}

			// Update index if it exists
			return UpdateIndex(new IndexUpdateInput
			{
				Name = input.Name,
				Parameters = input.Parameters
			}, connection, options, cancellationToken);
		}


		internal static RedisKey ObjectToRedisKey(object obj)
        {
            switch (obj)
            {
                case string strVal:
                    return (string)strVal;
                case byte[] byteArray:
                    return (RedisKey)byteArray;
                default:
                    throw new ArgumentException("Key is a of invalid type. Accepted types are byte[] and string");
            }
        }

		internal static RedisValue ObjectToRedisValue(object obj)
        {
            switch (obj)
            {
                case int int32Val:
                    return (RedisValue)int32Val;
                case long int64Val:
                    return (RedisValue)int64Val;
                case byte[] byteArray:
                    return (RedisValue)byteArray;
                case double doubleVal:
                    return (RedisValue)doubleVal;
                case bool booleanVal:
                    return (RedisValue)booleanVal;
                case string strVal:
                    return (RedisValue)strVal;
				default:
                    throw new ArgumentException("Could not convert key to correct format. Accepted types are int32, int64, byte[], double, boolean or string");
            }
        }

        private static void setMinValue(Options options)
        {
            int minWorker, minIOC;
            ThreadPool.GetMinThreads(out minWorker, out minIOC);

            if (minWorker < options.Workers || minIOC < options.InputOutputCompletionPorts)
            {
                ThreadPool.SetMinThreads(options.Workers, options.InputOutputCompletionPorts);
            }
        }
    }
}
