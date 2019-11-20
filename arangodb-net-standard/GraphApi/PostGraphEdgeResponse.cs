using System.Net;

namespace ArangoDBNetStandard.GraphApi
{
    public class PostGraphEdgeResponse
    {
        public PostEdge Edge { get; set; }

        public HttpStatusCode Code { get; set; }

        public PostEdge New { get; set; }

        public bool? Error { get; set; }
    }
}
