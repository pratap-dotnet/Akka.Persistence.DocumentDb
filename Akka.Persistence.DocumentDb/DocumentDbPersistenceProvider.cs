using Akka.Actor;

namespace Akka.Persistence.DocumentDb
{
    /// <summary>
    /// Persistence Provider
    /// </summary>
    /// <seealso cref="Akka.Actor.ExtensionIdProvider{Akka.Persistence.DocumentDb.DocumentDbPersistence}" />
    public class DocumentDbPersistenceProvider : ExtensionIdProvider<DocumentDbPersistence>
    {
        /// <summary>
        /// Creates the current extension using a given actor system.
        /// </summary>
        /// <param name="system">The actor system to use when creating the extension.</param>
        /// <returns>
        /// The extension created using the given actor system.
        /// </returns>
        public override DocumentDbPersistence CreateExtension(ExtendedActorSystem system)
        {
            return new DocumentDbPersistence(system);
        }
    }
}