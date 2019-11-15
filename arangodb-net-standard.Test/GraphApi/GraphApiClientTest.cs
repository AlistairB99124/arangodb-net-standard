﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using ArangoDBNetStandard.GraphApi;
using System.Collections.Generic;
using ArangoDBNetStandard;
using ArangoDBNetStandard.CollectionApi;

namespace ArangoDBNetStandardTest.GraphApi
{
    public class GraphApiClientTest : IClassFixture<GraphApiClientTestFixture>
    {
        private readonly GraphApiClientTestFixture _fixture;
        private readonly GraphApiClient _client;

        public GraphApiClientTest(GraphApiClientTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.ArangoDBClient.Graph;
        }

        [Fact]
        public async Task GetGraphsAsync_ShouldSucceed()
        {
            // get the list of graphs
            var graphsResult = await _fixture.ArangoDBClient.Graph.GetGraphs();

            // test result
            Assert.Equal(HttpStatusCode.OK, graphsResult.Code);
            Assert.NotEmpty(graphsResult.Graphs);

            var graph = graphsResult.Graphs.First(x => x._key == _fixture.TestGraph);
            Assert.Single(graph.EdgeDefinitions);
            Assert.Empty(graph.OrphanCollections);
            Assert.Equal(1, graph.NumberOfShards);
            Assert.Equal(1, graph.ReplicationFactor);
            Assert.False(graph.IsSmart);
            Assert.Equal(_fixture.TestGraph, graph._key);
            Assert.Equal("_graphs/" + _fixture.TestGraph, graph._id);
            Assert.NotNull(graph._rev);
        }

        [Fact]
        public async Task DeleteGraphAsync_ShouldSucceed()
        {
            await _fixture.ArangoDBClient.Graph.PostGraph(new PostGraphBody
            {
                Name = "temp_graph",
                EdgeDefinitions = new List<EdgeDefinition>
                {
                    new EdgeDefinition
                    {
                        From = new string[] { "fromclx" },
                        To = new string[] { "toclx" },
                        Collection = "clx"
                    }
                }
            });
            var query = new DeleteGraphQuery
            {
                DropCollections = false
            };
            var response = await _client.DeleteGraphAsync("temp_graph", query);
            Assert.Equal(HttpStatusCode.Accepted, response.Code);
            Assert.True(response.Removed);
            Assert.False(response.Error);
        }

        [Fact]
        public async Task DeleteGraphAsync_ShouldThrow_WhenNotFound()
        {
            var exception = await Assert.ThrowsAsync<ApiErrorException>(async () =>
            {
                await _client.DeleteGraphAsync("boggus_graph", new DeleteGraphQuery
                {
                    DropCollections = false
                });
            });

            Assert.Equal(HttpStatusCode.NotFound, exception.ApiError.Code);
            Assert.Equal(1924, exception.ApiError.ErrorNum); // GRAPH_NOT_FOUND
        }

        [Fact]
        public async Task GetGraphAsync_ShouldSucceed()
        {
            var response = await _client.GetGraphAsync(_fixture.TestGraph);
            Assert.Equal(HttpStatusCode.OK, response.Code);
            Assert.Equal("_graphs/" + _fixture.TestGraph, response.Graph._id);
            Assert.Single(response.Graph.EdgeDefinitions);
            Assert.Empty(response.Graph.OrphanCollections);
            Assert.Equal(1, response.Graph.NumberOfShards);
            Assert.Equal(1, response.Graph.ReplicationFactor);
            Assert.False(response.Graph.IsSmart);
            Assert.Equal(_fixture.TestGraph, response.Graph._key);
            Assert.Equal("_graphs/" + _fixture.TestGraph, response.Graph._id);
            Assert.NotNull(response.Graph._rev);
        }

        [Fact]
        public async Task GetGraphAsync_ShouldThrow_WhenNotFound()
        {
            var exception = await Assert.ThrowsAsync<ApiErrorException>(async () =>
            {
                await _client.GetGraphAsync("bogus_graph");
            });
            Assert.Equal(HttpStatusCode.NotFound, exception.ApiError.Code);
            Assert.Equal(1924, exception.ApiError.ErrorNum); // GRAPH_NOT_FOUND
        }

        [Fact]
        public async Task GetVertexCollectionsAsync_ShouldSucceed()
        {
            // Create an edge collection

            string edgeClx = nameof(GetGraphsAsync_ShouldSucceed) + "_EdgeClx";

            var createClxResponse = await _fixture.ArangoDBClient.Collection.PostCollectionAsync(
                new PostCollectionBody()
                {
                    Name = edgeClx,
                    Type = 3
                });

            Assert.Equal(edgeClx, createClxResponse.Name);

            // Create a Graph

            string graphName = nameof(GetVertexCollectionsAsync_ShouldSucceed);

            PostGraphResponse createGraphResponse = await _client.PostGraph(new PostGraphBody()
            {
                Name = graphName,
                EdgeDefinitions = new List<EdgeDefinition>()
                {
                    new EdgeDefinition()
                    {
                        Collection = edgeClx,
                        From = new string[] { "FromCollection" },
                        To = new string[] { "ToCollection" }
                    }
                }
            });

            // List the vertex collections

            GetVertexCollectionsResponse response = await _client.GetVertexCollections(graphName);

            Assert.Equal(2, response.Collections.Count());
            Assert.Contains("FromCollection", response.Collections);
            Assert.Contains("ToCollection", response.Collections);
        }

        [Fact]
        public async Task GetVertexCollectionsAsync_ShouldThrow_WhenGraphDoesNotExist()
        {
            var ex = await Assert.ThrowsAsync<ApiErrorException>(async () =>
            {
                await _client.GetVertexCollections("GraphThatDoesNotExist");
            });

            ApiErrorResponse apiError = ex.ApiError;

            Assert.Equal(HttpStatusCode.NotFound, apiError.Code);
            Assert.Equal(1924, apiError.ErrorNum);
        }

        // PostVertexCollectionAsync 
        [Fact]
        public async Task PostVertexCollectionAsync_ShouldSucceed()
        {
            var edge = nameof(PostVertexCollectionAsync_ShouldSucceed) + "_EdgeClx";
            var graph = nameof(PostVertexCollectionAsync_ShouldSucceed) + "_GraphClx";

            // Create edge collection
            var createClxResponse = await _fixture.ArangoDBClient.Collection.PostCollectionAsync(
                new PostCollectionBody()
                {
                    Name = edge,
                    Type = 3
                });

            // Create GRaph
            PostGraphResponse createGraphResponse = await _client.PostGraph(new PostGraphBody()
            {
                Name = graph,
                EdgeDefinitions = new List<EdgeDefinition>()
                {
                    new EdgeDefinition()
                    {
                        Collection = edge,
                        From = new string[] { "FromCollection" },
                        To = new string[] { "ToCollection" }
                    }
                }
            });

            var query = new PostVertexCollectionQuery
            {
                WaitForSync = false,
                ReturnNew = true
            };

            var response = await _client.PostVertexCollectionAsync(graph, edge, query);

            Assert.Equal(HttpStatusCode.OK, response.Code);
        }
    }
}
