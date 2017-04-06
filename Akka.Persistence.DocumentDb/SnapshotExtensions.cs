using Newtonsoft.Json.Linq;

namespace Akka.Persistence.DocumentDb
{
    public static class SnapshotExtensions
    {
        public static T ToObject<T>(this SnapshotOffer snapshot)
        {
            if (snapshot.Snapshot is JObject)
            {
                return (snapshot.Snapshot as JObject).ToObject<T>();
            }
            else return default(T);
        }
    }
}
