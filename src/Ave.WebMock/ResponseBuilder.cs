using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Ave.WebMock
{
    /// <summary>
    /// Provides a fluent builder pattern for creating HTTP responses with HTML or JSON content.
    /// </summary>
    public class ResponseBuilder
    {
        private string _title = string.Empty;
        private readonly List<(string Url, string Text)> _links = new();
        private int _statusCode = 200;

        /// <summary>
        /// Sets the title for the HTML page response.
        /// </summary>
        /// <param name="title">The title to set for the page. If null, an empty string will be used.</param>
        /// <returns>The current <see cref="ResponseBuilder"/> instance for method chaining.</returns>
        public ResponseBuilder WithTitle(string title)
        {
            _title = title ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Adds a link to the HTML page response.
        /// </summary>
        /// <param name="url">The URL for the link's href attribute.</param>
        /// <param name="text">The visible text content of the link.</param>
        /// <returns>The current <see cref="ResponseBuilder"/> instance for method chaining.</returns>
        public ResponseBuilder WithLink(string url, string text)
        {
            _links.Add((url, text));
            return this;
        }

        /// <summary>
        /// Adds multiple links to the HTML page response.
        /// </summary>
        /// <param name="links">Collection of URL and text pairs to add as links.</param>
        /// <returns>The current <see cref="ResponseBuilder"/> instance for method chaining.</returns>
        public ResponseBuilder WithLinks(IEnumerable<(string Url, string Text)> links)
        {
            if (links != null)
            {
                _links.AddRange(links);
            }
            return this;
        }

        /// <summary>
        /// Sets the HTTP status code for the response.
        /// </summary>
        /// <param name="statusCode">The HTTP status code (e.g., 200, 404, 500).</param>
        /// <returns>The current <see cref="ResponseBuilder"/> instance for method chaining.</returns>
        public ResponseBuilder WithStatusCode(int statusCode)
        {
            _statusCode = statusCode;
            return this;
        }

        /// <summary>
        /// Builds an HTML response with the configured properties.
        /// </summary>
        /// <returns>A <see cref="Response"/> object containing HTML content.</returns>
        public Response BuildHtml()
        {
            var htmlBuilder = new StringBuilder();
            
            // Start HTML document
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            
            // Head section with title
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine($"    <title>{_title}</title>");
            htmlBuilder.AppendLine("    <meta charset=\"UTF-8\">");
            htmlBuilder.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            htmlBuilder.AppendLine("</head>");
            
            // Body section
            htmlBuilder.AppendLine("<body>");
            
            // Add page title as H1
            if (!string.IsNullOrEmpty(_title))
            {
                htmlBuilder.AppendLine($"    <h1>{_title}</h1>");
            }
            
            // Add status code information for error pages
            if (_statusCode >= 400)
            {
                htmlBuilder.AppendLine($"    <div class=\"error\">Error {_statusCode}</div>");
            }
            
            // Add links section if there are any links
            if (_links.Count > 0)
            {
                htmlBuilder.AppendLine("    <nav>");
                htmlBuilder.AppendLine("        <ul>");
                
                foreach (var (url, text) in _links)
                {
                    htmlBuilder.AppendLine($"            <li><a href=\"{url}\">{text}</a></li>");
                }
                
                htmlBuilder.AppendLine("        </ul>");
                htmlBuilder.AppendLine("    </nav>");
            }
            
            // Close tags
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");
            
            // Return the response with HTML content and appropriate content type
            return new Response(htmlBuilder.ToString(), "text/html", _statusCode);
        }

        /// <summary>
        /// Builds a JSON response with the provided content object.
        /// </summary>
        /// <param name="content">The object to serialize as JSON in the response.</param>
        /// <returns>A <see cref="Response"/> object containing JSON content.</returns>
        public Response BuildJson(object content)
        {
            string jsonContent = JsonSerializer.Serialize(content, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            return new Response(jsonContent, "application/json", _statusCode);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ResponseBuilder"/> class.
        /// </summary>
        /// <returns>A new <see cref="ResponseBuilder"/> instance.</returns>
        public static ResponseBuilder Create()
        {
            return new ResponseBuilder();
        }
    }
}