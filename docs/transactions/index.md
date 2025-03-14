# Transaction Operations

This page documents how to use transactions in etcd using the `dotnet-etcd` client.

## Overview

etcd provides transaction operations that allow you to execute multiple operations atomically. Transactions in etcd follow a "compare-and-swap" pattern:

1. **Compare**: Check if a set of conditions are true
2. **Success**: Execute a set of operations if all conditions are true
3. **Failure**: Execute a different set of operations if any condition is false

This pattern ensures that operations are executed atomically and consistently, which is essential for distributed systems.

## Basic Transaction

Here's a basic example of a transaction that checks if a key exists and then either puts a value or gets the current value:

```csharp
// Create a transaction
var txn = new Transaction();

// Add a comparison to check if the key exists
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Create,
    Result = Compare.Types.CompareResult.Equal,
    CreateRevision = 0 // 0 means the key doesn't exist
});

// If the comparison is true (key doesn't exist), put a value
txn.Success.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        Value = ByteString.CopyFromUtf8("initial-value")
    }
});

// If the comparison is false (key exists), get the current value
txn.Failure.Add(new RequestOp
{
    RequestRange = new RangeRequest
    {
        Key = ByteString.CopyFromUtf8("my-key")
    }
});

// Execute the transaction
var response = client.Transaction(txn);

// Check if the transaction succeeded
if (response.Succeeded)
{
    Console.WriteLine("Transaction succeeded: Key didn't exist, value was set");
}
else
{
    // Get the current value from the failure response
    var rangeResponse = response.Responses[0].ResponseRange;
    var currentValue = rangeResponse.Kvs[0].Value.ToStringUtf8();
    Console.WriteLine($"Transaction failed: Key exists with value '{currentValue}'");
}
```

## Transaction with Multiple Comparisons

You can add multiple comparisons to a transaction. All comparisons must be true for the success operations to execute:

```csharp
// Create a transaction
var txn = new Transaction();

// Add comparisons
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("key1"),
    Target = Compare.Types.CompareTarget.Value,
    Result = Compare.Types.CompareResult.Equal,
    Value = ByteString.CopyFromUtf8("expected-value1")
});

txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("key2"),
    Target = Compare.Types.CompareTarget.Version,
    Result = Compare.Types.CompareResult.Greater,
    Version = 2
});

// Add success operations
txn.Success.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("key3"),
        Value = ByteString.CopyFromUtf8("value3")
    }
});

// Add failure operations
txn.Failure.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("key4"),
        Value = ByteString.CopyFromUtf8("value4")
    }
});

// Execute the transaction
var response = client.Transaction(txn);

// Check if the transaction succeeded
if (response.Succeeded)
{
    Console.WriteLine("Transaction succeeded: All comparisons were true");
}
else
{
    Console.WriteLine("Transaction failed: At least one comparison was false");
}
```

## Transaction with Multiple Operations

You can add multiple operations to both the success and failure branches:

```csharp
// Create a transaction
var txn = new Transaction();

// Add a comparison
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("counter"),
    Target = Compare.Types.CompareTarget.Value,
    Result = Compare.Types.CompareResult.Equal,
    Value = ByteString.CopyFromUtf8("5")
});

// Add multiple success operations
txn.Success.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("counter"),
        Value = ByteString.CopyFromUtf8("6")
    }
});

txn.Success.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("status"),
        Value = ByteString.CopyFromUtf8("incremented")
    }
});

// Add multiple failure operations
txn.Failure.Add(new RequestOp
{
    RequestRange = new RangeRequest
    {
        Key = ByteString.CopyFromUtf8("counter")
    }
});

txn.Failure.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("status"),
        Value = ByteString.CopyFromUtf8("failed-to-increment")
    }
});

// Execute the transaction
var response = client.Transaction(txn);

// Check if the transaction succeeded
if (response.Succeeded)
{
    Console.WriteLine("Transaction succeeded: Counter was incremented");
}
else
{
    var rangeResponse = response.Responses[0].ResponseRange;
    var currentValue = rangeResponse.Kvs[0].Value.ToStringUtf8();
    Console.WriteLine($"Transaction failed: Counter value was '{currentValue}', not '5'");
}
```

## Asynchronous Transaction

You can execute a transaction asynchronously:

```csharp
// Create a transaction
var txn = new Transaction();

// Add a comparison
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Create,
    Result = Compare.Types.CompareResult.Equal,
    CreateRevision = 0
});

// Add success operations
txn.Success.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        Value = ByteString.CopyFromUtf8("initial-value")
    }
});

// Add failure operations
txn.Failure.Add(new RequestOp
{
    RequestRange = new RangeRequest
    {
        Key = ByteString.CopyFromUtf8("my-key")
    }
});

// Execute the transaction asynchronously
var response = await client.TransactionAsync(txn);

// Check if the transaction succeeded
if (response.Succeeded)
{
    Console.WriteLine("Transaction succeeded: Key didn't exist, value was set");
}
else
{
    var rangeResponse = response.Responses[0].ResponseRange;
    var currentValue = rangeResponse.Kvs[0].Value.ToStringUtf8();
    Console.WriteLine($"Transaction failed: Key exists with value '{currentValue}'");
}
```

## Comparison Types

etcd supports several types of comparisons:

### Value Comparison

Compare the value of a key:

```csharp
// Compare if the value of "my-key" equals "expected-value"
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Value,
    Result = Compare.Types.CompareResult.Equal,
    Value = ByteString.CopyFromUtf8("expected-value")
});
```

### Version Comparison

Compare the version of a key (incremented each time the key is modified):

```csharp
// Compare if the version of "my-key" is greater than 2
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Version,
    Result = Compare.Types.CompareResult.Greater,
    Version = 2
});
```

### Create Revision Comparison

Compare the creation revision of a key (the revision when the key was first created):

```csharp
// Compare if the create revision of "my-key" is equal to 100
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Create,
    Result = Compare.Types.CompareResult.Equal,
    CreateRevision = 100
});
```

### Mod Revision Comparison

Compare the modification revision of a key (the revision when the key was last modified):

```csharp
// Compare if the mod revision of "my-key" is less than 200
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Mod,
    Result = Compare.Types.CompareResult.Less,
    ModRevision = 200
});
```

### Lease Comparison

Compare the lease ID of a key:

```csharp
// Compare if the lease of "my-key" equals the specified lease ID
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Lease,
    Result = Compare.Types.CompareResult.Equal,
    Lease = leaseId
});
```

## Comparison Results

etcd supports several comparison results:

- `Equal`: Check if the target is equal to the provided value
- `NotEqual`: Check if the target is not equal to the provided value
- `Greater`: Check if the target is greater than the provided value
- `Less`: Check if the target is less than the provided value

## Operation Types

You can include several types of operations in the success and failure branches:

### Put Operation

Put a key-value pair:

```csharp
txn.Success.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("key"),
        Value = ByteString.CopyFromUtf8("value")
    }
});
```

### Get Operation

Get a key or range of keys:

```csharp
txn.Success.Add(new RequestOp
{
    RequestRange = new RangeRequest
    {
        Key = ByteString.CopyFromUtf8("key")
    }
});
```

### Delete Operation

Delete a key or range of keys:

```csharp
txn.Success.Add(new RequestOp
{
    RequestDeleteRange = new DeleteRangeRequest
    {
        Key = ByteString.CopyFromUtf8("key")
    }
});
```

### Txn Operation (Nested Transaction)

You can even include a nested transaction:

```csharp
var nestedTxn = new Transaction();
// Configure the nested transaction...

txn.Success.Add(new RequestOp
{
    RequestTxn = nestedTxn
});
```

## Common Transaction Patterns

### Compare-and-Swap

The classic compare-and-swap pattern ensures that a value is only updated if it matches an expected value:

```csharp
// Create a transaction for compare-and-swap
var txn = new Transaction();

// Compare if the current value equals the expected value
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Value,
    Result = Compare.Types.CompareResult.Equal,
    Value = ByteString.CopyFromUtf8("old-value")
});

// If true, update the value
txn.Success.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        Value = ByteString.CopyFromUtf8("new-value")
    }
});

// Execute the transaction
var response = client.Transaction(txn);

// Check if the swap was successful
if (response.Succeeded)
{
    Console.WriteLine("Compare-and-swap succeeded");
}
else
{
    Console.WriteLine("Compare-and-swap failed: Current value doesn't match expected value");
}
```

### Create-if-Not-Exists

Create a key only if it doesn't already exist:

```csharp
// Create a transaction for create-if-not-exists
var txn = new Transaction();

// Compare if the key doesn't exist
txn.Compare.Add(new Compare
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Target = Compare.Types.CompareTarget.Create,
    Result = Compare.Types.CompareResult.Equal,
    CreateRevision = 0
});

// If true, create the key
txn.Success.Add(new RequestOp
{
    RequestPut = new PutRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        Value = ByteString.CopyFromUtf8("initial-value")
    }
});

// Execute the transaction
var response = client.Transaction(txn);

// Check if the creation was successful
if (response.Succeeded)
{
    Console.WriteLine("Key created successfully");
}
else
{
    Console.WriteLine("Key already exists");
}
```

### Atomic Counter

Implement an atomic counter that increments a value:

```csharp
// Function to atomically increment a counter
async Task<int> IncrementCounterAsync(string counterKey)
{
    while (true)
    {
        // Get the current counter value
        var getResponse = await client.GetAsync(counterKey);
        
        int currentValue = 0;
        if (getResponse.Count > 0)
        {
            currentValue = int.Parse(getResponse.Kvs[0].Value.ToStringUtf8());
        }
        
        int newValue = currentValue + 1;
        
        // Create a transaction
        var txn = new Transaction();
        
        if (getResponse.Count > 0)
        {
            // Compare if the current value hasn't changed
            txn.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(counterKey),
                Target = Compare.Types.CompareTarget.Value,
                Result = Compare.Types.CompareResult.Equal,
                Value = ByteString.CopyFromUtf8(currentValue.ToString())
            });
        }
        else
        {
            // Compare if the key still doesn't exist
            txn.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(counterKey),
                Target = Compare.Types.CompareTarget.Create,
                Result = Compare.Types.CompareResult.Equal,
                CreateRevision = 0
            });
        }
        
        // If true, set the new value
        txn.Success.Add(new RequestOp
        {
            RequestPut = new PutRequest
            {
                Key = ByteString.CopyFromUtf8(counterKey),
                Value = ByteString.CopyFromUtf8(newValue.ToString())
            }
        });
        
        // Execute the transaction
        var response = await client.TransactionAsync(txn);
        
        if (response.Succeeded)
        {
            return newValue;
        }
        
        // If the transaction failed, retry
        await Task.Delay(10); // Small delay before retrying
    }
}

// Usage
int newCounterValue = await IncrementCounterAsync("counter");
Console.WriteLine($"Counter incremented to: {newCounterValue}");
```

### Distributed Lock

Implement a simple distributed lock using transactions:

```csharp
// Function to acquire a lock
async Task<bool> AcquireLockAsync(string lockKey, string lockValue, int ttlSeconds)
{
    // Create a lease with the specified TTL
    var leaseResponse = await client.LeaseGrantAsync(ttlSeconds);
    long leaseId = leaseResponse.ID;
    
    try
    {
        // Create a transaction
        var txn = new Transaction();
        
        // Compare if the lock doesn't exist
        txn.Compare.Add(new Compare
        {
            Key = ByteString.CopyFromUtf8(lockKey),
            Target = Compare.Types.CompareTarget.Create,
            Result = Compare.Types.CompareResult.Equal,
            CreateRevision = 0
        });
        
        // If true, create the lock with the lease
        txn.Success.Add(new RequestOp
        {
            RequestPut = new PutRequest
            {
                Key = ByteString.CopyFromUtf8(lockKey),
                Value = ByteString.CopyFromUtf8(lockValue),
                Lease = leaseId
            }
        });
        
        // Execute the transaction
        var response = await client.TransactionAsync(txn);
        
        if (response.Succeeded)
        {
            // Start a background task to keep the lease alive
            _ = KeepLeaseAliveAsync(leaseId, lockKey);
            return true;
        }
        else
        {
            // Failed to acquire the lock
            await client.LeaseRevokeAsync(leaseId);
            return false;
        }
    }
    catch
    {
        // Revoke the lease on error
        await client.LeaseRevokeAsync(leaseId);
        throw;
    }
}

// Function to release a lock
async Task ReleaseLockAsync(string lockKey, string lockValue)
{
    // Create a transaction
    var txn = new Transaction();
    
    // Compare if the lock value matches
    txn.Compare.Add(new Compare
    {
        Key = ByteString.CopyFromUtf8(lockKey),
        Target = Compare.Types.CompareTarget.Value,
        Result = Compare.Types.CompareResult.Equal,
        Value = ByteString.CopyFromUtf8(lockValue)
    });
    
    // If true, delete the lock
    txn.Success.Add(new RequestOp
    {
        RequestDeleteRange = new DeleteRangeRequest
        {
            Key = ByteString.CopyFromUtf8(lockKey)
        }
    });
    
    // Execute the transaction
    await client.TransactionAsync(txn);
}

// Function to keep the lease alive
async Task KeepLeaseAliveAsync(long leaseId, string lockKey)
{
    try
    {
        while (true)
        {
            // Check if the lock still exists
            var getResponse = await client.GetAsync(lockKey);
            if (getResponse.Count == 0)
            {
                break;
            }
            
            // Keep the lease alive
            await client.LeaseKeepAliveAsync(leaseId);
            
            // Wait before the next keep-alive
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
    catch
    {
        // Ignore errors
    }
}

// Usage
string lockKey = "my-lock";
string lockValue = Guid.NewGuid().ToString();

bool acquired = await AcquireLockAsync(lockKey, lockValue, 10);
if (acquired)
{
    try
    {
        Console.WriteLine("Lock acquired, performing work...");
        // Perform work while holding the lock
        await Task.Delay(5000);
    }
    finally
    {
        await ReleaseLockAsync(lockKey, lockValue);
        Console.WriteLine("Lock released");
    }
}
else
{
    Console.WriteLine("Failed to acquire lock");
}
```

## See Also

- [API Reference](api-reference.md) - Complete API reference for transaction operations
- [Key-Value Operations](../key-value/index.md) - Working with key-value operations
- [Lock Operations](../lock/index.md) - Using etcd's distributed locking
