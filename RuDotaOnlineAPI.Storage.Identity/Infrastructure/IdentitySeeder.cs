using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RuDotaOnlineAPI.Storage.Identity.Abstraction.Entities;

namespace RuDotaOnlineAPI.Storage.Identity.Infrastructure;

/// <summary>
/// Создаёт базовые роли и при необходимости первого admin-пользователя.
/// Вызывается из Program.cs после auto-migrate.
/// Идемпотентен — повторный вызов безопасен.
/// </summary>
public sealed class IdentitySeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roles;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        RoleManager<IdentityRole<Guid>> roles,
        UserManager<ApplicationUser> users,
        ILogger<IdentitySeeder> logger)
    {
        _roles = roles;
        _users = users;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureRoleAsync("Admin");
        await EnsureRoleAsync("Moderator");
        await EnsureRoleAsync("User");
    }

    private async Task EnsureRoleAsync(string name)
    {
        if (await _roles.RoleExistsAsync(name)) return;

        var result = await _roles.CreateAsync(new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = name,
        });

        if (result.Succeeded)
            _logger.LogInformation("Role created: {Role}", name);
        else
            _logger.LogError("Failed to create role {Role}: {Errors}", name,
                string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}