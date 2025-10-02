using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyQuery.AppGateway.Domain.Entities.Auth;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SkyQuery.AppGateway.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly DaprClient _daprClient;

        public AuthController(ILogger<ImageController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            _logger.LogInformation("Login has been called by {model.Email}", model.Email);

            try
            {
                // 1) Create request body without body
                var request = _daprClient.CreateInvokeMethodRequest(
                    HttpMethod.Post,
                    "skyquery-authservice-dapr",
                    "auth/login");

                // 2) Add body as json
                request.Content = JsonContent.Create(model);

                // 3) Send
                var response = await _daprClient.InvokeMethodWithResponseAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return Unauthorized();
                }

                var json = await response.Content.ReadAsStringAsync();

                return Ok(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to login");
                return BadRequest(ex);
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            _logger.LogInformation("Register request made for new user with email {model.Email}", model.Email);

            var authHeader = Request.Headers["Authorization"].ToString();

            try
            {

                // Gets token from call and sends with new call
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // 1) Create request body without body
                var request = _daprClient.CreateInvokeMethodRequest(
                    HttpMethod.Post,
                    "skyquery-authservice-dapr",
                    "auth/register");


                // 2) Add body as json
                request.Content = JsonContent.Create(model); // Content-Type: application/json


                // 3) Adds Authorization: Beaerer <token>
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); // app.UseHttpsRedirection(); on recieving end (AuthService) has to be disabled in development environment


                // 4) Send
                var response = await _daprClient.InvokeMethodWithResponseAsync(request);

                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(body);
                }
                return BadRequest(body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to login");
                return BadRequest(ex);
            }
        }

        //TODO: 
        //[HttpPost("assign-role")]
        //[Authorize(Roles = "admin")]

        //TODO: 
        //[HttpPatch("remove-role")]
        //[Authorize(Roles = "admin")]

        //TODO: 
        //[HttpPut("update-user")]
        //[Authorize(Roles = "admin")]

        //TODO: 
        //[HttpDelete("delete-user/{userId}")]
        //[Authorize(Roles = "admin")]

        //TODO: 
        //[HttpGet("all-users")]
        //[Authorize(Roles = "admin")]
    }
}
