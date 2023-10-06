using nxLINEadmin.Entity;
using Microsoft.EntityFrameworkCore;
using nxLINEadmin.Repositories.GenericRepos;

namespace nxLINEadmin.Repositories.MemberRepos
{
    public class MemberRepos : GenericRepos<Member>, IMemberRepos
    {
        private readonly nxLINEadminContext _context;
        public MemberRepos(nxLINEadminContext adminContext) : base(adminContext)
        {
            _context = adminContext;
        }
    }
}
