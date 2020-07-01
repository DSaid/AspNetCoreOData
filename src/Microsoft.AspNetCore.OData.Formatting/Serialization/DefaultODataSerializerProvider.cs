// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatting.Serialization
{
    /// <summary>
    /// The default implementation of <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        private readonly IServiceProvider _rootContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataSerializerProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The root container.</param>
        public DefaultODataSerializerProvider(IServiceProvider serviceProvider)
        {
            _rootContainer = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Enum:
                    return _rootContainer.GetRequiredService<ODataEnumSerializer>();

                //case EdmTypeKind.Primitive:
                //    return _rootContainer.GetRequiredService<ODataPrimitiveSerializer>();

                //case EdmTypeKind.Collection:
                //    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                //    if (collectionType.Definition.IsDeltaFeed())
                //    {
                //        return _rootContainer.GetRequiredService<ODataDeltaFeedSerializer>();
                //    }
                //    else if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                //    {
                //        return _rootContainer.GetRequiredService<ODataResourceSetSerializer>();
                //    }
                //    else
                //    {
                //        return _rootContainer.GetRequiredService<ODataCollectionSerializer>();
                //    }

                //case EdmTypeKind.Complex:
                //case EdmTypeKind.Entity:
                //    return _rootContainer.GetRequiredService<ODataResourceSerializer>();

                default:
                    return null;
            }
        }

        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
        {
            // Using a Func<IEdmModel> to delay evaluation of the model.
            return GetODataPayloadSerializerImpl(type, () => request.GetModel(), request.ODataFeature().Path, typeof(SerializableError));
        }

        /// <inheritdoc />
        internal ODataSerializer GetODataPayloadSerializerImpl(Type type, Func<IEdmModel> modelFunction, ODataPath path, Type errorType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (modelFunction == null)
            {
                throw Error.ArgumentNull("modelFunction");
            }

            // handle the special types.
            if (type == typeof(ODataServiceDocument))
            {
                return _rootContainer.GetRequiredService<ODataServiceDocumentSerializer>();
            }
            else if (type == typeof(Uri) || type == typeof(ODataEntityReferenceLink))
            {
                return _rootContainer.GetRequiredService<ODataEntityReferenceLinkSerializer>();
            }
            else if (TypeHelper.IsTypeAssignableFrom(typeof(IEnumerable<Uri>), type) || type == typeof(ODataEntityReferenceLinks))
            {
                return _rootContainer.GetRequiredService<ODataEntityReferenceLinksSerializer>();
            }
            else if (type == typeof(ODataError) || type == errorType)
            {
                return _rootContainer.GetRequiredService<ODataErrorSerializer>();
            }
            else if (TypeHelper.IsTypeAssignableFrom(typeof(IEdmModel), type))
            {
                return _rootContainer.GetRequiredService<ODataMetadataSerializer>();
            }

            // Get the model. Using a Func<IEdmModel> to delay evaluation of the model
            // until after the above checks have passed.
            IEdmModel model = modelFunction();

            // if it is not a special type, assume it has a corresponding EdmType.
            //ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            // IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);
            IEdmTypeReference edmType = null;

            if (edmType != null)
            {
                bool isCountRequest = path != null && path.LastSegment is CountSegment;
                bool isRawValueRequest = path != null && path.LastSegment is ValueSegment;

                if (((edmType.IsPrimitive() || edmType.IsEnum()) && isRawValueRequest) || isCountRequest)
                {
                    return _rootContainer.GetRequiredService<ODataRawValueSerializer>();
                }
                else
                {
                    return GetEdmTypeSerializer(edmType);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
