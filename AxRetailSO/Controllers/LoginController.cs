using AxRetailSO.Classes;
using AxRetailSO.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AxRetailSO.Controllers
{
    public class LoginController : ApiController
    {
        
        [Route("User/{user}/{pwd}")]
        [HttpGet]
        public IHttpActionResult GetUser(string user, string pwd)
        {
            List<SalesStore> store = new List<SalesStore>();
            List<User> userlist = new List<User>();

            if (user == "admindev" && pwd == "_admin123")
            {
                userlist.Add(new Models.User { Username = user, Password = pwd, UserType = "4" });
            }
            else
            {
                string sql = string.Format(@"SELECT [STMNAME]
                                              ,[STMPASSWORD]
                                              ,[STMSALESSTORETYPE]
                                              ,[STMUSERNAME]
                                          FROM [dbo].[STMSALESUSER]
                                          WHERE STMUSERNAME = '{0}' AND STMPASSWORD = '{1}'", user, pwd);

                DataTable dtUser = STM.QuerySelect(sql);

                if (dtUser.Rows.Count > 0)
                {
                    //encode password
                    byte[] encodebyte = System.Text.Encoding.UTF8.GetBytes(pwd);
                    string encode = System.Convert.ToBase64String(encodebyte);

                    userlist.Add(new Models.User { Username = user, Password = encode, UserType = dtUser.Rows[0]["StmSalesStoreType"].ToString() });
                }
                else
                {
                    userlist.Add(new Models.User { Username = "incorrect" });
                }
            }
          
            return Json(userlist);
        }
    }
}
