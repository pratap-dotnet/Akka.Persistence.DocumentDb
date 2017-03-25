# Akka.Persistence.DocumentDb

Akka persistent journal and snapshot store backed by Microsoft Azure DocumentDb.

**Note: There is an official [Akka.Persistence.MongoDb](https://github.com/akkadotnet/Akka.Persistence.MongoDB) which might work with DocumentDb as well, since DocumentDb supports Mongo protocol.** 

#### Setup

To activate the journal plugin, add the following lines to actor system configuration file

``` 
akka.persistence.journal.plugin = "akka.persistence.journal.documentdb"
akka.persistence.journal.documentdb.service-uri= "<documentdb service uri>"
akka.persistence.journal.documentdb.secret-key= "<documentdb secret key>"
akka.persistence.journal.documentdb.database = "<documentdb database>"
akka.persistence.journal.documentdb.collection = "<journal collection>"
```

For snapshot store configuration, add the following lines
``` 
akka.persistence.snapshot-store.plugin = "akka.persistence.snapshot-store.documentdb"
akka.persistence.snapshot-store.documentdb.service-uri= "<documentdb service uri>"
akka.persistence.snapshot-store.documentdb.secret-key= "<documentdb secret key>"
akka.persistence.snapshot-store.documentdb.database = "<documentdb database>"
akka.persistence.snapshot-store.documentdb.collection = "<snapshot collection>"
```


The connection details needs to be given separately to both the configuration

#### Tests

Tests are run against the documentdb emulator, you could download it from [here](https://docs.microsoft.com/en-us/azure/documentdb/documentdb-nosql-local-emulator)