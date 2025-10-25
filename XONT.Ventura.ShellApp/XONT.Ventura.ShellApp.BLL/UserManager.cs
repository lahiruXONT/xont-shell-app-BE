using System.Collections.Generic;
using System.Data;
using XONT.Common.Message;
using XONT.Ventura.ShellApp.DOMAIN;
using XONT.Ventura.ShellApp.DAL;

namespace XONT.Ventura.ShellApp.BLL
{
    public class UserManager : IUserDAO
    {
        

        private readonly UserDAL _userDal;

        public UserManager(UserDAL userDAL)
        {
            _userDal = userDAL;
        }

        public User GetUserInfo(string userName, string password, ref MessageSet message)
        {
            User user = _userDal.GetUserInfo(userName, password, ref message);
            return user;
        }

        public List<UserMenu> GetUserManu(string userName, string roleCode, ref MessageSet message)
        {
            List<UserMenu> userMenus = _userDal.GetUserMenu(userName, roleCode, ref message);
            return userMenus;
        }
        public List<UserTask> GetUserTask(string menuCode, string userName, ref MessageSet message)
        {
            List<UserTask> userTasks = _userDal.GetUserTask(menuCode, userName, ref message);
            return userTasks;
        }

        public void SaveUserLoginData(User userOb, string refreshToken, DateTime refreshTokenExpire, ref MessageSet message)
        {
            _userDal.SaveUserLoginData(userOb, refreshToken, refreshTokenExpire, ref message);
        }
        public (string?, string?) ValidateRefreshToken(string refreshToken, ref MessageSet message)
        {
            return _userDal.ValidateRefreshToken(refreshToken, ref message);
        }
        public List<UserRole> GetUserRoles(string userName, ref MessageSet message)
        {
            return _userDal.GetUserRole(userName, ref message);
        }


        public void UpdateDailyMenu(string MenuCode, ref MessageSet message)
        {
            _userDal.UpdateDailyMenu(MenuCode, ref message);
        }

        public bool CheckDailyMenu(string MenuCode, ref MessageSet message)
        {
            bool isExists = _userDal.CheckDailyMenu(MenuCode, ref message);

            return isExists;
        }

        public List<string> GetUnAuthorizedTasksForUser(string userName, ref MessageSet message)
        {
            var dt = _userDal.GetUnAuthorizedTasks(userName, ref message);
            if (dt == null || dt.Rows.Count == 0 || message!=null)
                return new List<string>();
            if (!dt.Columns.Contains("TaskCode"))
                return new List<string>();
            return dt.AsEnumerable()
                    .Where(row => !string.IsNullOrWhiteSpace(row["TaskCode"]?.ToString() ?? ""))
                    .Select(row => row["TaskCode"]?.ToString() ?? "")
                    .ToList();
        }

    }
}