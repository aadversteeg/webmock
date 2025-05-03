using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Ave.WebMock
{
    public class ResponseBuilder
    {
        private string _title = string.Empty;
        private readonly List<(string Url, string Text)> _links = new();
        private int _statusCode = 200;

        public ResponseBuilder WithTitle(string title)
        {
            _title = title ?? string.Empty;
            return this;
        }

        public ResponseBuilder WithLink(string url, string text)
        {
            _links.Add((url, text));
            return this;
        }

        public ResponseBuilder WithLinks(IEnumerable<(string Url, string Text)> links)
        {
            if (links != null)
            {
                _links.AddRange(links);
            }
            return this;
        }

        public ResponseBuilder WithStatusCode(int statusCode)
        {
            _statusCode = statusCode;
            return this;
        }

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

        public Response BuildJson(object content)
        {
            string jsonContent = JsonSerializer.Serialize(content, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            return new Response(jsonContent, "application/json", _statusCode);
        }

        public static ResponseBuilder Create()
        {
            return new ResponseBuilder();
        }
    }
}