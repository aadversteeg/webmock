using Ave.WebMock;
using System.Collections.Generic;

namespace UnitTests.WebMock
{
    public class PageTests
    {
        private readonly Fixture _fixture;

        public PageTests()
        {
            _fixture = new Fixture();
        }

        [Fact(DisplayName = "P-001: Constructor sets properties correctly")]
        public void P001()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var links = _fixture.Create<List<string>>();

            // Act
            var page = new Page(title, links);

            // Assert
            page.Title.Should().Be(title);
            page.Links.Should().BeEquivalentTo(links);
        }

        [Fact(DisplayName = "P-002: Pages with same values should be equal")]
        public void P002()
        {
            // Arrange
            var title = _fixture.Create<string>();
            var links = _fixture.Create<List<string>>();

            // Act
            var page1 = new Page(title, links);
            var page2 = new Page(title, links);

            // Assert
            page1.Should().Be(page2);
            page1.GetHashCode().Should().Be(page2.GetHashCode());
        }

        [Fact(DisplayName = "P-003: Pages with different values should not be equal")]
        public void P003()
        {
            // Arrange
            var title1 = _fixture.Create<string>();
            var links1 = _fixture.Create<List<string>>();
            
            var title2 = _fixture.Create<string>() + "_different";
            var links2 = new List<string>(links1) { _fixture.Create<string>() + "_different" };

            // Act
            var page1 = new Page(title1, links1);
            var page2 = new Page(title2, links2);

            // Assert
            page1.Should().NotBe(page2);
        }
    }
}