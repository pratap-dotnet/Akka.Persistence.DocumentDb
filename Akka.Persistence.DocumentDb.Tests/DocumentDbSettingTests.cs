using FluentAssertions;
using Xunit;

namespace Akka.Persistence.DocumentDb.Tests
{
    [Collection("DocumentDbSpec")]
    public class DocumentDbSettingTests : Akka.TestKit.Xunit2.TestKit
    {
        [Fact]
        public void DocumentDb_JournalSettings_must_have_default_values()
        {
            var documentDbPersistence = DocumentDbPersistence.Get(Sys);

            documentDbPersistence.JournalSettings.ServiceUri.Should().Be(string.Empty);
            documentDbPersistence.JournalSettings.SecretKey.Should().Be(string.Empty);
            documentDbPersistence.JournalSettings.AutoInitialize.Should().BeFalse();
            documentDbPersistence.JournalSettings.Database.Should().Be("Actors");
            documentDbPersistence.JournalSettings.Collection.Should().Be("EventJournal");
            documentDbPersistence.JournalSettings.MetadataCollection.Should().Be("Metadata");
        }

        [Fact]
        public void DocumentDb_SnapshotStoreSettingsSettings_must_have_default_values()
        {
            var documentDbPersistence = DocumentDbPersistence.Get(Sys);

            documentDbPersistence.SnapshotStoreSettings.ServiceUri.Should().Be(string.Empty);
            documentDbPersistence.SnapshotStoreSettings.SecretKey.Should().Be(string.Empty);
            documentDbPersistence.SnapshotStoreSettings.AutoInitialize.Should().BeFalse();
            documentDbPersistence.SnapshotStoreSettings.Database.Should().Be("Actors");
            documentDbPersistence.SnapshotStoreSettings.Collection.Should().Be("SnapshotStore");
        }
    }
}
