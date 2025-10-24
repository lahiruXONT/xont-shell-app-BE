using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Transactions;
using XONT.Common.Data;
using XONT.Common.Message;
using XONT.Ventura.ShellApp.DAL;
using XONT.Ventura.ShellApp.DOMAIN;

namespace XONT.Ventura.ShellApp.DAL
{
    public class UserDAL
    {
        private readonly DBHelper _dbHelper;
        private readonly User user;
        private readonly string _userDbConnectionString;
        private readonly string _systemDbConnectionString;
        private readonly IConfiguration _configuration;



        public UserDAL(DBHelper dbHelper,IConfiguration configuration)
        {
            _configuration = configuration;
            _userDbConnectionString = _configuration.GetConnectionString("UserDB");
            _systemDbConnectionString = _configuration.GetConnectionString("SystemDB");
            _dbHelper = dbHelper;
            user = new User();
        }

        private void GetUserMainData(string userName, string password, ref MessageSet message)
        {
            string encriptPass = password; // VR029: assuming password is already encrypted
            try
            {
                string spName = "[dbo].[usp_GetUserData]";

                var parameters = new[]
                {
            new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
            {
                Value = userName.Trim()
            },
            new SqlParameter("@Password", SqlDbType.NVarChar, 100)
            {
                Value = encriptPass
            },
            new SqlParameter("@ExecutionType", SqlDbType.Char, 1)
            {
                Value = "1"
            },
            new SqlParameter("@DefaultBusinessUnit", SqlDbType.Char, 1)
            {
                Value = "1"
            }
        };

                DataTable dtResult = _dbHelper.ExecuteStoredProcedure(
                    _systemDbConnectionString,
                    spName,
                    parameters
                );

                // Check if user exists
                if (dtResult.Rows.Count > 0)
                {
                    string activeFlag = "";
                    foreach (DataRow row in dtResult.Rows)
                    {
                        user.isExists = true;
                        user.BusinessUnit = row["BusinessUnit"]?.ToString();
                        user.UserName = row["UserName"]?.ToString();
                        user.UserFullName = row["UserFullName"]?.ToString();
                        user.UserLevelGroup = row["UserLevelGroup"]?.ToString();
                        user.Password = row["Password"]?.ToString();
                        user.PasswordLocked = Convert.ToChar(row["PasswordLocked"]);
                        activeFlag = row["ActiveFlag"]?.ToString();
                        user.PowerUser = row["PowerUser"]?.ToString();
                        user.Theme = row["Theme"]?.ToString();
                        user.Language = row["Language"]?.ToString();
                        user.CaptionEditor = row["CaptionEditor"]?.ToString().Trim() == "1"; // V2033
                        user.FontColor = row["FontColor"]?.ToString();
                        user.FontSize = int.TryParse(row["FontSize"]?.ToString(), out int fontSize) ? fontSize : 0;
                        user.FontName = row["FontName"]?.ToString();
                        user.HasProPicture = row["ProPicAvailable"]?.ToString().Length > 0 ? row["ProPicAvailable"].ToString()[0] : '\0'; // New Ventura
                        user.DefaultRoleCode = row["RoleCode"]?.ToString(); // v2014

                        // Distributor Code - VR002
                        string distributorCode = row["DistributorCode"]?.ToString();
                        user.DistributorCode = string.IsNullOrEmpty(distributorCode) ? null : distributorCode.Trim();

                        // VR013
                        user.RestrictFOCInvoice = row["RestrictFOCInvoice"]?.ToString() ?? "0";

                        // VR024
                        user.SupplierCode = row["SupplierCode"]?.ToString()?.Trim() ?? "";
                        user.CustomerCode = row["CustomerCode"]?.ToString()?.Trim() ?? "";

                        // VR028
                        user.ExecutiveCode = row["ExecutiveCode"]?.ToString()?.Trim() ?? "";

                        // V2004
                        user.PasswordChange = row["PasswordChange"]?.ToString() ?? "";

                        // V2042
                        user.POReturnAuthorizationLevel = row["POReturnAuthorizationLevel"]?.ToString() ?? "";
                        user.POReturnAuthorizationUpTo = row["POReturnAuthorizationUpTo"]?.ToString() ?? "";

                        // V2048
                        user.PUSQQtyEdit = row["PUSQQtyEdit"]?.ToString() ?? "";
                    }

                    // Check user status
                    if (user.PasswordLocked == '0')
                    {
                        CheckActiveFlagUser(activeFlag, userName, ref message);
                        if (user.ActiveFlag)
                        {
                            GetPasswordData(user.BusinessUnit?.Trim(), ref message);
                            GetLastLoggingDAte(userName, ref message);

                            if (!user.IsUserExpire && !user.IsPassExpire)
                            {
                                CheckUserAlradyInSession(userName, ref message);

                                if (string.IsNullOrEmpty(user.AlreadyLogin))
                                {
                                    GetLastPasswordChangeDate(userName, ref message);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    user.UserName = userName;
                    user.Password = password;
                    user.PasswordChange = ""; // V2004
                }
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "GetUserMainData",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }
        }
        private void CheckUserAlradyInSession(string userName, ref MessageSet message)
        {
            // VR008
            user.AlreadyLogin = "";
            return;
            // VR008

            string dateTime = "";

            try
            {
                string commandText = @"
            SELECT MAX(Date) AS Date, MAX(Time) AS Time
            FROM ZYPasswordLoginDetails 
            WHERE UserName = @UserName
              AND SuccessfulLogin = N'1'
              AND LogOutTime IS NULL
            GROUP BY UserName, SuccessfulLogin, LogOutTime";

                var parameter = new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                {
                    Value = userName.Trim()
                };

                DataTable dtResult = _dbHelper.ExecuteQuery(
                    _systemDbConnectionString,
                    commandText,
                    new[] { parameter }
                );

                if (dtResult.Rows.Count > 0)
                {
                    foreach (DataRow row in dtResult.Rows)
                    {
                        if (row["Date"] != DBNull.Value && row["Time"] != DBNull.Value)
                        {
                            DateTime date = Convert.ToDateTime(row["Date"]);
                            DateTime time = Convert.ToDateTime(row["Time"]);
                            dateTime = date.ToShortDateString() + " " + time.ToLongTimeString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "CheckUserAlradyInSession",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }

            user.AlreadyLogin = dateTime;
        }

        private void GetPasswordData(string businessUnit, ref MessageSet message)
        {
            var password = new Password();
            try
            {
                string commandText = @"
            SELECT DISTINCT 
                ZYPasswordControl.MinLength, 
                ZYPasswordControl.MaxLength, 
                ZYPasswordControl.NoOfSpecialCharacters,
                ZYPasswordControl.ReusePeriod, 
                ZYPasswordControl.NoOfAttempts,
                ZYPasswordControl.ExpirePeriodInMonths,
                ZYPasswordControl.UserExpirePeriodInMonths
            FROM ZYPasswordControl
            INNER JOIN ZYUserBusUnit ON ZYUserBusUnit.BusinessUnit = ZYPasswordControl.BusinessUnit
            WHERE ZYUserBusUnit.BusinessUnit = @BusinessUnit";

                var parameter = new SqlParameter("@BusinessUnit", SqlDbType.NVarChar, 50)
                {
                    Value = businessUnit.Trim()
                };

                DataTable dt = _dbHelper.ExecuteQuery(
                    _systemDbConnectionString,
                    commandText,
                    new[] { parameter }
                );

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    password.MaxLength = row["MaxLength"]?.ToString() ?? "";
                    password.MinLength = row["MinLength"]?.ToString() ?? "";
                    password.NoOfSpecialCharacters = row["NoOfSpecialCharacters"]?.ToString() ?? "";
                    password.ReusePeriod = row["ReusePeriod"] != DBNull.Value
                        ? Convert.ToInt32(row["ReusePeriod"])
                        : 0;
                    password.NoOfAttempts = row["NoOfAttempts"] != DBNull.Value
                        ? Convert.ToInt32(row["NoOfAttempts"])
                        : 0;
                    password.ExpirePeriodInMonths = row["ExpirePeriodInMonths"] != DBNull.Value
                        ? Convert.ToInt32(row["ExpirePeriodInMonths"])
                        : 0;
                    password.UserExpirePeriodInMonths = row["UserExpirePeriodInMonths"] != DBNull.Value
                        ? Convert.ToInt32(row["UserExpirePeriodInMonths"])
                        : 0;
                }
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "GetPasswordData",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }

            user.PasswordData = password;
        }
        private void GetLastLoggingDAte(string userName, ref MessageSet message)
        {
            string date = string.Empty;
            string time = string.Empty;
            DateTime datetime = DateTime.Now;
            DateTime dateExpire = new DateTime(); // VR010
            long dif = -1;

            try
            {
                if (user.PasswordData?.UserExpirePeriodInMonths == 0) // VR011
                {
                    user.IsUserExpire = false;
                    return;
                }

                string commandText = @"
            SELECT ISNULL(LEFT(MAX(Date), 12), LEFT(CONVERT(VARCHAR(26), GETDATE(), 100), 12)) AS Date,
                   ISNULL(RIGHT(MAX([Time]), 7), RIGHT((CONVERT(VARCHAR(26), GETDATE(), 100)), 7)) AS [Time]
            FROM ZYPasswordLoginDetails
            WHERE UserName = @UserName AND SuccessfulLogin = '1'
              AND Date = (
                  SELECT MAX(Date)
                  FROM ZYPasswordLoginDetails
                  WHERE UserName = @UserName AND SuccessfulLogin = '1'
              )";

                var parameter = new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                {
                    Value = userName.Trim()
                };

                DataTable dtResult = _dbHelper.ExecuteQuery(
                    _systemDbConnectionString,
                    commandText,
                    new[] { parameter }
                );

                foreach (DataRow row in dtResult.Rows)
                {
                    date = row["Date"]?.ToString();
                    time = row["Time"]?.ToString();
                }

                if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(time))
                {
                    datetime = Convert.ToDateTime(date + " " + time);
                    dateExpire = datetime.AddMonths(user.PasswordData.UserExpirePeriodInMonths); // VR010
                }
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "GetLastLoggingDAte",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }

            if (DateTime.Now > dateExpire)
            {
                user.IsUserExpire = true;
            }
            else
            {
                user.IsUserExpire = false;
            }
        }
        private void GetLastPasswordChangeDate(string userName, ref MessageSet message)
        {
            string date = string.Empty;
            string time = string.Empty;
            DateTime datetime = DateTime.Now;
            long dif = -1;

            try
            {
                string commandText = @"
            SELECT ISNULL(LEFT(MAX(Date), 12), LEFT(CONVERT(VARCHAR(26), GETDATE(), 100), 12)) AS Date,
                   ISNULL(RIGHT(MAX([Time]), 7), RIGHT((CONVERT(VARCHAR(26), GETDATE(), 100)), 7)) AS [Time]
            FROM ZYPasswordHistory
            WHERE UserName = @UserName";

                var parameter = new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                {
                    Value = userName.Trim()
                };

                DataTable dtResult = _dbHelper.ExecuteQuery(
                    _systemDbConnectionString,
                    commandText,
                    new[] { parameter }
                );

                foreach (DataRow row in dtResult.Rows)
                {
                    date = row["Date"]?.ToString() ?? string.Empty;
                    time = row["Time"]?.ToString() ?? string.Empty;
                }

                if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(time))
                {
                    datetime = Convert.ToDateTime(date + " " + time);
                    dif = (long)(DateTime.Now - datetime).TotalDays / 30; // Approximate months
                }
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "GetLastPasswordChangeDate",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }

            if (dif > user.PasswordData?.ExpirePeriodInMonths)
            {
                user.IsPassExpire = true;
            }
            else
            {
                user.IsPassExpire = false;
            }
        }
        private void CheckActiveFlagUser(string activeFlag, string userName, ref MessageSet message)
        {
            bool activeVal = false;
            try
            {
                var stroEncript = new StroEncript();
                string tt = stroEncript.Encript("1", userName);
                string activeData = stroEncript.Decript(activeFlag, userName);
                if (activeData.Equals("1"))
                {
                    activeVal = true;
                }
            }
            catch (Exception)
            {
                // throw;
            }
            // string decriptPsass = stroEncript.Encript(password);
            user.ActiveFlag = activeVal;
        }

        public User GetUserInfo(string userName, string password, ref MessageSet message)
        {
            GetUserMainData(userName, password, ref message);
            //VR003 Begin
            if (user == null)
            {
                return null;
            }
            //VR003 End

            if (user.AlreadyLogin != null)
            {
                if (user.isExists && (user.AlreadyLogin.Equals(string.Empty)))
                {
                    GetUserRoles(userName, ref message);
                }
            }
            else
            { return user; }

            return user;
        }

        private void GetUserRoles(string userName, ref MessageSet message)
        {
            var userRoles = new List<UserRole>();
            try
            {
                if (userName.Trim() != "administrator") // VR006
                {
                    string commandText = @"
                SELECT ZYUserRole.RoleCode, ZYRole.Description, ZYRole.Icon
                FROM ZYUser
                INNER JOIN ZYUserRole ON ZYUser.UserName = ZYUserRole.UserName
                INNER JOIN ZYRole ON ZYUserRole.RoleCode = ZYRole.RoleCode
                WHERE ZYUser.UserName = @UserName
                ORDER BY ZYUserRole.Sequence";

                    var parameter = new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                    {
                        Value = userName.Trim()
                    };

                    DataTable dtResult = _dbHelper.ExecuteQuery(
                        _systemDbConnectionString,
                        commandText,
                        new[] { parameter }
                    );

                    foreach (DataRow row in dtResult.Rows)
                    {
                        userRoles.Add(new UserRole
                        {
                            RoleCode = row["RoleCode"]?.ToString() ?? "",
                            Description = row["Description"]?.ToString() ?? "",
                            Icon = row["Icon"]?.ToString()?.Trim() ?? ""
                        });
                    }
                }
                else
                {
                    // VR006: Special case for administrator
                    userRoles.Add(new UserRole
                    {
                        RoleCode = "05_ROLE",
                        Description = "SYSTEM",
                        Icon = "role1.png"
                    });
                }

                user.UserRoles = userRoles;
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "GetUserRoles",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }
        }


        public void SaveUserLoginData(User userOb, ref MessageSet message)
        {
            SaveLoginData(userOb, ref message);
        }

        public List<UserMenu> GetUserMenu(string userName, string roleCode, ref MessageSet message)
        {
            var userMenus = new List<UserMenu>();

            try
            {
                string commandText = @"
            SELECT ZYRoleMenu.MenuCode, 
                   ZYMenuHeader.Description, 
                   ZYMenuHeader.Icon
            FROM ZYRoleMenu
            INNER JOIN ZYUserRole ON ZYRoleMenu.RoleCode = ZYUserRole.RoleCode
            INNER JOIN ZYMenuHeader ON ZYRoleMenu.MenuCode = ZYMenuHeader.MenuCode
            WHERE ZYUserRole.UserName = @UserName
              AND ZYRoleMenu.RoleCode = @RoleCode
            ORDER BY ZYRoleMenu.Sequence";

                var parameters = new[]
                {
            new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName.Trim() },
            new SqlParameter("@RoleCode", SqlDbType.NVarChar) { Value = roleCode.Trim() }
        };

                DataTable dt = _dbHelper.ExecuteQuery(
                    _systemDbConnectionString,
                    commandText,
                    parameters
                );

                foreach (DataRow row in dt.Rows)
                {
                    userMenus.Add(new UserMenu
                    {
                        MenuCode = row["MenuCode"]?.ToString() ?? "",
                        Description = row["Description"]?.ToString() ?? "",
                        Icon = row["Icon"]?.ToString()?.Trim() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "GetUserMenu",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }

            return userMenus;
        }

        public List<UserTask> GetUserTask(string menuCode, string userName, ref MessageSet message)
        {
            var userTasks = new List<UserTask>();

            try
            {
                string commandText = @"
            SELECT 
                ZYTask.TaskCode,
                ZYTask.Caption,
                ZYTask.Description,
                ZYTask.ExecutionScript,
                ZYTask.Icon,
                ISNULL(ZYTask.TaskType, '') AS TaskType,
                ISNULL(ZYTask.ExclusivityMode, '') AS ExclusivityMode,
                ISNULL(ZYTask.ApplicationCode, '') AS ApplicationCode
            FROM ZYTask
            INNER JOIN ZYMenuDetail ON ZYTask.TaskCode = ZYMenuDetail.TaskCode
            WHERE ZYMenuDetail.MenuCode = @MenuCode
            ORDER BY ZYMenuDetail.Sequence";

                var parameter = new SqlParameter("@MenuCode", SqlDbType.NVarChar)
                {
                    Value = menuCode.Trim()
                };

                DataTable dt = _dbHelper.ExecuteQuery(
                    _systemDbConnectionString,
                    commandText,
                    new[] { parameter }
                );

                var dllsInSiteBin = AppDomain.CurrentDomain.GetAssemblies().ToList();

                foreach (DataRow row in dt.Rows)
                {
                    string taskCode = row["TaskCode"]?.ToString().Trim() ?? "";
                    string executionScript = row["ExecutionScript"]?.ToString() ?? "";
                    bool isV2Component = executionScript.Contains(".aspx");
                    string version = "0.0.0.0";

                    if (!isV2Component)
                    {
                        var assemblyVersion = dllsInSiteBin.FindAll(a => a.FullName.Contains(taskCode));
                        if (assemblyVersion.Count > 0)
                        {
                            version = assemblyVersion[0].GetName().Version.ToString();
                            executionScript += "?v=" + version;
                        }
                    }

                    userTasks.Add(new UserTask
                    {
                        TaskCode = taskCode,
                        Caption = row["Caption"]?.ToString() ?? "",
                        Description = row["Description"]?.ToString() ?? "",
                        ExecutionScript = executionScript,
                        Icon = row["Icon"]?.ToString()?.Trim() ?? "",
                        TaskType = row["TaskType"]?.ToString() ?? "",
                        UserName = userName.Trim(),
                        ExclusivityMode = row["ExclusivityMode"]?.ToString()?.Trim() ?? "",
                        ApplicationCode = row["ApplicationCode"]?.ToString()?.Trim() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "GetUserTask",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }

            return userTasks;
        }
        private string SaveLoginData(User userOb, ref MessageSet message)
        {
            try
            {
                string commandText = @"
            INSERT INTO [ZYPasswordLoginDetails]
                ([UserName], [Date], [Time], [Password], [WorkstationID], [SuccessfulLogin], [SessionID], [Reson])
            VALUES
                (@UserName, @Date, @Time, @Password, @WorkstationID, @SuccessfulLogin, @SessionID, @Reson)";

                var parameters = new[]
                {
            new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
            {
                Value = userOb.UserName.Trim()
            },
            new SqlParameter("@Date", SqlDbType.Char, 8)
            {
                Value = DateTime.Now.ToString("yyyyMMdd")
            },
            new SqlParameter("@Time", SqlDbType.Char, 8)
            {
                Value = DateTime.Now.ToLongTimeString()
            },
            new SqlParameter("@Password", SqlDbType.NVarChar, 255)
            {
                Value = userOb.Password
            },
            new SqlParameter("@WorkstationID", SqlDbType.NVarChar, 50)
            {
                Value = userOb.WorkstationId.Trim()
            },
            new SqlParameter("@SuccessfulLogin", SqlDbType.Char, 1)
            {
                Value = userOb.SuccessfulLogin
            },
            new SqlParameter("@SessionID", SqlDbType.NVarChar, 50)
            {
                Value = userOb.SessionId.Trim()
            },
            new SqlParameter("@Reson", SqlDbType.NVarChar, 255)
            {
                Value = string.IsNullOrEmpty(userOb.Reson) ? (object)DBNull.Value : userOb.Reson.Trim()
            }
        };

                _dbHelper.ExecuteNonQuery(
                    _systemDbConnectionString,
                    commandText,
                    parameters
                );
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "SaveLoginData",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }

            return string.Empty;
        }

        public List<UserRole> GetUserRole(string userName, ref MessageSet message)
        {
            var userRoles = new List<UserRole>();

            try
            {
                if (userName.Trim() != "administrator")
                {
                    string commandText = @"
                SELECT ZYUserRole.RoleCode, ZYRole.Description, ZYRole.Icon
                FROM ZYUser
                INNER JOIN ZYUserRole ON ZYUser.UserName = ZYUserRole.UserName
                INNER JOIN ZYRole ON ZYUserRole.RoleCode = ZYRole.RoleCode
                WHERE ZYUser.UserName = @UserName
                ORDER BY ZYUserRole.Sequence";

                    var parameter = new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                    {
                        Value = userName.Trim()
                    };

                    DataTable dtResult = _dbHelper.ExecuteQuery(
                        _systemDbConnectionString,
                        commandText,
                        new[] { parameter }
                    );

                    foreach (DataRow row in dtResult.Rows)
                    {
                        userRoles.Add(new UserRole
                        {
                            RoleCode = row["RoleCode"]?.ToString() ?? "",
                            Description = row["Description"]?.ToString() ?? "",
                            Icon = row["Icon"]?.ToString()?.Trim() ?? ""
                        });
                    }
                }
                else
                {
                    userRoles.Add(new UserRole
                    {
                        RoleCode = "05_ROLE",
                        Description = "SYSTEM",
                        Icon = "role1.png"
                    });
                }
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "GetUserRole",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }


            return userRoles;
        }
        public void UpdateDailyMenu(string menuCode, ref MessageSet message)
        {
            string commandText = @"
        UPDATE dbo.ZYMenuHeader
        SET LastAutoexecutedOn = @CurrentDateTime
        WHERE MenuCode = @MenuCode";

            var parameters = new[]
            {
        new SqlParameter("@CurrentDateTime", SqlDbType.DateTime)
        {
            Value = DateTime.Now
        },
        new SqlParameter("@MenuCode", SqlDbType.NVarChar, 50)
        {
            Value = menuCode?.Trim() ?? ""
        }
    };

            try
            {
                _dbHelper.ExecuteNonQuery(
                    _systemDbConnectionString,
                    commandText,
                    parameters
                );
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "UpdateDailyMenu",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }
        }
        public bool CheckDailyMenu(string menuCode, ref MessageSet message)
        {
            bool available = false;

            string commandText = @"
        SELECT MenuCode
        FROM ZYMenuHeader
        WHERE MenuCode = @MenuCode
          AND (LastAutoexecutedOn IS NULL OR CAST(LastAutoexecutedOn AS DATE) < CAST(@CurrentDateTime AS DATE))";

            var parameters = new[]
            {
        new SqlParameter("@CurrentDateTime", SqlDbType.DateTime)
        {
            Value = DateTime.Now
        },
        new SqlParameter("@MenuCode", SqlDbType.NVarChar, 50)
        {
            Value = menuCode?.Trim() ?? ""
        }
    };

            try
            {
                DataTable result = _dbHelper.ExecuteQuery(
                    _systemDbConnectionString,
                    commandText,
                    parameters
                );

                available = result.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                    0,
                    ex,
                    "CheckDailyMenu",
                    "XONT.Ventura.AppConsole.DAL.dll"
                );
                Console.WriteLine(ex);
            }

            return available;
        }

        public DataTable GetUnAuthorizedTasks(string userName, ref MessageSet message)
        {
            try
            {
                string spName = "[dbo].[usp_TaskGatewayGetUnAuthorizedTasks]";

                var parameters = new[]
                {
                new SqlParameter("@UserName", SqlDbType.NVarChar, 50)
                { Value = (object)userName ?? DBNull.Value }
                };

                return _dbHelper.ExecuteStoredProcedure(
                    _systemDbConnectionString,
                    spName,
                    parameters
                );

            }
            catch (Exception ex)
            {
                message = MessageCreate.CreateErrorMessage(
                 0,
                 ex,
                 "GetUnAuthorizedTasks",
                 "XONT.Ventura.AppConsole.DAL.dll"
             );
                Console.WriteLine(ex);
                return new DataTable();
            }
        }
    }
}