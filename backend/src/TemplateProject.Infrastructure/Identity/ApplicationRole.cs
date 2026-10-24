using Microsoft.AspNetCore.Identity;

namespace TemplateProject.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public List<ApplicationUserRole> UserRoles { get; set; } = [];
}
