using PictureAnalyzer.Server.Utility;
using PictureAnalyzer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PictureAnalyzer.Server.Controllers
{
    public class FolderController : Controller
    {
        private FolderService _folderService;
        private AccountService _accountService;

        public FolderController(FolderService folderService, AccountService accountService)
        {
            _folderService = folderService;
            _accountService = accountService;
        }

        [HttpPost]
        [Authorize]
        [Route("api/v1/folder")]
        async public Task<IActionResult> FolderCreate([FromBody] Common.Folder model)
        {
            string userId = User.GetUserId();
            var result = await _folderService.FolderCreate(model, userId);
            
            if(result.Success)
                return Ok(result.Data);
            else 
                return BadRequest(result.Message);
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/folder/{folderId}")]
        async public Task<IActionResult> GetFolder(string folderId)
        {
            string userId = User.GetUserId();
            var result = await _folderService.FolderGetById(folderId, userId);

            if (result.Success)
                return Ok(result.Data.ToCommonFolder(userId));
            else
                return BadRequest(result.Message);
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/folder/{folderId}/settings")]
        async public Task<IActionResult> GetFolderSettings(string folderId)
        {
            string userId = User.GetUserId();
            var result = await _folderService.FolderGetById(folderId, userId);
            if (result == null)
                return BadRequest("Folder not found");
            else if (result != null && result.Data.AuthorizedUsers.Any(x => x.UserId == userId && x.AllowEdit) == false)
                return BadRequest("Not authorized");

            var response = result.Data.ToCommonFolder(userId);

            var authorizedUserEmailsByIds = await _accountService.GetUsersById(result.Data.AuthorizedUsers.Select(x => x.UserId).ToList());
            response.AuthorizedUsers = response.AuthorizedUsers.Select(x => new Common.AuthorizedUser()
            {
                Id = x.Id,
                UserId = x.UserId,
                AllowEdit = x.AllowEdit,
                Email = authorizedUserEmailsByIds.FirstOrDefault(a => a.Key == x.UserId)?.Value,
                IsSelf = userId == x.UserId
            }).ToList();

            if (result.Success)
                return Ok(response);
            else
                return BadRequest(result.Message);
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/folder/{folderId}")]
        async public Task<IActionResult> UpdateFolder([FromBody] Common.Folder model, string folderId)
        {
            string userId = User.GetUserId();
            var folder = await _folderService.FolderUpdate(model, userId);

            if (folder.Success)
                return Ok();
            else
                return BadRequest(folder.Message);
        }

        [HttpDelete]
        [Authorize]
        [Route("api/v1/folder/{folderId}")]
        async public Task<IActionResult> DeleteFolder(string folderId)
        {
            string userId = User.GetUserId();
            var folder = await _folderService.FolderDelete(folderId, userId);

            if (folder.Success)
                return Ok();
            else
                return BadRequest(folder.Message);
        }

        [HttpPost]
        [Authorize]
        [Route("api/v1/folders")]
        async public Task<IActionResult> FoldersSearch([FromBody]Common.Search model)
        {
            string userId = User.GetUserId();

            var results = await _folderService.FolderSearch(model, userId);

            return Ok(results);
        }


        [HttpPut]
        [Authorize]
        [Route("api/v1/folder/{folderId}/authorized")]
        async public Task<IActionResult> FolderAddAuthorizedUser([FromBody] Common.AuthorizedUser user, string folderId)
        {
            var userId = User.GetUserId();
            if (user == null || string.IsNullOrEmpty(user.Email))
                return BadRequest("User is required");

            var authorizedUserId = await _accountService.GetUserIdByUserEmail(user.Email);
            if(!authorizedUserId.Success)
                return BadRequest(authorizedUserId.Message);

            var result = await _folderService.FolderAddAuthorizedUser(folderId, authorizedUserId.Data, userId);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        [Route("api/v1/folder/{folderId}/authorized/{authorizedUserId}")]
        async public Task<IActionResult> FolderDeleteAuthorizedUser(string folderId, string authorizedUserId)
        {
            var userId = User.GetUserId();
            var result = await _folderService.FolderDeleteAuthorizedUser(folderId, authorizedUserId, userId);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/folder/{folderId}/authorized/{authorizedUserId}/allowedit")]
        async public Task<IActionResult> FolderToggleWrite(string folderId, string authorizedUserId)
        {
            var userId = User.GetUserId();
            var result = await _folderService.FolderToggleAllowEdit(folderId, authorizedUserId, userId);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/folder/{folderId}/upload")]
        public async Task<IActionResult> FolderItemGenerateUploadSasUri(string folderId)
        {
            var userId = User.GetUserId();

            var result = await _folderService.FolderItemGenerateUploadSasUri();

            if(result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize]
        [Route("api/v1/folder/{folderId}/item")]
        public async Task<IActionResult> FolderItemCreate(string folderId, [FromBody] Common.UploadFolderItem model)
        {
            var userId = User.GetUserId();

            var result = await _folderService.FolderItemSave(model, folderId, userId);

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/folder/{folderId}/item/{folderItemId}")]
        public async Task<IActionResult> FolderItemGetById(string folderId, string folderItemId)
        {
            string userId = User.GetUserId();

            var folderItem = await _folderService.FolderItemGetById(folderId, folderItemId, userId);
            var sasUri = await _folderService.FolderItemGenerateViewSasUri(folderId, folderItemId, userId);

            Common.FolderItem response = new Common.FolderItem()
            {
                Id = folderItem.Id,
                PrettyFileName = folderItem.PrettyFileName,
                FileSize = folderItem.FileSize,
                BlobId = folderItem.BlobId,
                ItemUri = sasUri.Sas,
                MimeType = folderItem.MimeType,
                ImageTags = folderItem.ImageTags.Select(s => new Common.ImageTag()
                {
                    Id = s.Id,
                    Tag = s.Tag,
                    Confidence = s.Confidence
                }).ToList(),
                ImageRatings = folderItem.ImageRatings != null ? new Common.ImageRatings()
                {
                    AdultScore = folderItem.ImageRatings.AdultScore,
                    RacyScore = folderItem.ImageRatings.RacyScore,
                    GoreScore = folderItem.ImageRatings.GoreScore
                } : null,
                FolderId = folderId,
                FolderName = folderItem.Folder.Name,
                UploadedBy = folderItem.UploadedBy,
                CreatedOn = folderItem.CreatedOn
            };

            return Ok(response);
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/folder/{folderId}/item/{folderItemId}")]
        public async Task<IActionResult> FolderItemUpdateById([FromBody]Common.FolderItem model, string folderId, string folderItemId)
        {
            string userId = User.GetUserId();
            await _folderService.FolderItemUpdateById(model, folderId, folderItemId, userId);
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        [Route("api/v1/folder/{folderId}/item/{folderItemId}")]
        public async Task<IActionResult> FolderItemDeleteById(string folderId, string folderItemId)
        {
            string userId = User.GetUserId();
            await _folderService.FolderItemDeleteById(folderId, folderItemId, userId);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("api/v1/folder/{folderId}/items")]
        async public Task<IActionResult> FolderItemsSearch([FromBody]Common.Search model, string folderId)
        {
            string userId = User.GetUserId();

            var results = await _folderService.FolderItemsSearch(model, folderId, userId);

            return Ok(results);
        }
        
        [HttpPut]
        [Authorize]
        [Route("api/v1/folder/{folderId}/item/{folderItemId}/tag")]
        public async Task<IActionResult> FolderItemAddTabById([FromBody]Common.ImageTag model, string folderId, string folderItemId)
        {
            string userId = User.GetUserId();
            var newTag = await _folderService.FolderItemAddTagById(model, folderId, folderItemId, userId);
            return Ok(new Common.ImageTag()
            {
                Id = newTag.Id,
                Tag = newTag.Tag
            });
        }

        [HttpDelete]
        [Authorize]
        [Route("api/v1/folder/{folderId}/item/{folderItemId}/tag/{tagId}")]
        public async Task<IActionResult> FolderItemDeleteTabById(string folderId, string folderItemId, string tagId)
        {
            string userId = User.GetUserId();
            await _folderService.FolderItemDeleteTagById(folderId, folderItemId, tagId, userId);
            return Ok();
        }
    }
}
