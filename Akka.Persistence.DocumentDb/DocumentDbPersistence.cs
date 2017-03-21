using System;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Persistence.DocumentDb
{
    public class DocumentDbPersistence : IExtension
    {
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<DocumentDbPersistence>
                ("Akka.Persistence.DocumentDb.reference.conf");
        }

        public static DocumentDbPersistence Get(ActorSystem system)
        {
            return system.WithExtension<DocumentDbPersistence, DocumentDbPersistenceProvider>();
        }

        public DocumentDbJournalSettings JournalSettings { get; }
        public DocumentDbSnapshotSettings SnapshotStoreSettings { get; }

        public DocumentDbPersistence(ExtendedActorSystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            system.Settings.InjectTopLevelFallback(DefaultConfiguration());

            var journalConfig = system.Settings.Config.GetConfig("akka.persistence.journal.documentdb");
            JournalSettings = new DocumentDbJournalSettings(journalConfig);

            var snapShotConfig = system.Settings.Config.GetConfig("akka.persistence.snapshot-store.documentdb");
            SnapshotStoreSettings = new DocumentDbSnapshotSettings(snapShotConfig);
        }
    }
}
