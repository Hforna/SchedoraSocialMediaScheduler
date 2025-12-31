using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class PostMedia : Entity
    {
        public long PostId { get; private set; }
        public long MediaId { get; private set; }
        public int OrderIndex { get; private set; }
        public string? AltText { get; private set; }
        public string? PlatformSpecificSettings
        {
            get; private set;
        }

        public PostMedia(long postId, long mediaId, int orderIndex, string altText, string platformSpecificSettings)
        {
            PostId = postId;
            MediaId = mediaId;
            OrderIndex = orderIndex;
            AltText = altText;
            PlatformSpecificSettings = platformSpecificSettings;
        }
        
        public PostMedia(long postId, long mediaId, int orderIndex, string altText)
        {
            PostId = postId;
            MediaId = mediaId;
            OrderIndex = orderIndex;
            AltText = altText;
        }
    }
}
