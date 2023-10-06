using nxLINEadmin.Entity;
using nxLINEadmin.Repositories.GenericRepos;

namespace nxLINEadmin.Repositories.LineAccountRepos
{
    public class LineAccountRepos : GenericRepos<LineAccount>, ILineAccountRepos
    {
        public LineAccountRepos(nxLINEadminContext adminContext) : base(adminContext)
        {
        }
    }
}