using System;
using Akka.Configuration;

namespace Akka.Persistence.DocumentDb
{
    public abstract class DocumentDbSettings
    {
        public string ServiceUri { get; private set; }
        public string SecretKey { get; private set; }
        public bool AutoInitialize { get; private set; }
        public string Database { get; private set; }
        public string Collection { get; private set; }

        public DocumentDbSettings(Config config)
        {
            ServiceUri = config.GetString("service-uri");
            SecretKey = config.GetString("secret-key");
            Database = config.GetString("database");
            Collection = config.GetString("collection");
            AutoInitialize = config.GetBoolean("auto-initialize");
        }
    }

    public class DocumentDbJournalSettings : DocumentDbSettings
    {
        public string MetadataCollection { get; private set; }

        public DocumentDbJournalSettings(Config config)
            : base(config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config),
                    "DocumentDb settings cannot be initialized, because required HOCON section couldn't been found");
            MetadataCollection = config.GetString("metadata-collection");
        }
    }

    public class DocumentDbSnapshotSettings : DocumentDbSettings
    {
        public DocumentDbSnapshotSettings(Config config)
            : base(config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config),
                    "DocumentDb settings cannot be initialized, because required HOCON section couldn't been found");
        }
    }
}
