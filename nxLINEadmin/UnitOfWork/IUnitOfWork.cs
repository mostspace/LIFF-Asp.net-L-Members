using nxLINEadmin.Repositories.UserRepos;
using nxLINEadmin.Repositories.LineAccountRepos;
using nxLINEadmin.Repositories.MemberRepos;

namespace nxLINEadmin.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IMemberRepos MemberRepos { get; }
        IUserRepos UserRepos { get; }
        ILineAccountRepos LineAccountRepos { get; }
        int Complete();
    }
}
