using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RPGCompanion.Server.Infrastructure;
using RPGCompanion.Server.Infrastructure.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RpgCompanionContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication()
    .AddJwtBearer(o =>
    {
        var c = builder.Configuration;
        o.TokenValidationParameters = new()
        {
            ValidIssuer = c["Jwt:Issuer"],
            ValidAudience = c["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(c["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RPG Companion API",
        Version = "v1"
    });

    // 1) Define the security scheme
    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "JWT only",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme, // "Bearer"
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [jwtScheme] = Array.Empty<string>()
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/ping-db", async (RpgCompanionContext db) =>
    $"Users table has {await db.Users.CountAsync()} rows");

app.MapGet("/whoami", [Authorize] (ClaimsPrincipal user) =>
{
    // `user` is populated only if the JWT is valid
    var email = user.FindFirstValue(ClaimTypes.Email);
    var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value);
    return new
    {
        Email = email,
        Roles = roles
    };
});

app.Run();
