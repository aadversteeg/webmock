using System.Collections.Generic;

namespace Ave.WebMock
{
    public record Page
    {
        public Page(string title, IReadOnlyCollection<string> links)
        {
            Title = title;
            Links = links;
        }

        public string Title { get; }

        public IReadOnlyCollection<string> Links { get; }
    }
}