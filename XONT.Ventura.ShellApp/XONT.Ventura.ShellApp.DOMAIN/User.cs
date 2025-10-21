using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XONT.Ventura.ShellApp.DOMAIN
{
    public class User
    {
        public string BusinessUnit { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        // public byte[] TimeStamp { get; set; }
        public string UserFullName { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public List<UserMenu> UserMenus { get; set; }
        public List<UserTask> UserTasks { get; set; }
        public bool isExists { get; set; }
        public string UserLevelGroup { get; set; }
        public string WorkstationId { get; set; }
        public string SuccessfulLogin { get; set; }
        public string SessionId { get; set; }
        public string LodOutMethod { get; set; }
        public string Reson { get; set; }
        public bool IsError { get; set; }
        public string AlreadyLogin { get; set; }
        public string PowerUser { get; set; }
        public bool ActiveFlag { get; set; }
        public bool IsPassSuccess { get; set; }
        public Password PasswordData { get; set; }
        public char PasswordLocked { get; set; }
        public bool IsPassExpire { get; set; }
        public bool IsUserExpire { get; set; }
        public string Theme { get; set; }
        public string DefaultRoleCode { get; set; }
        public string DefaultMenuCode { get; set; }
        //public string MenuCode { get; set; }  //jan08
        public bool Enabled { get; set; }
        public bool CanModify { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public int CharacterSet { get; set; }
        public bool UseTranslateTable { get; set; }
        public int PaperSize { get; set; }
        public string Language { get; set; }
        public string FontColor { get; set; }
        public string ActiveFlagDescription { get; set; }

        public string DistributorCode { get; set; } //VR002 Added

        public string RestrictFOCInvoice { get; set; }//VR013
        public string ExecutiveCode { get; set; }//VR016
        public char HasProPicture { get; set; }//new Ventura
        public string SupplierCode { get; set; }//VR024
        public string CustomerCode { get; set; }//VR024
        public string PasswordChange { get; set; }


        #region//For the modifications in ZYMNT05(User Maintenance)
        public string NICNo { get; set; }
        public string TelephoneNo { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string PostalCode { get; set; }
        public bool IsAutoUnlock { get; set; }
        public bool IsPowerUser { get; set; }
        public byte[] UserImage { get; set; }

        public string POReturnAuthorizationLevel { get; set; } //V2042

        public string POReturnAuthorizationUpTo { get; set; }  //V2042

        public string FirstName { get; set; } //V2046
        public string LastName { get; set; } //V2046
        public string Salutation { get; set; } //V2046
        public DateTime DateOfBirth { get; set; } //V2046

        public string PUSQQtyEdit { get; set; }//V2048
        /// <summary>
        /// do NOT(වලකින්න) populate this when you hard code the sessions on the page.
        /// </summary>
        public bool CreatedByAppConsole { get; set; }//V2025
        public bool CaptionEditor { get; set; }//V233Added
        #endregion
    }
}
