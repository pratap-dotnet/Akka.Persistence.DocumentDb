using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Persistence.TestKit.Snapshot;
using Microsoft.Azure.Documents.Client;
using Xunit;

namespace Akka.Persistence.DocumentDb.Tests
{
    [Collection("DocumentDbSpec")]
    public class DocumentDbSnapshotStoreTests : SnapshotStoreSpec
    {
        private static readonly string SpecConfig = @"
            akka.test.single-expect-default = 3000s
            akka.persistence {
                publish-plugin-commands = on
                snapshot-store {
                    plugin = ""akka.persistence.snapshot-store.documentdb""
                    documentdb {
                        class = ""Akka.Persistence.DocumentDb.Snapshot.DocumentDbSnapshotStore, Akka.Persistence.DocumentDb""
                        service-uri = ""<serviceUri>""
                        secret-key = ""<secretKey>""
                        auto-initialize = on
                        database = ""testactors""
                    }
                }
            }";

        public static string GetConfigSpec()
        {
            return SpecConfig.Replace("<serviceUri>", ConfigurationManager.AppSettings["serviceUri"])
                .Replace("<secretKey>", ConfigurationManager.AppSettings["secretKey"]);
        }

        public DocumentDbSnapshotStoreTests() : base(GetConfigSpec(), "DocumentDbSnapshotStoreSpec")
        {
            Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            var documentClient = new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

            documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("testactors", "SnapshotStore")).Wait();

            base.Dispose(disposing);
        }
    }
}
