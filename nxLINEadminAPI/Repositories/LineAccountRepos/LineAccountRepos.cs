using nxLINEadminAPI.Entity;
using nxLINEadminAPI.Repositories.GenericRepos;

namespace nxLINEadminAPI.Repositories.LineAccountRepos
{
    public class LineAccountRepos : GenericRepos<LineAccount>, ILineAccountRepos
    {
        public LineAccountRepos(nxLINEadminAPIContext adminContext) : base(adminContext)
        {
        }
    }
}