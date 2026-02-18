using ArchitectureAI.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchitectureAI.Api.Controllers;

[ApiController]
[Route("api/users")]
// Instead of Roles="Admin", we use Policy="users.view" which checks for the permission claim
public class UsersController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.Users.View)]
    public IActionResult GetAllUsers()
    {
        return Ok(new { message = "You have permission to view users." });
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Users.Create)]
    public IActionResult CreateUser()
    {
        return Ok(new { message = "You have permission to create users." });
    }
}
