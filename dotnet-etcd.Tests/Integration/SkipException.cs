namespace dotnet_etcd.Tests.Integration;

public class SkipException : Exception
{
    public SkipException(string message) : base(message)
    {
    }
}