using nxLINEadmin.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using nxLINEadmin.UnitOfWork;
using nxLINEadmin.Utilities;
using nxLINEadmin.Entity;
using AutoMapper;
using Microsoft.AspNetCore.Identity;


namespace nxLINEadmin.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        protected string UserId
        {
            get
            {
                return User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }

        public AuthController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginUserViewModel());
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login(LoginUserViewModel user, string? returnUrl = null)
        {
            // here is test code
            // string s = returnUrl == null ? "/Home/" : returnUrl;
            if (!ModelState.IsValid)
                return View(user);

            var atUser = await _unitOfWork.UserRepos.GetByCondition(u => u.UserLoginId == user.UserLoginId).FirstOrDefaultAsync();

            if (atUser == null)
            {
                ModelState.AddModelError("", "ユーザー名またはパスワードが違います");
                return View(user);
            }

            // パスワードの比較
            if (!AuthHelper.CheckPasswordLogin(user.Password, atUser.UserPwd))
            {
                ModelState.AddModelError("", "ユーザー名またはパスワードが違います");
                return View(user);
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, atUser.UserLoginId));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            string redirectUrl = returnUrl == null ? "/Home/" : returnUrl;

            return Redirect(redirectUrl);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        public IActionResult ForGot()
        {
            return View(new RegisterUserViewModel());
        }

        public IActionResult SendCode()
        {
            return View(new RegisterUserViewModel());
        }

        public IActionResult Register()
        {
            return View(new RegisterUserViewModel());
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel user)
        {
            if (!ModelState.IsValid)
                return View(user);

            if (user.UserRePwd != user.UserPwd)
            {
                ModelState.AddModelError("", "値が違います");
                return View(user);
            }
                
            var atUser = await _unitOfWork.UserRepos.GetByCondition(u => u.UserLoginId == user.UserLoginId).FirstOrDefaultAsync();

            if (atUser != null)
            {
                ModelState.AddModelError("", "その名前はすでに使用されています");
                return View(user);
            }

            atUser = await _unitOfWork.UserRepos.GetByCondition(u => u.UserEmail == user.UserEmail).FirstOrDefaultAsync();

            if (atUser != null)
            {
                ModelState.AddModelError("", "そのメールアドレスは既に使用されています");
                return View(user);
            }
            User model = new User();

            model.UserName = user.UserName;
            model.UserLoginId = user.UserLoginId;
            model.UserEmail = user.UserEmail;
            model.UserPwd = AuthHelper.HashPassword(user.UserPwd);

            model.UserCreated = DateTime.Now;
            _unitOfWork.UserRepos.Add(model);
            _unitOfWork.Complete();

            return RedirectToAction("Login");
        }
    }
}
