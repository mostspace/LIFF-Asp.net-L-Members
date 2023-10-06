using nxLINEadmin.Entity;
using nxLINEadmin.Repositories.UserRepos;
using nxLINEadmin.Repositories.LineAccountRepos;
using nxLINEadmin.Repositories.MemberRepos;

namespace nxLINEadmin.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly nxLINEadminContext _adminContext;

        public UnitOfWork(nxLINEadminContext adminContext)
        {
            _adminContext = adminContext;
            UserRepos = new UserRepos(adminContext);
            LineAccountRepos = new LineAccountRepos(adminContext);
            MemberRepos = new MemberRepos(adminContext);
        }

        public IUserRepos UserRepos { get; private set; }
        public IMemberRepos MemberRepos { get; private set; }
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
