using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.AuthService.Infrastructure.Data
{
    public class AuthServiceDbContext : IdentityDbContext<IdentityUser>
    {
        public AuthServiceDbContext(DbContextOptions<AuthServiceDbContext> options) : base(options) { }
    }
}
