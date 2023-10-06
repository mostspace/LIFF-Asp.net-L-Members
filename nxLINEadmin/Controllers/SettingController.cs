using nxLINEadmin.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using nxLINEadmin.UnitOfWork;
using nxLINEadmin.Entity;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nxLINEadmin.Utilities;

namespace nxLINEadmin.Controllers
{
    public class SettingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        
        private string _userID;

        public static string noImgUrl = "/images/no_image.jpeg";

        public SettingController(
            IWebHostEnvironment hostingEnvironment,
            IUnitOfWork unitOfWork, 
            IHttpContextAccessor contextAccessor,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            _contextAccessor = contextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;

            var httpContext = _contextAccessor.HttpContext;
            var user = httpContext.User;
            _userID = user.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public async Task<IActionResult> Basic(string errorType = "", string errorMsg = "")
        {
            User currentUser = await _unitOfWork.UserRepos.FindUserAsync(_userID);
            int lineAccountID = currentUser.UserLineAccountID ?? 0;
            LineAccount lineAccount;

            if (lineAccountID == 0)
            {
                lineAccount = new LineAccount();
                lineAccount.LineaccountCode = "1";
                lineAccount.LineaccountShortcode = "1";
                lineAccount.LineaccountEmail = currentUser.UserEmail;
                lineAccount.LineaccountCreated = DateTime.Now;

                _unitOfWork.LineAccountRepos.Add(lineAccount);
                _unitOfWork.Complete();

                int newLineID = lineAccount.LineaccountId;
                currentUser.UserLineAccountID = newLineID;
                currentUser.UserUpdated = DateTime.Now;
                _unitOfWork.UserRepos.Update(currentUser);
                _unitOfWork.Complete();
            }
            else
            {
                lineAccount = _unitOfWork.LineAccountRepos.GetById(lineAccountID);
            }

            SettingViewModel vm = new SettingViewModel();
            vm.Account = lineAccount;
            vm.newUser = new RegisterUserViewModel();

            ViewBag.errorType = errorType; 
            ViewBag.errorMsg = errorMsg;

            List<User> users = _unitOfWork.UserRepos.GetAll().ToList();
            vm.Users = users;
            return View(vm);
        }
        public async Task<IActionResult> Membership(string errorType = "", string errorMsg = "")
        {
            User currentUser = await _unitOfWork.UserRepos.FindUserAsync(_userID);
            int lineAccountID = currentUser.UserLineAccountID ?? 0;
            LineAccount lineAccount;

            if (lineAccountID == 0)
            {
                lineAccount = new LineAccount();
                lineAccount.LineaccountCode = "1";
                lineAccount.LineaccountShortcode = "1";
                lineAccount.LineaccountEmail = currentUser.UserEmail;
                lineAccount.LineaccountCreated = DateTime.Now;

                _unitOfWork.LineAccountRepos.Add(lineAccount);
                _unitOfWork.Complete();

                int newLineID = lineAccount.LineaccountId;
                currentUser.UserLineAccountID = newLineID;
                currentUser.UserUpdated = DateTime.Now;
                _unitOfWork.UserRepos.Update(currentUser);
                _unitOfWork.Complete();
            }
            else
            {
                lineAccount = _unitOfWork.LineAccountRepos.GetById(lineAccountID);
            }

            SettingViewModel vm = new SettingViewModel();
            vm.Account = lineAccount;
            vm.newUser = new RegisterUserViewModel();

            ViewBag.errorType = errorType; 
            ViewBag.errorMsg = errorMsg;

            List<User> users = _unitOfWork.UserRepos.GetAll().ToList();
            vm.Users = users;
            return View(vm);
        }
        public async Task<IActionResult> Permission(string errorType = "", string errorMsg = "")
        {
            User currentUser = await _unitOfWork.UserRepos.FindUserAsync(_userID);
            int lineAccountID = currentUser.UserLineAccountID ?? 0;
            LineAccount lineAccount;

            if (lineAccountID == 0)
            {
                lineAccount = new LineAccount();
                lineAccount.LineaccountCode = "1";
                lineAccount.LineaccountShortcode = "1";
                lineAccount.LineaccountEmail = currentUser.UserEmail;
                lineAccount.LineaccountCreated = DateTime.Now;

                _unitOfWork.LineAccountRepos.Add(lineAccount);
                _unitOfWork.Complete();

                int newLineID = lineAccount.LineaccountId;
                currentUser.UserLineAccountID = newLineID;
                currentUser.UserUpdated = DateTime.Now;
                _unitOfWork.UserRepos.Update(currentUser);
                _unitOfWork.Complete();
            }
            else
            {
                lineAccount = _unitOfWork.LineAccountRepos.GetById(lineAccountID);
            }

            SettingViewModel vm = new SettingViewModel();
            vm.Account = lineAccount;
            vm.newUser = new RegisterUserViewModel();

            ViewBag.errorType = errorType; 
            ViewBag.errorMsg = errorMsg;

            List<User> users = _unitOfWork.UserRepos.GetAll().ToList();
            vm.Users = users;
            return View(vm);
        }

        [HttpPost]
        public IActionResult ConfigSave(int line_account_id, string profile_setting)
        {
            LineAccount model = _unitOfWork.LineAccountRepos.GetById(line_account_id);
            model.ProfileSetting = profile_setting;
            model.LineaccountUpdated = DateTime.Now;
            _unitOfWork.LineAccountRepos.Update(model);
            _unitOfWork.Complete();

            return RedirectToAction(nameof(Basic), new { errorType = "success", errorMsg = "登録が成功しました" });
        }

        [HttpPost]
        public IActionResult Save(LineAccount Account, IFormFile? cardDesign, IFormFile? logoImg) 
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index), new { errorType = "error", errorMsg = "登録が失敗しました" });

            if (cardDesign != null)
            {
                string cardValid = UploadFileValidation(cardDesign);
                if(!string.IsNullOrEmpty(cardValid))
                {
                    ModelState.AddModelError("", cardValid);
                    return RedirectToAction(nameof(Index), new { errorType = "error", errorMsg = cardValid });
                }
                if (!string.IsNullOrEmpty(Account.MembersCardDesignUrl))
                {
                    DeleteFile(Account.MembersCardDesignUrl);
                }
                string url = SaveUploadFile(cardDesign);
                Account.MembersCardDesignUrl = url;
            }

            if (logoImg != null)
            {
                string logoValid = UploadFileValidation(logoImg);
                if (!string.IsNullOrEmpty(logoValid))
                {
                    ModelState.AddModelError("", logoValid);
                    return RedirectToAction(nameof(Index), new { errorType = "error", errorMsg = logoValid });
                }
                if (!string.IsNullOrEmpty(Account.LineaccountLogoUrl))
                {
                    DeleteFile(Account.LineaccountLogoUrl);
                }
                string url = SaveUploadFile(logoImg);
                Account.LineaccountLogoUrl = url;
            }

            Account.LineaccountUpdated = DateTime.Now;
            _unitOfWork.LineAccountRepos.Update(Account);
            _unitOfWork.Complete();

            return RedirectToAction(nameof(Index), new { errorType = "success", errorMsg = "登録が成功しました" });
        }

        [HttpPost]
        public async Task<IActionResult> UserSave(RegisterUserViewModel newUser)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new {errorType="error", errorMsg= "登録が失敗しました" });
            }
                

            if (newUser.UserRePwd != newUser.UserPwd)
            {
                return RedirectToAction(nameof(Index), new { errorType = "error", errorMsg = "値が違います" });
            }

            var atUser = await _unitOfWork.UserRepos.GetByCondition(u => u.UserLoginId == newUser.UserLoginId).FirstOrDefaultAsync();

            if (atUser != null)
            {
                return RedirectToAction(nameof(Index), new { errorType = "error", errorMsg = "その名前はすでに使用されています" });
            }

            atUser = await _unitOfWork.UserRepos.GetByCondition(u => u.UserEmail == newUser.UserEmail).FirstOrDefaultAsync();

            if (atUser != null)
            {
                return RedirectToAction(nameof(Index), new { errorType = "error", errorMsg = "そのメールアドレスは既に使用されています" });
            }
            User model = new User();

            model.UserName = newUser.UserName;
            model.UserLoginId = newUser.UserLoginId;
            model.UserEmail = newUser.UserEmail;
            model.UserPwd = AuthHelper.HashPassword(newUser.UserPwd);
            model.UserLineAccountRole = newUser.UserLineAccountRole;

            model.UserCreated = DateTime.Now;
            _unitOfWork.UserRepos.Add(model);
            _unitOfWork.Complete();

            return RedirectToAction(nameof(Index), new { errorType = "success", errorMsg = "登録が成功しました" });
        }

        public IActionResult UserDelete(int userID)
        {
            User user = _unitOfWork.UserRepos.GetById(userID);
            if (user != null) 
            {
                _unitOfWork.UserRepos.Remove(user);
                _unitOfWork.Complete();
            }

            return RedirectToAction(nameof(Index), new { errorType = "success", errorMsg = "登録が成功しました" });
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
    }
}
