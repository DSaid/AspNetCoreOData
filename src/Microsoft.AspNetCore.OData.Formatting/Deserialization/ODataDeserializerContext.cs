// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatting.Deserialization
{
    /// <summary>
    /// This class encapsulates the state and settings that get passed to <see cref="ODataDeserializer"/>.
    /// </summary>
    public class ODataDeserializerContext
    {
        /// <summary>
        /// Gets or sets the type of the top-level object the request needs to be deserialized into.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmTypeReference"/> of the top-level object the request needs to be deserialized into.
        /// </summary>
        public IEdmTypeReference ResourceEdmType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataPath"/> of the request.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
        /// </summary>
        public HttpRequest Request { get; set; }
    }
}
