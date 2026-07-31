# ParserApi

A .NET 8 Web API that exposes `POST /api/v1/parse-content`.

It accepts Base64-encoded `CSV` or `INTERNAL_JSON` content and returns

```json
{
  "status": "Success",
  "processedCount": 2,
  "data": [<decoded objects>]
}
```

## How to run locally

```ps1
dotnet restore
dotnet build
dotnet run --project ParserApi\ParserApi.csproj
```

## How to test

```ps1
dotnet test
```
