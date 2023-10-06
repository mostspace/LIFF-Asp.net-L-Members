using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using nxLINEadmin.UnitOfWork;
using AutoMapper;
using nxLINEadmin.Utilities;
using System.Net.Http;
using NuGet.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace nxLINEadmin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public HomeController(IWebHostEnvironment hostingEnvironment, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _hostingEnvironment = hostingEnvironment;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
