using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Ave.WebMock;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace UnitTests.WebMock
{
    public class ResponseExtensionsTests
    {
        private readonly Fixture _fixture;

        public ResponseExtensionsTests()
        {
            _fixture = new Fixture();
        }

        [Fact(DisplayName = "RE-001: ToPage extracts title and links from HTML")]
        public void RE001()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var url1 = "https://example.com/page1";
            var url2 = "https://example.com/page2";
            
            var templatePath = Path.Combine(GetTestResourcesPath(), "RE001_CompleteHtml.html");
            var template = File.ReadAllText(templatePath);
            var html = string.Format(template, title, url1, url2);
            
            var response = new Response(html, "text/html");

            // Act
            var page = response.ToPage();

            // Assert
            page.Should().NotBeNull();
            page.Title.Should().Be(title);
            page.Links.Should().HaveCount(2);
            page.Links.Should().Contain(url1);
            page.Links.Should().Contain(url2);
        }

        [Fact(DisplayName = "RE-002: ToPage handles empty HTML")]
        public void RE002()
        {
            // Arrange
            var templatePath = Path.Combine(GetTestResourcesPath(), "RE002_EmptyHtml.html");
            var html = File.ReadAllText(templatePath);
            var response = new Response(html, "text/html");

            // Act
            var page = response.ToPage();

            // Assert
            page.Should().NotBeNull();
            page.Title.Should().BeEmpty();
            page.Links.Should().BeEmpty();
        }

        [Fact(DisplayName = "RE-003: ToPage returns null for null response")]
        public void RE003()
        {
            // Arrange
            Response? response = null;

            // Act
            var page = response.ToPage();

            // Assert
            page.Should().BeNull();
        }

        [Fact(DisplayName = "RE-004: ToPage throws for non-HTML content type")]
        public void RE004()
        {
            // Arrange
            var response = new Response("{ \"key\": \"value\" }", "application/json");

            // Act & Assert
            Action act = () => response.ToPage();
            act.Should().Throw<ArgumentException>()
                .WithMessage("*HTML content type*");
        }

        [Fact(DisplayName = "RE-005: ToPage uses h1 when title tag is missing")]
        public void RE005()
        {
            // Arrange
            var title = _fixture.Create<string>();
            
            var templatePath = Path.Combine(GetTestResourcesPath(), "RE005_OnlyH1Title.html");
            var template = File.ReadAllText(templatePath);
            var html = string.Format(template, title);
            
            var response = new Response(html, "text/html");

            // Act
            var page = response.ToPage();

            // Assert
            page.Should().NotBeNull();
            page.Title.Should().Be(title);
        }
        
        [Fact(DisplayName = "RE-006: ToPage extension for Task<Response> works correctly")]
        public async Task RE006()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var url1 = "https://example.com/page1";
            var url2 = "https://example.com/page2";
            
            var templatePath = Path.Combine(GetTestResourcesPath(), "RE001_CompleteHtml.html");
            var template = File.ReadAllText(templatePath);
            var html = string.Format(template, title, url1, url2);
            
            var response = new Response(html, "text/html");
            Task<Response?> responseTask = Task.FromResult<Response?>(response);

            // Act
            var page = await responseTask.ToPage();

            // Assert
            page.Should().NotBeNull();
            page!.Title.Should().Be(title);
            page.Links.Should().HaveCount(2);
            page.Links.Should().Contain(url1);
            page.Links.Should().Contain(url2);
        }
        
        [Fact(DisplayName = "RE-007: ToPage extension for Task<Response> handles null response")]
        public async Task RE007()
        {
            // Arrange
            Task<Response?> responseTask = Task.FromResult<Response?>(null);

            // Act
            var page = await responseTask.ToPage();

            // Assert
            page.Should().BeNull();
        }

        private string GetTestResourcesPath()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            return Path.Combine(assemblyDirectory ?? string.Empty, "TestResources");
        }
    }
}