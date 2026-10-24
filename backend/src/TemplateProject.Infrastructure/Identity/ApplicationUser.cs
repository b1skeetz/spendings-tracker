using Microsoft.AspNetCore.Identity;

namespace TemplateProject.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? Name { get; set; }
    public int? Age { get; set; }
    public DateTime RegistrationDate { get; set; }
    
    public List<ApplicationUserRole> UserRoles { get; set; } = [];
}
