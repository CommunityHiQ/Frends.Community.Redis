#pragma warning disable 1591

using StackExchange.Redis;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.Redis
{
    /// <summary>
    /// Input-object
    /// </summary>
    public enum ObjectType { KeyValuePair, Set }

    /// <summary>
    /// When value exists
    /// </summary>
    public enum WhenValueExist { InsertAlways = 0, InsertOnlyIfValueExists = 1, InsertOnlyIfValueDoesNotExist = 2 };

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
    }

    /// <summary>
    /// Contains input elements for the command task
    /// </summary>
    public class CommandInput
    {
        /// <summary>
        /// The command to be executed
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Array of parameters for the command
        /// </summary>
        public object[] Parameters { get; set; }
    }

    /// <summary>
    /// Output-object
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Possible output values that depend on the task
        /// </summary>
        public object Value { get; set; }
    }
}
