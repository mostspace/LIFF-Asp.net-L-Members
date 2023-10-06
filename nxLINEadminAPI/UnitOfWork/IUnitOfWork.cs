using nxLINEadminAPI.Repositories.UserRepos;
using nxLINEadminAPI.Repositories.LineAccountRepos;

namespace nxLINEadminAPI.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepos UserRepos { get; }
        ILineAccountRepos LineAccountRepos { get; }
        int Complete();
    }
}
