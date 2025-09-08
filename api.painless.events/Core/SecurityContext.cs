using api.painless.events.DTOs;
using api.painless.events.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace api.painless.events.Core
{
    public class SecurityContext
    {

        private readonly IConfiguration _configuration;
        private ReadContext _readContext;
        private WriteContext _writeContext;

        public SecurityContext(IConfiguration configuration, ReadContext readContext, WriteContext writeContext)
        {
            _configuration = configuration;
            _readContext = readContext;
            _writeContext = writeContext;
        }



        public async Task <string> CreateToken(User us, int secondsToExpire)
        {
            Account ac = await (from a1 in _readContext.Accounts where a1.Id == us.AccountId select a1).FirstOrDefaultAsync() ?? new Account();
            Event ev = await (from e1 in _readContext.Events where e1.Id == us.EventId select e1).FirstOrDefaultAsync() ?? new Event();
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, us.Firstname));
            claims.Add(new Claim(ClaimTypes.Surname, us.Lastname));
            claims.Add(new Claim(ClaimTypes.Dns, ac.Domain + "/" + ev.Url + "/"));
            claims.Add(new Claim(ClaimTypes.Email, us.Username));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, ac.Id.ToString() + "_" + ev.Id.ToString() + "_" + us.Id.ToString()));
            List<UserRole> roles = await (from r1 in _readContext.UserRoles where r1.UserId == us.Id select r1).ToListAsync();
            foreach (UserRole userrole in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userrole.Role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Appsettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Appsettings:Issuer").Value,
                audience: _configuration.GetSection("Appsettings:Audience").Value,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(secondsToExpire),
                signingCredentials: creds
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public TokenDto ReadToken(string token)
        {
            TokenDto newToken = new TokenDto();
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                newToken.Username = "401";
                return newToken;
            }
            if (jwtToken.Issuer != _configuration.GetSection("Appsettings:Issuer").Value)
            {
                //newToken.Username = "401";
                //return newToken;
            }
            if (jwtToken.Audiences.First() != _configuration.GetSection("Appsettings:Audience").Value)
            {
                //newToken.Username = "401";
                //return newToken;
            }
            try
            {
                List<UserRole> roles = new List<UserRole>();
                foreach (Claim claim in jwtToken.Claims)
                {
                    if (claim.Type == ClaimTypes.Name)
                        newToken.FirstName = claim.Value;
                    if (claim.Type == ClaimTypes.Surname)
                        newToken.LastName = claim.Value;
                    if (claim.Type == ClaimTypes.NameIdentifier)
                    {
                        string idString = claim.Value;
                        string[] ids = idString.Split('_');
                        int accountId = 0;
                        int eventId = 0;
                        int userId = 0;
                        if (ids.Length == 3)
                        {
                            _ = int.TryParse(ids[0], out accountId);
                            _ = int.TryParse(ids[1], out eventId);
                            _ = int.TryParse(ids[2], out userId);
                        }
                        newToken.AccountId = accountId;
                        newToken.EventId = eventId;
                        newToken.Id = userId;
                    }
                    if (claim.Type == ClaimTypes.Dns)
                    {
                        string dns = claim.Value;
                        string[] dnsParts = dns.Split('/');
                        string domain = "";
                        string url = "";
                        if (dnsParts.Length > 1)
                        {
                            domain = dnsParts[0];
                            url = dnsParts[1];
                        }
                        newToken.Domain = domain;
                        newToken.Url = url;
                    }
                    if (claim.Type == ClaimTypes.Email)
                        newToken.Username = claim.Value;
                    if (claim.Type == ClaimTypes.Role)
                    { 
                        roles.Add(new UserRole { Role = claim.Value });
                    }
                }
                newToken.Roles = roles.Select(x => x.Role).ToArray();
            }
            catch (Exception ex)
            {
                newToken.Username = "401";
                return newToken;
            }
            return newToken;
        }



        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) return false;
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (storedHash.Length != 64) return false;
            if (storedSalt.Length != 128) return false;
            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }





    }
}
