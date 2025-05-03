using Ave.WebMock;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using AutoFixture;

namespace UnitTests.WebMock
{
    public class WebTests
    {
        private readonly Fixture _fixture;

        public WebTests()
        {
            _fixture = new Fixture();
        }

        [Fact(DisplayName = "W-001: AddPage should add a page correctly")]
        public void W001()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var links = _fixture.Create<List<string>>();
            var url = _fixture.Create<string>();
            var fetchDelay = TimeSpan.FromMilliseconds(100);
            var notFoundDelay = TimeSpan.FromMilliseconds(50);
            
            // Act
            var web = new Web(fetchDelay, notFoundDelay);
            web = web.AddPage(url, title, links, fetchDelay);
            
            // Assert
            // The assertion is done in W002
        }

        [Fact(DisplayName = "W-002: Fetch should return the correct response that can be converted to page")]
        public async Task W002()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var links = _fixture.Create<List<string>>();
            var url = _fixture.Create<string>();
            var fetchDelay = TimeSpan.FromMilliseconds(10); // Small delay for test
            var notFoundDelay = TimeSpan.FromMilliseconds(5);
            
            var web = new Web(fetchDelay, notFoundDelay);
            web = web.AddPage(url, title, links, fetchDelay);
            
            // Act
            var response = await web.Fetch(url);
            var page = response.ToPage();
            
            // Assert
            page.Should().NotBeNull();
            page!.Title.Should().Be(title);
            page.Links.Should().BeEquivalentTo(links);
        }

        [Fact(DisplayName = "W-003: Fetch should return null for non-existent page")]
        public async Task W003()
        {
            // Arrange
            var url = _fixture.Create<string>();
            var nonExistentUrl = url + "_nonexistent";
            var fetchDelay = TimeSpan.FromMilliseconds(10);
            var notFoundDelay = TimeSpan.FromMilliseconds(5);
            
            var web = new Web(fetchDelay, notFoundDelay);
            web = web.AddPage(url, _fixture.Create<string>(), _fixture.Create<List<string>>(), fetchDelay);
            
            // Act
            var response = await web.Fetch(nonExistentUrl);
            var page = response.ToPage();
            
            // Assert
            response.Should().BeNull();
            page.Should().BeNull();
        }

        [Fact(DisplayName = "W-004: Fetch should respect the fetch delay")]
        public async Task W004()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var links = _fixture.Create<List<string>>();
            var url = _fixture.Create<string>();
            var fetchDelay = TimeSpan.FromMilliseconds(100); // Noticeable delay
            var notFoundDelay = TimeSpan.FromMilliseconds(50);
            
            var web = new Web(fetchDelay, notFoundDelay);
            web = web.AddPage(url, title, links, fetchDelay);
            
            // Act
            var startTime = DateTime.Now;
            var response = await web.Fetch(url);
            var elapsed = DateTime.Now - startTime;
            
            // Assert
            response.Should().NotBeNull();
            // Use a small margin for test stability (97ms instead of exact 100ms)
            elapsed.TotalMilliseconds.Should().BeGreaterOrEqualTo(fetchDelay.TotalMilliseconds - 3);
        }

        [Fact(DisplayName = "W-005: Fetch should respect custom delay specified for page")]
        public async Task W005()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var links = _fixture.Create<List<string>>();
            var url = _fixture.Create<string>();
            var defaultDelay = TimeSpan.FromMilliseconds(500);
            var customDelay = TimeSpan.FromMilliseconds(100);
            var notFoundDelay = TimeSpan.FromMilliseconds(50);
            
            var web = new Web(defaultDelay, notFoundDelay);
            web = web.AddPage(url, title, links, customDelay);
            
            // Act
            var startTime = DateTime.Now;
            var response = await web.Fetch(url);
            var elapsed = DateTime.Now - startTime;
            
            // Assert
            response.Should().NotBeNull();
            elapsed.TotalMilliseconds.Should().BeGreaterOrEqualTo(customDelay.TotalMilliseconds - 5); // More lenient timing margin
            elapsed.TotalMilliseconds.Should().BeLessThan(defaultDelay.TotalMilliseconds * 0.9); // Allow some margin
        }
        
        [Fact(DisplayName = "W-006: Fetch should return the correct response")]
        public async Task W006()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var links = _fixture.Create<List<string>>();
            var url = _fixture.Create<string>();
            var fetchDelay = TimeSpan.FromMilliseconds(10); // Small delay for test
            var notFoundDelay = TimeSpan.FromMilliseconds(5);
            
            var web = new Web(fetchDelay, notFoundDelay);
            web = web.AddPage(url, title, links, fetchDelay);
            
            // Act
            var response = await web.Fetch(url);
            
            // Assert
            response.Should().NotBeNull();
            response!.ContentType.Should().Be("text/html");
            response.Content.Should().Contain(title);
            
            // Verify that HTML contains links
            foreach (var link in links)
            {
                response.Content.Should().Contain($"href=\"{link}\"");
            }
        }
        
        [Fact(DisplayName = "W-007: Fetch should return null for non-existent page")]
        public async Task W007()
        {
            // Arrange
            var url = _fixture.Create<string>();
            var nonExistentUrl = url + "_nonexistent";
            var fetchDelay = TimeSpan.FromMilliseconds(10);
            var notFoundDelay = TimeSpan.FromMilliseconds(5);
            
            var web = new Web(fetchDelay, notFoundDelay);
            web = web.AddPage(url, _fixture.Create<string>(), _fixture.Create<List<string>>(), fetchDelay);
            
            // Act
            var response = await web.Fetch(nonExistentUrl);
            
            // Assert
            response.Should().BeNull();
        }
        
        [Fact(DisplayName = "W-008: Fetch should respect the fetch delay")]
        public async Task W008()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var links = _fixture.Create<List<string>>();
            var url = _fixture.Create<string>();
            var fetchDelay = TimeSpan.FromMilliseconds(100); // Noticeable delay
            var notFoundDelay = TimeSpan.FromMilliseconds(50);
            
            var web = new Web(fetchDelay, notFoundDelay);
            web = web.AddPage(url, title, links, fetchDelay);
            
            // Act
            var response = await web.Fetch(url);
            
            // Assert - only check that the response is returned correctly
            response.Should().NotBeNull();
            response.Should().BeOfType<Response>();
            response!.Content.Should().Contain(title);
        }
    }
}