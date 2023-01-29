using PictureAnalyzer.Data;
using Microsoft.EntityFrameworkCore;

namespace PictureAnalyzer.Service
{
    public class FolderService
    {
        private DBContext _db;
        private AzureBlobService _azureBlobService;

        public FolderService(DBContext dbContext, AzureBlobService azureBlobService)
        {
            _db = dbContext;
            _azureBlobService = azureBlobService;
        }

        async public Task<ServiceResponse<string>> FolderCreate(Common.Folder model, string userId)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            if (string.IsNullOrEmpty(model.Name))
            {
                response.Success = false;
                response.Message = "Folder name is required";
                return response;
            }

            Folder folder = new Folder()
            {
                Name = model.Name,
                AuthorizedUsers = new List<AuthorizedUser>()
                {
                    new AuthorizedUser()
                    {
                        UserId = userId,
                        AllowEdit = true,
                    }
                }
            };
            _db.Folders.Add(folder);
            await _db.SaveChangesAsync();

            response.Data = folder.Id;
            return response;
        }

        async public Task<ServiceResponse<Folder>> FolderGetById(string folderId, string userId)
        {
            ServiceResponse<Folder> response = new ServiceResponse<Folder>();

            var folder = await _db.Folders
                        .Include(x => x.AuthorizedUsers)
                        .Where(o => o.Id == folderId && o.AuthorizedUsers.Any(a => a.UserId == userId)).FirstOrDefaultAsync();

            if (folder == null)
            {
                response.Message = "Folder not found";
                response.Success = false;
                return response;
            }
            
            response.Data = folder;
            return response;
        }

        async public Task<ServiceResponse> FolderUpdate(Common.Folder model, string userId)
        {
            ServiceResponse response = new ServiceResponse();

            var folder = await _db.Folders.FirstOrDefaultAsync(x => x.Id == model.Id && x.AuthorizedUsers.Any(a => a.UserId == userId && a.AllowEdit));
            if(folder == null)
            {
                response.Message = "Folder not found";
                response.Success = false;
                return response;
            }

            folder.Name = model.Name;
            folder.UpdatedOn = DateTime.UtcNow;
            
            await _db.SaveChangesAsync();

            return response;
        }

        async public Task<ServiceResponse> FolderDelete(string folderId, string userId)
        {
            ServiceResponse response = new ServiceResponse();
            var folder = _db.Folders
                            .Include(x => x.Items).ThenInclude(x => x.ImageTags)
                            .Include(x => x.Items).ThenInclude(x => x.ImageRatings)
                            .FirstOrDefault(x => x.Id == folderId && x.AuthorizedUsers.Any(a => a.UserId == userId && a.AllowEdit));
            if (folder == null)
            {
                response.Message = "Folder not found";
                response.Success = false;
                return response;
            }

            var authorizations = _db.AuthorizedUsers.Where(x => x.FolderId == folderId);
            _db.AuthorizedUsers.RemoveRange(authorizations);
            await _db.SaveChangesAsync();

            foreach(var item in folder.Items)
            {
                await _azureBlobService.DeleteBlob(item.BlobId);

                _db.ImageTags.RemoveRange(item.ImageTags);
                _db.ImageRatings.Remove(item.ImageRatings);
            }
            await _db.SaveChangesAsync();

            _db.FolderItems.RemoveRange(folder.Items);
            _db.Folders.Remove(folder);
            await _db.SaveChangesAsync();

            return response;
        }

        async public Task<Common.SearchResponse<Common.Folder>> FolderSearch(Common.Search model, string userId)
        {
            var query = _db.Folders
                        .Include(o => o.AuthorizedUsers)
                        .Include(o => o.Items)
                        .Where(o => o.AuthorizedUsers.Any(a => a.UserId == userId));

            if (!string.IsNullOrEmpty(model.FilterText))
            {
                query = query.Where(x => x.Name.Contains(model.FilterText)
                                    || x.Items.Any(i => i.PrettyFileName.Contains(model.FilterText))
                                    || x.Items.Any(i => i.ImageTags.Any(a => a.Tag.Contains(model.FilterText)))
                                    || (model.FilterText.ToLower().Contains("adult") && x.Items.Any(i => i.ImageRatings.AdultScore > 0.25))
                                    || (model.FilterText.ToLower().Contains("racy") && x.Items.Any(i => i.ImageRatings.RacyScore > 0.25))
                                    || (model.FilterText.ToLower().Contains("gore") && x.Items.Any(i => i.ImageRatings.GoreScore > 0.25)));
            }

            if (model.SortBy == nameof(Common.Folder.Name))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name);
            }
            else if (model.SortBy == nameof(Common.Folder.ItemCount))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Items.Count)
                            : query.OrderByDescending(c => c.Items.Count);
            }
            else if (model.SortBy == nameof(Common.Folder.TotalSize))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Items.Sum(x => x.FileSize))
                            : query.OrderByDescending(c => c.Items.Sum(x => x.FileSize));
            }
            else
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name);
            }

            Common.SearchResponse<Common.Folder> response = new Common.SearchResponse<Common.Folder>();
            response.Total = await query.CountAsync();

            var dataResponse = await query.Skip(model.Page * model.PageSize)
                                        .Take(model.PageSize)
                                        .ToListAsync();

            response.Data = dataResponse.Select(c => new Common.Folder()
            {
                Id = c.Id,
                Name = c.Name,
                TotalSize = c.Items.Select(o => o.FileSize).Sum(),
                ItemCount = c.Items.Count
            }).ToList();

            return response;
        }

        async public Task<ServiceResponse<string>> FolderAddAuthorizedUser(string folderId, string authorizedUserId, string currentUserId)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            if(authorizedUserId == currentUserId)
            {
                response.Message = "Cannot edit your own user authorization";
                response.Success = false;
                return response;
            }
            
            var folder = await _db.Folders.FirstOrDefaultAsync(x => x.Id == folderId && x.AuthorizedUsers.Any(a => a.UserId == currentUserId && a.AllowEdit));
            if(folder == null)
            {
                response.Message = "Not authorized";
                response.Success = false;
                return response;
            }
            else if (folder.AuthorizedUsers.Any(a => a.FolderId == folderId && a.Id == authorizedUserId))
            {
                response.Message = "Authorization already exists for this user";
                response.Success = false;
                return response;
            }
            else
            {
                var authorizedUser = new AuthorizedUser()
                {
                    UserId = authorizedUserId,
                    FolderId = folderId
                };
                _db.AuthorizedUsers.Add(authorizedUser);
                await _db.SaveChangesAsync();
            }

            return response;
        }

        async public Task<ServiceResponse<string>> FolderDeleteAuthorizedUser(string folderId, string authorizedUserId, string currentUserId)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            var authorizedUser = await _db.AuthorizedUsers.FirstOrDefaultAsync(a => a.FolderId == folderId && a.Id == authorizedUserId);
            if (authorizedUser.UserId == currentUserId)
            {
                response.Message = "Cannot edit your own user authorization";
                response.Success = false;
                return response;
            }

            var folder = await _db.Folders.FirstOrDefaultAsync(x => x.Id == folderId && x.AuthorizedUsers.Any(a => a.UserId == currentUserId && a.AllowEdit));
            if (folder == null)
            {
                response.Message = "Not authorized";
                response.Success = false;
                return response;
            }
            else if (authorizedUser != null)
            {
                _db.AuthorizedUsers.RemoveRange(authorizedUser);
                await _db.SaveChangesAsync();
            }

            return response;
        }

        async public Task<ServiceResponse<string>> FolderToggleAllowEdit(string folderId, string authorizedUserId, string currentUserId)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            var authorizedUser = await _db.AuthorizedUsers.FirstOrDefaultAsync(a => a.FolderId == folderId && a.Id == authorizedUserId);
            if (authorizedUser.UserId == currentUserId)
            {
                response.Message = "Cannot edit your own user authorization";
                response.Success = false;
                return response;
            }

            var folder = await _db.Folders.FirstOrDefaultAsync(x => x.Id == folderId && x.AuthorizedUsers.Any(a => a.UserId == currentUserId && a.AllowEdit));
            if (folder == null)
            {
                response.Message = "Not authorized";
                response.Success = false;
                return response;
            }
            else if (authorizedUser != null)
            {
                authorizedUser.AllowEdit = !authorizedUser.AllowEdit;
                await _db.SaveChangesAsync();
            }

            return response;
        }

        
        async public Task<ServiceResponse<FolderItem>> FolderItemSave(Common.UploadFolderItem model, string folderId, string currentUserId)
        {
            ServiceResponse<FolderItem> response = new ServiceResponse<FolderItem>();

            FolderItem newFolderItem = new FolderItem()
            {
                FolderId = folderId,
                BlobId = model.BlobId,
                PrettyFileName = model.PrettyFileName,
                MimeType = model.MimeType,
                UploadedBy = currentUserId,
                FileSize = model.FileSize
            };
            _db.FolderItems.Add(newFolderItem);
            await _db.SaveChangesAsync();

            response.Data = newFolderItem;
            return response;
        }

        async public Task<Common.SasUri?> FolderItemGenerateUploadSasUri()
        {
            return await _azureBlobService.GetUploadSasForNewBlob();
        }

        async public Task<Common.SasUri?> FolderItemGenerateViewSasUri(string folderId, string folderItemId, string currentUserId)
        {
            var folderItem = await FolderItemGetById(folderId, folderItemId, currentUserId);
            if(folderItem == null)
                return default(Common.SasUri);

            return await _azureBlobService.GetViewSasForBlob(folderItem.BlobId);
        }

        async public Task<FolderItem> FolderItemGetById(string folderId, string folderItemId, string currentUserId)
        {
            var folderItem = await _db.FolderItems
                        .Include(x => x.Folder)
                        .Include(x => x.ImageTags)
                        .Include(x => x.ImageRatings)
                        .FirstOrDefaultAsync(o => o.FolderId == folderId && o.Id == folderItemId && o.Folder.AuthorizedUsers.Any(a => a.UserId == currentUserId));

            return folderItem;
        }

        async public Task FolderItemUpdateById(Common.FolderItem model, string folderId, string folderItemId, string currentUserId)
        {
            var folderItem = await _db.FolderItems.Include(x => x.Folder)
                        .FirstOrDefaultAsync(o => o.FolderId == folderId && o.Id == folderItemId && o.Folder.AuthorizedUsers.Any(a => a.UserId == currentUserId));

            if(folderItem != null)
            {
                folderItem.PrettyFileName = model.PrettyFileName;
                await _db.SaveChangesAsync();
            }
        }

        async public Task FolderItemDeleteById(string folderId, string folderItemId, string currentUserId)
        {
            var folderItem = await _db.FolderItems.Include(x => x.Folder)
                        .FirstOrDefaultAsync(o => o.FolderId == folderId && o.Id == folderItemId && o.Folder.AuthorizedUsers.Any(a => a.UserId == currentUserId));

            if(folderItem != null)
            {
                await _azureBlobService.DeleteBlob(folderItem.BlobId);

                _db.FolderItems.Remove(folderItem);
                await _db.SaveChangesAsync();
            }
        }

        async public Task<Common.SearchResponse<Common.FolderItem>> FolderItemsSearch(Common.Search model, string folderId, string userId)
        {
            var query = _db.FolderItems
                        .Include(x => x.ImageRatings)
                        .Where(o => o.FolderId == folderId && o.Folder.AuthorizedUsers.Any(a => a.UserId == userId));

            if (!string.IsNullOrEmpty(model.FilterText))
            {
                query = query.Where(x => x.PrettyFileName.Contains(model.FilterText)
                                        || x.ImageTags.Any(a => a.Tag.Contains(model.FilterText))
                                        || (model.FilterText.ToLower().Contains("adult") && x.ImageRatings.AdultScore > 0.25) 
                                        || (model.FilterText.ToLower().Contains("racy") && x.ImageRatings.RacyScore > 0.25) 
                                        || (model.FilterText.ToLower().Contains("gore") && x.ImageRatings.GoreScore > 0.25)
                                        );
            }

            if (model.SortBy == nameof(Common.FolderItem.PrettyFileName))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.PrettyFileName)
                            : query.OrderByDescending(c => c.PrettyFileName);
            }
            else if (model.SortBy == nameof(Common.FolderItem.FileSize))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.FileSize)
                            : query.OrderByDescending(c => c.FileSize);
            }
            else
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.PrettyFileName)
                            : query.OrderByDescending(c => c.PrettyFileName);
            }

            Common.SearchResponse<Common.FolderItem> response = new Common.SearchResponse<Common.FolderItem>();
            response.Total = await query.CountAsync();

            var dataResponse = await query.Skip(model.Page * model.PageSize)
                                        .Take(model.PageSize)
                                        .ToListAsync();

            response.Data = dataResponse.Select(c => new Common.FolderItem()
            {
                Id = c.Id,
                BlobId = c.BlobId,
                PrettyFileName = c.PrettyFileName,
                MimeType = c.MimeType,
                UploadedBy = c.UploadedBy,
                CreatedOn = c.CreatedOn,
                FolderId = c.FolderId,
                FileSize = c.FileSize
                
            }).ToList();

            return response;
        }

        async public Task<ImageTag> FolderItemAddTagById(Common.ImageTag model, string folderId, string folderItemId, string currentUserId)
        {
            var folderItem = await _db.FolderItems
                        .Include(x => x.Folder)
                        .Include(x => x.ImageTags)
                        .FirstOrDefaultAsync(o => o.FolderId == folderId && o.Id == folderItemId && o.Folder.AuthorizedUsers.Any(a => a.UserId == currentUserId));

            if(folderItem != null && !string.IsNullOrEmpty(model.Tag))
            {
                var newTag = new ImageTag()
                {
                    Tag = model.Tag
                };
                folderItem.ImageTags.Add(newTag);

                await _db.SaveChangesAsync();

                return newTag;
            }
            else
            {
                return default(ImageTag);
            }
        }

        async public Task FolderItemDeleteTagById(string folderId, string folderItemId, string tagId, string currentUserId)
        {
            var folderItem = await _db.FolderItems
                        .Include(x => x.Folder)
                        .Include(x => x.ImageTags)
                        .FirstOrDefaultAsync(o => o.FolderId == folderId && o.Id == folderItemId && o.Folder.AuthorizedUsers.Any(a => a.UserId == currentUserId));

            if(folderItem != null)
            {
                var deletedTag = _db.ImageTags.Where(x => x.Id == tagId);
                _db.ImageTags.RemoveRange(deletedTag);

                await _db.SaveChangesAsync();
            }
        }
    }
}
