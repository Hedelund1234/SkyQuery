using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.AuthService.Domain.Entities.Auth
{
    public class UpdateUser
    {
        public string UserId { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
