using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Frends.Community.Redis
{
    /// <summary>
    /// Contains Redis connection implementations.
    /// </summary>
    public sealed class RedisConnectionFactory : IDisposable
    {
        private static readonly Lazy<RedisConnectionFactory> instanceHolder = new Lazy<RedisConnectionFactory>(() => new RedisConnectionFactory());

        /// <summary>
        /// The connection holder.
        /// </summary>
        public static RedisConnectionFactory Instance
        {
            get { return instanceHolder.Value; }
        }

        private static readonly object lockObject = new object();

        private readonly ConcurrentDictionary<string, StackExchange.Redis.ConnectionMultiplexer> _connections = new ConcurrentDictionary<string, StackExchange.Redis.ConnectionMultiplexer>();

        private RedisConnectionFactory()
        {

        }

        /// <summary>
        /// Fetches the cached connection.
        /// </summary>
        /// <param name="connectionString">The connection string to Redis db.</param>
        /// <param name="timeout">Timeout threshold for the connection.</param>
        /// <returns></returns>
        public ConnectionMultiplexer GetCachedRedisConnectionFactory(string connectionString, TimeSpan timeout)
        {
            string key = $"{timeout.TotalSeconds}-{connectionString}";

            if (!_connections.ContainsKey(key))
            {
                lock (lockObject)
                {
                    if (!_connections.ContainsKey(key))
                    {
                        _connections.TryAdd(key, CreateConnectionWithTimeout(connectionString, timeout));
                    }
                }
            }
            return _connections[key];
        }

        internal static ConnectionMultiplexer CreateConnectionWithTimeout(string connectionString, TimeSpan timeout)
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = (int)timeout.TotalMilliseconds;
            return ConnectionMultiplexer.Connect(options);
        }


        private bool _disposedValue = false;
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                var factoriesToClose = _connections.ToList();
                _connections.Clear();

                if (disposing)
                {
                    //dispose managed objects
                }

                foreach (var item in factoriesToClose)
                {
                    try
                    {
                        item.Value.Close();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Error when disposing Redis factory connections: {ex.Message}");
                    }
                }
                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the current connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
