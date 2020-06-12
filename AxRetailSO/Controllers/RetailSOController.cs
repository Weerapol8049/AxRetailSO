using AxRetailSO.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace AxRetailSO.Controllers
{
    public class RetailSOController : Controller
    {
        public ActionResult Index(string user)
        {
            string sql = string.Format(@"DECLARE @x AS varchar(60) = '{0}';
                                        IF @x = 'admindev'
	                                        Begin;
		                                        SELECT [SALESAMOUNT]
			                                            ,[SALESDATE]
			                                            ,[SALESPOOLID]
			                                            ,[SALESQTY]
			                                            ,[STMSTOREID]
			                                            ,[RECID]
			                                            ,[CREATEDDATETIME]
			                                            ,[CONFIRMDATE]
			                                            ,[DUEDATE]
			                                            ,[PURCHID]
			                                            ,[SALESID]
			                                            ,[SALESNAME]
		                                            FROM [dbo].[STMSALESSODAILY]	
	                                        End;
                                        ELSE
	                                        Begin;
		                                        SELECT 
				                                            so.[SALESAMOUNT]
				                                            ,so.[SALESDATE]
				                                            ,so.[SALESPOOLID]
				                                            ,so.[SALESQTY]
				                                            ,so.[STMSTOREID]
				                                            ,so.[RECID]
				                                            ,so.[CREATEDDATETIME]
				                                            ,so.[CONFIRMDATE]
				                                            ,so.[DUEDATE]
				                                            ,so.[PURCHID]
				                                            ,so.[SALESID]
				                                            ,so.[SALESNAME]
				                                            ,u.[STMNAME]
				                                            ,u.[STMPASSWORD]
				                                            ,u.[STMSALESSTORETYPE]
				                                            ,u.[STMUSERNAME]
				                                            ,store.STMSTOREID
				                                            ,store.STMSTORENAME
			                                            FROM [dbo].[STMSALESUSER] u
			                                            LEFT JOIN dbo.STMSALESSTORE store
					                                        ON u.STMNAME = CASE
										                                        WHEN u.STMSALESSTORETYPE = 4 THEN store.KEYACMANAGER
										                                        WHEN u.STMSALESSTORETYPE = 3 THEN store.AREAMANAGER
										                                        WHEN u.STMSALESSTORETYPE = 2 THEN store.SALESMANAGER
										                                        WHEN u.STMSALESSTORETYPE = 1 THEN store.SALES
								                                            END
			                                            LEFT JOIN [dbo].[STMSALESSODAILY] so
					                                        ON so.STMSTOREID = store.STMSTOREID
			                                            WHERE STMUSERNAME = @x 
	                                        End;", user);

            DataTable dt = STM.QuerySelect(sql);

            ViewData["Order"] = dt;
            
            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(dt);
            return View();
        }
    }
}
