using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LineMemberManagement.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class memberController : ControllerBase
    {

        private LoginLogic LoginLogic = new LoginLogic();

        private readonly SmartCrmContext _context;
        private readonly SmaregiUtil.SmaregiApiSettings _smaregiApiSettings;
        private readonly Models.LineApiSettings _lineApiSettings;
        private readonly ILogger _logger;

        MessageLogic MessageLogic { get; set; } = new MessageLogic();

        public memberController(
              SmartCrmContext context
            , IOptions<SmaregiUtil.SmaregiApiSettings> smaregiApiSettings
            , IOptions<LineApiSettings> lineApiSettings
            , ILogger<memberController> logger)
        {
            _context = context;
            _context.member.Load();
            _smaregiApiSettings = smaregiApiSettings.Value;
            _lineApiSettings = lineApiSettings.Value;
            _logger = logger;            
        }

        /// <summary>
        ///  POST: ｛endpoint}/api/v1/member/create
        /// </summary>
        /// <returns>LINEログインリクエスト</returns>
        [HttpPost("member/create", Name = "Create Member")]
        public async Task<ActionResult<member>> CreateMember([FromBody] member member)
        {
            try
            {
                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                var now = DateTime.Now;
                member.member_createat = now;
                member.member_updateat = now;
                member.member_join_date = now;

                // データベースに新しいユーザーを登録
                _context.member.Add(member);

                // スマレジ会員情報を登録
                try
                {
                    // pos_idが更新されるのでmemberで受ける
                    member = await this.UpdateSmaregiCustomer(member);
				}
                catch (ApplicationException e)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, e.Message);
				}

                // 登録したユーザーを保存
                await _context.SaveChangesAsync();

                // 新規作成したユーザーを返す
                return member;
            }
            catch (Exception e)
            {
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }


        [HttpPost("member/login", Name = "Line Login")]
        public async Task<ActionResult<string>> LineLogin(member loginParams)
        {
            try
            {
                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                string state = GenerateStateString(12) + "," + loginParams.member_code + "," + loginParams.callbackurl;
                string nonce = GenerateNonceString();
                
                // リクエストURLを生成
                string loginRequestURL = $"{AUTHORIZATION_REQUEST}response_type=code&client_id={_lineApiSettings.ChannelId}&redirect_uri={_lineApiSettings.CallbackUrl}&state={state}&bot_prompt=aggressive&scope=profile%20openid&nonce={nonce}";

                // ログイン画面に遷移
                return loginRequestURL;
            }
            catch (Exception)
            {
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        /// <summary>
        /// 既存のユーザーを更新
        /// </summary>
        /// <returns>LINEログインリクエスト</returns>
        [HttpPost("member/update", Name = "Update Member")]
        public async Task<ActionResult<member>> UpdateMember(member member)
        {
            try
            {
                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }
                // 更新するユーザーを取得
                var updateMember = _context.member.Where(m => m.member_code == member.member_code).FirstOrDefault();

                // 更新するユーザーが存在しない場合
                if (updateMember == null)
                {
                    // bad request
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }

                // nullではない値を設定する
                foreach (var propInfo in member.GetType().GetProperties())
                {
                    var propValue = propInfo.GetValue(member, null);
                    if (propValue != null)
                    {
                        updateMember.GetType().GetProperty(propInfo.Name).SetValue(updateMember, propValue);
                    }
                }
                //updateMember.member_createat = member_createat;
                updateMember.member_updateat = DateTime.Now;

                // スマレジ会員情報を更新
                try
                {
                    updateMember = await this.UpdateSmaregiCustomer(updateMember);
                }
                catch (ApplicationException e)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, e.Message);
				}

                // ユーザーを更新
                await _context.SaveChangesAsync();
                return member;
            }
            catch (Exception e)
            {
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        /// <summary>
        /// LINEログインAPIの処理
        /// </summary>
        /// <param name="code">認可コード</param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpGet("member/hook")]
        public async Task<IActionResult> LineLoginCallback([FromQuery(Name = "code")] string code, [FromQuery(Name = "state")] string state,
                                                              [FromQuery(Name = "error")] string error, [FromQuery(Name = "error_description")] string error_description)
        {
            try
            {
                // stateのデータを解析
                var temp = state.Split(",");

                // stateのフォーマットが間違いの場合
                if (temp.Length != 3)
                {
                    // callbackurlが読めないので、ERROR_CALLBACK_URLに遷移
                    return Redirect(ERROR_CALLBACK_URL);
                }

                // ユーザーIDを取得
                string userid = temp[1];

                // コールバックURLを取得
                string callbackurl = temp[2];

                // ユーザーのアクセス権が取れない場合
                if (string.IsNullOrEmpty(code))
                {
                    return Redirect(callbackurl + "?error=" + error ?? "Unknown" + "&error_description=" + error_description ?? "Unknown");
                }

                // LINEユーザーIDを取得
                //var user_lineid = await LoginLogic.GetLineUserID(code);
                var user_profile = await LoginLogic.GetLineUserProfile(code);
                var user_lineid = user_profile.Split(",")[0];
                var user_displayname = user_profile.Split(",")[1];

                // 同じlineidの他のユーザーを取得する
                var duplicateLineUsers = _context.member.Where(u => u.member_lineid == user_lineid).ToList();

                // ユーザー引継ぎの場合？
                if (userid != "null" && userid != "")
                {
                    // システムユーザーを取得
                    var member = await _context.member.Where(u => u.member_code == userid).SingleOrDefaultAsync();

                    // ユーザーが存在する場合
                    if (member != null)
                    {
                        // 同じlineidの他のユーザーのlineidをnullに更新
                        duplicateLineUsers.ForEach(d_u => d_u.member_lineid = null);

                        string member_nonce = member.member_code; //GenerateNonceString();
                        member.member_nonce = member_nonce;

                        member.member_lineid = user_lineid;
                        await _context.SaveChangesAsync();
                        return Redirect(callbackurl + "?nonce=" + member_nonce);
                    }

                    // 新規登録にいく
                }

                // 新規登録の場合？
                {

                    // ノンスを作成
                    string member_nonce = GenerateNonceString();

                    // lineidでユーザーを取得
                    var member = _context.member.Local.Where(u => u.member_lineid == user_lineid).FirstOrDefault();

                    // ユーザーが存在しない場合
                    if (member == null)
                    {
                        // userをnonceとlineidだけ作成
                        Request.Headers[HeaderNames.Authorization] = API_ACCESS_TOKEN;

                        Func<string, long?> stringToLong = s =>
                        {
                            long temp;
                            if (long.TryParse(s, out temp))
                            {
                                return temp;
                            }
                            return null;
                        };

                        var newUserId = _context.member.Local.Select(u => stringToLong(u.member_code)).Where(x => x.HasValue).Max() + 1;

                        var result = await CreateMember(new member()
                        {
                            member_nonce = member_nonce,
                            member_lineid = user_lineid,
                            member_code = newUserId.ToString(),
                            member_firstname = user_displayname,
                            //member_firstname = "",
                            member_lastname = "",
                            member_firstname_kana = "",
                            member_lastname_kana = "",
                            member_rank = "",
                        }) ;

                        // ユーザーの作成が失敗する場合
                        if (result.Value == null)
                        {
                            // bad request
                            return Redirect(ERROR_CALLBACK_URL);
                        }

                        ////2021.8.20 detail（登録画面）に飛ばしてみる ⇒ 8.28 やめる
                        //Uri u = new Uri(callbackurl);
                        //string url = DETAIL_URL + "?nonce=" + member_nonce;
                        //return Redirect(url);
                        
                        return Redirect(callbackurl + "?nonce=" + member_nonce);
                    }

                    // 同じlineidの他のユーザーをmergeする
                    foreach (var propInfo in member.GetType().GetProperties())
                    {
                        if (propInfo.GetValue(member, null) == null)
                        {
                            foreach (var duplicateUser in duplicateLineUsers)
                            {
                                var propValue = duplicateUser.GetType().GetProperty(propInfo.Name).GetValue(duplicateUser, null);
                                if (propValue != null)
                                {
                                    propInfo.SetValue(member, propValue);
                                    break;
                                }
                            }
                        }
                    }

                    // ユーザーに新しいノンスを更新
                    member.member_nonce = member_nonce;
                    await _context.SaveChangesAsync();

                    return Redirect(callbackurl + "?nonce=" + member_nonce);
                }
            }
            catch (Exception)
            {
                // callbackurlが読めないので、ERROR_CALLBACK_URLに遷移
                return Redirect(ERROR_CALLBACK_URL);
            }
        }

        //[HttpGet("member/afterlogin")]
        //public async Task<string> LineAfterLogin([FromQuery(Name = "acesstoken")] string acesstoken, [FromQuery(Name = "userid")] string memberid, [FromQuery(Name = "displayName")] string displayname, [FromQuery(Name = "member_lineid")] string member_lineid)
        //{
        //    try
        //    {
        //        // ノンスを作成
        //        string member_nonce = GenerateNonceString();

        //        //// 同じlineidの他のユーザーを取得する
        //        //var duplicateLineUsers = _context.member.Where(u => u.member_lineid == member_lineid).ToList();

        //        //// ユーザー引継ぎの場合(いったん未使用)
        //        //if (memberid != null)
        //        //{
        //        //    // システムユーザーを取得
        //        //    var member = await _context.member.FindAsync(memberid);

        //        //    // ユーザーが存在する場合
        //        //    if (member != null)
        //        //    {
        //        //        // 同じlineidの他のユーザーのlineidをnullに更新
        //        //        duplicateLineUsers.ForEach(d_u => d_u.member_lineid = null);

        //        //        member.member_nonce = member_nonce;

        //        //        member.member_lineid = member_lineid;
        //        //        await _context.SaveChangesAsync();
        //        //        return member_nonce;
        //        //    }

        //        //    // 新規登録にいく
        //        //}

        //        // 新規登録 or 登録済みの場合
        //        {
        //            // lineidでユーザーを取得
        //            var member = _context.member.Local.Where(u => u.member_lineid == member_lineid).FirstOrDefault();

        //            // ユーザーが存在しない場合
        //            if (member == null)
        //            {
        //                // userをnonceとlineidだけ作成
        //                Request.Headers[HeaderNames.Authorization] = API_ACCESS_TOKEN;

        //                Func<string, long?> stringToLong = s =>
        //                {
        //                    long temp;
        //                    if (long.TryParse(s, out temp))
        //                    {
        //                        return temp;
        //                    }
        //                    return null;
        //                };

        //                //var newMemberCode = _context.member.Local.Select(u => stringToLong(u.member_code)).Where(x => x.HasValue).Max() + 1;
        //                //チェックデジットを付ける
        //                var newMemberCode = _context.member.Local.Select(u => stringToLong(u.member_code)).Where(x => x.HasValue).Max();
        //                var newMemberCodeStr = (stringToLong(newMemberCode.ToString()[0..^1]) + 1).ToString();
        //                var result = await CreateMember(new member()
        //                {
        //                    member_nonce = member_nonce,
        //                    member_lineid = member_lineid,
        //                    member_code = newMemberCodeStr + GetModulus10Weight3(newMemberCodeStr),
        //                    member_firstname = displayname,
        //                    member_lastname = "",
        //                    member_firstname_kana = "",
        //                    member_lastname_kana = "",
        //                    member_rank = "2",  //一般
        //                });

        //                // ユーザーの作成が失敗する場合
        //                if (result.Value == null)
        //                {
        //                    // bad request
        //                    return null;
        //                }

        //                return member_nonce;

        //                /// <summary>  
        //                /// モジュラス10/ウェイト3　チェックデジット計算  
        //                /// </summary>  
        //                string GetModulus10Weight3(string Value)
        //                {
        //                    if (!System.Text.RegularExpressions.Regex.IsMatch(Value, @"^[0-9]+$"))
        //                    {
        //                        throw new FormatException();
        //                    }

        //                    int x = 0;
        //                    for (int i = 0; i < Value.Length; i++)
        //                    {
        //                        x += int.Parse(Value[Value.Length - 1 - i].ToString()) * ((i % 2 == 0) ? 3 : 1);
        //                    }

        //                    x = (10 - (x % 10)) % 10;

        //                    return x.ToString();
        //                }
        //            }

        //            //// 同じlineidの他のユーザーをmergeする（いったんコメント）
        //            //foreach (var propInfo in member.GetType().GetProperties())
        //            //{
        //            //    if (propInfo.GetValue(member, null) == null)
        //            //    {
        //            //        foreach (var duplicateUser in duplicateLineUsers)
        //            //        {
        //            //            var propValue = duplicateUser.GetType().GetProperty(propInfo.Name).GetValue(duplicateUser, null);
        //            //            if (propValue != null)
        //            //            {
        //            //                propInfo.SetValue(member, propValue);
        //            //                break;
        //            //            }
        //            //        }
        //            //    }
        //            //}

        //            // ユーザーに新しいノンスを更新
        //            member.member_nonce = member_nonce;
        //            await _context.SaveChangesAsync();
        //            return member_nonce;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // callbackurlが読めないので、ERROR_CALLBACK_URLに遷移
        //        return null;
        //    }
        //}

        [HttpGet("member/afterlogin")]
        public async Task<member> LineAfterLogin([FromQuery(Name = "acesstoken")] string acesstoken, [FromQuery(Name = "userid")] string memberid, [FromQuery(Name = "displayName")] string displayname, [FromQuery(Name = "member_lineid")] string member_lineid)
        {
            try
            {
                // ノンスを作成
                string member_nonce = GenerateNonceString();

                // lineidでユーザーを取得
                var member = _context.member.Local.Where(u => u.member_lineid == member_lineid).FirstOrDefault();

                // ユーザーが存在しない場合
                if (member == null)
                {
                    // userをnonceとlineidだけで会員作成
                    Request.Headers[HeaderNames.Authorization] = API_ACCESS_TOKEN;

                    Func<string, long?> stringToLong = s =>
                    {
                        long temp;
                        if (long.TryParse(s, out temp))
                        {
                            return temp;
                        }
                        return null;
                    };

                    //var newMemberCode = _context.member.Local.Select(u => stringToLong(u.member_code)).Where(x => x.HasValue).Max() + 1;
                    //チェックデジットを付ける
                    var newMemberCode = _context.member.Local.Select(u => stringToLong(u.member_code)).Where(x => x.HasValue).Max();
                    var newMemberCodeStr = (stringToLong(newMemberCode.ToString()[0..^1]) + 1).ToString();
                    var result = await CreateMember(new member()
                    {
                        member_nonce = member_nonce,
                        member_lineid = member_lineid,
                        member_code = newMemberCodeStr + GetModulus10Weight3(newMemberCodeStr),
                        member_firstname = displayname,
                        member_lastname = "",
                        member_firstname_kana = "",
                        member_lastname_kana = "",
                        member_rank = "2",  //一般
                    });

                    // ユーザーの作成が失敗する場合
                    if (result.Value == null)
                    {
                        // bad request
                        return null;
                    }

                    /// <summary>  
                    /// モジュラス10/ウェイト3　チェックデジット計算  
                    /// </summary>  
                    string GetModulus10Weight3(string Value)
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(Value, @"^[0-9]+$"))
                        {
                            throw new FormatException();
                        }

                        int x = 0;
                        for (int i = 0; i < Value.Length; i++)
                        {
                            x += int.Parse(Value[Value.Length - 1 - i].ToString()) * ((i % 2 == 0) ? 3 : 1);
                        }

                        x = (10 - (x % 10)) % 10;

                        return x.ToString(); 
                    }
                }

                // ユーザーに新しいノンスを更新
                if (member != null)
                {
                    member.member_nonce = member_nonce;
                    await _context.SaveChangesAsync();
                }
                return member;
            }
            catch (Exception)
            {
                // callbackurlが読めないので、ERROR_CALLBACK_URLに遷移
                return null;
            }
        }

        [HttpGet("member/find/{id}")]
        public async Task<ActionResult<member>> FindMember(string id)
        {
            member ret = null;
            try
            {
                if (string.IsNullOrEmpty(id) || id.ToLower() == "null")
                {
                    // bad request
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }

                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                // ①user_nonceでユーザーを取得
                var member_by_member_nonce = _context.member.Where(u => u.member_nonce == id).FirstOrDefault();

                // ユーザーを取得できる場合は
                if (member_by_member_nonce != null)
                {
                    //return member_by_member_nonce;
                    ret = member_by_member_nonce;
                }
                else
                {
                    // ②user_lineidでユーザーを取得
                    var member_by_member_lineid = _context.member.Where(u => u.member_lineid == id).FirstOrDefault();

                    // ユーザーを取得できる場合は
                    if (member_by_member_lineid != null)
                    {
                        //return member_by_member_lineid;
                        ret = member_by_member_lineid;
                    }
                    else
                    {
                        // ③member_codeでユーザーを取得
                        var member_by_member_code = _context.member.Where(u => u.member_code == id).FirstOrDefault();

                        // ユーザーを取得できる場合は
                        if (member_by_member_code != null)
                        {
                            //return member_by_member_code;
                            ret = member_by_member_code;
                        }
                    }
                }

                ////スマレジ会員を取得する（匠宿専用）TODO 同期後は不要
                //if (ret != null)
                //{
                //    //var member_by_smaregi = await GetSmaregiCustomer(ret.member_pos_id ?? 0);
                //    var member_by_smaregi = await GetSmaregiCustomerPoint(ret.member_pos_id ?? 0);
                //    if (member_by_smaregi.Value != null)
                //    {
                //        //ポイントと期限を差し替え
                //        ret.member_hold_point = member_by_smaregi.Value.member_hold_point;
                //        ret.member_point_limit_date = member_by_smaregi.Value.member_point_limit_date;
                //    }
                //    else
                //    {
                //        ret.member_hold_point = null;
                //    }
                //}

                // 取得したユーザーを返す
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        /// <summary>
        /// 見つからないとき作成して返す
        /// </summary>
        /// <param name="lineid">lineid</param>
        /// <returns></returns>
        [HttpGet("member/createFind/{id}")]
        public async Task<ActionResult<member>> FindCreateMember(string id)
        {
            member ret = null;
            try
            {
                if (string.IsNullOrEmpty(id) || id.ToLower() == "null")
                {
                    // bad request
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }

                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                // ①user_nonceでユーザーを取得
                var member_by_member_nonce = _context.member.Where(u => u.member_nonce == id).FirstOrDefault();

                // ユーザーを取得できる場合は
                if (member_by_member_nonce != null)
                {
                    //return member_by_member_nonce;
                    ret = member_by_member_nonce;
                }
                else
                {
                    // ②user_lineidでユーザーを取得
                    var member_by_member_lineid = _context.member.Where(u => u.member_lineid == id).FirstOrDefault();

                    // ユーザーを取得できる場合は
                    if (member_by_member_lineid != null)
                    {
                        //return member_by_member_lineid;
                        ret = member_by_member_lineid;
                    }
                    else
                    {
                        // ③member_codeでユーザーを取得
                        var member_by_member_code = _context.member.Where(u => u.member_code == id).FirstOrDefault();

                        // ユーザーを取得できる場合は
                        if (member_by_member_code != null)
                        {
                            //return member_by_member_code;
                            ret = member_by_member_code;
                        }
                    }
                }

                //存在しない場合、新規作成
                if (ret == null)
                {
                    var member_by_create_member = await LineAfterLogin(accessToken, null, "@", id);
                    //// user_nonceでユーザーを取得
                    //var member_by_create_member = _context.member.Where(u => u.member_nonce == res).FirstOrDefault();
                    // ユーザーを取得できる場合は
                    if (member_by_create_member != null)
                    {
                        //return member_by_create_member;
                        ret = member_by_create_member;
                    }
                }

                ////スマレジ会員を取得する（匠宿専用）TODO 同期後は不要
                //if (ret != null)
                //{
                //    var member_by_smaregi = await GetSmaregiCustomerPoint(ret.member_pos_id ?? 0);
                //    if (member_by_smaregi.Value != null)
                //    {
                //        //ポイントと期限を差し替え
                //        ret.member_hold_point = member_by_smaregi.Value.member_hold_point;
                //        ret.member_point_limit_date = member_by_smaregi.Value.member_point_limit_date;
                //    }
                //    else
                //    {
                //        ret.member_hold_point = null;
                //    }
                //}

                // 取得したユーザーを返す
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        /// <summary>
        /// 見つからないとき作成して返す
        /// </summary>
        /// <param name="lineId">lineId</param>
        /// <param name="displayName">displayName</param>
        /// <returns></returns>
        [HttpGet("member/createFind2")]
        public async Task<ActionResult<member>> FindCreateMember_by_LineId([FromQuery] string lineId, [FromQuery] string displayName)
        {
            member ret = null;
            try
            {
                if (string.IsNullOrEmpty(lineId) || lineId.ToLower() == "null")
                {
                    // bad request
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }

                if (string.IsNullOrEmpty(displayName) || displayName.ToLower() == "null")
                {
                    // Default @
                    displayName = "@";
                }

                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                // user_lineidでユーザーを取得
                var member_by_member_lineid = _context.member.Where(u => u.member_lineid == lineId).FirstOrDefault();

                // ユーザーを取得できる場合は
                if (member_by_member_lineid != null)
                {
                    ret = member_by_member_lineid;
                }
                else
                {
                    //存在しない場合、新規作成
                    var member_by_create_member = await LineAfterLogin(accessToken, null, displayName, lineId);

                    // ユーザーを取得できる場合は
                    if (member_by_create_member != null)
                    {
                        ret = member_by_create_member;
                    }
                }

                // 取得したユーザーを返す
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }
        
        /// <summary>
                 /// 来店ポイント等の付与
                 /// </summary>
                 /// <param name="input">
                 ///     qrCode
                 ///     memberCode
                 /// </param>
        [HttpPost("member/pointget", Name = "Point Get")]
        public async Task<ActionResult<Response>> PointGet([FromBody] dynamic input)
        {
            try
            {
                //string action = "QRScan";
                //if (input.action != null){
                //    action = input.action;
                //}

                string qrCode = input.qrCode;
                if(string.IsNullOrEmpty(qrCode)) {
                    return new Response()
                    {
                        result = CLIENT_RESPONSE_SUCCESS,
                        message = "",
                        data = new member()
                    };
                }
                string memberCode = input.memberCode;

                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    this.CreatePointGetLog(qrCode, "", null, QrcodeLog.ResultType.error, $"認証失敗。memberCode:{memberCode}");
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                // ユーザー存在チェック
                var member = _context.member.FirstOrDefault(m => m.member_lineid == memberCode);
                // ユーザーを取得できなかった場合は
                if (member == null)
                {
                    // codeでユーザーを取得
                    var member_by_member_code = _context.member.FirstOrDefault(m => m.member_code == memberCode);

                    // ユーザーを取得できなかった場合は
                    if (member_by_member_code == null)
                    {
                        // nonceでユーザーを取得
                        var member_by_member_nonce = _context.member.FirstOrDefault(m => m.member_nonce == memberCode);

                        // ユーザーを取得できなかった場合は
                        if (member_by_member_nonce == null)
                        {
                            // bad request
                            this.CreatePointGetLog(qrCode, "", null, QrcodeLog.ResultType.error, $"不正な会員証。memberCode:{memberCode}");
                            // Error response
                            return new Response()
                            {
                                result = CLIENT_RESPONSE_ERROR,
                                message = CLIENT_RESPONSE_MESSAGE_NOT_FOUND,
                                data = new member()
                            };
                        }
                        else
                        {
                            member = member_by_member_nonce;
                        }
                    }
                    else
                    {
                        member = member_by_member_code;
                    }
                }

                // QRコード存在チェック
                var qr = _context.qrcode.FirstOrDefault(q => q.qrcode_id == qrCode.Trim());
                if (qr == null)
                {
                    // bad request
                    this.CreatePointGetLog(qrCode, "", member.member_id, QrcodeLog.ResultType.error, $"QRコードが存在しません。");
                    // Error response
                    return new Response()
                    {
                        result = CLIENT_RESPONSE_ERROR,
                        message = CLIENT_RESPONSE_MESSAGE_NOT_FOUND_QRCODE,
                        data = new member()
                    };
                }
                else if (qr.qrcode_expiry < DateTime.Now)
                {
                    // QRコードの有効期限切れ
                    // bad request
                    this.CreatePointGetLog(qr.qrcode_id, qr.qrcode_type.ToString(), member.member_id, QrcodeLog.ResultType.error, $"QRコードの有効期限切れ。有効期限:{qr.qrcode_expiry}");
                    // Error response
                    return new Response()
                    {
                        result = CLIENT_RESPONSE_ERROR,
                        message = CLIENT_RESPONSE_MESSAGE_EXPIRED_QRCODE,
                        data = new member()
                    };
                }

                // QRType=1:ポイント付与は一日一回まで、QRType=2:ポイント付与は一人一回まで
                if (qr.qrcode_type == 1)
                {
                    var today = DateTime.Today;
                    var tomorrow = DateTime.Today.AddDays(1);
                    var isAlreadyGiven = _context.qrcodeLog.Any(log =>
                        log.QrcodeLogMemberId == member.member_id &&
                        log.QrcodeLogQrcodeId == qr.qrcode_id &&
                        log.QrcodeLogResult == QrcodeLog.ResultType.success.ToString() &&
                        log.QrcodeLogCreated.HasValue &&
                        today <= log.QrcodeLogCreated.Value &&
                        log.QrcodeLogCreated.Value < tomorrow);

                    if (isAlreadyGiven)
                    {
                        // 既に本日ポイント付与済
                        this.CreatePointGetLog(qr.qrcode_id, qr.qrcode_type.ToString(), member.member_id, QrcodeLog.ResultType.error, $"既に本日ポイント付与済");
                        // Error response
                        return new Response()
                        {
                            result = CLIENT_RESPONSE_ERROR,
                            message = CLIENT_RESPONSE_MESSAGE_1DAY_1TIME,
                            data = new member()
                        };
                    }
                }
                else
                {
                    var isAlreadyGiven = _context.qrcodeLog.Any(log =>
                        log.QrcodeLogMemberId == member.member_id &&
                        log.QrcodeLogQrcodeId == qr.qrcode_id &&
                        log.QrcodeLogResult == QrcodeLog.ResultType.success.ToString() &&
                        log.QrcodeLogCreated.HasValue);

                    if (isAlreadyGiven)
                    {
                        // 既にポイント付与済
                        this.CreatePointGetLog(qr.qrcode_id, qr.qrcode_type.ToString(), member.member_id, QrcodeLog.ResultType.error, $"既にポイント付与済");
                        // Error response
                        return new Response()
                        {
                            result = CLIENT_RESPONSE_ERROR,
                            message = CLIENT_RESPONSE_MESSAGE_ONCE_PERSON,
                            data = new member()
                        };
                    }
                }

                // スマレジポイント付与
                try
                {
                    var currentPoint = await this.UpdateSmaregiPointGet(member, qr.qrcode_point);

                    // 会員のポイント情報を更新
                    member.member_hold_point = (int)currentPoint;
                    member.member_last_pointget_point = (short)qr.qrcode_point;
                    member.member_last_pointget_date = DateTime.Today;

                    // ログ追加 + DB更新
                    this.CreatePointGetLog(qr.qrcode_id, qr.qrcode_type.ToString(), member.member_id, QrcodeLog.ResultType.success, $"ポイント付与成功。付与ポイント:{qr.qrcode_point}, 総保有ポイント:{member.member_hold_point}");

                    // 更新された会員情報を返却
                    return new Response()
                    {
                        result = CLIENT_RESPONSE_SUCCESS,
                        message = CLIENT_RESPONSE_MESSAGE_POINT_SUCCESS + $"獲得付与ポイント:{qr.qrcode_point}pt  保有ポイント:{member.member_hold_point}pt",
                        data = member
                    };
                }
                catch (ApplicationException e)
                {
                    this.CreatePointGetLog(qr.qrcode_id, qr.qrcode_type.ToString(), member.member_id, QrcodeLog.ResultType.error, $"ポイント更新処理でエラー発生。{e.Message}");
                    return StatusCode(StatusCodes.Status400BadRequest, e.Message);
                }
            }
            catch (Exception e)
            {
                // bad request
                this.CreatePointGetLog("", "", null, QrcodeLog.ResultType.error, $"ポイント付与でエラー発生。{e.Message}");
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        /// <summary>
        /// スマレジ会員情報の取得
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("customer/find/{customerId}")]
        public async Task<ActionResult<member>> GetSmaregiCustomer(int customerId)
        {
            try
            {
                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                // アクセストークン取得
                var token = await SmaregiUtil.SmaregiApiUtility.RequestSmaregiToken(
                                _smaregiApiSettings
                                , "pos.customers:read");

                // 会員情報を取得
                var customerJson = await SmaregiUtil.SmaregiApiUtility.GetRequestSmaregiApi<JObject>(
                                    _smaregiApiSettings
                                    , $"/customers/{customerId}"
                                    , "customerId,customerCode,rank,firstName,lastName,firstKana,lastKana," +
                                        "postCode,address,phoneNumber,faxNumber,mobileNumber,mailAddress,sex,birthDate," +
                                        "pointExpireDate,entryDate,leaveDate,note,note2,status"
                                    , token
                                    , null);

                // ポイント情報を取得
                var pointsJson = await SmaregiUtil.SmaregiApiUtility.GetRequestSmaregiApi<JArray>(
                                    _smaregiApiSettings
                                    , $"/customers/point"
                                    , "point,pointExpireDate"
                                    , token
                                    , new Dictionary<string, object>
                                    {
                                        { "customer_id", customerId }
                                    });

                var customer = customerJson.ToObject<Models. SmaregiCustomer>();

                var member = customer.ToMember();

                // ポイント情報をセット
                int point = 0;
                member.member_hold_point = int.TryParse(pointsJson.FirstOrDefault()?.Value<string>("point"), out point) ? point : (int?)null;

                return member;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        /// <summary>
        /// スマレジ会員ポイント情報の取得
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("customerspoint/find/{customerId}")]
        public async Task<ActionResult<member>> GetSmaregiCustomerPoint(int customerId)
        {
            try
            {
                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                // アクセストークン取得
                var token = await SmaregiUtil.SmaregiApiUtility.RequestSmaregiToken(
                                _smaregiApiSettings
                                , "pos.customers:read");

                //// 会員情報を取得
                //var customerJson = await SmaregiUtil.SmaregiApiUtility.GetRequestSmaregiApi<JObject>(
                //                    _smaregiApiSettings
                //                    , $"/customers/{customerId}"
                //                    , "customerId,customerCode,rank,firstName,lastName,firstKana,lastKana," +
                //                        "postCode,address,phoneNumber,faxNumber,mobileNumber,mailAddress,sex,birthDate," +
                //                        "pointExpireDate,entryDate,leaveDate,note,note2,status"
                //                    , token
                //                    , null);

                // ポイント情報を取得
                var pointsJson = await SmaregiUtil.SmaregiApiUtility.GetRequestSmaregiApi<JArray>(
                                    _smaregiApiSettings
                                    , $"/customers/point?customer_id={customerId}"
                                    , "point,pointExpireDate"
                                    , token
                                    , new Dictionary<string, object>
                                    {
                                        { "customer_id", customerId }
                                    });

                //var customer = customerJson.ToObject<Models.SmaregiCustomer>();
                //var member = customer.ToMember();

                // ポイント情報をセット
                int point = 0;
                DateTime limitDate;
                var member = new member();
                member.member_hold_point = int.TryParse(pointsJson.FirstOrDefault()?.Value<string>("point"), out point) ? point : (int?)null;
                member.member_point_limit_date = DateTime.TryParse(pointsJson.FirstOrDefault()?.Value<string>("pointExpireDate"), out limitDate) ? limitDate : (DateTime?)null;

                return member;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        /// <summary>
        /// スマレジ会員情報の更新
        /// </summary>
        /// <param name="member"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private async Task<member> UpdateSmaregiCustomer(member member)
        {
            // アクセストークン取得
            var token = await SmaregiUtil.SmaregiApiUtility.RequestSmaregiToken(_smaregiApiSettings, "pos.customers:write");

            // スマレジ会員情報に変換
            var customer = SmaregiCustomer.FromMember(member);

            // スマレジ会員必須項目が指定されていない場合、一時的にそれを埋める
            if (String.IsNullOrWhiteSpace(customer.LastName))
            {
                customer.LastName = "@";
			}

            if (String.IsNullOrWhiteSpace(customer.FirstName))
            {
                customer.FirstName = "@";
			}

            if (String.IsNullOrWhiteSpace(customer.LastKana))
            {
                customer.LastKana = "@";
			}

            if (String.IsNullOrWhiteSpace(customer.FirstKana))
            {
                customer.FirstKana = "@";
			}

            if (String.IsNullOrEmpty(customer.Sex))
            {
                customer.Sex = "0";
			}

            if (String.IsNullOrEmpty(customer.Rank))
            {
                customer.Rank = "2";
            }

            var customerJson = JObject.FromObject(customer, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });

            try
            {
                if (member.member_pos_id.HasValue)
                {
                    // pos_idを持っている場合
                    // APIリクエストにて会員情報を更新
                    await SmaregiUtil.SmaregiApiUtility.RequestSmaregiApi(
                        _smaregiApiSettings
                        , $"/customers/{member.member_pos_id}"
                        , token
                        , customerJson
                        , "PATCH");

                }
                else
                {
                    // pos_idを持っていない場合
                    // APIリクエストにて会員情報を新規登録
                    var result = await SmaregiUtil.SmaregiApiUtility.RequestSmaregiApi(
                        _smaregiApiSettings
                        , "/customers"
                        , token
                        , customerJson);

                    // 新規に採番されたcustomerIdをpos_idとして設定
                    member.member_pos_id = Convert.ToInt32(result.Value<string>("customerId"));

                    // 新規登録時、100pt付与する　TODO：shopから指定できるようしたい
                    var result2 = await UpdateSmaregiPointGet(member, 100);
                    member.member_hold_point = 100;
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.ProtocolError)
            {
                // プロトコルエラーの場合、エラーレスポンスを取得する
                var json = SmaregiUtil.HttpRequestUtility.GetJsonFromResponse<JObject>(e.Response);
                _logger.LogError($"{json}{System.Environment.NewLine}{e}");

                // エラー内容を取得
                var error = json.Value<string>("detail");

                throw new ApplicationException(error);
            }

            return member;
        }
        /// <summary>
        /// スマレジ会員情報の更新
        /// </summary>
        /// <param name="member"></param>
        /// <param name="error"></param>
        /// <returns>総保有ポイント</returns>
        private async Task<int> UpdateSmaregiPointGet(member member, int point)
        {
            // アクセストークン取得
            var token = await SmaregiUtil.SmaregiApiUtility.RequestSmaregiToken(
                          _smaregiApiSettings
                        , "pos.customers:read"
                        , "pos.customers:write");

            var json = JObject.FromObject(new
            {
                point = point.ToString()
            });

            try
            {
                if (member.member_pos_id.HasValue)
                {
                    // pos_idを持っている場合
                    // APIリクエストにてポイントを付与
                    var resultJson = await SmaregiUtil.SmaregiApiUtility.RequestSmaregiApi(
                          _smaregiApiSettings
                        , $"/customers/{member.member_pos_id.Value}/point/add"
                        , token
                        , json);

                    int currentPoint = int.TryParse(resultJson.Value<string>("point"), out currentPoint) ? currentPoint : 0;

                    return currentPoint;
                }
                else
                {
                    _logger.LogError($"[ポイント付与に失敗]member_pos_id=null。member_id={member.member_id}");
                    throw new ArgumentException("ポイント付与に失敗しました。");
                }

            }
            catch (WebException e) when (e.Status == WebExceptionStatus.ProtocolError)
            {
                // プロトコルエラーの場合、エラーレスポンスを取得する
                var errorJson = SmaregiUtil.HttpRequestUtility.GetJsonFromResponse<JObject>(e.Response);
                _logger.LogError($"{errorJson}{Environment.NewLine}{e}");

                // エラー内容を取得
                var error = errorJson.Value<string>("detail");

                throw new ApplicationException(error);
            }
        }

        /// <summary>
        /// ポイント取得ログ作成
        /// </summary>
        /// <param name="qrcodeId"></param>
        /// <param name="logType"></param>
        /// <param name="memberId"></param>
        /// <param name="result"></param>
        /// <param name="message"></param>
        private void CreatePointGetLog(string qrcodeId, string logType, int? memberId, QrcodeLog.ResultType result, string message)
        {
            _context.qrcodeLog.Add(new QrcodeLog
            {
                QrcodeLogQrcodeId = qrcodeId,
                QrcodeLogType = logType,
                QrcodeLogMemberId = memberId,
                QrcodeLogResult = result.ToString(),
                QrcodeLogMessage = message,
            });

            _context.SaveChanges();
        }

        [HttpGet("member/find-by-lineId/{lineId}")]
        public async Task<ActionResult<member>> FindByMemberByLineId(string lineId)
        {
            member ret = null;
            try
            {
                if (string.IsNullOrEmpty(lineId) || lineId.ToLower() == "null")
                {
                    // bad request
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }
               
                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                // user_lineidでユーザーを取得
                var member_by_member_lineid = _context.member.Where(u => u.member_lineid == lineId).FirstOrDefault();

                // ユーザーを取得できる場合は
                if (member_by_member_lineid != null)
                {
                    ret = member_by_member_lineid;
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }

                // 取得したユーザーを返す
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        [HttpGet("member/find-by-member/{memberCode}")]
        public async Task<ActionResult<member>> FindByMember(string memberCode)
        {
            member ret = null;
            try
            {
                if (string.IsNullOrEmpty(memberCode) || memberCode.ToLower() == "null")
                {
                    // bad request
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }

                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                long memberId = long.Parse(memberCode);
                // ①user_nonceでユーザーを取得
                var member = _context.member.Where(u => u.member_code == memberCode || u.member_id == memberId).FirstOrDefault();

                // ユーザーを取得できる場合は
                if (member != null)
                {
                    //return member_by_member_nonce;
                    ret = member;
                } else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }


                ////スマレジ会員を取得する（匠宿専用）TODO 同期後は不要
                /*if (ret != null)
                {
                    //var member_by_smaregi = await GetSmaregiCustomer(ret.member_pos_id ?? 0);
                    var member_by_smaregi = await GetSmaregiCustomerPoint(ret.member_pos_id ?? 0);
                    if (member_by_smaregi.Value != null)
                    {
                        //ポイントと期限を差し替え
                        ret.member_hold_point = member_by_smaregi.Value.member_hold_point;
                        ret.member_point_limit_date = member_by_smaregi.Value.member_point_limit_date;
                    }
                    else
                    {
                        ret.member_hold_point = null;
                    }
                }*/

                // 取得したユーザーを返す
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        [HttpPost("member/updatePoint")]
        public async Task<ActionResult<member>> UpdateMemberPoint(MemberView memberReq)
        {
            member ret = null;
            try
            {
                if (memberReq == null || memberReq.member_code == null)
                {
                    // bad request
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }

                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                // ①user_nonceでユーザーを取得
                var member = _context.member.Where(u => u.member_code == memberReq.member_code).FirstOrDefault();

                // ユーザーを取得できる場合は
                if (member != null)
                {
                    //return member_by_member_nonce;
                    ret = member;
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
                }


                ////スマレジ会員を取得する（匠宿専用）TODO 同期後は不要
                if (ret != null)
                {
                    //var member_by_smaregi = await GetSmaregiCustomer(ret.member_pos_id ?? 0);
                    //var currentPoint = await UpdateSmaregiPointGet(member, point);
                    member.member_hold_point += memberReq.pointNeedUpdate;
                }

                var user = _context.user.Where(u => u.user_lineid == memberReq.member_lineid).FirstOrDefault();

                //create transaction
                transaction trans = new transaction()
                {
                    transaction_created = DateTime.Now,
                    transaction_datetime = (DateTime)memberReq.pointDate,
                    transaction_give_point = memberReq.give_point,
                    transaction_member_id = ret.member_id,
                    transaction_point_rate = new Decimal(0.01),
                    transaction_shop_id = ret.member_shop_id,
                    transaction_spend_point = memberReq.spend_point,
                    transaction_subtotal = memberReq.subtotal,
                    transaction_total = memberReq.subtotal - memberReq.spend_point,
                    transaction_status = CommonConst.transaction_add,
                    transaction_user_id = user == null ? null : user?.user_id
                };
                _context.transaction.Add(trans);               
                _context.SaveChangesAsync();
                // 取得したユーザーを返す
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        //find all transaction by line_id
        [HttpGet("member/find-transaction/{memberId}")]
        public async Task<ActionResult<List<transaction>>> FindTransactionByLineId(int memberId)
        {
            try
            {

                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                var transactions = _context.transaction.Where(u => u.transaction_member_id == memberId)
                    .OrderByDescending(u => u.transaction_datetime)
                    .OrderByDescending(u => u.transaction_id).ToList();

                return transactions;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }

        //rollback transaction
        [HttpPost("member/rollback-transaction/{transactionId}")]
        public async Task<ActionResult<string>> rollBackTransaction(int transactionId)
        {
            try
            {

                // Authorization check
                var accessToken = Request.Headers[HeaderNames.Authorization];
                if (accessToken != API_ACCESS_TOKEN)
                {
                    // Unauthorized
                    return StatusCode(StatusCodes.Status401Unauthorized, CLIENT_RESPONSE_CODE_401);
                }

                //find transaction
                var transaction = _context.transaction.Where(u => u.transaction_id == transactionId).FirstOrDefault();
                if(transaction != null)
                {
                    //update point for member
                    var member = _context.member.Where(u => u.member_id == transaction.transaction_member_id).FirstOrDefault();
                    if(member != null)
                    {
                        var subTotal = transaction.transaction_subtotal - transaction.transaction_spend_point;
                        var pointReceive = (int)Math.Ceiling(subTotal * 0.01);

                        member.member_hold_point = member.member_hold_point - pointReceive - transaction.transaction_give_point  + transaction.transaction_spend_point;

                        transaction trans = new transaction()
                        {
                            transaction_created = DateTime.Now,
                            transaction_datetime = DateTime.Now,
                            transaction_give_point = -transaction.transaction_give_point,
                            transaction_member_id = member.member_id,
                            transaction_point_rate = new Decimal(0.01),
                            transaction_shop_id = transaction.transaction_shop_id,
                            transaction_spend_point = -transaction.transaction_spend_point,
                            transaction_subtotal = -transaction.transaction_subtotal,
                            transaction_total = -transaction.transaction_total,
                            transaction_status = CommonConst.transaction_sub,

                            transaction_user_id = transaction.transaction_user_id
                        };

                        transaction.transaction_status = CommonConst.transaction_already_sub;
                        transaction.transaction_updated = DateTime.Now;
                        _context.transaction.Add(trans);
                        _context.SaveChangesAsync();
                        return "success";

                    }
                    
                }

                return CLIENT_RESPONSE_CODE_400;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // bad request
                return StatusCode(StatusCodes.Status400BadRequest, CLIENT_RESPONSE_CODE_400);
            }
        }
    }
}
