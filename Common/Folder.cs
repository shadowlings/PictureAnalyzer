using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureAnalyzer.Common
{
    public class Folder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        public long TotalSize { get; set; }
        public int ItemCount { get; set; }

        public bool AllowEdit { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
        public List<AuthorizedUser> AuthorizedUsers { get; set; } = new List<AuthorizedUser>();
    }

    public class AuthorizedUser
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        
        public bool AllowEdit { get; set; }

        public bool IsSelf { get; set; }
    }

    public class UploadFolderItem
    {
        public string PrettyFileName { get; set; }
        public string MimeType { get; set; }
        public string BlobId { get; set; }
        public long FileSize { get; set; }
    }

    public class FolderItem
    {
        public string Id { get; set; }

        public string PrettyFileName { get; set; }
        public string BlobId { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public Uri? ItemUri { get; set; }

        public ImageRatings ImageRatings { get; set; } = new ImageRatings();
        public List<ImageTag> ImageTags { get; set; } = new List<ImageTag>();

        public string UploadedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public string FolderId { get; set; }
        public string FolderName { get; set; }
    }

    public class SasUri
    {
        public Uri Sas { get; set; }
        public string BlobId { get; set; }
    }


    public class ImageRatings
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public double AdultScore { get; set; }
        public double RacyScore { get; set; }
        public double GoreScore { get; set; }
    }

    public class ImageTag
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Tag { get; set; }
        public double Confidence { get; set; }
    }
}
