using System.Collections.Generic;
using Ave.WebMock;

namespace UnitTests.WebMock
{
    public class ResponseBuilderTests
    {
        private readonly Fixture _fixture;

        public ResponseBuilderTests()
        {
            _fixture = new Fixture();
        }

        [Fact(DisplayName = "RB-001: ResponseBuilder creates HTML with title and links")]
        public void RB001()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var link1Url = "https://example.com/page1";
            var link1Text = "Page 1";
            var link2Url = "https://example.com/page2";
            var link2Text = "Page 2";

            // Act
            var response = ResponseBuilder.Create()
                .WithTitle(title)
                .WithLink(link1Url, link1Text)
                .WithLink(link2Url, link2Text)
                .BuildHtml();

            // Assert
            response.Should().NotBeNull();
            response.ContentType.Should().Be("text/html");
            response.Content.Should().Contain($"<title>{title}</title>");
            response.Content.Should().Contain($"<h1>{title}</h1>");
            response.Content.Should().Contain($"<a href=\"{link1Url}\">{link1Text}</a>");
            response.Content.Should().Contain($"<a href=\"{link2Url}\">{link2Text}</a>");
        }

        [Fact(DisplayName = "RB-002: ResponseBuilder creates HTML with multiple links")]
        public void RB002()
        {
            // Arrange
            var title = "Test Page";
            var links = new List<(string Url, string Text)>
            {
                ("https://example.com/page1", "Page 1"),
                ("https://example.com/page2", "Page 2"),
                ("https://example.com/page3", "Page 3")
            };

            // Act
            var response = ResponseBuilder.Create()
                .WithTitle(title)
                .WithLinks(links)
                .BuildHtml();

            // Assert
            response.Should().NotBeNull();
            response.ContentType.Should().Be("text/html");
            
            foreach (var (url, text) in links)
            {
                response.Content.Should().Contain($"<a href=\"{url}\">{text}</a>");
            }
        }

        [Fact(DisplayName = "RB-003: ResponseBuilder handles null values gracefully")]
        public void RB003()
        {
            // Act
            var response = ResponseBuilder.Create()
                .WithTitle(null!)  // Using null-forgiving operator
                .WithLinks(null!)  // Using null-forgiving operator
                .BuildHtml();

            // Assert
            response.Should().NotBeNull();
            response.ContentType.Should().Be("text/html");
            response.Content.Should().Contain("<title></title>");
            response.Content.Should().NotContain("<h1></h1>"); // Empty title shouldn't create an H1
        }
    }
}