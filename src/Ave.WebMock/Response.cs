using System;

namespace Ave.WebMock
{
    public record Response
    {
        public Response(string content, string contentType, int statusCode = 200)
        {
            Content = content;
            ContentType = contentType;
            StatusCode = statusCode;
        }

        public string Content { get; }
        public string ContentType { get; }
        public int StatusCode { get; }

        /// <summary>
        /// Indicates if this response represents a successful HTTP request (status code 200-299)
        /// </summary>
        public bool IsSuccess => StatusCode >= 200 && StatusCode <= 299;
    }
}