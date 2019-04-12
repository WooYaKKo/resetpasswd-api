using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ResetPasswdApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResetPasswdController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public string Get()
        {
            return "Alive";
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)
        {
            string retval = "failed";

            bool bSuccess = ResetPasswd(id, out retval);
            if (bSuccess) {
                retval = "OK";
            }
            return retval;
        }


        [HttpGet("email/{id}")]
        public ActionResult<string> GetEmailAddr(string id)
        {
            string retval = GetEMailAddress(id);
            return retval;
        }

        private static string CreatePassword(int length, bool special)
        {
            string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            if (special)
            {
                valid = "!@#$";
            }

            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        private static string CreatePassword()
        {
            string passwd = CreatePassword(9, false);
            passwd += CreatePassword(1, true);
			return passwd;
        }

        private static void SendNoticeMail(string addr, string accont, string passwd)
        {
            SmtpClient smtp = new SmtpClient("imail1.interpark.com");

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("ldap@interpark.com");
            msg.To.Add(addr);
            msg.Subject = string.Format("[LDAP] {0} 계정 패스워드 변경 안내", accont);
            msg.Body = string.Format("Password : {0}", passwd);
            smtp.Send(msg);
        }
        private static bool ResetPasswd(string userName, out string message)
        {
            bool bSuccess = false;
            message = "";
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                    {
						string passwd = CreatePassword();

                        user.SetPassword(passwd);
                        user.Save();

						string addr = string.Format("{0}<{1}>", user.GivenName, user.EmailAddress);
                        SendNoticeMail(addr, userName, passwd);
                    }
                }
                bSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                message = ex.Message;
            }

            return bSuccess;
        }

        private static string GetEMailAddress(string userName)
        {
            string retval = "";
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                    {
						retval = user.EmailAddress;
                    }
                }
            }
            catch (Exception ex)
            {
                retval = ex.Message;
            }

            return retval;
        }

        
    }
}
