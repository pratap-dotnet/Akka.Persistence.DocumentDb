using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.Journal;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Akka.Persistence.DocumentDb.Journal
{
    public class DocumentDbJournal : AsyncWriteJournal
    {
        private readonly DocumentDbJournalSettings settings;
        private Lazy<IDocumentClient> documentClient;
        private Lazy<Database> documentDbDatabase;
        private Lazy<DocumentCollection> journalCollection;
        private Lazy<DocumentCollection> metadataCollection;

        public DocumentDbJournal()
        {
            this.settings = DocumentDbPersistence.Get(Context.System).JournalSettings;
        }

        protected override void PreStart()
        {
            base.PreStart();

            documentClient = new Lazy<IDocumentClient>(() =>
            {
                return new DocumentClient(new Uri(settings.ServiceUri),
                    settings.SecretKey);
            });

            documentDbDatabase = new Lazy<Database>(() =>
            {
                var database = documentClient.Value.CreateDatabaseQuery()
                    .Where(db => db.Id == settings.Database).AsEnumerable().FirstOrDefault();
                if (database == null && settings.AutoInitialize)
                {
                    database = documentClient.Value.CreateDatabaseAsync(new Database
                    {
                        Id = settings.Database
                    }).GetAwaiter().GetResult();
                }
                else if (database == null)
                {
                    throw new ApplicationException("DocumentDb database is not initialized, set auto-initialize to on if you want it to be initialized");
                }
                return database;
            });

            journalCollection = new Lazy<DocumentCollection>(() =>
            {
                var documentDbName = documentDbDatabase.Value.Id;
                var documentCollection = documentClient.Value.CreateDocumentCollectionQuery
                    (UriFactory.CreateDatabaseUri(documentDbName))
                    .Where(a => a.Id == settings.Collection).AsEnumerable().FirstOrDefault();
                if (documentCollection == null && settings.AutoInitialize)
                {
                    documentCollection = documentClient.Value
                        .CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(documentDbName),
                    new DocumentCollection
                    {
                        Id = settings.Collection
                    }, new RequestOptions { OfferThroughput = 400 }).GetAwaiter().GetResult();
                }
                else if (documentCollection == null)
                {
                    throw new ApplicationException("DocumentDb document collection is not initialized, set auto-initialize to on if you want it to be initialized");
                }

                return documentCollection;
            });

            metadataCollection = new Lazy<DocumentCollection>(() =>
            {
                var documentDbName = documentDbDatabase.Value.Id;

                var collection = documentClient.Value.CreateDocumentCollectionQuery
                    (UriFactory.CreateDatabaseUri(documentDbName))
                    .Where(a => a.Id == settings.MetadataCollection).AsEnumerable().FirstOrDefault();
                if (collection == null && settings.AutoInitialize)
                {
                    collection = documentClient.Value.
                        CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(documentDbName),
                    new DocumentCollection
                    {
                        Id = settings.MetadataCollection
                    }, new RequestOptions { OfferThroughput = 400 }).GetAwaiter().GetResult();
                }
                else if (collection == null)
                {
                    throw new ApplicationException("DocumentDb metadata collection is not initialized, set auto-initialize to on if you want it to be initialized");
                }

                return collection;
            });
        }

        public override Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            IQueryable<MetadataEntry> query = GetMetadataEntryQuery()
                .Where(a => a.PersistenceId == persistenceId);

            return Task.FromResult(query.FirstOrDefault().SequenceNr);
        }

        public override Task ReplayMessagesAsync(IActorContext context, string persistenceId, long fromSequenceNr, long toSequenceNr, long max, Action<IPersistentRepresentation> recoveryCallback)
        {
            if (max == 0)
                return Task.FromResult(0);
            IQueryable<JournalEntry> query = GetJournalEntryQuery()
                .Where(a => a.PersistenceId == persistenceId 
                            && a.SequenceNr >= fromSequenceNr 
                            && a.SequenceNr <= toSequenceNr); 

            var documents = query.ToList();

            documents.ForEach(doc =>
            {
                recoveryCallback(new Persistent(doc.Payload, doc.SequenceNr, doc.PersistenceId, doc.Manifest, doc.IsDeleted, Sender));
            });

            return Task.FromResult(0);
        }

        private IQueryable<JournalEntry> GetJournalEntryQuery()
        {
            return documentClient.Value.CreateDocumentQuery<JournalEntry>(
                            UriFactory.CreateDocumentCollectionUri(documentDbDatabase.Value.Id, journalCollection.Value.Id), new FeedOptions { MaxItemCount = -1 });
        }

        private IQueryable<MetadataEntry> GetMetadataEntryQuery()
        {
            return documentClient.Value.CreateDocumentQuery<MetadataEntry>(
                            UriFactory.CreateDocumentCollectionUri(documentDbDatabase.Value.Id, metadataCollection.Value.Id), new FeedOptions { MaxItemCount = -1 });
        }

        protected override async Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            var query = GetJournalEntryQuery()
                .Where(a => a.PersistenceId == persistenceId);

            if (toSequenceNr != long.MaxValue)
                query = query.Where(a => a.SequenceNr <= toSequenceNr);

            var deleteTasks = query.ToList().Select(async a =>
            {
                await documentClient.Value.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(documentDbDatabase.Value.Id, journalCollection.Value.Id, a.Id));
            });

            await Task.WhenAll(deleteTasks);
        }

        protected override async Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            var messageList = messages.ToList();
            var documentDbWriteTasks = messageList.Select(async (message) =>
            {
                var persistentMessages = ((IImmutableList<IPersistentRepresentation>)message.Payload).ToArray();
                var journalEntries = persistentMessages.Select(a => new JournalEntry(a)).ToList();

                var induvidualWriteTasks = journalEntries.Select(async a => await documentClient.Value.CreateDocumentAsync(journalCollection.Value.SelfLink, a));
                await Task.WhenAll(induvidualWriteTasks);
            });

            await SetHighestSequenceId(messageList);

            return await Task<ImmutableList<Exception>>
                .Factory
                .ContinueWhenAll(documentDbWriteTasks.ToArray(),
                    tasks => tasks.Select(t => t.IsFaulted ? TryUnwrapException(t.Exception) : null)
                    .ToImmutableList());
        }

        private async Task SetHighestSequenceId(List<AtomicWrite> messages)
        {
            var persistenceId = messages.Select(c => c.PersistenceId).First();
            var highSequenceId = messages.Max(c => c.HighestSequenceNr);
            IQueryable<MetadataEntry> query = GetMetadataEntryQuery()
                .Where(a => a.PersistenceId == persistenceId);

            var metadataEntry = new MetadataEntry
            {
                Id = persistenceId,
                PersistenceId = persistenceId,
                SequenceNr = highSequenceId
            };

            await documentClient.Value.UpsertDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(documentDbDatabase.Value.Id, metadataCollection.Value.Id), metadataEntry);
        }
    }
}
