using nxLINEadmin.Entity;

namespace nxLINEadmin.Model
{
    public class SettingViewModel
    {
        public LineAccount Account { get; set; }
        public List<User> Users { get; set; }
        public RegisterUserViewModel newUser { get; set; }
    }
}
