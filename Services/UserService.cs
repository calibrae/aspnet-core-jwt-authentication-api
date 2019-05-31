using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpsReady.DAL.Models.User;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IUserService
    {
        IUserModel Authenticate(string username);
        IEnumerable<IUserModel> GetAll();
        void SendTokenByMail();
        void SetPassword();
        
        IEnumerable<IUserModel> Users { get; }
    }

    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;

        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
//        private readonly List<IUserModel> _users = new List<IUserModel>
//        {
//            new User {Id = 1, FirstName = "Test", LastName = "User", PersonalNumber = "test", HashedPassword = "test"}
//        };

        public IEnumerable<IUserModel> Users => ListUsers.Users;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IUserModel Authenticate(string username)
        {
            var user = ListUsers.Users.SingleOrDefault(x => x.PersonalNumber == username);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            // remove password before returning
            user.HashedPassword = null;

            return user;
        }

        public IEnumerable<IUserModel> GetAll()
        {
            // return users without passwords
            return ListUsers.Users.Select(x =>
            {
                x.HashedPassword = null;
                return x;
            });
        }


        public void SendTokenByMail()
        {
            
        }

        public void SetPassword()
        {
            
        }
    }

    public class ListUsers
    {
        public static List<IUserModel> Users = new List<IUserModel> {
            new User {Id = 1, FirstName = "Test", LastName = "User", PersonalNumber = "test", HashedPassword = "test"}
        };
    }
}