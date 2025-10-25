using System.Collections.Generic;
using XONT.Common.Message;
using System.Data;
using XONT.Ventura.ShellApp.DOMAIN; //VR007

namespace XONT.Ventura.ShellApp.BLL
{
    public interface IUserDAO
    {        
        User GetUserInfo(string userName, string password, ref MessageSet message);
        List<UserMenu> GetUserManu(string userName, string roleCode, ref MessageSet message);        
        List<UserTask> GetUserTask(string menuCode, string userName, ref MessageSet message);        
        void SaveUserLoginData(User userOb,string refreshToken,DateTime refreshTokenExpire, ref MessageSet message);
        (string?,string?) ValidateRefreshToken(string refreshToken, ref MessageSet message);
        List<UserRole> GetUserRoles(string userName, ref MessageSet message);
        void UpdateDailyMenu(string MenuCode, ref MessageSet message);
        bool CheckDailyMenu(string MenuCode, ref MessageSet message);
        List<string> GetUnAuthorizedTasksForUser(string userName, ref MessageSet message);
    }
}