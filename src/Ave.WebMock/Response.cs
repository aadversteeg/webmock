namespace Ave.WebMock
{
    /// <summary>
    /// Represents an HTTP response with content, content type, and status code.
    /// Used to simulate web server responses in tests.
    /// </summary>
    public record Response
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class.
        /// </summary>
        /// <param name="content">The response body content.</param>
        /// <param name="contentType">The MIME content type of the response (e.g., "text/html", "application/json").</param>
        /// <param name="statusCode">The HTTP status code (defaults to 200 OK).</param>
        public Response(string content, string contentType, int statusCode = 200)
        {
            Content = content;
            ContentType = contentType;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the body content of the response.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets the MIME content type of the response (e.g., "text/html", "application/json").
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Indicates if this response represents a successful HTTP request (status code 200-299).
        /// </summary>
        public bool IsSuccess => StatusCode >= 200 && StatusCode <= 299;
    }
}