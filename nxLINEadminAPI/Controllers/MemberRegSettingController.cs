using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using nxLINEadminAPI.Entity;
using System.Text;

namespace nxLINEadminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberRegSettingController : ControllerBase
    {
        private readonly nxLINEadminAPIContext _context;

        public MemberRegSettingController(nxLINEadminAPIContext context)
        {
            _context = context;
        }

        //Get: api/MemberRegSetting
        [HttpGet("save_setting")]
        public async Task<ActionResult> SaveSetting(String? is_name, String? is_furigana, String? is_tel,
                                        String? is_email, String? is_birthday, String? is_gender, String? is_address, String? overview)
        {
            Console.WriteLine(is_name);
            return NotFound();
        }
    }
}
