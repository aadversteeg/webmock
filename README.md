# Ave.WebMock

A lightweight library for mocking web requests in unit tests, providing control over response timing, status codes, and content.

## Features

- Create mock websites with hierarchical page structures
- Configure response delays to simulate network latency
- Add custom HTTP status codes for error testing
- Create JSON API responses for RESTful API testing
- Customize pages with links, titles, and content
- Easy fluent API with method chaining

## Getting Started

```csharp
// Create a mock website with pages and links
var web = WebBuilder.Create()
    .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(100))
    .WithWebsite("https://example.com")
        .WithTitle("Example Website")
        .WithSubLevels(2, 3) // 2 first-level pages, each with 3 second-level pages
    .Build();

// Fetch a page
var response = await web.Fetch("https://example.com");
var page = response.ToPage();

// Assert on the content
Assert.Equal("Example Website", page.Title);
Assert.Equal(2, page.Links.Count);
```

## API Mocking

```csharp
// Create a mock API
var web = WebBuilder.Create()
    .WithWebsite("https://api.example.com")
        .WithJsonResponse("/users/1", new { id = 1, name = "John Doe" })
        .WithJsonResponse("/error", new { error = "Not found" }, 404)
    .Build();

// Fetch API response
var response = await web.Fetch("https://api.example.com/users/1");
// response.ContentType will be "application/json"
// response.StatusCode will be 200
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.