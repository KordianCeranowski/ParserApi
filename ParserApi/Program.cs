using System.Text;
using System.Text.Json;
using CsvHelper;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/v1/parse-content", (HttpContext context, ParseRequest request) =>
{
    if (request == null)
        return Results.BadRequest(new { status = "Error", message = "Request body is required." });

    if (!context.Request.HasJsonContentType())
        return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);

    bool contentTypeIsValid = Enum.TryParse(
        request.Type, 
        ignoreCase: true, 
        out ContentType contentType
        );

    if (!contentTypeIsValid)
        return Results.BadRequest(new { status = "Error", message = "Unsupported type." });


    string decoded;
    try
    {
        var bytes = Convert.FromBase64String(request.Content);
        decoded = Encoding.UTF8.GetString(bytes);
    }
    catch
    {
        return Results.BadRequest(new { status = "Error", message = "Content is not valid Base64." });
    }

    switch (contentType)
    {
        case ContentType.CSV:
            return ParseCsv(decoded);

        case ContentType.INTERNAL_JSON:
            return ParseJson(decoded);

        default:
            return Results.BadRequest(new { status = "Error", message = "Unsupported type." });
    }
});

IResult ParseCsv(string content)
{
    try
    {
        using var reader = new StringReader(content);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        dynamic[] csvContents = csvReader.GetRecords<dynamic>().ToArray();
        return Results.Ok(new { status = "Success", processedCount = csvContents.Length, data = csvContents });
    }
    catch (CsvHelperException)
    {
        return Results.BadRequest(new { status = "Error", message = "Invalid CSV content" });
    }
}

IResult ParseJson(string content)
{
    try
    {
        JsonElement data = JsonSerializer.Deserialize<JsonElement>(content);
        // Standardize data into an Array
        JsonElement[] jsonContents;
        if (data.ValueKind == JsonValueKind.Array)
            jsonContents = [.. data.EnumerateArray()];
        else
            jsonContents = [data];
        return Results.Ok(new { status = "Success", processedCount = jsonContents.Length, data = jsonContents });

    }
    catch (JsonException)
    {
        return Results.BadRequest(new { status = "Error", message = "Invalid INTERNAL_JSON content" });
    }
}

app.Run();

enum ContentType
{
    CSV,
    INTERNAL_JSON
}

class ParseRequest
{
    public required string Type { get; set; }
    public required string Content { get; set; }
}