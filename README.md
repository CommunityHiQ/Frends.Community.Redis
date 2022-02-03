# Frends.Community.Redis

frends Community Task for Redis.

[![Actions Status](https://github.com/CommunityHiQ/Frends.Community.Redis/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Community.Redis/actions) ![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Community.Redis) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 

- [Installing](#installing)
- [Tasks](#tasks)
     - [Add](#Add)
       - [KeyValuePairInput](#KeyValuePairInput)
       - [SetInput](#SetInput)
     - [Get](#Get)
     - [Remove](#Remove)
     - [Command](#Command)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed
https://www.myget.org/F/frends-community/api/v3/index.json and in Gallery view in MyGet https://www.myget.org/feed/frends-community/package/nuget/Frends.Community.Redis

# Tasks

## Add

A task for adding and updating key-value pairs or Sets to Redis.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ObjectType | `Enum<KeyValuePair, Set>` | Choose the type how values are stored to Redis. | `KeyValuePair` |
| KeyValuePairInput | Array of `KeyValuePairInput` | Pairs to be stored in Redis. | See below |
| SetInput | Array of `SetInput` | Sets to be stored in Redis. | See below |

#### KeyValuePairInput

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Key | `object` | The chosen key for the key-value pair. | `myvalues` |
| Value | `object` | An object containing the values to be stored. | `"[{\"id\":201,\"name\":\"SkiBussi 1 GPS\",\"direction\":-1,\"geolocation\":{\"lat\":67.8045555581415,\"lon\":24.8096944500295},\"route\":2}]"` |
| TimeToLive | `TimeSpan` | TimeToLive limit for the value. |  |
| ValueExists | `Enum<InsertAlways, InsertOnlyIfValueExists, InsertOnlyIfValueDoesNotExist>` | What to do when value exists. | `InsertAlways` |

#### SetInput

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Key | `object` | The chosen key for the set of values. | `myvalues` |
| Value | Array of `object` | An array of values to be stored. | `"[{\"id\":201,\"name\":\"SkiBussi 1 GPS\",\"direction\":-1,\"geolocation\":{\"lat\":67.8045555581415,\"lon\":24.8096944500295},\"route\":2}]"` |

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

For each key-value pair or set, the task returns a list of result objects with following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the insert succeeded. | `true` |
| Value | `object` | Key for the object that was to be inserted to db. | `myvalues` |
| ToJToken() | JToken | Returns the value of `Value` as JToken. | |

## Get

A task for getting stored data from Redis.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ObjectType | `Enum<KeyValuePair, Set>` | Choose the type how values are stored in Redis. | `KeyValuePair` |
| Key | `object[]` | Array of keys for key-value pairs to fetch. | `["myvalue"]` |
| SetKey | `object` | The key for the set to fetch. | `"myvalue"` |

### Connection

For connections settings, see [Connection](#Connection)

### Options

For other settings, see [Options](#Options)

### Returns

Task returns an object with following properties

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Values | `List<object>` | List of result objects. | |
| ToJToken() | JToken | Returns the value of `Values` as JToken. | |

## Remove

A task for removing wanted data from Redis.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ObjectType | `Enum<KeyValuePair, Set>` | Choose the type how values are stored in Redis. | `KeyValuePair` |
| Key | `object[]` | Array of keys for key-value pairs to fetch. | `["myvalue"]` |
| SetInput | `SetInput` | Set values to be removed. For more info, see [SetInput](#SetInput). | `""` |

### Connection

For connections settings, see [Connection](#Connection)

### Options

For other settings, see [Options](#Options)

### Returns

For each key-value pair or set, the task returns a list of result objects with following properties:

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Success | `bool` | Tells whether or not the deletion succeeded. | `true` |
| Value | `object` | The number of removed objects. | `10` |
| ToJToken() | JToken | Returns the value of `Value` as JToken. | |

## Command

A task for executing commands on Redis.

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Command | `string` | The command to be executed. | `PING` |
| Parameters | `object[]` | The array of parameters for the command. | `["useless_param"]` |

### Connection

For connections settings, see [Connection](#Connection)

### Options

For other settings, see [Options](#Options)

### Returns

Task returns `IEnumerable<string>` that contains the results of the command.

# Building

Clone a copy of the repository

`git clone https://github.com/CommunityHiQ/Frends.Community.Redis.git`

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repository on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

# Change Log

| Version | Changes |
| ------- | ------- |
| 1.0.1   | Initial overhaul from custom task to Community task. Added option settings for thread min values. Added remark about unit tests. |
| 1.0.2   | Renamed the property for IOCs. |
| 1.0.3   | ToJToken() added to result objects. |
