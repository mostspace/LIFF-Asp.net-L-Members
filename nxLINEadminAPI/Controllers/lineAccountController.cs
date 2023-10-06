using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nxLINEadminAPI.UnitOfWork;
using nxLINEadminAPI.Entity;

namespace nxLINEadminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineAccountsController : ControllerBase
    {

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly nxLINEadminAPIContext _context;
        private readonly IUnitOfWork _unitOfWork;
        public LineAccountsController(
            IWebHostEnvironment hostingEnvironment,
            nxLINEadminAPIContext context
            )
        {
            _hostingEnvironment = hostingEnvironment;
            _context = context;
        }

        // GET: api/LineAccounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LineAccount>>> GetLineAccount()
        {
            if (_context.LineAccounts == null)
            {
                return NotFound();
            }
            return await _context.LineAccounts.ToListAsync();
        }

        // GET: api/LineAccounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LineAccount>> GetLineAccount(int id)
        {
            if (_context.LineAccounts == null)
            {
                return NotFound();
            }
            var lineAccount = await _context.LineAccounts.FirstOrDefaultAsync(u => u.LineaccountId == id);

            if (lineAccount == null)
            {
                return NotFound();
            }

            return lineAccount;
        }

        // PUT: api/LineAccounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLineAccount(int id, LineAccount lineAccount)
        {
            if (id != lineAccount.LineaccountId)
            {
                return BadRequest();
            }

            _context.Entry(lineAccount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LineAccountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/LineAccounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> SaveAsync([FromForm] LineAccount Account, IFormFile? cardDesign, IFormFile? logoImg)
        {
            if (!ModelState.IsValid)
                return NoContent();

            LineAccount lineAccount = new LineAccount();
            if (cardDesign != null)
            {
                string cardValid = UploadFileValidation(cardDesign);
                if (!string.IsNullOrEmpty(cardValid))
                {
                    ModelState.AddModelError("", cardValid);
                    return NoContent();
                }
                if (!string.IsNullOrEmpty(Account.MembersCardDesignUrl))
                {
                    DeleteFile(Account.MembersCardDesignUrl);
                }
                string url = SaveUploadFile(cardDesign);
                lineAccount.MembersCardDesignUrl = "https://localhost:7116" + url;
            }

            if (logoImg != null)
            {
                string logoValid = UploadFileValidation(logoImg);
                if (!string.IsNullOrEmpty(logoValid))
                {
                    ModelState.AddModelError("", logoValid);
                    return NoContent();
                }
                if (!string.IsNullOrEmpty(Account.LineaccountLogoUrl))
                {
                    DeleteFile(Account.LineaccountLogoUrl);
                }
                string url = SaveUploadFile(logoImg);
                lineAccount.LineaccountLogoUrl = "https://localhost:7116" + url;
            }

            lineAccount.LineaccountUpdated = DateTime.Now;

            lineAccount.LineaccountId = Account.LineaccountId;
            lineAccount.LineaccountCode = Account.LineaccountCode;
            lineAccount.LineaccountShortcode = Account.LineaccountShortcode;
            lineAccount.LineaccountName = Account.LineaccountName;
            lineAccount.LineaccountEmail = Account.LineaccountEmail;
            lineAccount.Istalk = Account.Istalk;
            lineAccount.TalkMessage = Account.TalkMessage;
            lineAccount.IsProfile = Account.IsProfile;
            lineAccount.ProfileSetting = Account.ProfileSetting;
            lineAccount.EntryPoint = Account.EntryPoint;
            lineAccount.StartRank = Account.StartRank;
            lineAccount.PointExpire = Account.PointExpire;
            lineAccount.MembersCardColor = Account.MembersCardColor;
            lineAccount.MembersCardIsUseCamera = Account.MembersCardIsUseCamera;
            lineAccount.MembersCardLiffId = Account.MembersCardLiffId;
            lineAccount.LineChannelId = Account.LineChannelId;
            lineAccount.LineChannelSecret = Account.LineChannelSecret;
            lineAccount.LineChannelAccessToken = Account.LineChannelAccessToken;
            lineAccount.IsSmaregi = Account.IsSmaregi;
            lineAccount.SmaregiContractId = Account.SmaregiContractId;
            lineAccount.LineaccountCreated = Account.LineaccountCreated;
            lineAccount.LineaccountDeleted = Account.LineaccountDeleted;
            try
            {
                if(lineAccount.LineaccountId <= 0)
                {
                    _context.LineAccounts.Add(lineAccount);
                    await _context.SaveChangesAsync();
                } else
                {
                    _context.Entry(lineAccount).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LineAccountExists(lineAccount.LineaccountId))
                {
                    return NoContent();
                }
                else
                {
                    throw;
                }
            }
            return Ok("ok");
        }

        // DELETE: api/LineAccounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLineAccount(int id)
        {
            if (_context.LineAccounts == null)
            {
                return NotFound();
            }
            var lineAccount = await _context.LineAccounts.FindAsync(id);
            if (lineAccount == null)
            {
                return NotFound();
            }

            _context.LineAccounts.Remove(lineAccount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LineAccountExists(int id)
        {
            return (_context.LineAccounts?.Any(e => e.LineaccountId == id)).GetValueOrDefault();
        }


        private string UploadFileValidation(IFormFile file)
        {
            if (file == null)
            {
                return "ファイルが存在しません";
            }

            string[] allowExts = { ".jpg", ".png" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            bool find = false;

            foreach (string allowExt in allowExts)
            {
                if (allowExt == ext)
                {
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                return "画像にはJPEGかPNGを指定してください";
            }

            return string.Empty;
        }

        private string SaveUploadFile(IFormFile file)
        {
            string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "upload", "image");
            string ext = Path.GetExtension(file.FileName).ToLower();
            string fileName;
            string filePath;
            var rand = new Random((int)DateTime.Now.Ticks);

            while (true)
            {
                fileName = string.Format("{0:D10}{1}", rand.Next(), ext);
                filePath = Path.Combine(uploadFolder, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    try
                    {
                        using (var fw = new FileStream(filePath, FileMode.CreateNew))
                        {
                            file.CopyTo(fw);
                        }
                        break;
                    }
                    catch (IOException ex)
                    {
                        // ファイルが既に存在する場合はリトライ
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return $"/upload/image/{fileName}";
        }
        private void DeleteFile(string url)
        {
            string imagePath = UrlToLocallPath(url);

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        private string UrlToLocallPath(string url)
        {
            return _hostingEnvironment.WebRootPath + url.Replace("/", "\\");
        }

        [HttpPost("UploadFile")]
        public String UploadFile(IFormFile uploadedFile)
        {
            string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "upload", "image");
            string ext = Path.GetExtension(uploadedFile.FileName).ToLower();
            string fileName;
            string filePath;
            var rand = new Random((int)DateTime.Now.Ticks);

            while (true)
            {
                fileName = string.Format("{0:D10}{1}", rand.Next(), ext);
                filePath = Path.Combine(uploadFolder, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    try
                    {
                        using (var fw = new FileStream(filePath, FileMode.CreateNew))
                        {
                            uploadedFile.CopyTo(fw);
                        }
                        break;
                    }
                    catch
                    {
                        return "";
                    }
                }
            }
            return fileName;
        }
    }
}
