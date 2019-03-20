## Installation

### Developer Installation (see below)

- [Install Azure Cosmos DB emulator](#emulator)
- [Restore NuGet packages](#nuget)
- [Unpack Zip file with CSV included](#unpack)


## Developer Installation

### Emulator

We recommend a Cosmos DB emulator preconfigured on your developer machine.
Check this article [Microsoft Docs for local Cosmos DB emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator).

### NuGet

These packages are in use for the stresstest.

```bash
dotnet add package CsvHelper
```

```bash
dotnet add package Microsoft.Azure.DocumentDB.Core
```

### Unpack

Unpack the ZIP file "1500000 Sales Records.zip" and copy the CSV-file to the same location like the console-application