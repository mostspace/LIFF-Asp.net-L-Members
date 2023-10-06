using nxLINEadmin.Entity;
using Microsoft.EntityFrameworkCore;
using nxLINEadmin.Repositories.GenericRepos;

namespace nxLINEadmin.Repositories.UserRepos
{
    public class UserRepos : GenericRepos<User>, IUserRepos
    {
        private readonly nxLINEadminContext _context;
        public UserRepos(nxLINEadminContext adminContext) : base(adminContext)
        {
            _context = adminContext;
        }

        public Task<User?> FindUserAsync(string loginId)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.UserLoginId == loginId);
        }
    }
}
