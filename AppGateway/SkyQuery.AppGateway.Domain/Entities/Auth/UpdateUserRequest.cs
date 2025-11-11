using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.AppGateway.Domain.Entities.Auth
{
    public class UpdateUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
