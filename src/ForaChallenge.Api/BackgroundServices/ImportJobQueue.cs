
using ForaChallenge.Core.Services;
using System.Threading.Channels;

namespace ForaChallenge.Api.BackgroundServices;

/// <summary>
/// In-memory background queue for import jobs.
/// For production at scale, replace with a durable queue (Hangfire, Azure Queue, etc).
/// </summary>
public sealed class ImportJobQueue : IImportJobQueue
{
    private readonly Channel<ImportWorkItem> _channel;

    public ImportJobQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<ImportWorkItem>(options);
    }

    public ValueTask EnqueueAsync(ImportWorkItem item, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(item, cancellationToken);

    public ValueTask<ImportWorkItem> DequeueAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAsync(cancellationToken);
}

