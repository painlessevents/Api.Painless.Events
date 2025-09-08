using api.painless.events.Core;
using api.painless.events.DTOs;
using api.painless.events.Entities;
using api.painless.events.Hubs;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Encodings;

namespace api.painless.events.Controllers
{

    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [EnableRateLimiting("TenInTwenty")]
    public class FileController : Controller
    {




        private readonly IConfiguration _configuration;
        private ReadContext _readContext;
        private WriteContext _writeContext;
        private IHubContext<SocketHub> _socketContext;
        private HelperFunctions _helperFunctions;
        private SecurityContext _securityContext;

        public FileController(IConfiguration configuration, ReadContext readContext, WriteContext writeContext, IHubContext<SocketHub> socketContext, HelperFunctions helperFunctions, SecurityContext sContext)
        {
            _configuration = configuration;
            _readContext = readContext;
            _writeContext = writeContext;
            _socketContext = socketContext;
            _helperFunctions = helperFunctions;
            _securityContext = sContext;
        }



        [Authorize]
        [HttpGet("Download")]
        public async Task<IActionResult> ListFilesForDownload()
        {
            // TODO: check for access
            List<FileDto> files = await (from f1 in _readContext.Files
                                         join u1 in _readContext.Users on f1.UploadedBy equals u1.Id
                                         join d1 in _readContext.FileDownloads on f1.Id equals d1.FileId into d2
                                         from d1 in d2.DefaultIfEmpty()
                                         where f1.Deleted == 0 && f1.Enabled == 1
                                         group new { f1, u1, d1 } by new { f1.Id, f1.Guid, f1.AccountId, f1.EventId, f1.Name, f1.Extension, f1.Size, f1.Description, f1.UploadedBy, f1.AdminAccess, f1.UserAccess, f1.VisitorAccess, f1.Enabled, f1.Deleted, f1.Created, u1.Username } into g
                                         orderby g.Key.Created descending
                                         select new FileDto()
                                          {
                                              Id = g.Key.Id,
                                              Guid = g.Key.Guid,
                                              AccountId = g.Key.AccountId,
                                              EventId = g.Key.EventId,
                                              Name = g.Key.Name,
                                              Extension = g.Key.Extension,
                                              Size = g.Key.Size,
                                              Downloads = g.Count(x => x.d1 != null),
                                              Checksum = "", //TODO: get checksum
                                              Description = g.Key.Description,
                                              Creator = g.Key.Username,
                                              Created = g.Key.Created
                                          }).ToListAsync() ?? new List<FileDto>();

            //if (files.Count == 0) return BadRequest("No files found");
            foreach (FileDto myfile in files)
            {
                myfile.Checksum = await _helperFunctions.GetFileChecksum(_configuration.GetSection("Appsettings:FileDump").Value + myfile.Id + myfile.Extension);
            }
            return Ok(files);
        }




        [Authorize]
        [HttpGet("Download/{FileId}")]
        public async Task<IActionResult> DownloadFile(int FileId)
        {
            // check for access
            // get file for download

            return Ok();
        }



        [HttpGet("PublicDownload/{FileId}")]
        public async Task<IActionResult> DownloadPublicFile(string FileId)
        {
            // get file for download
            Entities.File file = await (from f1 in _readContext.Files where f1.Guid == FileId && f1.Deleted == 0 && f1.Enabled == 1 && f1.VisitorAccess == 1 select f1).FirstOrDefaultAsync() ?? new Entities.File();
            if (file.Id == 0) return BadRequest("Invalid File");

            FileInfo fileInfo = new FileInfo(_configuration.GetSection("Appsettings:FileDump").Value + file.Id + file.Extension);
            if (!fileInfo.Exists) return BadRequest("File not found");

            return PhysicalFile(fileInfo.FullName, "application/octet-stream", file.Name + file.Extension);
        }




        [Authorize]
        [HttpPost("Upload")]
        [RequestSizeLimit(200_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 200_000_000)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // TODO: check for access
            Entities.File newFile = new Entities.File()
            {
                Guid = System.Guid.NewGuid().ToString(),
                AccountId = 0, //TODO: get account id
                EventId = 0, //TODO: get event id
                Name = file.FileName,
                Extension = Path.GetExtension(file.FileName),
                Size = (int)file.Length,
                Description = "File uploaded by user ", //get user name here
                UploadedBy = 0, //TODO: get user id
                AdminAccess = 1,
                UserAccess = 0,
                VisitorAccess = 0,
                Enabled = 1,
                Deleted = 0,
                Created = DateTime.UtcNow
            };
            _writeContext.Files.Add(newFile);
            await _writeContext.SaveChangesAsync();
            using (FileStream stream = new FileStream(_configuration.GetSection("Appsettings:FileDump").Value + newFile.Id + "." + newFile.Extension, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            // TODO: virus check
            return Ok();
        }








    }
}
