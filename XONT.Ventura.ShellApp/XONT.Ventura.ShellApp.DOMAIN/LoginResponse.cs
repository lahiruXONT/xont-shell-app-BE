using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XONT.Ventura.ShellApp.DOMAIN
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new();
    }

    public class UserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public List<RoleDto> Roles { get; set; } = new();
        public List<BusinessUnitDto> BusinessUnits { get; set; } = new();
        public string CurrentRole { get; set; } = string.Empty;
        public string CurrentBusinessUnit { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; }
    }

    public class RoleDto
    {
        public string RoleCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class BusinessUnitDto
    {
        public string BusinessUnit { get; set; } = string.Empty;
        public string BusinessUnitName { get; set; } = string.Empty;
    }
}
