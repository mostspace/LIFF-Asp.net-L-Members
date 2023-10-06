using AutoMapper;
using nxLINEadmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using nxLINEadmin.Entity;
using nxLINEadmin.UnitOfWork;
using Member = nxLINEadmin.Entity.Member;

namespace nxLINEadmin.Controllers
{
    public class TradingHistoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TradingHistoryController(
            IUnitOfWork unitOfWork,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
