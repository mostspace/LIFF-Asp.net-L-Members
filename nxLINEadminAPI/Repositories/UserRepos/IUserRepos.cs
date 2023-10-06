using nxLINEadminAPI.Entity;
using nxLINEadminAPI.Repositories.GenericRepos;

namespace nxLINEadminAPI.Repositories.UserRepos
{
    public interface IUserRepos : IGenericRepos<User>
    {
        public Task<User?> FindUserAsync(string loginId);
    }
}
