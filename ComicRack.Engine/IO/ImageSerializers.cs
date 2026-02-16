using System;
using System.Buffers;
using System.IO;
using Microsoft.Extensions.Caching.Hybrid;
using cYo.Common.IO;

namespace cYo.Projects.ComicRack.Engine.IO
{
    // Serializer for PageImage
    public class PageImageSerializer : IHybridCacheSerializer<PageImage>
    {
        public PageImage Deserialize(ReadOnlySequence<byte> source)
        {
            // Convert ReadOnlySequence to byte[]
            // Optimization: If single segment, use directly. If multi, copy.
            byte[] data = source.ToArray();
            return PageImage.CreateFrom(data);
        }

        public void Serialize(PageImage value, IBufferWriter<byte> target)
        {
            if (value == null) return;
            byte[] data = value.Data; // Get underlying jpeg bytes
            if (data != null)
            {
                target.Write(data);
            }
        }
    }

    // Serializer for ThumbnailImage
    public class ThumbnailImageSerializer : IHybridCacheSerializer<ThumbnailImage>
    {
        public ThumbnailImage Deserialize(ReadOnlySequence<byte> source)
        {
            byte[] data = source.ToArray();
            // ThumbnailImage.CreateFrom(Stream) does the binary reading
            using (var ms = new MemoryStream(data))
            {
                return ThumbnailImage.CreateFrom(ms);
            }
        }

        public void Serialize(ThumbnailImage value, IBufferWriter<byte> target)
        {
            if (value == null) return;
            // ThumbnailImage.Save writes custom binary headers + data
            // We need to write to the buffer. 
            // Since Save takes a Stream, implementation implies efficient writing.
            // But we have an IBufferWriter.
            // We can wrap a temporary stream or simple MemoryStream/Array if specialized support missing.
            // For simplicity and correctness with existing API:
            byte[] data = value.ToBytes();
            target.Write(data);
        }
    }
}
