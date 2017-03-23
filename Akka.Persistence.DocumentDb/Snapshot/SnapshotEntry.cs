using Newtonsoft.Json;

namespace Akka.Persistence.DocumentDb.Snapshot
{
    public class SnapshotEntry
    {
        public SnapshotEntry(SnapshotMetadata metadata, object snapshot)
        {
            Snapshot = snapshot;
            Id = $"{metadata.PersistenceId}_{metadata.SequenceNr}";
            SequenceNr = metadata.SequenceNr;
            Timestamp = new DateTimeJsonObject(metadata.Timestamp);
            PersistenceId = metadata.PersistenceId;
        }

        public SnapshotEntry()
        {

        }
        [JsonProperty("id")]
        public string Id { get; set; }
        public string PersistenceId { get; set; }
        public long SequenceNr { get; set; }
        public DateTimeJsonObject Timestamp { get; set; }
        public object Snapshot { get; set; }
    }
}
