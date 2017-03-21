namespace Akka.Persistence.DocumentDb.Snapshot
{
    public class SnapshotEntry
    {
        public SnapshotEntry(SnapshotMetadata metadata, object snapshot)
        {
            Snapshot = snapshot;
            Id = $"{metadata.PersistenceId}-{metadata.SequenceNr}";
            SequenceNr = metadata.SequenceNr;
            Timestamp = metadata.Timestamp.Ticks;
            PersistenceId = metadata.PersistenceId;
        }

        public SnapshotEntry()
        {

        }

        public string Id { get; set; }
        public string PersistenceId { get; set; }
        public long SequenceNr { get; set; }
        public long Timestamp { get; set; }
        public object Snapshot { get; set; }
    }
}
