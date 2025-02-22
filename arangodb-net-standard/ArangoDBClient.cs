﻿using System;
using System.Net.Http;

using ArangoDBNetStandard.CollectionApi;
using ArangoDBNetStandard.CursorApi;
using ArangoDBNetStandard.DatabaseApi;
using ArangoDBNetStandard.DocumentApi;
using ArangoDBNetStandard.TransactionApi;
using ArangoDBNetStandard.Transport;
using ArangoDBNetStandard.Transport.Http;

namespace ArangoDBNetStandard
{
    /// <summary>
    /// Wrapper class providing access to the complete set of ArangoDB REST resources.
    /// </summary>
    public class ArangoDBClient : IDisposable
    {
        private IApiClientTransport _transport;

        /// <summary>
        /// Cursor API
        /// </summary>
        public CursorApiClient Cursor { get; private set; }

        /// <summary>
        /// Database API
        /// </summary>
        public DatabaseApiClient Database { get; private set; }

        /// <summary>
        /// Document API
        /// </summary>
        public DocumentApiClient Document { get; private set; }

        /// <summary>
        /// Collection API
        /// </summary>
        public CollectionApiClient Collection { get; private set; }

        /// <summary>
        /// Transaction API
        /// </summary>
        public TransactionApiClient Transaction { get; private set; }

        /// <summary>
        /// Create an instance of <see cref="ArangoDBClient"/> from an existing
        /// <see cref="HttpClient"/> instance.
        /// </summary>
        /// <param name="client"></param>
        public ArangoDBClient(HttpClient client)
        {
            _transport = new HttpApiTransport(client);
            Cursor = new CursorApiClient(_transport);
            Database = new DatabaseApiClient(_transport);
            Document = new DocumentApiClient(_transport);
            Collection = new CollectionApiClient(_transport);
            Transaction = new TransactionApiClient(_transport);
            //Graph = new GraphApiClient(client);
        }

        /// <summary>
        /// Create an instance of <see cref="ArangoDBClient"/> from an existing
        /// <see cref="IApiClientTransport"/> instance.
        /// </summary>
        /// <param name="transport">The ArangoDB transport layer implementation.</param>
        public ArangoDBClient(IApiClientTransport transport)
        {
            _transport = transport;
            Cursor = new CursorApiClient(_transport);
            Database = new DatabaseApiClient(_transport);
            Document = new DocumentApiClient(_transport);
            Collection = new CollectionApiClient(_transport);
            Transaction = new TransactionApiClient(_transport);
        }

        /// <summary>
        /// Disposes the underlying transport instance.
        /// </summary>
        public void Dispose()
        {
            _transport.Dispose();
        }
    }
}
