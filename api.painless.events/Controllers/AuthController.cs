using api.painless.events.Core;
using api.painless.events.DTOs;
using api.painless.events.Entities;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace api.painless.events.Controllers
{

    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [EnableRateLimiting("TenInTwenty")]
    public class AuthController : Controller
    {

        private readonly IConfiguration _configuration;
        private ReadContext _readContext;
        private WriteContext _writeContext;
        private SecurityContext _securityContext;
        private HelperFunctions _helperFunctions;

        public AuthController(IConfiguration configuration, ReadContext readContext, WriteContext writeContext, SecurityContext securityContext, HelperFunctions helperFunctions)
        {
            _configuration = configuration;
            _readContext = readContext;
            _writeContext = writeContext;
            _securityContext = securityContext;
            _helperFunctions = helperFunctions;
        }


        /// <summary>
        /// This endpoint will return the account and event IDs together with some IP information for the time zone if possible.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpGet("Init")]
        public async Task<IActionResult> Init(string url)
        {
            InitDto iDto = new InitDto();
            iDto.AccountId = -1;
            String Ip = Request?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString() ?? "";
            (iDto.SortedNodes, iDto.UnsortedNodes, iDto.IpTimeZone, iDto.Latitude, iDto.Longitude) = await _helperFunctions.GetIpInfo(Ip);
            url = url.ToLower()
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace("painless.events/", "|")
                .Replace("localhost:5173/", "|")
                .Replace("/", "|");
            string[] urlParts = url.Split("|");
            if (urlParts.Length >= 1)
            {
                if (urlParts[0].Trim() == "" || urlParts[0].Trim() == "www.")
                    iDto.AccountId = 0;
                else
                {
                    Account? account = await _readContext.Accounts.FirstOrDefaultAsync(x => x.Domain.ToLower().Trim() == urlParts[0].Replace(".","").Trim());
                    if (account != null)
                    {
                        iDto.AccountId = account.Id;
                        if (urlParts.Length >= 2)
                        {
                            Event evnt = await (from e in _readContext.Events where e.AccountId == account.Id && e.Url == urlParts[1] select e).FirstOrDefaultAsync() ?? new Event();
                            if (evnt.Id > 0)
                            {
                                iDto.EventId = evnt.Id;
                                iDto.EventTimeZone = evnt.TimeZone;
                            }
                        }
                    }
                }
            }
            return Ok(iDto);
        }



        /// <summary>
        /// This endpoint will return a new AccessToken
        /// </summary>
        /// <param name="RefreshToken"></param>
        /// <returns></returns>
        [HttpGet("Refresh")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            TokenDto tkn = _securityContext.ReadToken(refreshToken);
            if (tkn.Username == "401")
                return Unauthorized();
            User usr = await _readContext.Users.FirstOrDefaultAsync(x => x.Id == tkn.Id);
            if (usr == null)
                return Unauthorized();
            string newAccessToken = await _securityContext.CreateToken(usr, 900); // 15 minutes
            return Ok(newAccessToken);
            //return Ok(tkn);
        }





        /// <summary>
        /// This Endpoint lets you register a new account
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A JSON containing an AccessToken, RefreshToken, Id, Domain, Email, Firstname and Lastname</returns>
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAccount(RegisterAccountDto request)
        { 
            if (request.Domain == null || request.Email == null || request.Firstname == null || request.Lastname == null || request.Password == null || request.Password2 == null)
                return BadRequest("Missing required fields");
            if (request.Domain == "" || request.Email == "" || request.Firstname == "" || request.Lastname == "" || request.Password == "" || request.Password2 == "")
                return BadRequest("Missing required fields");
            if (request.Password != request.Password2)
                return BadRequest("Passwords do not match");
            if (request.Domain.Length > 45 || request.Domain.Length < 3)
                return BadRequest("Domain must be between 3 and 45 characters");
            if (request.Email.Length > 320 || request.Email.Length < 5)
                return BadRequest("Email must be between 5 and 320 characters");
            if (request.Firstname.Length > 100 || request.Firstname.Length < 2)
                return BadRequest("Firstname must be between 2 and 100 characters");
            if (request.Lastname.Length > 100 || request.Lastname.Length < 2)
                return BadRequest("Lastname must be between 2 and 100 characters");
            if (!Regex.IsMatch(request.Email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
                return BadRequest("Invalid email format");
            if (await _readContext.Users.AnyAsync(x => x.Email.ToLower().Trim() == request.Email.ToLower().Trim()))
                return BadRequest("Email already in use");
            if (await _readContext.Accounts.AnyAsync(x => x.Domain.ToLower().Trim() == request.Domain.ToLower().Trim()))
                return BadRequest("Domain already in use");
            using (var fileStream = new FileStream("banned_domains.txt", FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line == request.Domain)
                            return BadRequest("Domain is not available");
                    }
                }
            }
            byte[] passwordHash = null;
            byte[] passwordSalt = null;
            _securityContext.CreatePasswordHash(request.Password, out passwordHash, out passwordSalt);
            User usr = new User
            {
                AccountId = 0,
                EventId = 0,
                Username = request.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Email = request.Email.ToLower().Trim(),
                EmailVerified = 0,
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                AvatarUrl = "",
                Enabled = 1,
                Deleted = 0,
                Created = DateTime.UtcNow
            };
            await _writeContext.Users.AddAsync(usr);
            await _writeContext.SaveChangesAsync();
            Account account = new Account
            {
                Guid = Guid.NewGuid().ToString(),
                Domain = request.Domain.ToLower().Trim(),
                OwnerId = usr.Id,
                Enabled = 1,
                Deleted = 0,
                Created = DateTime.UtcNow
            };
            await _writeContext.Accounts.AddAsync(account);
            await _writeContext.SaveChangesAsync();
            usr.AccountId = account.Id;
            _writeContext.Users.Update(usr);
            await _writeContext.SaveChangesAsync();
            UserRole ur = new UserRole
            {
                UserId = usr.Id,
                Role = "Owner",
                Created = DateTime.UtcNow
            };
            await _writeContext.UserRoles.AddAsync(ur);
            await _writeContext.SaveChangesAsync();
            Event evt = new Event();
            evt.AccountId = account.Id;
            InitDto iDto = new InitDto();
            String Ip = Request?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString() ?? "";
            (iDto.SortedNodes, iDto.UnsortedNodes, iDto.IpTimeZone, iDto.Latitude, iDto.Longitude) = await _helperFunctions.GetIpInfo(Ip);
            TokenDto tkn = new TokenDto()
            {
                AccessToken = await _securityContext.CreateToken(usr, 900),
                RefreshToken = await _securityContext.CreateToken(usr, 86400),
                Id = usr.Id,
                AccountId = account.Id,
                EventId = evt.Id,
                Domain = account.Domain,
                Url = evt.Url,
                Username = usr.Username,
                FirstName = usr.Firstname,
                LastName = usr.Lastname,
                IpTimeZone = iDto.IpTimeZone,
                EventTimeZone = iDto.EventTimeZone,
                Roles = new string[] { "Owner" },
                UnsortedNodes = iDto.UnsortedNodes,
                SortedNodes = iDto.SortedNodes
            };
            return Ok(tkn);
        }


        /// <summary>
        /// This Endpoint lets you login to your account
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A JSON containing an AccessToken, RefreshToken, Id, Domain, Email, Firstname and Lastname</returns>
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAccount(LoginAccountDto request)
        {
            if (request.Username == null || request.Password == null)
                return BadRequest("Missing required fields");
            if (request.Username == "" || request.Password == "")
                return BadRequest("Missing required fields");
            if (request.Persistent != 1)
                request.Persistent = 0;
            int AccountId = request.AccountId;
            if (AccountId < 0) AccountId = 0;
            int EventId = request.EventId;
            if (EventId < 0) EventId = 0;
            Account account = new Account();
            account = await (from a1 in _readContext.Accounts where a1.Id == AccountId && a1.Enabled == 1 && a1.Deleted == 0 select a1).FirstOrDefaultAsync() ?? new Account();
            Event evt = await (from e1 in _readContext.Events where e1.Id == EventId && e1.AccountId == account.Id && e1.Enabled == 1 && e1.Deleted == 0 select e1).FirstOrDefaultAsync() ?? new Event();
            User usr = new User();
            if (AccountId==0 && EventId==0)
                usr = await (from u1 in _readContext.Users where u1.Username == request.Username.ToLower().Trim() && u1.EventId == 0 && u1.Enabled == 1 && u1.Deleted == 0 select u1).FirstOrDefaultAsync() ?? new User();
            else
                usr = await (from u1 in _readContext.Users where u1.Username == request.Username.ToLower().Trim() && u1.AccountId == account.Id && u1.EventId == evt.Id && u1.Enabled == 1 && u1.Deleted == 0 select u1).FirstOrDefaultAsync() ?? new User();
            if (usr.Id == 0)
                return BadRequest("Invalid email or password");
            if (_securityContext.VerifyPasswordHash(request.Password, usr.PasswordHash, usr.PasswordSalt) == false)
                return BadRequest("Invalid email or password");
            List<UserRole> roles = await (from r1 in _readContext.UserRoles where r1.UserId == usr.Id select r1).ToListAsync();
            InitDto iDto = new InitDto();
            String Ip = Request?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString() ?? "";
            (iDto.SortedNodes, iDto.UnsortedNodes, iDto.IpTimeZone, iDto.Latitude, iDto.Longitude) = await _helperFunctions.GetIpInfo(Ip);
            string refreshToken = "";
            string accessToken = "";
            if (request.Persistent == 1)
            {
                refreshToken = await _securityContext.CreateToken(usr, 1814400); // 21 days
                accessToken = await _securityContext.CreateToken(usr, 900); // 15 minutes
            }
            else
            {
                refreshToken = await _securityContext.CreateToken(usr, 86400); // 1 day
                accessToken = await _securityContext.CreateToken(usr, 900); // 15 minutes
            }
            TokenDto tkn = new TokenDto()
            {
                Id = usr.Id,
                AccountId = account.Id,
                EventId = evt.Id,
                RefreshToken = refreshToken,
                AccessToken = accessToken,
                Domain = account.Domain,
                Url = evt.Url,
                Username = usr.Username,
                FirstName = usr.Firstname,
                LastName = usr.Lastname,
                IpTimeZone = iDto.IpTimeZone,
                EventTimeZone = evt.TimeZone,
                Roles = roles.ToArray().Select(x => x.Role).ToArray(),
                UnsortedNodes = iDto.UnsortedNodes,
                SortedNodes = iDto.SortedNodes
            };
            await Task.Delay(3000);
            return Ok(tkn);
        }


        [HttpGet("DomainAvailable")]
        public async Task<IActionResult> GetDomainAvailibility(string domain)
        { 
            if (string.IsNullOrEmpty(domain))
                return BadRequest("Please enter a domain");
            if (domain.Length > 45 || domain.Length < 4)
                return BadRequest("Domain must be between 4 and 45 characters");
            if (Regex.IsMatch(domain, @"[^a-zA-Z0-9\-_]"))
                return BadRequest("Domain can only contain letters, numbers, hyphens and underscores");
            if (await _readContext.Accounts.AnyAsync(x => x.Domain.ToLower().Trim() == domain.ToLower().Trim()))
                return BadRequest("Domain is not available");
            using (var fileStream = new FileStream("banned_domains.txt", FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line == domain)
                            return BadRequest("Domain is not available");
                    }
                }
            }
            return Ok();
        }




        [Authorize]
        [HttpGet("Test")]
        public async Task<IActionResult> GetTest()
        {
            return Ok("Test");
        }




    }
}
