using CityAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityAPI.Data
{
    public class AuthRepository : IAuthRepository
    {
        private DataContext _data;

        public AuthRepository(DataContext data)
        {
            _data = data;
        }
        public async Task<User> Login(string userName, string password)
        {
            var user = await _data.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if(user == null)
            {
                return null;

            }
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var ComputedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < ComputedHash.Length; i++)
                {
                    if (ComputedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
            }
            return true;
            
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _data.Users.AddAsync(user);
            await _data.SaveChangesAsync();

            return user;

        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string userName)
        {
           if(await _data.Users.AnyAsync(x => x.UserName == userName))
            {
                return true;
            }

            return false;
        }
    }
}
