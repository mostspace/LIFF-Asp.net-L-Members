using nxLINEadmin.Entity;
using nxLINEadmin.Repositories.GenericRepos;

namespace nxLINEadmin.Repositories.UserRepos
{
    public interface IUserRepos : IGenericRepos<User>
    {
        public Task<User?> FindUserAsync(string loginId);
    }
}
