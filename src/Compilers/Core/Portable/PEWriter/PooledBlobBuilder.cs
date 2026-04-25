// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.Cci
{
    internal sealed class PooledBlobBuilder : BlobBuilder, IDisposable
    {
        private const int PoolSize = 128;
        private const int PoolChunkSize = 8000;

        private static readonly ObjectPool<PooledBlobBuilder> s_chunkPool = new ObjectPool<PooledBlobBuilder>(() => new PooledBlobBuilder(), PoolSize);

        private PooledBlobBuilder()
            : base([], PoolChunkSize)
        {
        }

        /// <summary>
        /// Get a new instance of the <see cref="BlobBuilder"/> that has <see cref="BlobBuilder.ChunkCapacity"/> of
        /// at least <see cref="PoolChunkSize"/>
        /// </summary>
        public static PooledBlobBuilder GetInstance(int minimalSize = 0)
        {
            var builder = s_chunkPool.Allocate();
            builder.Buffer = ArrayPool<byte>.Shared.Rent(minimalSize <= 0 ? 256 : minimalSize);
            return builder;
        }

        protected override BlobBuilder AllocateChunk(int minimalSize)
        {
            var builder = s_chunkPool.Allocate();
            builder.Buffer = ArrayPool<byte>.Shared.Rent(minimalSize);
            return builder;
        }

        protected override void FreeChunk()
        {
            ArrayPool<byte>.Shared.Return(Buffer);
            Buffer = [];
            s_chunkPool.Free(this);
        }

        protected override void OnLinking(BlobBuilder other)
        {
            if (other is not PooledBlobBuilder)
            {
                throw new InvalidOperationException("Cannot link with a non-pooled blob builder.");
            }
        }

        public new void Free()
        {
            base.Free();
        }

        void IDisposable.Dispose()
        {
            Free();
        }
    }
}
