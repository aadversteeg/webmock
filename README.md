# Ave.WebMock

A lightweight library for mocking web requests in unit tests, providing control over response timing, status codes, and content.

## Features

- Create mock websites with hierarchical page structures
- Configure response delays to simulate network latency
- Add custom HTTP status codes for error testing
- Create JSON API responses for RESTful API testing
- Customize pages with links, titles, and content
- Easy fluent API with method chaining

## Installation

Ave.WebMock is available as a NuGet package. You can install it using the NuGet Package Manager Console or the .NET CLI:

```
dotnet add package Ave.WebMock
```

Or with the Package Manager Console:

```
Install-Package Ave.WebMock
```

### Framework Compatibility

Ave.WebMock targets .NET 8.0, which means it's compatible with:

- .NET 8.0 applications and libraries
- .NET MAUI applications targeting .NET 8.0
- ASP.NET Core 8.0 applications
- Any other project targeting .NET 8.0

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

## Advanced Usage

### Hierarchical Page Structures

WebMock allows you to create complex website structures with multiple levels of pages:

```csharp
var web = WebBuilder.Create()
    .WithWebsite("https://example.com")
        .WithTitle("Main Site")
        // Create 3 main pages, each with 2 subpages, each subpage with 2 further subpages
        .WithSubLevels(3, 2, 2)
        // Add parent/root links to navigation
        .WithSubLevel(3, includeParentLink: true, includeRootLink: true)
    .Build();
```

### Custom Response Configuration

You can configure specific pages with custom behavior:

```csharp
var web = WebBuilder.Create()
    .WithWebsite("https://example.com")
        .ConfigurePage("/about", page => page
            .WithTitle("About Us")
            .AddLinks("https://example.com/contact", "https://example.com/team")
            .WithFetchDelay(TimeSpan.FromSeconds(2))) // Slow page
        .WithErrorPage("/broken", 500, "Server Error")
    .Build();
```

### Testing with Custom Delays

You can simulate network conditions by configuring delays:

```csharp
[Fact]
public async Task Request_Timeout_Is_Handled_Correctly()
{
    // Arrange
    var web = WebBuilder.Create()
        .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(50))
        .WithWebsite("https://example.com")
            .ConfigurePage("/slow", page => 
                page.WithFetchDelay(TimeSpan.FromSeconds(5)))
        .Build();
            
    // Create a cancellation token with 1 second timeout
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
    
    // Act & Assert - should throw when timeout occurs
    await Assert.ThrowsAsync<TaskCanceledException>(() =>
        web.Fetch("https://example.com/slow").AsTask().WaitAsync(cts.Token));
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.