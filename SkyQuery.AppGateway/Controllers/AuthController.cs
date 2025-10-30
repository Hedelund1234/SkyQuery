using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyQuery.AppGateway.Domain.Entities.Auth;
using System.Net;
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

        [HttpPost("assign-role")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignmentRequest model)
        {
            _logger.LogInformation("AssignRole has been called to assign {model.Role} to user with Email {model.Email}", model.Role, model.Email);

            try
            {
                // Gets token from call and sends with new call
                var authHeader = Request.Headers["Authorization"].ToString();
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // 1) Create request body without body
                var request = _daprClient.CreateInvokeMethodRequest(
                    HttpMethod.Post,
                    "skyquery-authservice-dapr",
                    "auth/assign-role");


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
                _logger.LogError(ex, "Error trying to assign role");
                return BadRequest(ex);
            }
        }

        [HttpPost("remove-role")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveRole([FromBody] RoleAssignmentRequest model)
        {
            _logger.LogInformation("RemoveRole has been called to assign {model.Role} to user with Email {model.Email}", model.Role, model.Email);

            try
            {
                // Gets token from call and sends with new call
                var authHeader = Request.Headers["Authorization"].ToString();
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // 1) Create request body without body
                var request = _daprClient.CreateInvokeMethodRequest(
                    HttpMethod.Post,
                    "skyquery-authservice-dapr",
                    "auth/remove-role");


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
                _logger.LogError(ex, "Error trying to remove role");
                return BadRequest(ex);
            }
        }

        [HttpPut("update-user")] // TODO: Test if working with postman setup
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest model)
        {
            _logger.LogInformation("UpdateUser has been called for user: {model.UserId}", model.UserId);

            try
            {
                // Gets token from call and sends with new call
                var authHeader = Request.Headers["Authorization"].ToString();
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // 1) Create request body without body
                var request = _daprClient.CreateInvokeMethodRequest(
                    HttpMethod.Put,
                    "skyquery-authservice-dapr",
                    "auth/update-user");


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
                _logger.LogError(ex, "Error trying update user {model.UserId}", model.UserId);
                return BadRequest(ex);
            }
        }

        [HttpDelete("delete-user/{userId}")] // TODO: Test if working with postman setup
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            _logger.LogInformation("DeleteUser has been called for user: {userId}", userId);

            try
            {
                // Gets token from call and sends with new call
                var authHeader = Request.Headers["Authorization"].ToString();
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // 1) Create request body without body
                var request = _daprClient.CreateInvokeMethodRequest(
                    HttpMethod.Delete,
                    "skyquery-authservice-dapr",
                    $"auth/delete-user/{userId}");

                // 2) Adds Authorization: Beaerer <token>
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); // app.UseHttpsRedirection(); on recieving end (AuthService) has to be disabled in development environment

                // 3) Send
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
                _logger.LogError(ex, "Error trying to delete user {userId}", userId);
                return BadRequest(ex);
            }
        }

        [HttpGet("all-users")] // TODO: Test if working with postman setup
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("GetAllUsers has been called");

            try
            {
                // Gets token from call and sends with new call
                var authHeader = Request.Headers["Authorization"].ToString();
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // 1) Create request body without body
                var request = _daprClient.CreateInvokeMethodRequest(
                    HttpMethod.Get,
                    "skyquery-authservice-dapr",
                    "auth/all-users");

                // 2) Adds Authorization: Beaerer <token>
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); // app.UseHttpsRedirection(); on recieving end (AuthService) has to be disabled in development environment


                // 3) Send
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
                _logger.LogError(ex, "Error trying to GetAllUsers");
                return BadRequest(ex);
            }
        }
    }
}
