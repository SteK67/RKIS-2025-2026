using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class ProfileRepository
    {
        public List<Profile> GetAll()
        {
            using var context = new AppDbContext();
            return context.Profiles
                .AsNoTracking()
                .OrderBy(profile => profile.Login)
                .ToList();
        }

        public Profile? GetById(Guid id)
        {
            using var context = new AppDbContext();
            return context.Profiles
                .AsNoTracking()
                .FirstOrDefault(profile => profile.Id == id);
        }

        public Profile? GetByCredentials(string login, string password)
        {
            using var context = new AppDbContext();
            return context.Profiles
                .AsNoTracking()
                .FirstOrDefault(profile => profile.Login == login && profile.Password == password);
        }

        public bool ExistsByLogin(string login)
        {
            using var context = new AppDbContext();
            return context.Profiles.Any(profile => profile.Login == login);
        }

        public void Add(Profile profile)
        {
            using var context = new AppDbContext();
            context.Profiles.Add(profile);
            context.SaveChanges();
        }

        public void Update(Profile profile)
        {
            using var context = new AppDbContext();
            context.Profiles.Update(profile);
            context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            using var context = new AppDbContext();
            var profile = context.Profiles.FirstOrDefault(item => item.Id == id);
            if (profile == null)
            {
                return;
            }

            context.Profiles.Remove(profile);
            context.SaveChanges();
        }

        public void ReplaceAll(IEnumerable<Profile> profiles)
        {
            using var context = new AppDbContext();
            context.Profiles.RemoveRange(context.Profiles);
            context.Profiles.AddRange(profiles);
            context.SaveChanges();
        }
    }
}
