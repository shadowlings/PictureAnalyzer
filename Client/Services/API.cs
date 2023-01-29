using Blazored.Toast.Services;
using BlazorSpinner;
using PictureAnalyzer.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Newtonsoft.Json;
using System.Text;

namespace PictureAnalyzer.Client.Services
{
    public class API
    {
        private HttpClient _httpClient { get; set; }
        private NavigationManager _navigationManger { get; set; }
        private IToastService _toastService { get; set; }
        private SpinnerService _spinnerService { get; set; }
        private IAccessTokenProvider _authenticationService { get; set; }
        public API(HttpClient httpClient, IAccessTokenProvider authenticationService, NavigationManager navigationManager, IToastService toastService, SpinnerService spinnerService)
        {
            _httpClient = httpClient;

            _authenticationService = authenticationService;
            _navigationManger = navigationManager;
            _toastService = toastService;
            _spinnerService = spinnerService;
        }


        #region Account Management
        async public Task<bool> AccountChangeEmail(ChangeEmail content)
        {
            var response = await PostAsync<bool>($"api/v1/account/email", content);
            return response;
        }
        async public Task<bool> AccountChangePassword(ChangePassword content)
        {
            var response = await PostAsync<bool>($"api/v1/account/password", content);
            return response;
        }
        async public Task AccountDeleteUser()
        {
            await DeleteAsync($"api/v1/account");
        }
        #endregion

        #region Folders
        async public Task<string> FolderCreate(Folder model)
        {
            return await PostAsync<string>("api/v1/folder", model);
        }
        async public Task<Folder> FolderGetById(string folderId)
        {
            return await GetAsync<Folder>($"api/v1/folder/{folderId}");
        }
        async public Task<Folder> FolderSettingsGetById(string folderId)
        {
            return await GetAsync<Folder>($"api/v1/folder/{folderId}/settings");
        }
        async public Task FolderUpdate(Folder model, string folderId)
        {
            await PutAsync($"api/v1/folder/{folderId}", model);
        }
        async public Task FolderDelete(string folderId)
        {
            await DeleteAsync($"api/v1/folder/{folderId}");
        }
        async public Task<SearchResponse<Folder>> FolderSearch(Search model)
        {
            return await PostAsync<SearchResponse<Folder>>($"api/v1/folders", model);
        }

        async public Task<AuthorizedUser> FolderAddAuthorizedUser(AuthorizedUser model, string folderId)
        {
            return await PutAsync<AuthorizedUser>($"api/v1/folder/{folderId}/authorized", model);
        }
        async public Task FolderDeleteAuthorizedUser(string folderId, string authorizedUserId)
        {
            await DeleteAsync($"api/v1/folder/{folderId}/authorized/{authorizedUserId}");
        }
        async public Task<AuthorizedUser> FolderToggleAllowEdit(string folderId, string authorizedId)
        {
            return await GetAsync<AuthorizedUser>($"api/v1/folder/{folderId}/authorized/{authorizedId}/allowedit");
        }

        async public Task<FolderItem> FolderItemCreate(UploadFolderItem model, string folderId)
        {
            return await PostAsync<FolderItem>($"api/v1/folder/{folderId}/item", model, false);
        }
        async public Task<SasUri> FolderItemGenerateUploadSasUri(string folderId)
        {
            return await GetAsync<SasUri>($"api/v1/folder/{folderId}/upload", false);
        }
        async public Task<Common.FolderItem> FolderItemGetById(string folderId, string folderItemId)
        {
            return await GetAsync<Common.FolderItem>($"api/v1/folder/{folderId}/item/{folderItemId}");
        }
        async public Task FolderItemUpdateById(FolderItem model, string folderId, string folderItemId)
        {
            await PutAsync($"api/v1/folder/{folderId}/item/{folderItemId}", model);
        }
        async public Task FolderItemDeleteById(string folderId, string folderItemId)
        {
            await DeleteAsync($"api/v1/folder/{folderId}/item/{folderItemId}");
        }
        
        async public Task<SearchResponse<FolderItem>> FolderItemSearch(Search model, string folderId)
        {
            return await PostAsync<SearchResponse<FolderItem>>($"api/v1/folder/{folderId}/items", model);
        }

        async public Task<Common.ImageTag> FolderItemAddTabById(Common.ImageTag model, string folderId, string folderItemId)
        {
            return await PutAsync<Common.ImageTag>($"api/v1/folder/{folderId}/item/{folderItemId}/tag", model);
        }

        async public Task<bool> FolderItemDeleteTagById(string folderId, string folderItemId, string tagId)
        {
            return await DeleteAsync<bool>($"api/v1/folder/{folderId}/item/{folderItemId}/tag/{tagId}");
        }
        

        #endregion

        #region HTTP Methods
        private async Task GetAsync(string path, bool showSpinner = true)
        {
            await Send(HttpMethod.Get, path, showSpinner);
        }
        private async Task<T> GetAsync<T>(string path, bool showSpinner = true)
        {
            var response = await Send(HttpMethod.Get, path, showSpinner);
            T result = await ParseResponseObject<T>(response);
            return result;
        }
        private async Task PostAsync(string path, object content, bool showSpinner = true)
        {
            await Send(HttpMethod.Post, path, showSpinner, content);
        }
        private async Task<T> PostAsync<T>(string path, object content, bool showSpinner = true)
        {
            var response = await Send(HttpMethod.Post, path, showSpinner, content);
            return await ParseResponseObject<T>(response);
        }
        private async Task PutAsync(string path, object content, bool showSpinner = true)
        {
            await Send(HttpMethod.Put, path, showSpinner, content);
        }
        private async Task<T> PutAsync<T>(string path, object content, bool showSpinner = true)
        {
            var response = await Send(HttpMethod.Put, path, showSpinner, content);
            return await ParseResponseObject<T>(response);
        }
        private async Task PutAsync(string path, bool showSpinner = true)
        {
            await Send(HttpMethod.Put, path, showSpinner);
        }
        private async Task DeleteAsync(string path, bool showSpinner = true)
        {
            await Send(HttpMethod.Delete, path, showSpinner);
        }
        private async Task<T> DeleteAsync<T>(string path, bool showSpinner = true)
        {
            var response = await Send(HttpMethod.Delete, path, showSpinner);
            return await ParseResponseObject<T>(response);
        }
        private async Task DeleteAsync(string path, object content, bool showSpinner = true)
        {
            await Send(HttpMethod.Delete, path, showSpinner, content);
        }


        private async Task<HttpResponseMessage> Send(HttpMethod method, string path, bool showSpinner, object content = null)
        {
            if(showSpinner)
                _spinnerService.Show();

            var httpWebRequest = new HttpRequestMessage(method, path);
            httpWebRequest.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };

            if (content != null)
            {
                string json = JsonConvert.SerializeObject(content);
                StringContent postContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                httpWebRequest.Content = postContent;
            }

            HttpResponseMessage response = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
            try
            {
                response = await _httpClient.SendAsync(httpWebRequest);

                if (response.IsSuccessStatusCode == false)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        _toastService.ShowError(responseContent);
                    }
                }

                if(showSpinner)
                    _spinnerService.Hide();
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
            return response;
        }

        private async Task<T> ParseResponseObject<T>(HttpResponseMessage response)
        {
            if(typeof(T) == typeof(bool))
            {
                return (T)Convert.ChangeType(response.StatusCode == System.Net.HttpStatusCode.OK, typeof(T));
            }
            else if (response != null && response.IsSuccessStatusCode && response.Content != null)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                //Can't deseriazlize a string unless it starts with a "
                if (typeof(T) == typeof(string))
                    responseContent = $"\"{responseContent}\"";

                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            else
            {
                return default(T);
            }
        }
        #endregion
    }
}
