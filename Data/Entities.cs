using PictureAnalyzer.Common.Enumerations;
using System;
using System.Collections.Generic;

namespace PictureAnalyzer.Data
{
    public class Folder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        public List<AuthorizedUser> AuthorizedUsers { get; set; } = new List<AuthorizedUser>();
        public List<FolderItem> Items { get; set; } = new List<FolderItem>();

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
    }

    public class AuthorizedUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }

        public bool AllowEdit { get; set; }

        public string FolderId { get; set; }
        public Folder Folder { get; set; }
    }


    public class FolderItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string BlobId { get; set; }
        public string PrettyFileName { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }

        public string UploadedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ImageRatings ImageRatings { get; set; }
        public List<ImageTag> ImageTags { get; set; } = new List<ImageTag>();

        public string FolderId { get; set; }
        public Folder Folder { get; set; }
    }

    public class ImageRatings
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public bool IsAdultContent { get; set; }
        public bool IsRacyContent { get; set; }
        public bool IsGoryContent { get; set; }

        public double AdultScore { get; set; }
        public double RacyScore { get; set; }
        public double GoreScore { get; set; }

        public string FolderItemId { get; set; }
    }

    public class ImageTag
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Tag { get; set; }
        public double Confidence { get; set; }

        public string FolderItemId { get; set; }
        public FolderItem FolderItem { get; set; }
    }

}
