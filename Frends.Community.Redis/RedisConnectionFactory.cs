using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Frends.Community.Redis
{
    public sealed class RedisConnectionFactory : IDisposable
    {
        private static readonly Lazy<RedisConnectionFactory> instanceHolder = new Lazy<RedisConnectionFactory>(() => new RedisConnectionFactory());

        public static RedisConnectionFactory Instance
        {
            get { return instanceHolder.Value; }
        }

        private static readonly object lockObject = new object();

        private readonly ConcurrentDictionary<string, StackExchange.Redis.ConnectionMultiplexer> _connections = new ConcurrentDictionary<string, StackExchange.Redis.ConnectionMultiplexer>();

        private RedisConnectionFactory()
        {

        }

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
        public void Dispose(bool disposing)
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

        
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
