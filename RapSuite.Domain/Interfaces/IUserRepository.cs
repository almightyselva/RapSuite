using RapSuite.Domain.Entities;

namespace RapSuite.Domain.Interfaces;

public interface IUserRepository
{
    Task SaveProfileAsync(string userId, AppUser user);
}
