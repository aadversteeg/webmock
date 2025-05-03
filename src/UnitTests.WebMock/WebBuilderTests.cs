using Ave.WebMock;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.WebMock
{
    public class WebBuilderTests
    {
        [Fact(DisplayName = "WB-001: Can create a simple website")]
        public async Task WB001()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithNotFoundDelay(TimeSpan.FromMilliseconds(5))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevels(3, 2, 5) // Level 1: 3 links, Level 2: 2 links each, Level 3: 5 links each
                .Build();

            // Assert
            var rootResponse = await web.Fetch("https://example.com");
            var rootPage = rootResponse.ToPage();
            rootPage.Should().NotBeNull();
            rootPage!.Title.Should().Be("Example Website");
            rootPage.Links.Should().HaveCount(3);
            rootPage.Links.Should().Contain("https://example.com/page1");
            rootPage.Links.Should().Contain("https://example.com/page2");
            rootPage.Links.Should().Contain("https://example.com/page3");

            // Test first level page
            var level1Response = await web.Fetch("https://example.com/page1");
            var level1Page = level1Response.ToPage();
            level1Page.Should().NotBeNull();
            level1Page!.Title.Should().Be("Example Website [1]");
            level1Page.Links.Should().HaveCount(2);
            level1Page.Links.Should().Contain("https://example.com/page1/page1");
            level1Page.Links.Should().Contain("https://example.com/page1/page2");

            // Test second level page
            var level2Response = await web.Fetch("https://example.com/page1/page1");
            var level2Page = level2Response.ToPage();
            level2Page.Should().NotBeNull();
            level2Page!.Title.Should().Be("Example Website [1.1]");
            level2Page.Links.Should().HaveCount(5);
            level2Page.Links.Should().Contain("https://example.com/page1/page1/page1");
            level2Page.Links.Should().Contain("https://example.com/page1/page1/page5");

            // Test third level page (leaf node)
            var level3Response = await web.Fetch("https://example.com/page1/page1/page1");
            var level3Page = level3Response.ToPage();
            level3Page.Should().NotBeNull();
            level3Page!.Title.Should().Be("Example Website [1.1.1]");
            level3Page.Links.Should().BeEmpty();
        }

        [Fact(DisplayName = "WB-002: Can create multiple websites")]
        public async Task WB002()
        {
            // Arrange & Act
            var builder = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10));
                
            // First, add site1.com to the builder
            builder.WithWebsite("https://site1.com")
                .WithTitle("Site One")
                .WithSubLevels(2);
                
            // Then, add site2.com to the builder
            builder.WithWebsite("https://site2.com")
                .WithTitle("Site Two")
                .WithSubLevels(3);
                
            // Finally, build the web
            var web = builder.Build();

            // Assert
            var site1Response = await web.Fetch("https://site1.com");
            var site1Root = site1Response.ToPage();
            site1Root.Should().NotBeNull();
            site1Root!.Title.Should().Be("Site One");
            site1Root.Links.Should().HaveCount(2);

            var site2Response = await web.Fetch("https://site2.com");
            var site2Root = site2Response.ToPage();
            site2Root.Should().NotBeNull();
            site2Root!.Title.Should().Be("Site Two");
            site2Root.Links.Should().HaveCount(3);
        }
        
        [Fact(DisplayName = "WB-003: WithSubLevel method creates correct links with parent and root references")]
        public async Task WB003()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevel(numberOfPages: 3, includeParentLink: true, includeRootLink: true)
                .Build();

            // Assert
            // Check root page
            var rootResponse = await web.Fetch("https://example.com");
            var rootPage = rootResponse.ToPage();
            rootPage.Should().NotBeNull();
            rootPage!.Title.Should().Be("Example Website");
            rootPage.Links.Should().HaveCount(4); // 3 child links + self link (as root)
            rootPage.Links.Should().Contain("https://example.com"); // Self-reference from includeRootLink
            rootPage.Links.Should().Contain("https://example.com/page1");
            rootPage.Links.Should().Contain("https://example.com/page2");
            rootPage.Links.Should().Contain("https://example.com/page3");

            // Check child page
            var childResponse = await web.Fetch("https://example.com/page1");
            var childPage = childResponse.ToPage();
            childPage.Should().NotBeNull();
            childPage!.Title.Should().Be("Example Website [1]");
            childPage.Links.Should().HaveCount(2); // Parent link + root link
            childPage.Links.Should().Contain("https://example.com"); // Root link
            childPage.Links.Should().Contain("https://example.com"); // Parent link (same as root in this case)
        }
        
        [Fact(DisplayName = "WB-004: WithSubLevel creates pages with custom link settings")]
        public async Task WB004()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevels(2, 3) // Level 1: 2 pages, Level 2: 3 pages each
                .Build();
                
            var webWithCustomLinks = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://custom.com")
                    .WithTitle("Custom Website")
                    .WithSubLevel(numberOfPages: 2, includeParentLink: false, includeRootLink: true)
                .Build();

            // Assert
            // Standard website without custom links
            var standardResponse = await web.Fetch("https://example.com/page1");
            var standardPage = standardResponse.ToPage();
            standardPage.Should().NotBeNull();
            standardPage!.Links.Should().HaveCount(3); // Only next level links
            standardPage.Links.Should().NotContain("https://example.com"); // No root link
            
            // Custom website with root links but no parent links
            var customResponse = await webWithCustomLinks.Fetch("https://custom.com/page1");
            var customPage = customResponse.ToPage();
            customPage.Should().NotBeNull();
            customPage!.Links.Should().HaveCount(1); // Only root link (no next level defined)
            customPage.Links.Should().Contain("https://custom.com"); // Contains root link
        }
        
        [Fact(DisplayName = "WB-005: ConfigurePage allows custom page configuration")]
        public async Task WB005()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevel(numberOfPages: 3)
                    .ConfigurePage("https://example.com/page1", config => 
                        config.WithTitle("Custom Page Title")
                              .AddLink("https://external.com")
                              .AddLink("https://example.com/page3"))
                    .ConfigurePage("https://example.com", config =>
                        config.AddLinks("https://api.example.com", "https://blog.example.com"))
                .Build();

            // Assert
            // Check customized page
            var customResponse = await web.Fetch("https://example.com/page1");
            var customPage = customResponse.ToPage();
            customPage.Should().NotBeNull();
            customPage!.Title.Should().Be("Custom Page Title"); // Custom title instead of "Example Website [1]"
            customPage.Links.Should().HaveCount(2); // Custom links only
            customPage.Links.Should().Contain("https://external.com");
            customPage.Links.Should().Contain("https://example.com/page3");
            
            // Check root page with additional links
            var rootResponse = await web.Fetch("https://example.com");
            var rootPage = rootResponse.ToPage();
            rootPage.Should().NotBeNull();
            rootPage!.Links.Should().HaveCount(5); // 3 default child links + 2 custom links
            rootPage.Links.Should().Contain("https://example.com/page1");
            rootPage.Links.Should().Contain("https://example.com/page2");
            rootPage.Links.Should().Contain("https://example.com/page3");
            rootPage.Links.Should().Contain("https://api.example.com");
            rootPage.Links.Should().Contain("https://blog.example.com");
            
            // Check non-customized page
            var standardResponse = await web.Fetch("https://example.com/page2");
            var standardPage = standardResponse.ToPage();
            standardPage.Should().NotBeNull();
            standardPage!.Title.Should().Be("Example Website [2]"); // Standard title format
            standardPage.Links.Should().BeEmpty(); // No links (no next level defined)
        }
        
        [Fact(DisplayName = "WB-006: ConfigurePage works with relative paths")]
        public async Task WB006()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevel(numberOfPages: 2)
                    .ConfigurePage("page1", config => // Using relative path
                        config.WithTitle("Page One Custom")
                              .AddLink("https://example.com/page2"))
                    .ConfigurePage("/page2", config => // Using relative path with leading slash
                        config.WithTitle("Page Two Custom")
                              .WithFetchDelay(TimeSpan.FromMilliseconds(50))
                              .AddLink("https://example.com/page1"))
                .Build();

            // Assert
            var page1Response = await web.Fetch("https://example.com/page1");
            var page1 = page1Response.ToPage();
            page1.Should().NotBeNull();
            page1!.Title.Should().Be("Page One Custom"); // Custom title
            page1.Links.Should().HaveCount(1);
            page1.Links.Should().Contain("https://example.com/page2");
            
            var page2Response = await web.Fetch("https://example.com/page2");
            var page2 = page2Response.ToPage();
            page2.Should().NotBeNull();
            page2!.Title.Should().Be("Page Two Custom"); // Custom title
            page2.Links.Should().HaveCount(1);
            page2.Links.Should().Contain("https://example.com/page1");
        }
        
        [Fact(DisplayName = "WB-007: Can add special relationship links to pages")]
        public async Task WB007()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevel(numberOfPages: 5)
                    .ConfigurePage("page1", config => 
                        config.AddRootLink())
                    .ConfigurePage("page2", config => 
                        config.AddParentLink())
                    .ConfigurePage("page3", config => 
                        config.AddPreviousSiblingLink())
                    .ConfigurePage("page4", config => 
                        config.AddNextSiblingLink())
                    .ConfigurePage("page5", config => 
                        config.AddPreviousSiblingLink()
                        .AddNextSiblingLink()
                        .AddParentLink()
                        .AddRootLink())
                .Build();

            // Assert
            // Page with root link
            var page1Response = await web.Fetch("https://example.com/page1");
            var page1 = page1Response.ToPage();
            page1.Should().NotBeNull();
            page1!.Links.Should().Contain("https://example.com"); // Root link
            
            // Page with parent link
            var page2Response = await web.Fetch("https://example.com/page2");
            var page2 = page2Response.ToPage();
            page2.Should().NotBeNull();
            page2!.Links.Should().Contain("https://example.com"); // Parent link (same as root in this case)
            
            // Page with previous sibling link
            var page3Response = await web.Fetch("https://example.com/page3");
            var page3 = page3Response.ToPage();
            page3.Should().NotBeNull();
            page3!.Links.Should().Contain("https://example.com/page2"); // Previous sibling link
            
            // Page with next sibling link
            var page4Response = await web.Fetch("https://example.com/page4");
            var page4 = page4Response.ToPage();
            page4.Should().NotBeNull();
            page4!.Links.Should().Contain("https://example.com/page5"); // Next sibling link
            
            // Page with all relationship links
            var page5Response = await web.Fetch("https://example.com/page5");
            var page5 = page5Response.ToPage();
            page5.Should().NotBeNull();
            page5!.Links.Should().HaveCount(4); // All relationship links (no overlaps)
            page5.Links.Should().Contain("https://example.com"); // Root link
            page5.Links.Should().Contain("https://example.com"); // Parent link (same as root in this case)
            page5.Links.Should().Contain("https://example.com/page4"); // Previous sibling
            page5.Links.Should().Contain("https://example.com/page1"); // Next sibling (wraps around to page1)
        }
        
        [Fact(DisplayName = "WB-008: Correctly handles relationship links at different levels")]
        public async Task WB008()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevels(3, 3) // Level 1: 3 pages, Level 2: 3 pages each
                    .ConfigurePage("page1/page2", config => 
                        config.AddParentLink()
                        .AddRootLink()
                        .AddPreviousSiblingLink()
                        .AddNextSiblingLink())
                .Build();

            // Assert
            var nestedResponse = await web.Fetch("https://example.com/page1/page2");
            var nestedPage = nestedResponse.ToPage();
            nestedPage.Should().NotBeNull();
            nestedPage!.Title.Should().Be("Example Website [1.2]");
            nestedPage.Links.Should().HaveCount(4); // Only relationship links (the child links get added at a different level)
            
            // Parent link
            nestedPage.Links.Should().Contain("https://example.com/page1"); 
            
            // Root link
            nestedPage.Links.Should().Contain("https://example.com"); 
            
            // Previous sibling link
            nestedPage.Links.Should().Contain("https://example.com/page1/page1"); 
            
            // Next sibling link
            nestedPage.Links.Should().Contain("https://example.com/page1/page3"); 
            
            // We don't test for child links since they are not automatically added 
            // with the ConfigurePage approach - they'd need to be added explicitly
        }
        
        [Fact(DisplayName = "WB-009: Can configure pages at multiple levels with custom settings")]
        public async Task WB009()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevels(2, 2, 2) // 3 levels deep: 2 pages per level
                    // Configure the root page
                    .ConfigurePage("https://example.com", config => 
                        config.WithTitle("Root Page")
                        .AddLink("https://external.com/root"))
                    // Configure level 1 pages
                    .ConfigurePage("page1", config => 
                        config.WithTitle("Level 1 - Page 1")
                        .AddRootLink()
                        .AddLink("https://external.com/level1-1"))
                    .ConfigurePage("page2", config => 
                        config.WithTitle("Level 1 - Page 2")
                        .AddRootLink()
                        .AddPreviousSiblingLink())
                    // Configure level 2 pages
                    .ConfigurePage("page1/page1", config => 
                        config.WithTitle("Level 2 - Page 1")
                        .AddParentLink()
                        .AddRootLink()
                        .AddLink("https://external.com/level2-1"))
                    .ConfigurePage("page2/page2", config => 
                        config.WithTitle("Level 2 - Page 2")
                        .AddParentLink()
                        .AddNextSiblingLink())
                    // Configure level 3 pages
                    .ConfigurePage("page1/page1/page1", config => 
                        config.WithTitle("Level 3 - Page 1")
                        .AddParentLink()
                        .AddRootLink())
                    .ConfigurePage("page2/page1/page2", config => 
                        config.WithTitle("Level 3 - Page 2")
                        .AddNextSiblingLink()
                        .AddPreviousSiblingLink())
                .Build();

            // Assert
            // Test root page
            var rootResponse = await web.Fetch("https://example.com");
            var rootPage = rootResponse.ToPage();
            rootPage.Should().NotBeNull();
            rootPage!.Title.Should().Be("Root Page");
            rootPage.Links.Should().HaveCount(3); // 2 child pages + custom external link
            rootPage.Links.Should().Contain("https://example.com/page1");
            rootPage.Links.Should().Contain("https://example.com/page2");
            rootPage.Links.Should().Contain("https://external.com/root");

            // Test level 1 - page 1
            var level1Page1Response = await web.Fetch("https://example.com/page1");
            var level1Page1 = level1Page1Response.ToPage();
            level1Page1.Should().NotBeNull();
            level1Page1!.Title.Should().Be("Level 1 - Page 1");
            level1Page1.Links.Should().HaveCount(4); // 2 child pages + root link + external link
            level1Page1.Links.Should().Contain("https://example.com/page1/page1");
            level1Page1.Links.Should().Contain("https://example.com/page1/page2");
            level1Page1.Links.Should().Contain("https://example.com");
            level1Page1.Links.Should().Contain("https://external.com/level1-1");

            // Test level 1 - page 2
            var level1Page2Response = await web.Fetch("https://example.com/page2");
            var level1Page2 = level1Page2Response.ToPage();
            level1Page2.Should().NotBeNull();
            level1Page2!.Title.Should().Be("Level 1 - Page 2");
            level1Page2.Links.Should().HaveCount(4); // 2 child pages + root link + previous sibling link
            level1Page2.Links.Should().Contain("https://example.com/page2/page1");
            level1Page2.Links.Should().Contain("https://example.com/page2/page2");
            level1Page2.Links.Should().Contain("https://example.com");
            level1Page2.Links.Should().Contain("https://example.com/page1");

            // Test level 2 - page 1
            var level2Page1Response = await web.Fetch("https://example.com/page1/page1");
            var level2Page1 = level2Page1Response.ToPage();
            level2Page1.Should().NotBeNull();
            level2Page1!.Title.Should().Be("Level 2 - Page 1");
            level2Page1.Links.Should().HaveCount(5); // 2 child pages + parent link + root link + external link
            level2Page1.Links.Should().Contain("https://example.com/page1/page1/page1");
            level2Page1.Links.Should().Contain("https://example.com/page1/page1/page2");
            level2Page1.Links.Should().Contain("https://example.com/page1");
            level2Page1.Links.Should().Contain("https://example.com");
            level2Page1.Links.Should().Contain("https://external.com/level2-1");

            // Test level 2 - page 2
            var level2Page2Response = await web.Fetch("https://example.com/page2/page2");
            var level2Page2 = level2Page2Response.ToPage();
            level2Page2.Should().NotBeNull();
            level2Page2!.Title.Should().Be("Level 2 - Page 2");
            level2Page2.Links.Should().HaveCount(4); // 2 child pages + parent link + next sibling link
            level2Page2.Links.Should().Contain("https://example.com/page2/page2/page1");
            level2Page2.Links.Should().Contain("https://example.com/page2/page2/page2");
            level2Page2.Links.Should().Contain("https://example.com/page2");
            // Next sibling is page1 again (wraps around) since there's no page3
            level2Page2.Links.Should().Contain("https://example.com/page2/page1");

            // Test level 3 - page 1
            var level3Page1Response = await web.Fetch("https://example.com/page1/page1/page1");
            var level3Page1 = level3Page1Response.ToPage();
            level3Page1.Should().NotBeNull();
            level3Page1!.Title.Should().Be("Level 3 - Page 1");
            level3Page1.Links.Should().HaveCount(2); // Parent link + root link
            level3Page1.Links.Should().Contain("https://example.com/page1/page1");
            level3Page1.Links.Should().Contain("https://example.com");

            // Test level 3 - page 2 with sibling links
            var level3Page2Response = await web.Fetch("https://example.com/page2/page1/page2");
            var level3Page2 = level3Page2Response.ToPage();
            level3Page2.Should().NotBeNull();
            level3Page2!.Title.Should().Be("Level 3 - Page 2");
            level3Page2.Links.Should().HaveCount(2); // Previous sibling + next sibling link
            level3Page2.Links.Should().Contain("https://example.com/page2/page1/page1");
            level3Page2.Links.Should().Contain("https://example.com/page2/page1/page1"); // Wraps around to page1 since there's no page3
        }

        [Fact(DisplayName = "WB-010: Can create and fetch error pages with custom status codes")]
        public async Task WB010()
        {
            // Arrange & Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                // Define global status code templates
                .WithStatusCodeResponse(404, config =>
                    config.WithTitle("Global 404 - Not Found")
                          .WithLink("/", "Back to Home"))
                .WithStatusCodeResponse(500, config =>
                    config.WithTitle("Global 500 - Server Error"))
                .WithWebsite("https://example.com")
                    .WithTitle("Example Website")
                    .WithSubLevels(2)
                    // Define site-specific error pages
                    .WithErrorPage("notfound", 404, "Custom 404 Page Not Found")
                    .WithErrorPage("error", 500, "Custom 500 Server Error")
                    .WithErrorPage("unauthorized", 401, "Unauthorized Access")
                .Build();

            // Assert
            // Test custom 404 page
            var notFoundResponse = await web.Fetch("https://example.com/notfound");
            notFoundResponse.Should().NotBeNull();
            notFoundResponse!.StatusCode.Should().Be(404);
            var notFoundPage = notFoundResponse.ToPage();
            notFoundPage.Should().NotBeNull();
            notFoundPage!.Title.Should().Be("Custom 404 Page Not Found");

            // Test custom 500 page
            var errorResponse = await web.Fetch("https://example.com/error");
            errorResponse.Should().NotBeNull();
            errorResponse!.StatusCode.Should().Be(500);
            var errorPage = errorResponse.ToPage();
            errorPage.Should().NotBeNull();
            errorPage!.Title.Should().Be("Custom 500 Server Error");

            // Test custom 401 page
            var unauthorizedResponse = await web.Fetch("https://example.com/unauthorized");
            unauthorizedResponse.Should().NotBeNull();
            unauthorizedResponse!.StatusCode.Should().Be(401);
            var unauthorizedPage = unauthorizedResponse.ToPage();
            unauthorizedPage.Should().NotBeNull();
            unauthorizedPage!.Title.Should().Be("Unauthorized Access");

            // Verify IsSuccess property works correctly
            notFoundResponse.IsSuccess.Should().BeFalse();
            errorResponse.IsSuccess.Should().BeFalse();

            // Normal page should be successful
            var rootResponse = await web.Fetch("https://example.com");
            rootResponse!.IsSuccess.Should().BeTrue();
            rootResponse.StatusCode.Should().Be(200);
        }

        [Fact(DisplayName = "WB-011: Can create JSON API responses")]
        public async Task WB011()
        {
            // Arrange
            var todoItem = new { id = 1, title = "Complete task", completed = false };
            var userList = new[]
            {
                new { id = 1, name = "John Doe", email = "john@example.com" },
                new { id = 2, name = "Jane Smith", email = "jane@example.com" }
            };
            var errorResponse = new { error = "Not found", code = 404 };

            // Act
            var web = WebBuilder.Create()
                .WithDefaultFetchDelay(TimeSpan.FromMilliseconds(10))
                .WithWebsite("https://api.example.com")
                    // Add various API responses
                    .WithJsonResponse("/todos/1", todoItem)
                    .WithJsonResponse("/users", userList)
                    .WithJsonResponse("/users/999", errorResponse, 404)
                // Add a custom direct response without using the website context
                .WithCustomResponse("https://direct-api.example.com/status",
                    ResponseBuilder.Create().WithStatusCode(200).BuildHtml(),
                    TimeSpan.FromMilliseconds(5))
                .Build();

            // Assert
            // Check todo item response
            var todoResponse = await web.Fetch("https://api.example.com/todos/1");
            todoResponse.Should().NotBeNull();
            todoResponse!.StatusCode.Should().Be(200);
            todoResponse.ContentType.Should().Be("application/json");
            todoResponse.Content.Should().Contain("\"id\": 1");
            todoResponse.Content.Should().Contain("\"title\": \"Complete task\"");
            todoResponse.Content.Should().Contain("\"completed\": false");

            // Check users list response
            var usersResponse = await web.Fetch("https://api.example.com/users");
            usersResponse.Should().NotBeNull();
            usersResponse!.StatusCode.Should().Be(200);
            usersResponse.ContentType.Should().Be("application/json");
            usersResponse.Content.Should().Contain("\"id\": 1");
            usersResponse.Content.Should().Contain("\"name\": \"John Doe\"");
            usersResponse.Content.Should().Contain("\"id\": 2");
            usersResponse.Content.Should().Contain("\"name\": \"Jane Smith\"");

            // Check error response
            var errorApiResponse = await web.Fetch("https://api.example.com/users/999");
            errorApiResponse.Should().NotBeNull();
            errorApiResponse!.StatusCode.Should().Be(404);
            errorApiResponse.ContentType.Should().Be("application/json");
            errorApiResponse.Content.Should().Contain("\"error\": \"Not found\"");
            errorApiResponse.Content.Should().Contain("\"code\": 404");

            // Check direct API response 
            var directResponse = await web.Fetch("https://direct-api.example.com/status");
            directResponse.Should().NotBeNull();
            directResponse!.StatusCode.Should().Be(200);
        }
    }
}