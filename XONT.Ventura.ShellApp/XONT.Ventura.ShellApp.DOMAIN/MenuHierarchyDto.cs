using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XONT.Ventura.ShellApp.DOMAIN
{
    public class MenuHierarchyDto
    {
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsPriorityRole { get; set; }
        public bool IsDefaultRole { get; set; }
        public List<MenuGroupDto> MenuGroups { get; set; } = new();
        public int TotalTasks { get; set; }
    }

    public class MenuGroupDto
    {
        public string MenuCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsVisible { get; set; } = true;
        public List<MenuTaskDto> Tasks { get; set; } = new();
        public string? RoleCode { get; set; }
    }

    public class MenuTaskDto
    {
        public string TaskCode { get; set; } = string.Empty;
        public string MenuCode { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string ApplicationCode { get; set; } = string.Empty;
        public int ExclusivityMode { get; set; }
        public int ExecutionType { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsFavorite { get; set; }
    }
}
