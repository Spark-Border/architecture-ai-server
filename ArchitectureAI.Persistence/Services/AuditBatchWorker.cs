using System.Threading.Channels;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Domain.Audit;
using ArchitectureAI.Persistence.Context;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArchitectureAI.Persistence.Services;

public class AuditBatchWorker : BackgroundService, IAuditService
{
    private readonly Channel<AuditTrail> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditBatchWorker> _logger;
    private const int BatchSize = 50;
    private const int MaxQueueSize = 1000; // Cap memory usage
    private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(2); // Flush faster to minimize risk

    public AuditBatchWorker(IServiceProvider serviceProvider, ILogger<AuditBatchWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Bounded Channel:
        // - SingleReader (Worker)
        // - AllowSynchronousContinuations = false for safety
        // - FullMode = Wait (Default) -> This applies Backpressure if full
        var options = new BoundedChannelOptions(MaxQueueSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        };
        _channel = Channel.CreateBounded<AuditTrail>(options);
    }

    public ValueTask EnqueueAuditLogAsync(AuditTrail auditLog)
    {
        // returns ValueTask.
        // If queue < 1000, completes instantly.
        // If queue == 1000, waits until space is available (Backpressure).
        return _channel.Writer.WriteAsync(auditLog);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Async Audit Worker Started.");

        var batch = new List<AuditTrail>(BatchSize);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessBatchLoop(batch, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit Worker Failed unexpectedly.");
        }
        finally
        {
            // On Shutdown: Drain remaining items
            await DrainQueueAsync(batch);
        }
    }

    // Explicit StopAsync to ensure we don't just kill the thread immediately without trying to drain
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit Worker Stopping... Draining queue.");
        _channel.Writer.Complete(); // Stop accepting new logs
        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessBatchLoop(List<AuditTrail> batch, CancellationToken token)
    {
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
        {
            cts.CancelAfter(_flushInterval);
            try
            {
                while (batch.Count < BatchSize)
                {
                    // Wait for item or timeout
                    if (await _channel.Reader.WaitToReadAsync(cts.Token))
                    {
                        while (batch.Count < BatchSize && _channel.Reader.TryRead(out var item))
                        {
                            batch.Add(item);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Flush interval reached
            }
        }

        if (batch.Count > 0)
        {
            await FlushBatchAsync(batch);
            batch.Clear();
        }
    }

    private async Task DrainQueueAsync(List<AuditTrail> batch)
    {
        _logger.LogInformation("Draining Audit Queue to Firestore...");

        // Read everything left in channel
        while (_channel.Reader.TryRead(out var item))
        {
            batch.Add(item);
            if (batch.Count >= BatchSize)
            {
                await FlushBatchAsync(batch);
                batch.Clear();
            }
        }

        // Final flush
        if (batch.Count > 0)
        {
            await FlushBatchAsync(batch);
        }
        _logger.LogInformation("Audit Queue Drained.");
    }

    private async Task FlushBatchAsync(List<AuditTrail> batch)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<FirestoreDbContext>();
                var collection = context.Collection(nameof(AuditTrail));
                var firestoreBatch = context.Db.StartBatch();

                foreach (var item in batch)
                {
                    var docRef = collection.Document(item.Id);
                    firestoreBatch.Set(docRef, item);
                }

                await firestoreBatch.CommitAsync();
                _logger.LogDebug($"Flushed {batch.Count} audit logs.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush audit batch to Firestore.");
        }
    }
}
