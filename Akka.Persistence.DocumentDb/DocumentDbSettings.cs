using System;
using Akka.Configuration;

namespace Akka.Persistence.DocumentDb
{
    /// <summary>
    /// Document Db Settings abstract class
    /// </summary>
    public abstract class DocumentDbSettings
    {
        public string ServiceUri { get; private set; }
        public string SecretKey { get; private set; }
        public bool AutoInitialize { get; private set; }
        public string Database { get; private set; }
        public string Collection { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbSettings"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public DocumentDbSettings(Config config)
        {
            ServiceUri = config.GetString("service-uri");
            SecretKey = config.GetString("secret-key");
            Database = config.GetString("database");
            Collection = config.GetString("collection");
            AutoInitialize = config.GetBoolean("auto-initialize");
        }
    }

    /// <summary>
    /// Journal Settings
    /// </summary>
    /// <seealso cref="Akka.Persistence.DocumentDb.DocumentDbSettings" />
    public class DocumentDbJournalSettings : DocumentDbSettings
    {
        public string MetadataCollection { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbJournalSettings"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="ArgumentNullException">config - DocumentDb settings cannot be initialized, because required HOCON section couldn't been found</exception>
        public DocumentDbJournalSettings(Config config)
            : base(config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config),
                    "DocumentDb settings cannot be initialized, because required HOCON section couldn't been found");
            MetadataCollection = config.GetString("metadata-collection");
        }
    }

    /// <summary>
    /// Snapshot store settings
    /// </summary>
    /// <seealso cref="Akka.Persistence.DocumentDb.DocumentDbSettings" />
    public class DocumentDbSnapshotSettings : DocumentDbSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbSnapshotSettings"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="ArgumentNullException">config - DocumentDb settings cannot be initialized, because required HOCON section couldn't been found</exception>
        public DocumentDbSnapshotSettings(Config config)
            : base(config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config),
                    "DocumentDb settings cannot be initialized, because required HOCON section couldn't been found");
        }
    }
}
