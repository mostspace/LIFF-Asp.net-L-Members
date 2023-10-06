using nxLINEadminAPI.Entity;
using nxLINEadminAPI.Repositories.UserRepos;
using nxLINEadminAPI.Repositories.LineAccountRepos;

namespace nxLINEadminAPI.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly nxLINEadminAPIContext _adminContext;

        public UnitOfWork(nxLINEadminAPIContext adminContext)
        {
            _adminContext = adminContext;
            UserRepos = new UserRepos(adminContext);
            LineAccountRepos = new LineAccountRepos(adminContext);
        }

        public IUserRepos UserRepos { get; private set; }
        public ILineAccountRepos LineAccountRepos { get; private set; }
        public int Complete()
        {
            return _adminContext.SaveChanges();
        }

        public void Dispose()
        {
            _adminContext.Dispose();
        }
    }
}
