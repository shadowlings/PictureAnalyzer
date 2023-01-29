using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using PictureAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace PictureAnalyzer.Service
{
    public class AccountService
    {
        private FolderService _folderService;
        private IConfiguration _configuration;
        private DBContext _dbContext;

        public AccountService(IConfiguration configuration, DBContext dBContext, FolderService folderService)
        {
            _configuration = configuration;
            _dbContext = dBContext;
            _folderService = folderService;
        }

        async private Task<Auth0TokenResponse> GetAuth0Token()
        {
            var client = new HttpClient();// "https_//dev-e-8hqzx5.us.auth0.com/oauth/token");

            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            parameters.Add(new KeyValuePair<string, string>("client_id", _configuration.GetSection("Auth0").GetValue<string>("ClientId")));
            parameters.Add(new KeyValuePair<string, string>("client_secret", _configuration.GetSection("Auth0").GetValue<string>("Secret")));
            parameters.Add(new KeyValuePair<string, string>("audience", $"https://{_configuration.GetSection("Auth0").GetValue<string>("Domain")}/api/v2/"));
            var req = new HttpRequestMessage(HttpMethod.Post, $"https://{_configuration.GetSection("Auth0").GetValue<string>("Domain")}/oauth/token") { Content = new FormUrlEncodedContent(parameters) };
            var res = await client.SendAsync(req);

            var response = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Auth0TokenResponse>(response);

            return data;
        }

        async public Task<ServiceResponse> ChangeAccountEmail(string userId, Common.ChangeEmail model)
        {
            ServiceResponse response = new ServiceResponse();

            var tokenData = await GetAuth0Token();

            var apiClient = new ManagementApiClient(tokenData.access_token, new Uri($"https://{_configuration.GetSection("Auth0").GetValue<string>("Domain")}/api/v2/"));

            UserUpdateRequest request = new UserUpdateRequest()
            {
                Email = model.NewEmail,
                FullName = model.NewEmail
            };

            try
            {
                var updateResponse = await apiClient.Users.UpdateAsync(userId, request);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        async public Task<ServiceResponse> ChangeAccountPassword(string userId, Common.ChangePassword model)
        {
            ServiceResponse response = new ServiceResponse();

            if(model.NewPassword != model.ConfirmPassword)
            {
                response.Success = false;
                response.Message = "Passwords do not match";
            }
            else if(string.IsNullOrEmpty(model.NewPassword))
            {
                response.Success = false;
                response.Message = "Password is required";
            }

            var tokenData = await GetAuth0Token();

            var apiClient = new ManagementApiClient(tokenData.access_token, new Uri($"https://{_configuration.GetSection("Auth0").GetValue<string>("Domain")}/api/v2/"));

            UserUpdateRequest request = new UserUpdateRequest()
            {
                Password = model.NewPassword
            };

            try
            {
                var updateResponse = await apiClient.Users.UpdateAsync(userId, request);
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }


        async public Task<ServiceResponse> DeleteAccount(string userId)
        {
            ServiceResponse response = new ServiceResponse();

            var foldersThisUserAdmins = await _dbContext.Folders
                                        .Include(x => x.AuthorizedUsers)
                                        .Where(x => x.AuthorizedUsers.Any(a => a.UserId == userId && a.AllowEdit)).ToListAsync();

            foreach(var folder in foldersThisUserAdmins)
            {
                if(folder.AuthorizedUsers.Count == 1)
                {
                    //User is the only authorized user
                    await _folderService.FolderDelete(folder.Id, userId);
                }
                else if(folder.AuthorizedUsers.Count(x => x.AllowEdit) == 1)
                {
                    //This user is the only admin, delete the folder.
                    await _folderService.FolderDelete(folder.Id, userId);
                }
                else if(folder.AuthorizedUsers.Count(x => x.AllowEdit) > 1)
                {
                    //Do nothing. The folder will not be deleted because there is another admin.
                }
                else
                {
                    response.Success = false;
                    response.Message = $"Account cannot be deleted because older {folder.Name} cannot be deleted.";
                    return response;                    
                }
            }

            var tokenData = await GetAuth0Token();

            var apiClient = new ManagementApiClient(tokenData.access_token, new Uri($"https://{_configuration.GetSection("Auth0").GetValue<string>("Domain")}/api/v2/"));

            try
            {
                await apiClient.Users.DeleteAsync(userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        async public Task<ServiceResponse<string>> GetUserIdByUserEmail(string email)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            var tokenData = await GetAuth0Token();

            var apiClient = new ManagementApiClient(tokenData.access_token, new Uri($"https://{_configuration.GetSection("Auth0").GetValue<string>("Domain")}/api/v2/"));

            try
            {
                var user = await apiClient.Users.GetUsersByEmailAsync(email);
                if (user == null || user.Count == 0)
                {
                    response.Message = "User not found";
                    response.Success = false;
                    return response;
                }

                response.Data = user.First().UserId;
            }
            catch { }
            return response;
        }

        async public Task<List<ServiceResponseKeyValue<string, string>>> GetUsersById(List<string> userIds)
        {
            List<ServiceResponseKeyValue<string, string>> response = new List<ServiceResponseKeyValue<string, string>>();

            var tokenData = await GetAuth0Token();

            var apiClient = new ManagementApiClient(tokenData.access_token, new Uri($"https://{_configuration.GetSection("Auth0").GetValue<string>("Domain")}/api/v2/"));

            foreach(var userId in userIds)
            {
                var user = await apiClient.Users.GetAsync(userId);
                if(user != null)
                {
                    response.Add(new ServiceResponseKeyValue<string, string>()
                    {
                        Key = userId,
                        Value = user.Email
                    });
                }
            }

            return response;
        }


    }
}
