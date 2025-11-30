using Microsoft.AspNetCore.Identity;

namespace depi_real_state_management_system
{
    public class RolesSeedData
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RolesSeedData(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }


        public async Task AddRoles()
        {
            List<string> roles = new List<string>()
            {
                "Admin", "Tenant", "Manager"
            };

            foreach (var role in roles)
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
