using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPGCompanion.Server.Infrastructure.DataTransferObjects;
using RPGCompanion.Server.Infrastructure;
using RPGCompanion.Server.Services.Auth;
using RPGCompanion.Server.Infrastructure.Models;
using RPGCompanion.Server.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RpgCompanionContext _db;
    private readonly IConfiguration _cfg;

    public AuthController(RpgCompanionContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Email already registered");

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PasswordHash = PasswordService.Hash(dto.Password),
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var playerRoleId = await _db.Roles
            .Where(r => r.Name == "Player")
            .Select(r => r.RoleId)
            .SingleAsync();

        _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = playerRoleId });
        await _db.SaveChangesAsync();

        return Ok(new { user.UserId });
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !PasswordService.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        if (!user.IsActive) return Unauthorized("Account disabled");
        if (!user.EmailConfirmed) return Unauthorized("Email not verified");

        var roles = user.UserRoles.Select(ur => ur.Role.Name);
        var jwt = JwtTokenGenerator.Generate(user, roles, _cfg);

        return Ok(new { token = jwt });
    }
}
