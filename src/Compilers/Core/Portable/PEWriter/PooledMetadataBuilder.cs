// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection.Metadata.Ecma335;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.Cci
{
    internal static class PooledMetadataBuilder
    {
        private static readonly ObjectPool<MetadataBuilder> s_objectPool =
            new(() => new MetadataBuilder(createBlobBuilderFunc: PooledBlobBuilder.GetInstance));

        public static MetadataBuilder GetInstance()
        {
            return s_objectPool.Allocate();
        }

        public static MetadataBuilder GetInstance(int userStringHeapStartOffset, int stringHeapStartOffset, int blobHeapStartOffset, int guidHeapStartOffset)
        {
            var builder = s_objectPool.Allocate();
            builder.Clear(userStringHeapStartOffset, stringHeapStartOffset, blobHeapStartOffset, guidHeapStartOffset);
            return builder;
        }

        public static void Free(MetadataBuilder builder)
        {
            builder.Clear();
            s_objectPool.Free(builder);
        }
    }
}
