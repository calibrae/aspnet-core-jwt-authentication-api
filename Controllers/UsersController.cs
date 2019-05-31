using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpsReady.DAL.Models.User;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] string personalNumber)
        {
            var user = _userService.Authenticate(personalNumber);

            if (user == null)
                return BadRequest(new {message = "Username or password is incorrect"});

            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
//            var mp = MessagePackSerializer.Serialize(users,
//                ContractlessStandardResolver.Instance);
//            var str = Convert.ToBase64String(mp);
            return Ok(users);
        }

        [HttpGet("sendtoken")]
        public IActionResult SendTokenByMail()
        {
            Debug.WriteLine("Pretend we're sending a mail with the generated token");

            var user = GetUserFromRequest(User);

            user.MailToken = "1234";

            return Ok("1234");
        }

        [HttpPost("authtoken")]
        public IActionResult CheckPersonalNumberAndToken([FromBody] User user)
        {
            var reqUser = GetUserFromRequest(User);

            if (reqUser.MailToken == user.MailToken && reqUser.PersonalNumber == user.PersonalNumber)
            {
                return Ok(user);
            }

            return Unauthorized("invalid user");
        }

        [HttpPost("setpassword")]
        public IActionResult SetPassword([FromBody] string password)
        {
            var user = GetUserFromRequest(User);

            user.HashedPassword = password;

            return Ok();
        }

        private User GetUserFromRequest(ClaimsPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            if (userId == null)
            {
                throw new ArgumentException("User cannot be found");
            }

            return _userService.Users.FirstOrDefault(i => i.Id.ToString() == userId) as User;
        }
    }
}