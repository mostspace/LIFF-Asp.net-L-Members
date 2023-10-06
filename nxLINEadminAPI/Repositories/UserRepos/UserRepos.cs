using nxLINEadminAPI.Entity;
using Microsoft.EntityFrameworkCore;
using nxLINEadminAPI.Repositories.GenericRepos;

namespace nxLINEadminAPI.Repositories.UserRepos
{
    public class UserRepos : GenericRepos<User>, IUserRepos
    {
        private readonly nxLINEadminAPIContext _context;
        public UserRepos(nxLINEadminAPIContext adminContext) : base(adminContext)
        {
            _context = adminContext;
        }

        public Task<User?> FindUserAsync(string loginId)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.UserLoginId == loginId);
        }
    }
}
