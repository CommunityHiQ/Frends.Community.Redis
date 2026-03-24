#pragma warning disable 1591

using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.Redis
{
    /// <summary>
    /// Input-object
    /// </summary>
    public enum ObjectType { KeyValuePair, Set, Json }

    /// <summary>
    /// When value exists
    /// </summary>
    public enum WhenValueExist { InsertAlways = 0, InsertOnlyIfValueExists = 1, InsertOnlyIfValueDoesNotExist = 2 };

	/// <summary>
	/// Index field definition type
	/// </summary>
	public enum FieldDefinitionType
	{
		Text,
		Geo,
		GeoShape,
		Numeric,
		Tag,
		Vector
	}


	/// <summary>
	/// Insert input-object 
	/// </summary>
	public class AddInput
    {
        /// <summary>
        /// Type of inserted object
        /// </summary>
        [DefaultValue(ObjectType.KeyValuePair)]
        public ObjectType InputObjectType { get; set; }

        /// <summary>
        /// Key-value pairs to insert
        /// </summary>
        [UIHint(nameof(InputObjectType), "", ObjectType.KeyValuePair)]
        public KeyValuePairInput[] KeyValuePairInput { get; set; }

        /// <summary>
        /// Set to insert
        /// </summary>
        [UIHint(nameof(InputObjectType), "", ObjectType.Set)]
        public SetInput[] SetInput { get; set; }

		/// <summary>
		/// Json to insert
		/// </summary>
		[UIHint(nameof(InputObjectType), "", ObjectType.Json)]
		public JsonInput[] JsonInput { get; set; }
	}

    /// <summary>
    /// Get values input
    /// </summary>
    public class GetInput
    {
        /// <summary>
        /// Object type
        /// </summary>
        [DefaultValue(ObjectType.KeyValuePair)]
        public ObjectType ObjectType { get; set; }

        /// <summary>
        /// Keys for Key-Value pairs
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(ObjectType), "", ObjectType.KeyValuePair)]
        public object[] Key { get; set; }

        /// <summary>
        /// Set key
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(ObjectType), "", ObjectType.Set)]
        public object SetKey { get; set; }

		/// <summary>
		/// Json key
		/// </summary>
		[DisplayFormat(DataFormatString = "Text")]
		[UIHint(nameof(ObjectType), "", ObjectType.Json)]
		public object JsonKey { get; set; }
	}

	/// <summary>
	/// Contains input elements for the command task
	/// </summary>
	public class CommandInput
	{
		/// <summary>
		/// The command to be executed.
		/// Ex: idx:users
		/// </summary>
		public string Command { get; set; }

		/// <summary>
		/// Array of parameters for the command.
		/// Ex: user:
		/// </summary>
		public object[] Parameters { get; set; }
	}

	/// <summary>
	/// Merge input object for Redis JSON. 
	/// Contains the JSON to merge and the path where the JSON should be merged. 
	/// If path is not provided, it will be merged at root ($)
	/// </summary>
	public class MergeInput
	{
		/// <summary>
		/// Object type
		/// </summary>
		[DefaultValue(ObjectType.KeyValuePair)]
		public ObjectType ObjectType { get; set; }

		/// <summary>
		/// Json to merge
		/// </summary>
		[UIHint(nameof(ObjectType), "", ObjectType.Json)]
		public JsonInput JsonInput { get; set; }
	}

	public class DeleteInput
	{
		/// <summary>
		/// Object type
		/// </summary>
		[DefaultValue(ObjectType.KeyValuePair)]
		public ObjectType ObjectType { get; set; }

		/// <summary>
		/// Set key and set values to delete
		/// </summary>
		[UIHint(nameof(ObjectType), "", ObjectType.Set)]
		public SetInput SetInput { get; set; }

		/// <summary>
		/// Keys for Key-Value pairs to delete
		/// </summary>
		[UIHint(nameof(ObjectType), "", ObjectType.KeyValuePair)]
		[DisplayFormat(DataFormatString = "Text")]
		public object[] Key { get; set; }

		[UIHint(nameof(ObjectType), "", ObjectType.Json)]
		public JsonInput JsonInput { get; set; }
	}

	/// <summary>
	/// Connection options
	/// </summary>
	public class Connection
    {
        /// <summary>
        /// Connection string
        /// </summary>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("contoso5.redis.cache.windows.net,ssl=true,password=password")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Timeout for operations in seconds
        /// </summary>
        [DefaultValue(5)]
        public int Timeout { get; set; }

        /// <summary>
        /// Should the task use cached connections or create a new connection each time. Recomended value is TRUE
        /// </summary>
        [DefaultValue(true)]
        public bool UseCachedConnection { get; set; }
    }

    /// <summary>
    /// Additional options
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Minimum number of worker threads
        /// </summary>
        [DefaultValue(4)]
        public int Workers { get; set; }

        /// <summary>
        /// Minimum number of asynchronous I/O completion threads
        /// </summary>
        [DefaultValue(1)]
        public int InputOutputCompletionPorts { get; set; }
	}

    /// <summary>
    /// Input object for key-value pairs
    /// </summary>
    public class KeyValuePairInput
    {
        /// <summary>
        /// Cache key
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// Cache Value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Time to live for the object. Can be null/empty if there is no need to set a TTL,
        /// otherwise create a new TimeSpan() with desired values
        /// </summary>      
        public TimeSpan? TimeToLive { get; set; }

        /// <summary>
        /// If value exist
        /// </summary>
        [DefaultValue(WhenValueExist.InsertAlways)]
        public WhenValueExist ValueExists { get; set; }
        internal When GetWhen()
        {
            switch (this.ValueExists)
            {
                case WhenValueExist.InsertAlways:
                    return When.Always;
                case WhenValueExist.InsertOnlyIfValueDoesNotExist:
                    return When.NotExists;
                case WhenValueExist.InsertOnlyIfValueExists:
                    return When.Exists;
                default:
                    throw new ArgumentException($"Invalid enum {this.ValueExists}");
            }
        }
    }

    /// <summary>
    /// Object for when inputing a set
    /// </summary>
    public class SetInput
    {
        /// <summary>
        /// Set key
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public object Key { get; set; }

        /// <summary>
        /// Set values
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public object[] Value { get; set; }
    }

	public class JsonInput
	{
		/// <summary>
		/// Json key
		/// </summary>
		[DisplayFormat(DataFormatString = "Text")]
		public object Key { get; set; }

		/// <summary>
		/// Json value
		/// </summary>
		[DisplayFormat(DataFormatString = "Text")]
		public object Value { get; set; }

		/// <summary>
		/// JSONPath to specify. Default is root $. Nonexisting paths are ignored.
		/// </summary>
		[DisplayFormat(DataFormatString = "Text")]
		public object Path { get; set; }
	}

	public class JsonQueryInput
	{
		/// <summary>
		/// Index name for Json query
		/// </summary>
		[DisplayFormat(DataFormatString = "Text")]
		public string IndexName { get; set; }

		/// <summary>
		/// Json query string
		/// Ex: Paul @age:[30 40]
		/// </summary>
		[DisplayFormat(DataFormatString = "Text")]
		public string Query { get; set; }
	}

	public class IndexFieldDefinition
	{
		/// <summary>
		/// Field definition type
		/// </summary>
		public FieldDefinitionType FieldType { get; set; }

		/// <summary>
		/// The identifier is a JSON Path expression
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Alias for th JSONPath expression 
		/// </summary>
		public string Attribute { get; set; }
	}

	public class IndexCreateInput
	{
		/// <summary>
		/// The name of the index.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The prefix of the keys that should be indexed.
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// Array of fields for the index.
		/// </summary>
		public IndexFieldDefinition[] Parameters { get; set; }
	}

	public class IndexDeleteInput
	{
		/// <summary>
		/// The name of the index.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///  Delete the documents associated with the index.
		/// </summary>
		[DefaultValue(false)]
		public bool DeleteDocuments { get; set; }
	}

	public class IndexUpdateInput
	{
		/// <summary>
		/// The name of the index.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Array of fields for the index.
		/// </summary>
		public IndexFieldDefinition[] Parameters { get; set; }
	}

	public class IndexCreateOrUpdateInput
	{
		/// <summary>
		/// The name of the index.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The prefix of the keys that should be indexed.
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// Array of fields for the index.
		/// </summary>
		public IndexFieldDefinition[] Parameters { get; set; }
	}

	/// <summary>
	/// Output-object
	/// </summary>
	public class Result
    {
        private readonly Lazy<JToken> _jToken;

        /// <summary>
        /// Constructor for the result object
        /// </summary>
        /// <param name="succeeded">Boolean status if task succeeded.</param>
        /// <param name="data">Value(s) returned by the Redis task.</param>
        public Result(bool succeeded, object data)
        {
            Success = succeeded;
            Value = data;
            _jToken = new Lazy<JToken>(() => JToken.FromObject(Value));
        }
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Possible output values that depend on the task
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Returns the result object as JToken
        /// </summary>
        /// <returns>JToken</returns>
        public JToken ToJToken()
        {
            return _jToken.Value;
        }
    }

    /// <summary>
    /// Output-object
    /// </summary>
    public class GetResult
    {
        private readonly Lazy<JToken> _jToken;

        /// <summary>
        /// Constructor for the result object
        /// </summary>
        /// <param name="results">Value(s) returned by the Redis task.</param>
        public GetResult(List<object> results)
        {
            Values = results;
            _jToken = new Lazy<JToken>(() => JToken.FromObject(Values));
        }

        /// <summary>
        /// Possible output values that depend on the task
        /// </summary>
        public List<object> Values { get; set; }

        /// <summary>
        /// Returns the result object as JToken
        /// </summary>
        /// <returns>JToken</returns>
        public JToken ToJToken()
        {
            return _jToken.Value;
        }
    }
}
