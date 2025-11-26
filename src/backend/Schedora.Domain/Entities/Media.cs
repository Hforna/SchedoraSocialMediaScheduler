using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class Media : Entity
    {
        public long UserId { get; private set; }
        public string FileName { get; private set; }
        public string? OriginalFileName { get; private set; }
        public long FileSize { get; private set; }
        public string MimeType { get; private set; }
        public string BlobUrl { get; private set; }
        public string? ThumbnailUrl { get; private set; }
        public int? Width { get; private set; }
        public int? Height { get; private set; }
        public int? Duration { get; private set; }
        public decimal? AspectRatio { get; private set; }
        public long? FolderId { get; private set; }
        public string? Tags { get; private set; }
        public string? Description { get; private set; }
        public bool IsProcessed { get; private set; }
        public ProcessingStatus ProcessingStatus { get; private set; }
        public string? ProcessingErrorMessage { get; private set; }
        public string? Hash { get; private set; } 
        public int UsageCount { get; private set; }
        public DateTime UploadedAt { get; private set; }
        public DateTime? LastUsedAt { get; private set; }
    }

   
}
