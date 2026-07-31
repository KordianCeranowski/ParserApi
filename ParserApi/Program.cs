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


    // Decode content from base64 to string
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

    
    if (contentType == ContentType.CSV)
    {
        using var reader = new StringReader(decoded);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var rows = csv.GetRecords<dynamic>().ToList();

        return Results.Ok(new { status = "Success", processedCount = rows.Count, data = rows });
    }

    try
    {
        var json = JsonSerializer.Deserialize<JsonElement>(decoded);
        var count = json.ValueKind == JsonValueKind.Array ? json.GetArrayLength() : 1;
        return Results.Ok(new { status = "Success", processedCount = count, data = json });
    }
    catch
    {
        return Results.BadRequest(new { status = "Error", message = "Invalid INTERNAL_JSON payload." });
    }
});

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