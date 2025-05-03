using System.Collections.Generic;

namespace Ave.WebMock
{
    /// <summary>
    /// Represents a web page with a title and collection of links.
    /// Used as a simplified model of HTML pages for testing purposes.
    /// </summary>
    public record Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> class.
        /// </summary>
        /// <param name="title">The title of the page.</param>
        /// <param name="links">Collection of URLs representing links in the page.</param>
        public Page(string title, IReadOnlyCollection<string> links)
        {
            Title = title;
            Links = links;
        }

        /// <summary>
        /// Gets the title of the page.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the collection of links contained in the page.
        /// </summary>
        public IReadOnlyCollection<string> Links { get; }
    }
}