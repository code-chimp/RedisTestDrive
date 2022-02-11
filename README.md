# Redis/C# Test Drive

**NOTE:** Examples were run against a Redis Docker container started with the following parameters
```shell
docker run \
  --name redis_dev \
  -p 6379:6379 \
  -d redis:6.2.6
```

## Data Types

### String

The most basic Redis use case is caching anything as a binary-safe string. "Anything"
being any data that can be represented as string such as PDF documents, serialized objects,
images, etc.

StackExchange supplies some handy abstractions such as the ability to store objects
or collections by leveraging Newtonsoft to serialize/deserialize them as JSON strings.

#### Use Case(s):

- Stable static strings fetched from a data source
- Expensive ORM objects
- Rendered articles

#### Example(s):

```c#
// add a simple string to the database
var success = await db.AddAsync("some:string", "now is the winter of our discontent");

// or something that does not need to be re-run through a rendering pipeline
var success = await db.AddAsync("some:content", "<article>....</article>");

// objects are handled seamlessly by StackOverflow's extension methids
var myObj = new SimpleObject { Id = 5, Value = "Supercalifragilistic", Created = DateTime.Now };
var success = await db.AddAsync("some:object", myObject);

// retrieval and deserialization is just as easy
var value = await db.GetAsync<SimpleObject>("some:object");

// or we can bypass the StackExchange abstractions to the the core Redis functionality
//  in this case to specify we are retrieving a string and we do not need
//  it to go through Newtonsoft for deserialization
var value = await db.Database.StringGetAsync(key);
```

### Hash

Summary description.

#### Use Case(s):

- Thing
- Other thing

#### Example(s):

```c#
// show the redis code guy
```

### List

Summary description.

#### Use Case(s):

- Thing
- Other thing

#### Example(s):

```c#
// show the redis code guy
```

### Set

Summary description.

#### Use Case(s):

- Thing
- Other thing

#### Example(s):

```c#
// show the redis code guy
```

### Sorted Set

Summary description.

#### Use Case(s):

- Thing
- Other thing

#### Example(s):

```c#
// show the redis code guy
```

### Geo

Summary description.

#### Use Case(s):

- Thing
- Other thing

#### Example(s):

```c#
// Create a set of Geographic points
var key = "my:geo:set";

foreach (var geoPoint in _geoPoints)
{
    await db.Database.GeoAddAsync(key, geoPoint.Longitude, geoPoint.Latitude, geoPoint.Label);
}

```

## Features

### Publish / Subscribe

### Streams
