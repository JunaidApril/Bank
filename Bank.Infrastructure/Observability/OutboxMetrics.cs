
using System.Diagnostics.Metrics;

namespace Bank.Infrastructure.Observability
{
    public sealed class OutboxMetrics : IDisposable
    {
        private readonly Meter _meter =
            new("Bank.Infrastructure.Outbox");

        private readonly Counter<long> _published;
        private readonly Counter<long> _failed;
        private readonly Counter<long> _deadLettered;

        private readonly Histogram<double> _processingDuration;

        private long _pendingCount;

        public OutboxMetrics()
        {
            _published = _meter.CreateCounter<long>(
                "outbox.messages.published");

            _failed = _meter.CreateCounter<long>(
                "outbox.messages.failed");

            _deadLettered = _meter.CreateCounter<long>(
                "outbox.messages.dead_lettered");

            _processingDuration = _meter.CreateHistogram<double>(
                "outbox.processing.duration",
                unit: "ms");

            _meter.CreateObservableGauge<long>(
                "outbox.messages.pending",
                () => Interlocked.Read(ref _pendingCount));
        }

        public void RecordPublished() =>
            _published.Add(1);

        public void RecordFailure() =>
            _failed.Add(1);

        public void RecordDeadLettered() =>
            _deadLettered.Add(1);

        public void SetPending(long count) =>
            Interlocked.Exchange(
                ref _pendingCount,
                count);

        public void RecordProcessingDuration(
            double milliseconds) =>
            _processingDuration.Record(milliseconds);

        public void Dispose()
        {
            _meter.Dispose();
        }
    }
}
