using Grpc.Core;

namespace dotnet_etcd.Tests.Unit.Mocks;

/// <summary>
///     Implementation of IAsyncStreamReader for testing
/// </summary>
public class TestAsyncStreamReader<T> : IAsyncStreamReader<T>
{
    private readonly IEnumerator<T> _enumerator;
    private readonly IEnumerable<T> _responses;

    public TestAsyncStreamReader(IEnumerable<T> responses)
    {
        _responses = responses ?? throw new ArgumentNullException(nameof(responses));
        _enumerator = _responses.GetEnumerator();
    }

    public T Current => _enumerator.Current;

    public Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        return Task.FromResult(_enumerator.MoveNext());
    }
}