using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class ProfileRepository
{
    public List<Profile> GetAll()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();
        return context.Profiles
            .AsNoTracking()
            .OrderBy(profile => profile.Login)
            .ToList();
    }

    public Profile? GetByCredentials(string login, string password)
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();
        return context.Profiles
            .AsNoTracking()
            .FirstOrDefault(profile => profile.Login == login && profile.Password == password);
    }

    public bool ExistsByLogin(string login)
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();
        return context.Profiles.Any(profile => profile.Login == login);
    }

    public void Add(Profile profile)
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();
        context.Profiles.Add(profile);
        context.SaveChanges();
    }
}
