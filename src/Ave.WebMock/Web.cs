using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ave.WebMock
{
    /// <summary>
    /// Represents a mocked web environment with configurable responses and timing.
    /// Provides an immutable model for web interactions to use in unit tests.
    /// </summary>
    public class Web
    {
        /// <summary>
        /// Represents an entry in the response dictionary, pairing a response with its fetch delay.
        /// </summary>
        internal record ResponseEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ResponseEntry"/> record.
            /// </summary>
            /// <param name="response">The HTTP response to return.</param>
            /// <param name="fetchDelay">The delay to simulate when fetching this response.</param>
            public ResponseEntry(Response response, TimeSpan fetchDelay)
            {
                Response = response;
                FetchDelay = fetchDelay;
            }

            /// <summary>
            /// Gets the HTTP response.
            /// </summary>
            public Response Response { get; }

            /// <summary>
            /// Gets the delay to simulate when fetching this response.
            /// </summary>
            public TimeSpan FetchDelay { get; }
        }

        private readonly Dictionary<string, ResponseEntry> _responses = new();
        private readonly TimeSpan _defaultFetchDelay = TimeSpan.FromSeconds(0);
        private readonly TimeSpan _notFoundDelay = TimeSpan.FromSeconds(0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Web"/> class with the provided responses and delay settings.
        /// </summary>
        /// <param name="responses">Dictionary mapping URLs to response entries.</param>
        /// <param name="defaultFetchDelay">Default delay for responses without a specific delay.</param>
        /// <param name="notFoundDelay">Delay for URLs that aren't found in the collection.</param>
        private Web(Dictionary<string, ResponseEntry> responses, TimeSpan defaultFetchDelay, TimeSpan notFoundDelay)
        {
            _responses = responses;
            _defaultFetchDelay = defaultFetchDelay;
            _notFoundDelay = notFoundDelay;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Web"/> class with the specified delay settings.
        /// </summary>
        /// <param name="fetchDelay">Default delay for responses without a specific delay.</param>
        /// <param name="notFoundDelay">Delay for URLs that aren't found in the collection.</param>
        public Web(TimeSpan fetchDelay, TimeSpan notFoundDelay)
        {
            _defaultFetchDelay = fetchDelay;
            _notFoundDelay = notFoundDelay;
        }

        /// <summary>
        /// Adds a custom response to the web for a specific URL.
        /// </summary>
        /// <param name="url">The URL that will return this response.</param>
        /// <param name="response">The response to return when the URL is fetched.</param>
        /// <param name="fetchDelay">Optional custom delay when fetching this URL.</param>
        /// <returns>A new Web instance with the added response.</returns>
        /// <exception cref="ArgumentNullException">Thrown if url or response is null.</exception>
        public Web AddResponse(string url, Response response, TimeSpan? fetchDelay = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (response == null)
                throw new ArgumentNullException(nameof(response));
                
            var entry = new ResponseEntry(response, fetchDelay ?? _defaultFetchDelay);
            
            // Create a new dictionary and copy existing entries
            var newResponses = new Dictionary<string, ResponseEntry>();
            foreach (var kvp in _responses)
            {
                newResponses.Add(kvp.Key, kvp.Value);
            }
            
            // Add or update the new entry
            if (newResponses.ContainsKey(url))
            {
                newResponses[url] = entry;
            }
            else
            {
                newResponses.Add(url, entry);
            }
            
            return new Web(newResponses, _defaultFetchDelay, _notFoundDelay);
        }
        
        /// <summary>
        /// Adds a page to the web with the specified title and links.
        /// </summary>
        /// <param name="url">The URL that will return this page.</param>
        /// <param name="title">The title of the page.</param>
        /// <param name="links">Collection of links to include in the page.</param>
        /// <param name="fetchDelay">Optional custom delay when fetching this page.</param>
        /// <returns>A new Web instance with the added page.</returns>
        public Web AddPage(string url, string title, IReadOnlyCollection<string> links, TimeSpan? fetchDelay = null)
        {
            // Create response using ResponseBuilder
            var responseBuilder = ResponseBuilder.Create()
                .WithTitle(title);
                
            // Add each link
            foreach (var link in links)
            {
                responseBuilder.WithLink(link, link);
            }
            
            var response = responseBuilder.BuildHtml();
            
            // Use the AddResponse method to add the response
            return AddResponse(url, response, fetchDelay);
        }
        
        /// <summary>
        /// Fetches a response from the web by URL.
        /// </summary>
        /// <param name="url">The URL to fetch.</param>
        /// <returns>A task that resolves to the response, or null if not found.</returns>
        public Task<Response?> Fetch(string url)
        {
            if (_responses.TryGetValue(url, out var entry))
            {
                // Make sure delay is the full amount or slightly more, never less
                return Task.Delay(entry.FetchDelay)
                    .ContinueWith<Response?>(_ => entry.Response);
            }
            return Task.Delay(_notFoundDelay)
                .ContinueWith<Response?>(_ => null);
        }
    }
}
