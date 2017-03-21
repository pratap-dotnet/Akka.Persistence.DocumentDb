namespace Akka.Persistence.DocumentDb.Journal
{
    public class MetadataEntry
    {
        public string Id { get; set; }
        public string PersistenceId { get; set; }
        public long SequenceNr { get; set; }
    }
}
