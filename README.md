# ParserApi

A .NET 8 Web API that exposes `POST /api/v1/parse-content`.

It accepts Base64-encoded `CSV` or `INTERNAL_JSON` content and responds with status, number of processed rows or objects, and the parsed data in a unified structure.

```json
{
  "status": "Success",
  "processedCount": 2,
  "data": [<decoded objects>]
}
```

## How to run locally
```ps1
dotnet build
dotnet run --project ParserApi
```
> ❗ Make sure that port 5000 is available

## How to test

```ps1
dotnet test
```

### Manual Testing Example

```ps1
$body = @{
  type = "CSV"
  content = [Convert]::ToBase64String(
    [Text.Encoding]::UTF8.GetBytes(
      "name,age`nAlice,30`nBob,25"
    )
  )
} | ConvertTo-Json

Invoke-RestMethod `
  -Uri "http://localhost:5000/api/v1/parse-content" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```
### output
```
status  processedCount data                                        
------  -------------- ----                                        
Success              2 {@{name=Alice; age=30}, @{name=Bob; age=25}}
```
