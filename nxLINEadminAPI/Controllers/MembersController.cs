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
    public class MembersController : ControllerBase
    {
        private readonly nxLINEadminAPIContext _context;

        public MembersController(nxLINEadminAPIContext context)
        {
            _context = context;
        }

        // GET: api/Members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMember()
        {
          if (_context.Member == null)
          {
              return NotFound();
          }
            return await _context.Member.ToListAsync();
        }
        // GET: api/Members
        [HttpGet("test")]
        public Array Test(String? gender)
        {
            int[] intArray = gender.Split(',').Select(int.Parse).ToArray();
            return intArray;
        }

        // GET: api/Members
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Member>>> SearchMember(String? member_code, String? code,
            String? name, String? kana, int? year, int? month, int? day, String? gender, String? phone,
            String? email, String? join_date_start, String? join_date_end, int? point_start, int? point_end,
            String? tag_include, String? tag_except)
        {
            if (_context.Member == null)
            {
                return NotFound();
            }
            var members = from m in _context.Member
                          select m;
            if (member_code != null)
            {
                member_code = member_code.Trim();
                members = members.Where(s => s.member_code!.Contains(member_code!));
            }
            if (code != null)
            {
                code = code.Trim();
                members = members.Where(s => s.member_code!.Contains(code!));
            }
            if (name != null)
            {
                name = name.Trim();
                members = members.Where(s => (s.member_firstname + " " + s.member_lastname)!.Contains(name!));
            }
            if (kana != null)
            {
                kana = kana.Trim();
                members = members.Where(s => (s.member_firstname_kana + s.member_lastname_kana)!.Contains(kana!));
            }
            if (year != null)
            {
                members = members.Where(s => s.member_birthday != null && s.member_birthday.Value.Year == year);
            }
            if (month != null)
            {
                members = members.Where(s => s.member_birthday != null && s.member_birthday.Value.Month == month);
            }
            if (day != null)
            {
                members = members.Where(s => s.member_birthday != null && s.member_birthday.Value.Day == day);
            }
            if (gender != null)
            {
                int[] intArray = gender.Split(',').Select(int.Parse).ToArray();
                members = members.Where(s => intArray.Contains(s.member_gender));
            }
            if (phone != null)
            {
                phone = phone.Trim();
                members = members.Where(s => s.member_tel!.Contains(phone!));
            }
            if (email != null)
            {
                email = email.Trim();
                members = members.Where(s => s.member_email!.Contains(email!));
            }
            if (join_date_start != null)
            {
                join_date_start = join_date_start.Trim();
                members = members.Where(s => s.member_join_date!.Value > DateTime.ParseExact(join_date_start, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
            }
            if (join_date_end != null)
            {
                join_date_end = join_date_end.Trim();
                members = members.Where(s => s.member_join_date!.Value < DateTime.ParseExact(join_date_end, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
            }
            if(point_start != null)
            {
                members = members.Where(s => s.member_hold_point >= point_start);
            }
            if(point_end != null)
            {
                members = members.Where(s => s.member_hold_point <= point_end);
            }
            if (tag_include != null)
            {
                tag_include = tag_include.Trim();
                string[] tags = tag_include.Split(',');
                foreach (string tag in tags)
                {
                    members = members.Where(s => s.member_tag!.Contains(tag!));
                }
            }
            if (tag_except != null)
            {
                tag_except = tag_except.Trim();
                string[] tags = tag_except.Split(',');
                foreach (string tag in tags)
                {
                    members = members.Where(s => !s.member_tag!.Contains(tag!));
                }
            }

            return await members.ToListAsync();
        }

        // GET: api/Members/lineid_csv_download
        [HttpGet("lineid_csv_download")]
        public IActionResult LineIdCSVDownload()
        {
            string csvContent = "Name,Email,Phone\nJohn Doe,johndoe@example.com,1234567890\nJane Smith,janesmith@example.com,0987654321";

            byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent);

            Response.Headers.Add("Content-Disposition", "attachment; filename=myfile.csv");

            return File(csvBytes, "text/csv");            
        } 

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
          if (_context.Member == null)
          {
              return NotFound();
          }
            var member = await _context.Member.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // PUT: api/Members/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            if (id != member.member_id)
            {
                return BadRequest();
            }

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Members
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
          if (_context.Member == null)
          {
              return Problem("Entity set 'nxLINEadminAPIContext.Member'  is null.");
          }
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMember", new { id = member.member_id }, member);
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            if (_context.Member == null)
            {
                return NotFound();
            }
            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Member.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemberExists(int id)
        {
            return (_context.Member?.Any(e => e.member_id == id)).GetValueOrDefault();
        }
    }
}
