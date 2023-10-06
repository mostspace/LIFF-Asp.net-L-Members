using AutoMapper;
using nxLINEadmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using nxLINEadmin.Entity;
using nxLINEadmin.UnitOfWork;
using Member = nxLINEadmin.Entity.Member;

namespace nxLINEadmin.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomersController(
            IUnitOfWork unitOfWork,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            List<Member> members = _unitOfWork.MemberRepos.GetAll().ToList();
            MemberViewModel vm = new MemberViewModel();
            vm.members = members;
            return View(vm);
        }
    }
}
