namespace PictureAnalyzer.Server.Utility
{
    static public class Mappings
    {
        static public Common.Folder ToCommonFolder(this Data.Folder model, string userId)
        {
            var folder = new Common.Folder()
            {
                Id = model.Id,
                Name = model.Name,
                AuthorizedUsers = model.AuthorizedUsers.Select(a => a.ToCommonAuthorizedUser()).ToList(),
                AllowEdit = model.AuthorizedUsers.Any(x => x.UserId == userId && x.AllowEdit)
            };

            if(model.AuthorizedUsers != null)
            {
                folder.AuthorizedUsers = model.AuthorizedUsers.Select(a => a.ToCommonAuthorizedUser()).ToList();
            }

            return folder;
        }

        static public Common.AuthorizedUser ToCommonAuthorizedUser(this Data.AuthorizedUser model)
        {
            var authorizedUser = new Common.AuthorizedUser()
            {
                Id = model.Id,
                UserId = model.UserId,
                AllowEdit = model.AllowEdit
            };

            return authorizedUser;
        }

        static public Data.AuthorizedUser ToDataAuthorizedUser(this Common.AuthorizedUser model)
        {
            var authorizedUser = new Data.AuthorizedUser()
            {
                Id = model.Id,
                UserId = model.UserId,
                AllowEdit = model.AllowEdit
            };

            return authorizedUser;
        }
    }
}
