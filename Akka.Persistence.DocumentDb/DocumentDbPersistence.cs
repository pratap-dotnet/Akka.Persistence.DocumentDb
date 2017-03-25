using System;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Persistence.DocumentDb
{
    /// <summary>
    /// Persistence Extensions
    /// </summary>
    /// <seealso cref="Akka.Actor.IExtension" />
    public class DocumentDbPersistence : IExtension
    {
        /// <summary>
        /// Gets the default configuration
        /// </summary>
        /// <returns></returns>
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<DocumentDbPersistence>
                ("Akka.Persistence.DocumentDb.reference.conf");
        }

        /// <summary>
        /// Gets persistence provided for specified actor system.
        /// </summary>
        /// <param name="system">The system.</param>
        /// <returns></returns>
        public static DocumentDbPersistence Get(ActorSystem system)
        {
            return system.WithExtension<DocumentDbPersistence, DocumentDbPersistenceProvider>();
        }

        public DocumentDbJournalSettings JournalSettings { get; }
        public DocumentDbSnapshotSettings SnapshotStoreSettings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbPersistence"/> class.
        /// </summary>
        /// <param name="system">The system.</param>
        /// <exception cref="ArgumentNullException">system</exception>
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
