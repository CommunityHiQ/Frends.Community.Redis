﻿using System.ComponentModel;
using System.Threading;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Frends.Community.Redis
{
    public static class Redis
    {
        /// <summary>
        /// A Frends task for Adding or Updating key-value pairs or a Sets to Redis.
        /// Returns a List of Result-objects that have properties Success and Value.
        /// </summary>
        /// <param name="input">Key-Value pairs or a Set</param>
        /// <param name="connection">Connection-options</param>
        /// <param name="options">Additional options for the task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A List of Result objects with following properties: bool Success, object Value.</returns>
        public static List<Result> Add([PropertyTab] AddInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
        {

            ConnectionMultiplexer connectionMultiplexer = null;
            List<Result> results = new List<Result>();
            setMinValue(options);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
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
                        results.Add(new Result() { Success = (long)_result > -1, Value = item.Key });
                    }
                }
                else
                {
                    foreach (var item in input.KeyValuePairInput)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var _result = database.StringSet(ObjectToRedisKey(item.Key), ObjectToRedisValue(item.Value), item.TTL, item.GetWhen());
                        results.Add(new Result() { Success = (bool)_result, Value = item.Key });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return results;

        }

        /// <summary>
        /// A Frends task for getting Key-Values or Sets from Redis
        /// </summary>
        /// <param name="input">An array of keys for key-value pairs or a key to a set</param>
        /// <param name="connection">Connection-options</param>
        /// <param name="options">Additional options for the task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List &lt;object&gt;</returns>
        public static List<object> Get([PropertyTab] GetInput input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            ConnectionMultiplexer connectionMultiplexer = null;
            List<object> results = new List<object>();
            setMinValue(options);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
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
                else
                {
                    redisValues = database.SetMembers(ObjectToRedisKey(input.SetKey));
                }

                foreach (RedisValue value in redisValues)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (value.HasValue)
                    {
                        results.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return results;
        }


        /// <summary>
        /// A Frends task for removing Key-Value pairs or Set values from Redis. 
        /// Returns a Result-object with properties Success and Value. 
        /// Value tells how many cache objects were removed
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

            try
            {
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
                    return new Result { Success = _result > -1, Value = _result };
                }
                else
                {
                    var _result = database.SetRemove(ObjectToRedisKey(input.SetInput.Key), input.SetInput.Value.Select(value => ObjectToRedisValue(value)).ToArray());
                    return new Result() { Success = _result > -1, Value = _result };
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// A Frends task for sending commands to Redis. Returns a list that has the output result.
        /// </summary>
        /// <param name="command">Redis command</param>
        /// <param name="parameters">parameters for command</param>
        /// <param name="connection">Connection-options</param>
        /// <param name="options">Additional options for the task</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IEnumerable &lt;string&gt;</returns>
        public static IEnumerable<string> Command(string command, object[] parameters, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            ConnectionMultiplexer connectionMultiplexer = null;
            setMinValue(options);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (connection.UseCachedConnection)
                {
                    connectionMultiplexer = RedisConnectionFactory.Instance.GetCachedRedisConnectionFactory(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
                }
                else
                {
                    connectionMultiplexer = RedisConnectionFactory.CreateConnectionWithTimeout(connection.ConnectionString, new TimeSpan(0, 0, connection.Timeout));
                }

                IDatabase database = connectionMultiplexer.GetDatabase();

                RedisResult redisResult = database.Execute(command, parameters);

                if (redisResult.IsNull || redisResult.Type == ResultType.None)
                {
                    return new List<string>();
                }

                switch (redisResult.Type)
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
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static RedisKey ObjectToRedisKey(object obj)
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
        private static RedisValue ObjectToRedisValue(object obj)
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

            if (minWorker < options.Workers || minIOC < options.IOCs)
            {
                ThreadPool.SetMinThreads(options.Workers, options.IOCs);
            }
        }
    }
}
