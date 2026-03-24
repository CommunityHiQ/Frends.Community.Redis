# Frends.Community.Redis

frends Community Task for Redis.

[![Actions Status](https://github.com/CommunityHiQ/Frends.Community.Redis/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Community.Redis/actions) ![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Community.Redis) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 

- [Installing](#installing)
- [Tasks](#tasks)
     - [Add](#add)
       - [KeyValuePairInput](#keyvaluepairinput)
       - [SetInput](#setinput)
       - [JsonInput](#jsoninput)
     - [Get](#get)
     - [Remove](#remove)
     - [Command](#command)
     - [Merge](#merge)
     - [Query](#query)
     - [CreateIndex](#createindex)
     - [DeleteIndex](#deleteindex)
     - [UpdateIndex](#updateindex)
     - [CreateOrUpdateIndex](#createorupdateindex)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed:
- [NuGet Feed](https://www.myget.org/F/frends-community/api/v3/index.json)
- [MyGet Gallery](https://www.myget.org/feed/frends-community/package/nuget/Frends.Community.Redis)

# Tasks

## Add

A task for adding and updating key-value pairs, Sets, or JSON documents to Redis.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ObjectType | `Enum<KeyValuePair, Set, Json>` | Choose the type how values are stored to Redis. | `KeyValuePair` |
| KeyValuePairInput | Array of `KeyValuePairInput` | Pairs to be stored in Redis. | See below |
| SetInput | Array of `SetInput` | Sets to be stored in Redis. | See below |
| JsonInput | Array of `JsonInput` | JSON documents to be stored in Redis. | See below |

#### KeyValuePairInput

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Key | `object` | The chosen key for the key-value pair. | `myvalues` |
| Value | `object` | An object containing the values to be stored. | `"[{\"id\":201,\"name\":\"SkiBussi 1 GPS\"}]"` |
| TimeToLive | `TimeSpan` | TimeToLive limit for the value. |  |
| ValueExists | `Enum<InsertAlways, InsertOnlyIfValueExists, InsertOnlyIfValueDoesNotExist>` | What to do when value exists. | `InsertAlways` |

#### SetInput

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Key | `object` | The chosen key for the set of values. | `myvalues` |
| Value | Array of `object` | An array of values to be stored. | `["value1", "value2"]` |

#### JsonInput

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Key | `object` | The chosen key for the JSON document. | `user:1` |
| Value | `object` | The JSON value to be stored. Can be a JSON string or object. | `"{\"name\":\"John\",\"age\":30}"` |
| Path | `object` | JSONPath to specify where to store the value. Defaults to root `$`. Nonexistent paths are ignored. | `$.address` |

### Connection

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ConnectionString | `string` | The connection string to Redis db. | `contoso5.redis.cache.windows.net,ssl=true,password=password` |
| Timeout | `int` | Timeout threshold for the connection in seconds. | `60` |
| UseCachedConnection | `bool` | Use cached connection for the task? | `true` |

### Options

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Workers | `int` | The minimum number of worker threads to be used. Only applied if larger than default. | `3` |
| InputOutputCompletionPorts | `int` | The minimum number of asynchronous I/O completion threads to be used. Only applied if larger than default. | `6` |

### Returns

For each key-value pair, set, or JSON document, the task returns a list of result objects with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the insert succeeded. | `true` |
| Value | `object` | Key for the object that was to be inserted to db. | `myvalues` |
| ToJToken() | `JToken` | Returns the value of `Value` as JToken. | |

## Get

A task for getting stored data from Redis.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ObjectType | `Enum<KeyValuePair, Set, Json>` | Choose the type how values are stored in Redis. | `KeyValuePair` |
| Key | `object[]` | Array of keys for key-value pairs to fetch. | `["myvalue"]` |
| SetKey | `object` | The key for the set to fetch. | `"myvalue"` |
| JsonKey | `object` | The key for the JSON document to fetch. | `"user:1"` |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

Task returns an object with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Values | `List<object>` | List of result objects. | |
| ToJToken() | `JToken` | Returns the value of `Values` as JToken. | |

## Remove

A task for removing unwanted data from Redis.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ObjectType | `Enum<KeyValuePair, Set, Json>` | Choose the type how values are stored in Redis. | `KeyValuePair` |
| Key | `object[]` | Array of keys for key-value pairs to delete. | `["myvalue"]` |
| SetInput | `SetInput` | Set values to be removed. For more info, see [SetInput](#setinput). | |
| JsonInput | `JsonInput` | JSON document or path to be removed. For more info, see [JsonInput](#jsoninput). | |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

The task returns a result object with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the deletion succeeded. | `true` |
| Value | `object` | The number of removed objects. | `10` |
| ToJToken() | `JToken` | Returns the value of `Value` as JToken. | |

## Command

A task for executing commands on Redis.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Command | `string` | The command to be executed. | `PING` |
| Parameters | `object[]` | The array of parameters for the command. | `["useless_param"]` |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

Task returns `IEnumerable<string>` that contains the results of the command.

## Merge

A task for merging JSON values in Redis using the JSON.MERGE command.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ObjectType | `Enum<Json>` | The type of object. Currently only `Json` is supported. | `Json` |
| JsonInput | `JsonInput` | The JSON document to merge. For more info, see [JsonInput](#jsoninput). | |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

Task returns a result object with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the merge succeeded. | `true` |
| Value | `object` | Always `null` for this task. | |
| ToJToken() | `JToken` | Returns the value of `Value` as JToken. | |

## Query

A task for executing RediSearch queries on Redis (requires RedisSearch module).

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| IndexName | `string` | The name of the RediSearch index to query. | `idx:users` |
| Query | `string` | The RediSearch query string. | `Paul @age:[30 40]` |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

Task returns an object with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Values | `List<object>` | List of matching JSON documents. | |
| ToJToken() | `JToken` | Returns the value of `Values` as JToken. | |

## CreateIndex

A task for creating a RediSearch index on Redis (requires RedisSearch module). The index is created on JSON documents with the specified prefix.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Name | `string` | The name of the index to create. | `idx:users` |
| Prefix | `string` | The key prefix for documents to be indexed. | `user:` |
| Parameters | Array of `IndexFieldDefinition` | The fields to include in the index. | See below |

#### IndexFieldDefinition

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| FieldType | `Enum<Text, Geo, GeoShape, Numeric, Tag, Vector>` | The type of the field. | `Text` |
| Name | `string` | The JSONPath expression for the field identifier. | `$.name` |
| Attribute | `string` | An alias for the JSONPath expression. | `name` |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

Task returns a result object with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the index was created. | `true` |
| Value | `object` | Always `null` for this task. | |
| ToJToken() | `JToken` | Returns the value of `Value` as JToken. | |

## DeleteIndex

A task for deleting a RediSearch index on Redis (requires RedisSearch module).

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Name | `string` | The name of the index to delete. | `idx:users` |
| DeleteDocuments | `bool` | Whether to also delete the documents associated with the index. | `false` |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

Task returns a result object with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the index was deleted. | `true` |
| Value | `object` | Always `null` for this task. | |
| ToJToken() | `JToken` | Returns the value of `Value` as JToken. | |

## UpdateIndex

A task for updating an existing RediSearch index by adding new fields (requires RedisSearch module). Only fields that do not already exist in the index are added.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Name | `string` | The name of the index to update. | `idx:users` |
| Parameters | Array of `IndexFieldDefinition` | The fields to add to the index. Fields already present are skipped. | See [IndexFieldDefinition](#indexfielddefinition) |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

Task returns a result object with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the index was updated. | `true` |
| Value | `object` | `"No new fields to add."` if no changes were needed, otherwise `null`. | |
| ToJToken() | `JToken` | Returns the value of `Value` as JToken. | |

## CreateOrUpdateIndex

A task for creating or updating a RediSearch index on Redis (requires RedisSearch module). If the index does not exist it will be created; if it already exists, only new fields will be added.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Name | `string` | The name of the index to create or update. | `idx:users` |
| Prefix | `string` | The key prefix for documents to be indexed. Used only when creating a new index. | `user:` |
| Parameters | Array of `IndexFieldDefinition` | The fields to include in or add to the index. | See [IndexFieldDefinition](#indexfielddefinition) |

### Connection

For connections settings, see [Connection](#connection)

### Options

For other settings, see [Options](#options)

### Returns

Task returns a result object with the following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the operation succeeded. | `true` |
| Value | `object` | `"No new fields to add."` if no changes were needed, otherwise `null`. | |
| ToJToken() | `JToken` | Returns the value of `Value` as JToken. | |

# Building

Clone a copy of the repository:

git clone https://github.com/CommunityHiQ/Frends.Community.Redis.git

Rebuild the project:

dotnet build

Run tests:

dotnet test

Create a NuGet package:

dotnet pack --configuration Release

# Contributing

When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repository on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

**NOTE:** Be sure to merge the latest from "upstream" before making a pull request!

# Change Log

| Version | Changes |
| ------- | ------- |
| 1.0.1   | Initial overhaul from custom task to Community task. Added option settings for thread min values. Added remark about unit tests. |
| 1.0.2   | Renamed the property for IOCs. |
| 1.0.3   | ToJToken() added to result objects. |
| 1.0.4   | Downgraded Newtonsoft.Json to 12.0.0.0 |
| 1.1.0   | Added targeting to .NET6 and .NET8. Updated StackExchange.Redis to 2.8.24 and downgraded Newtonsoft.Json to 12.0.1. |
| 2.0.0   | **Breaking:** Dropped .NET 6 support, targeting .NET 8 only. Updated Newtonsoft.Json to 13.0.4, NRedisStack to 1.3.0, and StackExchange.Redis to 2.12.4. Added JSON support (RedisJSON) to Add, Get, and Remove tasks. Added new tasks: Merge, Query, CreateIndex, DeleteIndex, UpdateIndex, and CreateOrUpdateIndex (requires RedisSearch module). |