using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace OneNoteMindMap.Core.Serialization
{
    public static class MindMapPayloadCodec
    {
        private const string Marker = "ONENOTE_MINDMAP_DATA_V1:";

        public static string Encode(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            using var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionLevel.Optimal))
                gzip.Write(bytes, 0, bytes.Length);
            return Marker + Convert.ToBase64String(ms.ToArray());
        }

        public static string Decode(string payload)
        {
            if (payload == null || !payload.StartsWith(Marker))
                return null;
            var base64 = payload.Substring(Marker.Length);
            var compressed = Convert.FromBase64String(base64);
            using var ms = new MemoryStream(compressed);
            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public static bool HasMindMapData(string pageText)
        {
            return pageText != null && pageText.Contains(Marker);
        }

        public static string ExtractPayload(string pageText)
        {
            if (string.IsNullOrEmpty(pageText)) return null;
            int start = pageText.IndexOf(Marker);
            if (start < 0) return null;
            return pageText.Substring(start);
        }
    }
}
