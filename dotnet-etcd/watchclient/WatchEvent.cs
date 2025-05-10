using Mvccpb;

namespace dotnet_etcd;

/// <summary>
///     WatchEvent class is used for retrieval of minimal
///     data from watch events on etcd.
/// </summary>
public class WatchEvent
{
    /// <summary>
    ///     etcd Key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     etcd value
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     etcd watch event type (PUT,DELETE etc.)
    /// </summary>
    public Event.Types.EventType Type { get; set; }
}
