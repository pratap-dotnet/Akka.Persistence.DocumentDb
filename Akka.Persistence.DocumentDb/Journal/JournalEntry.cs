namespace Akka.Persistence.DocumentDb.Journal
{
    public class JournalEntry
    {
        internal JournalEntry(IPersistentRepresentation message)
        {
            Id = message.PersistenceId + "_" + message.SequenceNr;
            IsDeleted = message.IsDeleted;
            Payload = message.Payload;
            PersistenceId = message.PersistenceId;
            SequenceNr = message.SequenceNr;
            Manifest = message.Manifest;
        }

        public JournalEntry()
        {

        }

        public string Id { get; set; }
        public string PersistenceId { get; set; }
        public long SequenceNr { get; set; }
        public bool IsDeleted { get; set; }
        public object Payload { get; set; }
        public string Manifest { get; set; }
    }
}
