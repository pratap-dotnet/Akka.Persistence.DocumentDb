using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Persistence.Snapshot;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Akka.Persistence.DocumentDb.Snapshot
{
    public class DocumentDbSnapshotStore : SnapshotStore
    {
        private readonly DocumentDbSnapshotSettings settings;
        private Lazy<IDocumentClient> documentClient;
        private Lazy<Database> documentDbDatabase;
        private Lazy<DocumentCollection> snapShotCollection;

        public DocumentDbSnapshotStore()
        {
            settings = DocumentDbPersistence.Get(Context.System).SnapshotStoreSettings;
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

            snapShotCollection = new Lazy<DocumentCollection>(() =>
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

        }

        private IQueryable<SnapshotEntry> GetSnapshotQuery(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            IQueryable<SnapshotEntry> query = documentClient.Value.CreateDocumentQuery<SnapshotEntry>(
                UriFactory.CreateDocumentCollectionUri(documentDbDatabase.Value.Id, snapShotCollection.Value.Id));

            query = query.Where(a => a.PersistenceId == persistenceId);

            if (criteria.MaxSequenceNr > 0 && criteria.MaxSequenceNr < long.MaxValue)
                query = query.Where(a => a.SequenceNr <= criteria.MaxSequenceNr);

            if (criteria.MaxTimeStamp != DateTime.MinValue && criteria.MaxTimeStamp != DateTime.MaxValue)
                query = query.Where(a => a.Timestamp <= criteria.MaxTimeStamp.Ticks);

            return query;
        }

        protected override async Task DeleteAsync(SnapshotMetadata metadata)
        {
            IQueryable<SnapshotEntry> query = documentClient.Value.CreateDocumentQuery<SnapshotEntry>(
                UriFactory.CreateDocumentCollectionUri(documentDbDatabase.Value.Id, snapShotCollection.Value.Id));

            query = query.Where(a => a.PersistenceId == metadata.PersistenceId);

            if (metadata.SequenceNr > 0 && metadata.SequenceNr < long.MaxValue)
                query = query.Where(a => a.SequenceNr == metadata.SequenceNr);

            if (metadata.Timestamp != DateTime.MinValue && metadata.Timestamp != DateTime.MaxValue)
                query = query.Where(a => a.Timestamp == metadata.Timestamp.Ticks);

            var document = query.FirstOrDefault();

            if (document != null)
                await documentClient.Value.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(documentDbDatabase.Value.Id, snapShotCollection.Value.Id, document.Id));
        }

        protected override async Task DeleteAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            var query = GetSnapshotQuery(persistenceId, criteria);

            var deleteTasks = query.ToList().Select(async a =>
            {
                await documentClient.Value.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(documentDbDatabase.Value.Id, snapShotCollection.Value.Id, a.Id));
            });

            await Task.WhenAll(deleteTasks.ToArray());
        }

        protected override Task<SelectedSnapshot> LoadAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            var query = GetSnapshotQuery(persistenceId, criteria);

            return Task.FromResult(query
                    .OrderByDescending(a => a.SequenceNr)
                    .Select(a => new SelectedSnapshot(new SnapshotMetadata(a.PersistenceId, a.SequenceNr, new DateTime(a.Timestamp)), a.Snapshot))
                    .FirstOrDefault()
                );
        }

        protected override Task SaveAsync(SnapshotMetadata metadata, object snapshot)
        {
            var snapshotEntry = new SnapshotEntry(metadata, snapshot);

            documentClient.Value.UpsertDocumentAsync(
                UriFactory.CreateDocumentUri(documentDbDatabase.Value.Id, snapShotCollection.Value.Id, snapshotEntry.Id), snapshotEntry);

            return Task.FromResult(true);
        }
    }
}
