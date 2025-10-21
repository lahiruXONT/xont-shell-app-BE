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

        public void SaveUserLoginData(User userOb, ref MessageSet message)
        {
            _userDal.SaveUserLoginData(userOb, ref message);
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


    }
}