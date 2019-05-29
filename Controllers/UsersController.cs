using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpsReady.DAL.Models.User;
using OpsReady.Models;
using Remotion.Linq.Clauses;
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
        public IActionResult Authenticate([FromBody]User userParam)
        {
            var user = _userService.Authenticate(userParam.PersonalNumber, userParam.HashedPassword);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            var mp = MessagePackSerializer.Serialize(users, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            var str = Convert.ToBase64String(mp);
            return Ok(str);
        }

        [HttpGet("sentdtoken")]
        public IActionResult SendTokenByMail()
        {
            Debug.WriteLine("Pretend we're sending a mail with the generated token");

            return Ok();
        }

        [HttpPost("setpassword")]
        public IActionResult SetPassword([FromBody]string password)
        {
            var re = Request;
            var header = re.Headers["Authorization"];
            var token = header[0].Split("Bearer ")[1];
            
            Debug.WriteLine(token);

            
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            
            var user = _userService.GetAll().FirstOrDefault(i => i.Id.ToString() == userId);
                Debug.WriteLine(user.FirstName);
            
            Debug.WriteLine("password");

            return Ok();
        }
    }
}
