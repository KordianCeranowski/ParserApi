using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Mvc.Testing;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly HttpClient client;

	public ApiTests(WebApplicationFactory<Program> factory)
	{
		client = factory.CreateClient();
	}

	[Fact]
	public async Task SingleJsonObject_IsReturnedAsOneItemArray()
	{
        var type = "INTERNAL_JSON";
        var data = """
        {
            "name": "Alice",
            "age": "30"
        }
        """;

        var expectedResponse = """
        {
            "status": "Success",
            "processedCount": 1,
            "data": [
                {
                "name": "Alice",
                "age": "30"
                }
            ]
        }
        """;

		var response = await SendRequest(type, data);
		response.EnsureSuccessStatusCode();
        var actualResponse = await response.Content.ReadAsStringAsync();

        var actual = JsonNode.Parse(actualResponse);
        var expected = JsonNode.Parse(expectedResponse);

        Assert.True(JsonNode.DeepEquals(expected, actual));
    }

    [Fact]
	public async Task MultipleJsonObjects_AreReturnedAsItemArray()
	{
        var type = "INTERNAL_JSON";
        var data = """
        [
            {
                "name": "Alice",
                "age": "30"
            },
            {
                "name": "Bob",
                "age": "40"
            },
            {
                "name": "Tom",
                "age": "50"
            }
        ]
        """;

        var expectedResponse = """
        {
            "status": "Success",
            "processedCount": 3,
            "data": [
                {
                    "name": "Alice",
                    "age": "30"
                },
                {
                    "name": "Bob",
                    "age": "40"
                },
                {
                    "name": "Tom",
                    "age": "50"
                }
            ]
        }
        """;

		var response = await SendRequest(type, data);
		response.EnsureSuccessStatusCode();
        var actualResponse = await response.Content.ReadAsStringAsync();

        var actual = JsonNode.Parse(actualResponse);
        var expected = JsonNode.Parse(expectedResponse);

        Assert.True(JsonNode.DeepEquals(expected, actual));
    }

    [Fact]
	public async Task SingleCSVRow_IsReturnedAsOneItemArray()
	{
        var type = "CSV";
        var data = """
            name,age
            Alice,30
            """;

        var expectedResponse = """
        {
            "status": "Success",
            "processedCount": 1,
            "data": [
                {
                "name": "Alice",
                "age": "30"
                }
            ]
        }
        """;

		var response = await SendRequest(type, data);
		response.EnsureSuccessStatusCode();
        var actualResponse = await response.Content.ReadAsStringAsync();

        var actual = JsonNode.Parse(actualResponse);
        var expected = JsonNode.Parse(expectedResponse);

        Assert.True(JsonNode.DeepEquals(expected, actual));
    }

    [Fact]
	public async Task MultipleCSVRows_AreReturnedAsItemArray()
	{
        var type = "CSV";
        var data = """
            name,age
            Alice,30
            Bob,40
            Tom,50
            """;

        var expectedResponse = """
        {
            "status": "Success",
            "processedCount": 3,
            "data": [
                {
                    "name": "Alice",
                    "age": "30"
                },
                {
                    "name": "Bob",
                    "age": "40"
                },
                {
                    "name": "Tom",
                    "age": "50"
                }
            ]
        }
        """;

		var response = await SendRequest(type, data);
		response.EnsureSuccessStatusCode();
        var actualResponse = await response.Content.ReadAsStringAsync();

        var actual = JsonNode.Parse(actualResponse);
        var expected = JsonNode.Parse(expectedResponse);

        Assert.True(JsonNode.DeepEquals(expected, actual));
    }

	[Fact]
	public async Task InvalidBase64_ReturnsBadRequest()
	{
		var response = await client.PostAsJsonAsync("/api/v1/parse-content", new
		{
			type = "CSV",
			content = "not-valid-base64"
		});

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        string actual = await GetResponseMessage(response);
        var expected = "Content is not valid Base64.";
        Assert.Equal(expected, actual);
	}

    [Fact]
	public async Task UnsupportedType_ReturnsBadRequest()
	{
		var response = await client.PostAsJsonAsync("/api/v1/parse-content", new
		{
			type = "BadType",
			content = "Irrelevant"
		});

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        string actual = await GetResponseMessage(response);
        var expected = "Unsupported type.";
        Assert.Equal(expected, actual);
	}

    [Fact]
	public async Task InvalidJsonContent_ReturnsBadRequest()
	{
        var response = await SendRequest("INTERNAL_JSON", "InvalidJsonContent");
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        string actual = await GetResponseMessage(response);
        string expected = "Invalid INTERNAL_JSON content";
        Assert.Equal(expected, actual);
	}

        [Fact]
	public async Task EmptyBody_ReturnsBadRequest()
	{
		var response = await client.PostAsJsonAsync("/api/v1/parse-content", new {});
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	private Task<HttpResponseMessage> SendRequest(string type, string content)
	{
		var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

		return client.PostAsJsonAsync("/api/v1/parse-content", new
		{
			type,
			content = base64Content
		});
	}


    private async Task<string> GetResponseMessage(HttpResponseMessage response)
	{
        using var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var responseMessage = json!.RootElement.GetProperty("message").GetString();
        return responseMessage!;
	}
}
