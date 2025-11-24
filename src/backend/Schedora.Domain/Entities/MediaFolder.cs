using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedora.Domain.Entities
{
    public class MediaFolder : Entity
    {
        // Properties
        public Guid UserId { get; private set; }
        public string Name { get; private set; }
        public Guid? ParentFolderId { get; private set; }
        public string? Path { get; private set; }
        public string? Color { get; private set; }
        public string? Icon { get; private set; }
        public int MediaCount { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual MediaFolder? ParentFolder { get; private set; }
        public virtual ICollection<MediaFolder> SubFolders { get; private set; }
        public virtual ICollection<Media> MediaFiles { get; private set; }

        // Private constructor for EF
        private MediaFolder()
        {
            SubFolders = new HashSet<MediaFolder>();
            MediaFiles = new HashSet<Media>();
        }

        // Factory method
        public static MediaFolder Create(Guid userId, string name, Guid? parentFolderId = null)
        {
            return new MediaFolder
            {
                UserId = userId,
                Name = name,
                ParentFolderId = parentFolderId,
                MediaCount = 0
            };
        }

        // Domain methods
        public void Rename(string newName)
        {
            Name = newName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Move(Guid? newParentFolderId)
        {
            ParentFolderId = newParentFolderId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPath(string path)
        {
            Path = path;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAppearance(string color, string icon)
        {
            Color = color;
            Icon = icon;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncrementMediaCount()
        {
            MediaCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DecrementMediaCount()
        {
            if (MediaCount > 0)
            {
                MediaCount--;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void RecalculateMediaCount()
        {
            MediaCount = MediaFiles.Count;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
