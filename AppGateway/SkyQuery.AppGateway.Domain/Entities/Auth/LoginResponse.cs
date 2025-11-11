using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.AppGateway.Domain.Entities.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
