using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ave.WebMock
{
    public static class ResponseExtensions
    {
        /// <summary>
        /// Converts a Response to a Page by extracting the title and links from the HTML content.
        /// </summary>
        /// <param name="response">The response to convert</param>
        /// <returns>A Page object with title and links extracted from the HTML content, or null if the response is null</returns>
        public static Page? ToPage(this Response? response)
        {
            if (response == null)
            {
                return null;
            }

            if (response.ContentType != "text/html")
            {
                throw new ArgumentException("Response must have HTML content type to be converted to a Page", nameof(response));
            }

            // Extract title from HTML
            string title = ExtractTitle(response.Content);
            
            // Extract links from HTML
            var links = ExtractLinks(response.Content);

            return new Page(title, links);
        }
        
        /// <summary>
        /// Converts a Task of Response to a Task of Page by extracting the title and links from the HTML content.
        /// </summary>
        /// <param name="responseTask">The task containing a response to convert</param>
        /// <returns>A task that, when completed, returns a Page object with title and links extracted from the HTML content, 
        /// or null if the response is null</returns>
        public static Task<Page?> ToPage(this Task<Response?> responseTask)
        {
            if (responseTask == null)
            {
                return Task.FromResult<Page?>(null);
            }
            
            return responseTask.ContinueWith(task => task.Result?.ToPage());
        }

        private static string ExtractTitle(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            // Match title between <title> tags
            var titleMatch = Regex.Match(html, @"<title>\s*(.*?)\s*</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            if (titleMatch.Success && titleMatch.Groups.Count > 1)
            {
                return titleMatch.Groups[1].Value;
            }

            // If no title found, try to extract from h1
            var h1Match = Regex.Match(html, @"<h1>\s*(.*?)\s*</h1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            if (h1Match.Success && h1Match.Groups.Count > 1)
            {
                return h1Match.Groups[1].Value;
            }

            return string.Empty;
        }

        private static IReadOnlyCollection<string> ExtractLinks(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return Array.Empty<string>();
            }

            var links = new List<string>();
            var matches = Regex.Matches(html, @"<a\s+[^>]*href\s*=\s*[""']([^""']+)[""'][^>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count > 1)
                {
                    string href = match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        links.Add(href);
                    }
                }
            }

            return links;
        }
    }
}