using System;
using System.Collections.Generic;

namespace Ave.WebMock
{
    /// <summary>
    /// Provides a fluent builder pattern for creating mock web environments.
    /// Used to configure and construct <see cref="Web"/> instances for testing.
    /// </summary>
    public class WebBuilder
    {
        private TimeSpan _defaultFetchDelay = TimeSpan.FromMilliseconds(100);
        private TimeSpan _notFoundDelay = TimeSpan.FromMilliseconds(50);
        private readonly List<WebsiteContext> _websites = new();
        private readonly Dictionary<int, Response> _statusCodeResponses = new();

        /// <summary>
        /// Prevents external instantiation. Use <see cref="Create"/> instead.
        /// </summary>
        private WebBuilder()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WebBuilder"/> class.
        /// </summary>
        /// <returns>A new <see cref="WebBuilder"/> instance for fluent configuration.</returns>
        public static WebBuilder Create()
        {
            return new WebBuilder();
        }

        /// <summary>
        /// Sets the default delay for all responses that don't have a custom delay.
        /// </summary>
        /// <param name="delay">The delay to apply to responses by default.</param>
        /// <returns>The <see cref="WebBuilder"/> instance for method chaining.</returns>
        public WebBuilder WithDefaultFetchDelay(TimeSpan delay)
        {
            _defaultFetchDelay = delay;
            return this;
        }

        /// <summary>
        /// Sets the delay for requests to URLs that don't exist in the mock web.
        /// </summary>
        /// <param name="delay">The delay to apply to 'not found' responses.</param>
        /// <returns>The <see cref="WebBuilder"/> instance for method chaining.</returns>
        public WebBuilder WithNotFoundDelay(TimeSpan delay)
        {
            _notFoundDelay = delay;
            return this;
        }

        /// <summary>
        /// Adds a global status code response template that will be used when a specific 
        /// status code is returned but no custom content has been provided.
        /// </summary>
        /// <param name="statusCode">The HTTP status code (e.g., 404, 500)</param>
        /// <param name="configureResponse">Action to configure the response</param>
        /// <returns>The WebBuilder instance for method chaining</returns>
        public WebBuilder WithStatusCodeResponse(int statusCode, Action<ResponseBuilder> configureResponse)
        {
            var responseBuilder = ResponseBuilder.Create()
                .WithStatusCode(statusCode)
                .WithTitle($"Error {statusCode}");
                
            configureResponse?.Invoke(responseBuilder);
            
            _statusCodeResponses[statusCode] = responseBuilder.BuildHtml();
            return this;
        }
        
        /// <summary>
        /// Adds a custom response to the web without requiring a website context.
        /// Useful for direct API mocking.
        /// </summary>
        /// <param name="url">The URL that will return this response</param>
        /// <param name="response">The response to return</param>
        /// <param name="fetchDelay">Optional custom delay for this response</param>
        /// <returns>The WebBuilder instance for method chaining</returns>
        public WebBuilder WithCustomResponse(string url, Response response, TimeSpan? fetchDelay = null)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
                
            // Create an empty website context just to hold the response
            var customContext = new WebsiteContext(this, "");
            customContext.WithCustomResponse(url, response, fetchDelay);
            _websites.Add(customContext);
            
            return this;
        }

        /// <summary>
        /// Adds a website with the specified root URL to the builder.
        /// </summary>
        /// <param name="rootUrl">The root URL of the website (e.g., "https://example.com").</param>
        /// <returns>A <see cref="WebsiteContext"/> for configuring the website.</returns>
        public WebsiteContext WithWebsite(string rootUrl)
        {
            var websiteContext = new WebsiteContext(this, rootUrl);
            _websites.Add(websiteContext);
            return websiteContext;
        }

        /// <summary>
        /// Builds a <see cref="Web"/> instance with all the configured websites and responses.
        /// </summary>
        /// <returns>A fully constructed <see cref="Web"/> instance ready for testing.</returns>
        public Web Build()
        {
            var web = new Web(_defaultFetchDelay, _notFoundDelay);

            // Add status code responses first
            foreach (var kvp in _statusCodeResponses)
            {
                int statusCode = kvp.Key;
                Response response = kvp.Value;
                web = web.AddResponse($"_status_{statusCode}", response);
            }

            // Then build all websites
            foreach (var website in _websites)
            {
                web = website.BuildPages(web);
            }

            return web;
        }

        /// <summary>
        /// Represents a website context within the <see cref="WebBuilder"/>.
        /// Provides methods to configure a specific website with pages, links, and responses.
        /// </summary>
        public class WebsiteContext
        {
            private readonly WebBuilder _webBuilder;
            private readonly string _rootUrl;
            private string _title = "Website";
            private int[] _levels = Array.Empty<int>();
            private bool _includeParentLink = false;
            private bool _includeRootLink = false;
            private readonly Dictionary<string, PageConfiguration> _pageConfigurations = new();
            private readonly Dictionary<string, (int StatusCode, string Title, TimeSpan? FetchDelay)> _errorPages = new();
            private readonly Dictionary<string, (Response Response, TimeSpan? FetchDelay)> _customResponses = new();

            /// <summary>
            /// Initializes a new instance of the <see cref="WebsiteContext"/> class.
            /// </summary>
            /// <param name="webBuilder">The parent web builder instance.</param>
            /// <param name="rootUrl">The root URL of the website.</param>
            internal WebsiteContext(WebBuilder webBuilder, string rootUrl)
            {
                _webBuilder = webBuilder;
                _rootUrl = rootUrl;
            }

            /// <summary>
            /// Sets the title for the website.
            /// </summary>
            /// <param name="title">The title to set for the website.</param>
            /// <returns>The <see cref="WebsiteContext"/> instance for method chaining.</returns>
            public WebsiteContext WithTitle(string title)
            {
                _title = title;
                return this;
            }

            /// <summary>
            /// Configures multiple levels of subpages for the website.
            /// </summary>
            /// <param name="levels">An array where each value represents the number of pages at that level.</param>
            /// <returns>The <see cref="WebsiteContext"/> instance for method chaining.</returns>
            /// <remarks>
            /// For example, passing [3, 2] would create a website with 3 pages at the first level, 
            /// each with 2 subpages at the second level.
            /// </remarks>
            public WebsiteContext WithSubLevels(params int[] levels)
            {
                _levels = levels;
                return this;
            }

            /// <summary>
            /// Configures a single level of subpages for the website.
            /// </summary>
            /// <param name="numberOfPages">The number of pages to create at this level.</param>
            /// <param name="includeParentLink">Whether each page should link back to its parent page.</param>
            /// <param name="includeRootLink">Whether each page should link back to the root page.</param>
            /// <returns>The <see cref="WebsiteContext"/> instance for method chaining.</returns>
            public WebsiteContext WithSubLevel(int numberOfPages, bool includeParentLink = false, bool includeRootLink = false)
            {
                _levels = new int[] { numberOfPages };
                _includeParentLink = includeParentLink;
                _includeRootLink = includeRootLink;
                return this;
            }

            /// <summary>
            /// Configures a specific page within the website.
            /// </summary>
            /// <param name="pageUrl">The relative or absolute URL of the page to configure.</param>
            /// <param name="configureAction">Action to configure the page properties.</param>
            /// <returns>The <see cref="WebsiteContext"/> instance for method chaining.</returns>
            /// <remarks>
            /// If the pageUrl is relative (doesn't start with the root URL), it will be automatically
            /// prefixed with the root URL of this website.
            /// </remarks>
            public WebsiteContext ConfigurePage(string pageUrl, Action<PageConfiguration> configureAction)
            {
                var configuration = new PageConfiguration();
                configureAction(configuration);
                
                if (!pageUrl.StartsWith(_rootUrl))
                {
                    pageUrl = _rootUrl + (pageUrl.StartsWith("/") ? "" : "/") + pageUrl;
                }

                _pageConfigurations[pageUrl] = configuration;
                return this;
            }

            /// <summary>
            /// Configures an error page for a specific URL with a custom status code.
            /// </summary>
            /// <param name="pageUrl">The URL that will return the error</param>
            /// <param name="statusCode">The HTTP status code to return</param>
            /// <param name="title">Optional title for the error page</param>
            /// <param name="fetchDelay">Optional custom delay for this error response</param>
            /// <returns>The WebsiteContext instance for method chaining</returns>
            public WebsiteContext WithErrorPage(string pageUrl, int statusCode, string? title = null, TimeSpan? fetchDelay = null)
            {
                if (!pageUrl.StartsWith(_rootUrl))
                {
                    pageUrl = _rootUrl + (pageUrl.StartsWith("/") ? "" : "/") + pageUrl;
                }

                _errorPages[pageUrl] = (statusCode, title ?? $"Error {statusCode}", fetchDelay);
                return this;
            }
            
            /// <summary>
            /// Adds a custom response for a specific URL.
            /// </summary>
            /// <param name="url">The URL that will return this response</param>
            /// <param name="response">The response to return</param>
            /// <param name="fetchDelay">Optional custom delay for this response</param>
            /// <returns>The WebsiteContext instance for method chaining</returns>
            public WebsiteContext WithCustomResponse(string url, Response response, TimeSpan? fetchDelay = null)
            {
                if (!string.IsNullOrEmpty(_rootUrl) && !url.StartsWith(_rootUrl) && !url.StartsWith("http"))
                {
                    url = _rootUrl + (url.StartsWith("/") ? "" : "/") + url;
                }
                
                _customResponses[url] = (response, fetchDelay);
                return this;
            }
            
            /// <summary>
            /// Adds a custom API response with JSON content for a specific URL.
            /// </summary>
            /// <param name="url">The URL that will return this response</param>
            /// <param name="responseData">The data to serialize to JSON</param>
            /// <param name="statusCode">The HTTP status code to return (defaults to 200)</param>
            /// <param name="fetchDelay">Optional custom delay for this response</param>
            /// <returns>The WebsiteContext instance for method chaining</returns>
            public WebsiteContext WithJsonResponse(string url, object responseData, int statusCode = 200, TimeSpan? fetchDelay = null)
            {
                if (!string.IsNullOrEmpty(_rootUrl) && !url.StartsWith(_rootUrl))
                {
                    url = _rootUrl + (url.StartsWith("/") ? "" : "/") + url;
                }
                
                var responseBuilder = ResponseBuilder.Create()
                    .WithStatusCode(statusCode);
                
                var response = responseBuilder.BuildJson(responseData);
                _customResponses[url] = (response, fetchDelay);
                
                return this;
            }

            /// <summary>
            /// Builds a <see cref="Web"/> instance with all the configured websites and pages.
            /// </summary>
            /// <returns>A fully constructed <see cref="Web"/> instance.</returns>
            public Web Build()
            {
                return _webBuilder.Build();
            }

            /// <summary>
            /// Internal method that builds the pages for this website and adds them to the given web instance.
            /// </summary>
            /// <param name="web">The web instance to add pages to.</param>
            /// <returns>The updated web instance with the website's pages added.</returns>
            internal Web BuildPages(Web web)
            {
                // Add the root page first
                var rootLinks = new List<string>();
                
                // Generate the first level of links if there are any levels
                if (_levels.Length > 0 && _levels[0] > 0)
                {
                    for (int i = 1; i <= _levels[0]; i++)
                    {
                        rootLinks.Add($"{_rootUrl}/page{i}");
                    }
                }
                
                // Add root link to itself if requested (for consistency in link structure)
                if (_includeRootLink)
                {
                    rootLinks.Add(_rootUrl);
                }

                // Add custom links from page configuration if exists
                PageConfiguration? rootConfig = null;
                if (_pageConfigurations.TryGetValue(_rootUrl, out var config))
                {
                    rootConfig = config;
                    rootLinks.AddRange(config.AdditionalLinks);
                }

                // Add the root page
                var rootTitle = rootConfig?.Title ?? _title;
                web = web.AddPage(_rootUrl, rootTitle ?? "Website", rootLinks, rootConfig?.FetchDelay);

                // Add any error pages
                foreach (var kvp in _errorPages)
                {
                    string url = kvp.Key;
                    var errorData = kvp.Value;
                    int statusCode = errorData.StatusCode;
                    string title = errorData.Title;
                    TimeSpan? fetchDelay = errorData.FetchDelay;
                    
                    var responseBuilder = ResponseBuilder.Create()
                        .WithTitle(title)
                        .WithStatusCode(statusCode);
                        
                    if (_includeRootLink)
                    {
                        responseBuilder.WithLink(_rootUrl, "Return to homepage");
                    }
                    
                    var response = responseBuilder.BuildHtml();
                    web = web.AddResponse(url, response, fetchDelay);
                }
                
                // Add any custom responses
                foreach (var kvp in _customResponses)
                {
                    string url = kvp.Key;
                    var customData = kvp.Value;
                    Response response = customData.Response;
                    TimeSpan? fetchDelay = customData.FetchDelay;
                    
                    web = web.AddResponse(url, response, fetchDelay);
                }

                // Build the page tree for each level
                if (_levels.Length > 0)
                {
                    web = BuildLevelPages(web, _rootUrl, _title, 0, new List<int>());
                }

                return web;
            }

            /// <summary>
            /// Recursive helper method to build pages at each level of the website hierarchy.
            /// </summary>
            /// <param name="web">The web instance to add pages to.</param>
            /// <param name="parentUrl">The URL of the parent page.</param>
            /// <param name="parentTitle">The title of the parent page.</param>
            /// <param name="currentLevel">The current level in the hierarchy being built.</param>
            /// <param name="pathIndices">List of indices representing the path to the current page.</param>
            /// <returns>The updated web instance with the level's pages added.</returns>
            private Web BuildLevelPages(Web web, string parentUrl, string parentTitle, int currentLevel, List<int> pathIndices)
            {
                // If we've reached the max depth, return
                if (currentLevel >= _levels.Length)
                    return web;

                // For each page at this level
                for (int i = 1; i <= _levels[currentLevel]; i++)
                {
                    // Create the page URL and title
                    var pagePathIndices = new List<int>(pathIndices) { i };
                    var pageUrl = $"{parentUrl}/page{i}";
                    var pageTitle = $"{_title} [{string.Join(".", pagePathIndices)}]";

                    // Create links for the next level (if any)
                    var pageLinks = new List<string>();
                    
                    // Get custom configuration if exists
                    PageConfiguration? pageConfig = null;
                    bool hasCustomConfig = _pageConfigurations.TryGetValue(pageUrl, out var config);
                    if (hasCustomConfig)
                    {
                        pageConfig = config;
                        
                        // Override title if specified
                        if (!string.IsNullOrEmpty(config?.Title))
                        {
                            pageTitle = config.Title;
                        }
                    }
                    
                    // Add parent link if requested (either globally or per-page)
                    if (_includeParentLink || (hasCustomConfig && pageConfig!.IncludeParentLink))
                    {
                        pageLinks.Add(parentUrl);
                    }
                    
                    // Add root link if requested (either globally or per-page)
                    if ((_includeRootLink || (hasCustomConfig && pageConfig!.IncludeRootLink)) && pageUrl != _rootUrl)
                    {
                        pageLinks.Add(_rootUrl);
                    }

                    // Add previous sibling link if requested in config
                    if (hasCustomConfig && pageConfig!.IncludePreviousSiblingLink && i > 1)
                    {
                        pageLinks.Add($"{parentUrl}/page{i-1}");
                    }
                    
                    // Add next sibling link if requested in config
                    if (hasCustomConfig && pageConfig!.IncludeNextSiblingLink)
                    {
                        int nextIndex = i < _levels[currentLevel] ? i + 1 : 1; // Wrap around to first sibling
                        pageLinks.Add($"{parentUrl}/page{nextIndex}");
                    }
                    
                    // Add links to the next level pages
                    if (currentLevel + 1 < _levels.Length && _levels[currentLevel + 1] > 0)
                    {
                        for (int j = 1; j <= _levels[currentLevel + 1]; j++)
                        {
                            pageLinks.Add($"{pageUrl}/page{j}");
                        }
                    }

                    // Add custom links
                    if (hasCustomConfig)
                    {
                        pageLinks.AddRange(pageConfig!.AdditionalLinks);
                    }

                    // Add this page to the web
                    web = web.AddPage(pageUrl, pageTitle ?? "Page", pageLinks, pageConfig?.FetchDelay);

                    // Recursively build the next level for this page
                    web = BuildLevelPages(web, pageUrl, pageTitle ?? "Page", currentLevel + 1, pagePathIndices);
                }

                return web;
            }
        }

        /// <summary>
        /// Provides configuration options for an individual page within a website.
        /// </summary>
        public class PageConfiguration
        {
            private readonly List<string> _additionalLinks = new();
            
            /// <summary>
            /// Gets the title of the page, if set.
            /// </summary>
            public string? Title { get; private set; }
            
            /// <summary>
            /// Gets the custom fetch delay for this page, if set.
            /// </summary>
            public TimeSpan? FetchDelay { get; private set; }
            
            /// <summary>
            /// Gets the collection of additional links to include on this page.
            /// </summary>
            public IReadOnlyCollection<string> AdditionalLinks => _additionalLinks;
            
            /// <summary>
            /// Gets whether this page should include a link to the root page.
            /// </summary>
            public bool IncludeRootLink { get; private set; }
            
            /// <summary>
            /// Gets whether this page should include a link to its parent page.
            /// </summary>
            public bool IncludeParentLink { get; private set; }
            
            /// <summary>
            /// Gets whether this page should include a link to the previous sibling page.
            /// </summary>
            public bool IncludePreviousSiblingLink { get; private set; }
            
            /// <summary>
            /// Gets whether this page should include a link to the next sibling page.
            /// </summary>
            public bool IncludeNextSiblingLink { get; private set; }

            /// <summary>
            /// Sets the title for this page.
            /// </summary>
            /// <param name="title">The title to set for the page.</param>
            /// <returns>The <see cref="PageConfiguration"/> instance for method chaining.</returns>
            public PageConfiguration WithTitle(string title)
            {
                Title = title;
                return this;
            }

            /// <summary>
            /// Sets a custom fetch delay for this page.
            /// </summary>
            /// <param name="delay">The delay to apply when fetching this page.</param>
            /// <returns>The <see cref="PageConfiguration"/> instance for method chaining.</returns>
            public PageConfiguration WithFetchDelay(TimeSpan delay)
            {
                FetchDelay = delay;
                return this;
            }

            /// <summary>
            /// Adds a link to this page.
            /// </summary>
            /// <param name="url">The URL to link to.</param>
            /// <returns>The <see cref="PageConfiguration"/> instance for method chaining.</returns>
            public PageConfiguration AddLink(string url)
            {
                _additionalLinks.Add(url);
                return this;
            }

            /// <summary>
            /// Adds multiple links to this page.
            /// </summary>
            /// <param name="urls">The URLs to link to.</param>
            /// <returns>The <see cref="PageConfiguration"/> instance for method chaining.</returns>
            public PageConfiguration AddLinks(params string[] urls)
            {
                foreach (var url in urls)
                {
                    _additionalLinks.Add(url);
                }
                return this;
            }
            
            /// <summary>
            /// Configures this page to include a link to the root page of the website.
            /// </summary>
            /// <returns>The <see cref="PageConfiguration"/> instance for method chaining.</returns>
            public PageConfiguration AddRootLink()
            {
                IncludeRootLink = true;
                return this;
            }
            
            /// <summary>
            /// Configures this page to include a link to its parent page.
            /// </summary>
            /// <returns>The <see cref="PageConfiguration"/> instance for method chaining.</returns>
            public PageConfiguration AddParentLink()
            {
                IncludeParentLink = true;
                return this;
            }
            
            /// <summary>
            /// Configures this page to include a link to the previous sibling page.
            /// </summary>
            /// <returns>The <see cref="PageConfiguration"/> instance for method chaining.</returns>
            public PageConfiguration AddPreviousSiblingLink()
            {
                IncludePreviousSiblingLink = true;
                return this;
            }
            
            /// <summary>
            /// Configures this page to include a link to the next sibling page.
            /// </summary>
            /// <returns>The <see cref="PageConfiguration"/> instance for method chaining.</returns>
            public PageConfiguration AddNextSiblingLink()
            {
                IncludeNextSiblingLink = true;
                return this;
            }
        }
    }
}
