# Transaction API Reference

This page provides a complete reference of all transaction-related methods available in the `dotnet-etcd` client.

## Transaction Methods

### Transaction

Executes a transaction in etcd.

#### Transaction Overloads

```csharp
// Execute a transaction
public TxnResponse Transaction(
    Transaction txn,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Transaction Parameters

- `txn`: The transaction to execute.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Transaction Returns

- `TxnResponse`: The etcd response for the transaction operation.

### TransactionAsync

Executes a transaction in etcd asynchronously.

#### TransactionAsync Overloads

```csharp
// Execute a transaction asynchronously
public async Task<TxnResponse> TransactionAsync(
    Transaction txn,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### TransactionAsync Parameters

- `txn`: The transaction to execute.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### TransactionAsync Returns

- `Task<TxnResponse>`: The etcd response for the transaction operation.

## Transaction Class

The `Transaction` class represents a transaction in etcd.

### Transaction Properties

- `Compare`: The list of comparisons to check.
- `Success`: The list of operations to execute if all comparisons are true.
- `Failure`: The list of operations to execute if any comparison is false.

### Methods

- `Add(Compare compare)`: Adds a comparison to the transaction.
- `Add(RequestOp operation, bool isSuccess)`: Adds an operation to either the success or failure list.

## Transaction Components

### Compare

The `Compare` class represents a comparison operation in a transaction.

#### Compare Properties

- `Key`: The key to compare.
- `Target`: The target of the comparison (value, version, create revision, or mod revision).
- `Result`: The result of the comparison (equal, not equal, greater, or less).
- `Value`: The value to compare against.
- `RangeEnd`: The end of the range for range comparisons.

### TxnOp

The `TxnOp` class represents an operation in a transaction.

#### TxnOp Properties

- `RequestPut`: The put request to execute.
- `RequestRange`: The range request to execute.
- `RequestDeleteRange`: The delete range request to execute.

### TxnRequest

The `TxnRequest` class represents a transaction request.

#### TxnRequest Properties

- `Compare`: The list of comparisons to perform.
- `Success`: The list of operations to perform if the comparisons succeed.
- `Failure`: The list of operations to perform if the comparisons fail.

## Transaction Response Types

### TxnResponse

The `TxnResponse` class represents the response from a transaction operation.

#### TxnResponse Properties

- `Header`: The response header.
- `Succeeded`: Whether the transaction succeeded.
- `Responses`: The list of responses from the operations.

### ResponseOp

The `ResponseOp` class represents a response from an operation in a transaction.

#### ResponseOp Properties

- `ResponseRange`: The response from a range operation.
- `ResponsePut`: The response from a put operation.
- `ResponseDeleteRange`: The response from a delete range operation.
- `ResponseTxn`: The response from a nested transaction operation.

## Enums

### CompareTarget

The `Compare.Types.CompareTarget` enum represents the target of a comparison.

#### CompareTarget Values

- `Version`: Compare the version of the key.
- `Create`: Compare the creation revision of the key.
- `Mod`: Compare the modification revision of the key.
- `Value`: Compare the value of the key.
- `Lease`: Compare the lease ID of the key.

### CompareResult

The `Compare.Types.CompareResult` enum represents the result of a comparison.

#### CompareResult Values

- `Equal`: Check if the target is equal to the provided value.
- `Greater`: Check if the target is greater than the provided value.
- `Less`: Check if the target is less than the provided value.
- `NotEqual`: Check if the target is not equal to the provided value.

## Examples

### Basic Transaction

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

### Compare-and-Swap

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

### Transaction with Multiple Operations

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

### Asynchronous Transaction

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

## See Also

- [Transaction Operations](index.md) - Overview and examples of transaction operations
- [Key-Value API Reference](../key-value/api-reference.md) - API reference for key-value operations
- [Lock API Reference](../lock/api-reference.md) - API reference for lock operations
