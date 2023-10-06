using System;
using System.Collections.Generic;

namespace nxLINEadminAPI.Entity
{
    public partial class User
    {
        public int UserId { get; set; }
        public int Status { get; set; }
        public string UserLoginId { get; set; }
        public string UserPwd { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int? UserLineAccountID { get; set; }
        public string? UserLineAccountRole { get; set; }
        public DateTime? UserLastlogindatetime { get; set; }
        public DateTime UserCreated { get; set; }
        public DateTime? UserUpdated { get; set; }
    }
}
