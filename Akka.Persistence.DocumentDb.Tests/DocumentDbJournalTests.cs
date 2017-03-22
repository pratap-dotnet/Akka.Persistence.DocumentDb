using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Configuration;
using Akka.Persistence.TestKit.Journal;
using Microsoft.Azure.Documents.Client;
using Xunit;

namespace Akka.Persistence.DocumentDb.Tests
{
    [Collection("DocumentDbSpec")]
    public class DocumentDbJournalTests : JournalSpec
    {
        private static readonly string SpecConfig = @"
            akka.test.single-expect-default = 3000s
            akka.persistence {
                publish-plugin-commands = on
                journal {
                    plugin= ""akka.persistence.journal.documentdb"" 
                    documentdb {
                        class = ""Akka.Persistence.DocumentDb.Journal.DocumentDbJournal, Akka.Persistence.DocumentDb""
                        service-uri = ""https://localhost:8081""
                        secret-key = ""C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==""
                        auto-initialize = on
                        database = ""testactors""
                    }
                }
            }";

        protected override bool SupportsRejectingNonSerializableObjects { get; } = false;

        public DocumentDbJournalTests() : base(SpecConfig,"DocumentDbJournalSpec")
        {
            Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            var documentClient = new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");

            documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("testactors", "EventJournal")).Wait();
            documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("testactors", "Metadata")).Wait();

            base.Dispose(disposing);
        }
    }
}
