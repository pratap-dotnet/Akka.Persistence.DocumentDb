using System;
using Akka.Actor;

namespace Akka.Persistence.DocumentDb
{
    public class DocumentDbPersistenceProvider : ExtensionIdProvider<DocumentDbPersistence>
    {
        public override DocumentDbPersistence CreateExtension(ExtendedActorSystem system)
        {
            return new DocumentDbPersistence(system);
        }
    }
}