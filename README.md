# dotnet-etcd
![](https://raw.githubusercontent.com/shubhamranjan/dotnet-etcd/master/docs/img/etcd-logo-rectangle.png)

A C# .NET (dotnet) GRPC client for etcd v3+

[![Build Status](https://travis-ci.org/shubhamranjan/dotnet-etcd.svg?branch=master)](https://travis-ci.org/shubhamranjan/dotnet-etcd)
![Nuget Version Info](https://img.shields.io/nuget/v/dotnet-etcd.svg)
![Nuget Download Info](https://img.shields.io/nuget/dt/dotnet-etcd.svg)

## Supported .NET Versions

* .NETCoreApp 2.1
* .NETCoreApp 2.0
* .NETStandard 2.0
* .NETFramework 4.7.2
* .NETFramework 4.7.1
* .NETFramework 4.7
* .NETFramework 4.6.2
* .NETFramework 4.6.1
* .NETFramework 4.6
* .NETFramework 4.5.2
* .NETFramework 4.5.1
* .NETFramework 4.5


## Installing Package
Nuget package is published on [nuget.org](https://www.nuget.org/packages/dotnet-etcd/) and can be installed in the following ways :
    
### Nuget Package Manager
    
    Install-Package dotnet-etcd

### .NET CLI
    
    dotnet add package dotnet-etcd

### Paket CLI
    
    paket add dotnet-etcd
` The NuGet Team does not provide support for this client. Please contact its maintainers for support.`

## Usage :

Add using statement at the top of your class file

    using dotnet_etcd;

### Client Initialization

#### No Basic auth or SSL
    
    EtcdClient client = new EtcdClient(<HOSTNAME_STRING>, <PORTNO_INT>);
    // E.g.
    EtcdClient client = new EtcdClient("127.0.0.1", 2379);

#### Available Constructor Parameters

* username - String containing username for etcd basic auth. Default : Empty String 
* password - String containing password for etcd basic auth. Default : Empty String 
* caCert - String containing ca cert when using self signed certificates with etcd. Default : EmptyString 
* clientCert - String containing client cert when using self signed certificates with client auth enabled in etcd. Default : EmptyString 
* clientKey - String containing client key when using self signed certificates with client auth enabled in etcd. Default : EmptyString 
* publicRootCa - Bool depicting whether to use publicy trusted roots to connect to etcd. Default : false.
    

### Operations

A lot of methods have been implemented using etcd's default input/output parameters. I am simplifying a lot of methods by including more overloads as I come across use cases. If you have some, please feel free to raise and issue or a PR :)

#### Put a key

    client.Put(<KEY_STRING>,<VALUE_STRING>);
    // E.g Put key "foo/bar" with value "foobar"
    client.Put("foo/bar","barfoo");

    await client.PutAsync(<KEY_STRING>,<VALUE_STRING>);
    // E.g Put key "foo/bar" with value "foobar" in async
    await client.PutAsync("foo/bar","barfoo");

#### Get a key
    
    client.GetVal(<KEY_STRING>);
    // E.g Get key "foo/bar"
    client.GetVal("foo/bar");
    // To get full etcd response
    client.Get("foo/bar");

    await client.GetValAsync(<KEY_STRING>);
    // E.g. Get key "foo/bar" in async
    await client.GetValAsync("foo/bar");
    // To get full etcd response
    await client.GetAsync("foo/bar");

#### Get multiple keys with a common prefix

    client.GetRange(<PREFIX_STRING>);
    // E.g. Get all keys with pattern "foo/*"
    client.GetRange("foo/"); 

    await client.GetRangeAsync(<PREFIX_STRING>);
    // E.g. Get all keys with pattern "foo/*" in async
    await client.GetRangeAsync("foo/");

#### Watch a key

    WatchRequest request = new WatchRequest()
    {
        CreateRequest = new WatchCreateRequest()
        {
            Key = ByteString.CopyFromUtf8("foo")
        }
    };
    etcdClient.Watch(request, print);

    -------------------------------
    // Print function that prints key and value from the watch response
    private static void print(WatchResponse response)
    {   
        if (response.Events.Count == 0)
        {
            Console.WriteLine(response);
        }
        else
        {
            Console.WriteLine($"{response.Events[0].Kv.Key.ToStringUtf8()}:{response.Events .Kv.Value.ToStringUtf8()}");
        }
    }

    ----------------------------------
    // Print function that prints key and value from the minimal watch
    // response data 
    private static void print(WatchEvent[] response)
    {
        foreach(WatchEvent e1 in response)
        {
            Console.WriteLine($"{e1.Key}:{e1.Value}:{e1.Type}");
        }
    }
##### Watch also has a simple overload as follows
    etcdClient.Watch("foo", print);

###### More overloads are also available. You can check them using IntelliSense (Ctrl+Shift+Space). Detailed documentation coming soon.

#### Delete a key

    client.Delete(<KEY_STRING>);
    // E.g. Delete key "foo/bar"
    client.Delete("foo/bar");

    await client.DeleteAsync(<KEY_STRING>);
    // E.g. Delete key "foo/bar" in async
    await client.DeleteAsync("foo/bar");

#### Delete multiple keys with a common prefix

    client.DeleteRange(<PREFIX_STRING>);
    // E.g. Delete all keys with pattern "foo/*"
    client.DeleteRange("foo/"); 

    await client.DeleteRangeAsync(<PREFIX_STRING>);
    // E.g. Delete all keys with pattern "foo/*" in async
    await client.DeleteRangeAsync("foo/");


## Coming Soon
* Auth Operations (Add users,roles etc.)
* ~~Watch support~~ Released v2.0.0 +