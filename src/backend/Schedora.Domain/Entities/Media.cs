using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
        public sealed class MediaBuilder
        {
            internal long UserId;
            internal string FileName;
            internal long FileSize;
            internal string MimeType;
            internal string BlobUrl;
            internal DateTime UploadedAt;

            internal string? OriginalFileName;
            internal string? ThumbnailUrl;
            internal int? Width;
            internal int? Height;
            internal int? Duration;
            internal decimal? AspectRatio;
            internal long? FolderId;
            internal string? Tags;
            internal string? Description;
            internal bool IsProcessed;
            internal ProcessingStatus ProcessingStatus;
            internal string? ProcessingErrorMessage;
            internal string? Hash;
            internal int UsageCount;
            internal DateTime? LastUsedAt;

            public MediaBuilder(long userId, string fileName, long fileSize, string mimeType, string blobUrl)
            {
                UserId = userId;
                FileName = fileName;
                FileSize = fileSize;
                MimeType = mimeType;
                BlobUrl = blobUrl;
            }
            
            

            public MediaBuilder WithOriginalFileName(string? value) { OriginalFileName = value; return this; }
            public MediaBuilder WithThumbnailName(string? value) { ThumbnailUrl = value; return this; }
            public MediaBuilder WithDimensions(int? width, int? height) { Width = width; Height = height; return this; }
            public MediaBuilder WithDuration(int? value) { Duration = value; return this; }
            public MediaBuilder WithAspectRatio(decimal? value) { AspectRatio = value; return this; }
            public MediaBuilder WithFolder(long? value) { FolderId = value; return this; }
            public MediaBuilder WithTags(string? value) { Tags = value; return this; }
            public MediaBuilder WithDescription(string? value) { Description = value; return this; }
            public MediaBuilder WithProcessing(bool isProcessed, ProcessingStatus status, string? error)
            {
                IsProcessed = isProcessed;
                if (isProcessed)
                    UploadedAt = DateTime.UtcNow;
                ProcessingStatus = status;
                ProcessingErrorMessage = error;
                return this;
            }
            public MediaBuilder WithHash(string? value) { Hash = value; return this; }
            public MediaBuilder WithUsage(int value, DateTime? lastUsedAt)
            {
                UsageCount = value;
                LastUsedAt = lastUsedAt;
                return this;
            }

            public Media Build() => new Media(this);
        }
    
    public class Media : Entity
    {
        public Media()
        {
            
        }
        
        public Media(MediaBuilder builder)
        {
            UserId = builder.UserId;
            FileName = builder.FileName;
            OriginalFileName = builder.OriginalFileName;
            FileSize = builder.FileSize;
            MimeType = builder.MimeType;
            ThumbnailName = builder.ThumbnailUrl;
            Width = builder.Width;
            Height = builder.Height;
            Duration = builder.Duration;
            FolderId = builder.FolderId;
            Tags = builder.Tags;
            Description = builder.Description;
            IsProcessed = builder.IsProcessed;
            ProcessingStatus = builder.ProcessingStatus;
            ProcessingErrorMessage = builder.ProcessingErrorMessage;
            Hash = builder.Hash;
            UsageCount = builder.UsageCount;
            UploadedAt = builder.UploadedAt;
            LastUsedAt = builder.LastUsedAt;
        }

        public long UserId { get; private set; }
        public string FileName { get; private set; }
        public string? OriginalFileName { get; private set; }
        public long FileSize { get; private set; }
        public string MimeType { get; private set; }
        public string? ThumbnailName { get; private set; }
        public int? Width { get; private set; }
        public int? Height { get; private set; }
        public int? Duration { get; private set; }
        public long? FolderId { get; private set; }
        public string? Tags { get; private set; }
        public string? Description { get; private set; }
        public bool IsProcessed { get; private set; }
        public ProcessingStatus ProcessingStatus { get; private set; }
        public string? ProcessingErrorMessage { get; private set; }
        public string? Hash { get; private set; } 
        public int UsageCount { get; private set; }
        public DateTime? UploadedAt { get; private set; }
        public DateTime? LastUsedAt { get; private set; }
        
        public void SetFolder(long folderId) => FolderId = folderId;
    }

   
}
