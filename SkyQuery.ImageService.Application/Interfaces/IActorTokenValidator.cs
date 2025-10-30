using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Application.Interfaces
{
    public interface IActorTokenValidator
    {
        ClaimsPrincipal? Validate(string jwt);
    }
}
