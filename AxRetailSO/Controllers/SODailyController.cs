using AxRetailSO.AxSalesOrderDaily;
using AxRetailSO.Classes;
using AxRetailSO.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AxRetailSO.Controllers
{
    [RoutePrefix("SoDaily")]
    public class SODailyController : ApiController
    {
        AxdEntity_StmSalesSoDaily soEntity = new AxdEntity_StmSalesSoDaily();
        QueryCriteria qryCri = new QueryCriteria();

        [Route("SalesPool")]
        [HttpGet]
        public IHttpActionResult GetInventModelGroup()
        {
            string sql = @"SELECT Name
                                ,SalesPoolId
                            FROM dbo.SalesPool
                            WHERE SALESPOOLID NOT IN ('BUILT-IN','COMPACT-DC','Loose Fur','Wardrobe','PRO','Mogen')
                            ORDER BY SalesPoolId";
            DataTable dt = STM.QuerySelect(sql);
            List<SalesPool> pool = new List<SalesPool>();

            foreach (DataRow row in dt.Rows)
            {
                pool.Add(new SalesPool { Name = row["Name"].ToString(), PoolId = row["SalesPoolId"].ToString() });
            }

            return Json(pool);
        }

        [Route("GetStored/{user}")]
        [HttpGet]
        public IHttpActionResult GetStored(string user)
        {
            string sql = string.Format(@"DECLARE @user AS varchar(60) = '{0}';
                                        IF @user = 'admindev'
                                        Begin;
	                                        SELECT STMSTOREID
			                                        ,STMSTORENAME
                                                    ,'' AS NAME
	                                        FROM [dbo].STMSALESSTORE
	                                        ORDER BY STMSTOREID
                                        End;
                                        ELSE
                                        Begin;
	                                        SELECT store.STMSTOREID
		                                        ,store.STMSTORENAME
                                                ,u.STMNAME AS NAME
	                                        FROM [dbo].[STMSALESUSER] u
	                                        LEFT JOIN dbo.STMSALESSTORE store
		                                        ON u.STMNAME = CASE
							                                        WHEN u.STMSALESSTORETYPE = 4 THEN store.KEYACMANAGER
							                                        WHEN u.STMSALESSTORETYPE = 3 THEN store.AREAMANAGER
							                                        WHEN u.STMSALESSTORETYPE = 2 THEN store.SALESMANAGER
							                                        WHEN u.STMSALESSTORETYPE = 1 THEN store.SALES
						                                        END
	                                        WHERE STMUSERNAME = @user 
	                                        ORDER BY store.STMSTOREID
                                        End;", user);
            DataTable dt = STM.QuerySelect(sql);
            List<SalesStore> storeList = new List<SalesStore>();

            foreach (DataRow row in dt.Rows)
            {
                storeList.Add(new SalesStore { StoreId = row["STMSTOREID"].ToString(), StoreName = row["STMSTORENAME"].ToString(), UserAccount = row["NAME"].ToString() });
            }

            return Json(storeList);
        }

        [Route("SalesConfirm")]
        [HttpGet]
        public IHttpActionResult GetSalesConfirm()
        {
            List<SODaily> confirm = new List<SODaily>();

            DataSet dataSet = ExecuteStaticQuery.Get("STMSalesConfirm");

            foreach (DataRow rowConf in dataSet.Tables["StmSalesSoDailyConfirm"].Rows)
            {
                foreach (DataRow rowSo in dataSet.Tables["StmSalesSoDaily"].Rows)
                {
                    foreach (DataRow rowUser in dataSet.Tables["StmSalesSoLog"].Rows)
                    {
                        confirm.Add(new SODaily
                        {
                            DueDate = Convert.ToDateTime(rowSo["DueDate"]),
                            CustName = rowSo["SalesName"].ToString(),
                            Amount = Convert.ToDouble(rowSo["SalesAmount"]),
                            Qty = Convert.ToDouble(rowSo["SalesQty"]),
                            Pool = rowSo["SalesPoolId"].ToString(),
                            PurchId = rowSo["PurchId"].ToString(),
                            SalesId = rowSo["SalesId"].ToString(),
                            StoreId = rowSo["StmStoreId"].ToString(),
                            UserName = rowUser["CreateBy"].ToString(),
                            Remark = rowConf["Remark"].ToString()
                        });
                    }
                }

            }

            return Json(confirm);
        }

        [Route("Series/{pool}")]
        [HttpGet]
        public IHttpActionResult GetProductSeries(string pool)
        {
            string sql = string.Format(@"SELECT DISTINCT [SERIES]
                                            ,[SALESPOOLID]
                                        FROM [dbo].[STMPRODUCTSERIES]
                                        WHERE SALESPOOLID = '{0}'
                                        ORDER BY SERIES", pool);
            DataTable dt = STM.QuerySelect(sql);
            List<ProductSeries> series = new List<ProductSeries>();

            foreach (DataRow row in dt.Rows)
            {
                series.Add(new ProductSeries { Series = row["Series"].ToString(), Pool = row["SalesPoolId"].ToString() });
            }
           
            return Json(series);
        }

        [Route("Booking/{pool}")]
        [HttpGet]
        public IHttpActionResult Booking(string pool)
        {
            string sql = string.Format(@"DECLARE @Pool varchar(10) = '{0}'

                                        IF @Pool = 'COMPACT'
                                        BEGIN
	                                        WITH A AS 
	                                        (
		                                        SELECT SUM(SALESQTY) AS QTY
			                                        ,CONFIRMDATE
			                                        ,STMSTOREID
		                                        FROM [dbo].[STMSALESSODAILY] SODaily
		                                        WHERE STMSTOREID <> '' 
			                                        AND (SALESPOOLID LIKE 'COMPACT%' OR SALESPOOLID LIKE 'DIY%') 
		                                        GROUP BY CONFIRMDATE, STMSTOREID
	                                        )
	
	                                        SELECT SUM(C.QTY) AS QTY, CONFIRMDATE, STMSTOREID
	                                        FROM (
		                                        SELECT QTY
			                                        ,CONFIRMDATE
			                                        ,'' AS STMSTOREID
		                                        FROM A 
	                                        ) C 
	                                        GROUP BY CONFIRMDATE, STMSTOREID
                                        END
                                        ELSE
                                        BEGIN
	                                        WITH A AS 
	                                        (
		                                        SELECT SUM(SALESQTY) AS QTY
			                                        ,CONFIRMDATE
			                                        ,STMSTOREID
		                                        FROM [dbo].[STMSALESSODAILY] SODaily
		                                        WHERE STMSTOREID <> '' 
			                                        AND SALESPOOLID LIKE @Pool + '%' 
		                                        GROUP BY CONFIRMDATE, STMSTOREID
	                                        )
	                                        SELECT SUM(C.QTY) AS QTY, CONFIRMDATE, STMSTOREID
	                                        FROM (
		                                        SELECT QTY
			                                        ,CONFIRMDATE
			                                        ,'' AS STMSTOREID
		                                        FROM A 
	                                        ) C 
	                                        GROUP BY CONFIRMDATE, STMSTOREID
                                        END", pool);
            DataTable dt = STM.QuerySelect(sql);
            List<Booking> booking = new List<Booking>();

            foreach (DataRow row in dt.Rows)
            {
                string msg = "", color = "";
                int range = 12;
                string dayname = Convert.ToDateTime(row["CONFIRMDATE"]).ToString("dddd", new System.Globalization.CultureInfo("en-US"));

                if (dayname == "Saturday") range = 20;
                if (Convert.ToInt32(row["QTY"]) >= range)
                {
                    msg = "เต็ม";
                    color = "#FF5733";
                }

                booking.Add(new Booking {
                    title = string.Format("{0} ชุด {1}", Convert.ToInt32(row["QTY"]), msg),
                    start = Convert.ToDateTime(row["CONFIRMDATE"]).ToString("yyyy-MM-ddTHH:mm:00", new System.Globalization.CultureInfo("en-US")),
                    allDay = true,
                    color = color
                });
            }

            return Json(booking);
        }

        [Route("BookingDetail/{date}/{pool}")]
        [HttpGet]
        public IHttpActionResult BookingDetail(string date, string pool)
        {
            string sql = string.Format(@"DECLARE @Pool varchar(10) = '{1}'
                                        DECLARE @Date varchar(10) = '{0}'
                                        IF @Pool = 'COMPACT'
                                        BEGIN
	                                        SELECT SALESQTY
		                                        ,CONFIRMDATE
		                                        ,STMSTOREID
		                                        ,SALESPOOLID
		                                        ,SALESAMOUNT
		                                        ,SALESNAME
		                                        ,DUEDATE
                                                ,SALESDATE
		                                        ,PURCHID
		                                        ,SALESID
	                                        FROM [dbo].[STMSALESSODAILY] SODaily
	                                        WHERE STMSTOREID <> '' 
		                                        AND (SALESPOOLID LIKE 'COMPACT%' OR SALESPOOLID LIKE 'DIY%') 
		                                        AND CONFIRMDATE = @Date
                                        END
                                        ELSE
                                        BEGIN
	                                        SELECT SALESQTY
		                                        ,CONFIRMDATE
		                                        ,STMSTOREID
		                                        ,SALESPOOLID
		                                        ,SALESAMOUNT
		                                        ,SALESNAME
		                                        ,DUEDATE
                                                ,SALESDATE
		                                        ,PURCHID
		                                        ,SALESID
	                                        FROM [dbo].[STMSALESSODAILY] SODaily
	                                        WHERE STMSTOREID <> '' 
		                                        AND SALESPOOLID LIKE @Pool +'%'
		                                        AND CONFIRMDATE = @Date
                                        END",  date.Replace('-','/'), pool);
            DataTable dt = STM.QuerySelect(sql);
            List<SODaily> sodailyList = new List<SODaily>();
            foreach (DataRow row in dt.Rows)
            {
                sodailyList.Add(new SODaily
                {
                    Date = row["SalesDate"].ToString(),
                    StoreId = row["StmStoreId"].ToString(),
                    Pool = row["SalesPoolId"].ToString(),
                    Qty = Convert.ToDouble(row["SalesQty"]),
                    Amount = Convert.ToDouble(row["SalesAmount"]),
                    CustName = row["SalesName"].ToString(),
                    DueDate = Convert.ToDateTime(row["DueDate"]),
                    ConfirmDate = row["ConfirmDate"].ToString(),
                    PurchId = row["PurchId"].ToString(),
                    SalesId = row["SalesId"].ToString()
                });
            }
            
            return Json(sodailyList);
        }

        [Route("Model/{series}")]
        [HttpGet]
        public IHttpActionResult GetModel(string series)
        {
            string sql = string.Format(@"SELECT DISTINCT [MODEL]
                                            ,[SERIES]
                                        FROM [dbo].[STMPRODUCTSERIES]
                                        WHERE SERIES = '{0}'
                                        ORDER BY MODEL", series);
            DataTable dt = STM.QuerySelect(sql);
            List<ProductSeries> modelList = new List<ProductSeries>();

            foreach (DataRow row in dt.Rows)
            {
                modelList.Add(new ProductSeries { Series = row["Series"].ToString(), Model = row["Model"].ToString() });
            }
           
            return Json(modelList);
        }

        [Route("LoadDataV2")]
        [HttpPost]
        public IHttpActionResult LoadDataV2(User user)
        {
            List<SODaily> sodailyList = new List<SODaily>();
       
            string sqlSearch = "";
            if (!string.IsNullOrEmpty(user.TextSearch))
            {
                sqlSearch = string.Format(@"AND (LOWER(so.SALESPOOLID) LIKE @search
				                            OR LOWER(so.PURCHID) LIKE @search
				                            OR LOWER(so.SALESID) LIKE @search
				                            OR LOWER(so.SALESNAME) LIKE @search
				                            OR LOWER(so.STMSTOREID) LIKE @search)", user.TextSearch.ToString().ToLower());
            }

            string sql = string.Format(@"DECLARE @x AS varchar(60) = '{0}';
                                        DECLARE @page AS int = {1};
                                        DECLARE @search AS varchar(60) = '{3}%';
                                        DECLARE @endpage AS int = @page * {4};
                                        DECLARE @startpage AS int = @endpage - ({4} - 1);
                                        IF @x = 'admindev'
	                                        Begin;
		                                        WITH OrderedOrders AS
		                                        (
			                                        SELECT ROW_NUMBER() over(order by SALESDATE desc) as Seq
				                                        ,[SALESAMOUNT]
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
				                                        ,(SELECT COUNT(*) FROM dbo.STMSALESSODAILYLINE WHERE SALESORDERDAILY = so.[RECID]) AS LineNum
				                                        ,(SELECT COUNT(*) FROM dbo.STMSALESIMAGE WHERE REFRECID = so.[RECID]) AS ImageNum
			                                        FROM [dbo].[STMSALESSODAILY] so
			                                        WHERE STMSTOREID <> '' {2}
		                                        ) 

		                                        SELECT Seq 
			                                        ,[SALESAMOUNT]
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
			                                        ,LineNum
			                                        ,ImageNum
                                                    ,(SELECT COUNT(*) FROM OrderedOrders) AS Total
		                                        FROM OrderedOrders 
		                                        WHERE Seq >= @startpage AND Seq <= @endpage
		                                        ORDER BY SALESDATE DESC;
                                           End;
                                        ELSE
	                                        Begin;
			                                        WITH OrderUser AS
			                                        (
				                                        SELECT  ROW_NUMBER() over(order by so.SALESDATE desc) as Seq
						                                        ,so.[SALESAMOUNT]
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
						                                        ,store.STMSTORENAME
						                                        ,(SELECT COUNT(*) FROM dbo.STMSALESSODAILYLINE WHERE SALESORDERDAILY = so.[RECID]) AS LineNum
						                                        ,(SELECT COUNT(*) FROM dbo.STMSALESIMAGE WHERE REFRECID = so.[RECID]) AS ImageNum
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
					                                        WHERE STMUSERNAME = @x AND so.[STMSTOREID] <> ''  {2}               
			                                        )
			                                        SELECT Seq 
				                                        ,[SALESAMOUNT]
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
				                                        ,[STMNAME]
				                                        ,[STMPASSWORD]
				                                        ,[STMSALESSTORETYPE]
				                                        ,[STMUSERNAME]
				                                        ,STMSTORENAME
				                                        ,LineNum
				                                        ,ImageNum
                                                        ,(select count(*) from OrderUser) AS Total
			                                        FROM OrderUser 
			                                        WHERE Seq >= @startpage AND Seq <= @endpage
			                                        ORDER BY SALESDATE DESC;
	                                        End;    

                                    ", user.Username, user.Page, sqlSearch, user.TextSearch, user.PageSize);

            DataTable dt = STM.QuerySelect(sql);

            int total = Convert.ToInt32(dt.Rows[0]["Total"]);

            var pager = new Pager(total, Convert.ToInt32(user.Page), Convert.ToInt32(user.PageSize));

            foreach (DataRow row in dt.Rows)
            {
                sodailyList.Add(new SODaily
                {
                    No = Convert.ToInt32(row["Seq"]),
                    RecId = row["RecId"].ToString(),
                    StoreId = row["StmStoreId"].ToString(),
                    Pool = row["SalesPoolId"].ToString(),
                    Qty = Convert.ToDouble(row["SalesQty"]),
                    Amount = Convert.ToDouble(row["SalesAmount"]),
                    Date = dt.Locale.Name == "th-TH" ? Convert.ToDateTime(row["SalesDate"].ToString()).ToString("dd/MM/yyyy", new CultureInfo("en-US")) : row["SalesDate"].ToString(),
                    DueDate = Convert.ToDateTime(row["DueDate"]),
                    ConfirmDate = dt.Locale.Name == "th-TH" ? Convert.ToDateTime(row["ConfirmDate"].ToString()).ToString("dd/MM/yyyy", new CultureInfo("en-US")) : row["ConfirmDate"].ToString(),
                    CreatedDate = dt.Locale.Name == "th-TH" ? Convert.ToDateTime(row["CREATEDDATETIME"].ToString()).ToString("dd/MM/yyyy", new CultureInfo("en-US")) : row["CREATEDDATETIME"].ToString(),
                    PurchId = row["PurchId"].ToString(),
                    SalesId = row["SalesId"].ToString(),
                    CustName = row["SalesName"].ToString(),
                    LineCount = Convert.ToInt32(row["LineNum"]),
                    ImageCount = Convert.ToInt32(row["ImageNum"]),

                    StartPage = Pager.StartPage.ToString(),
                    EndPage = Pager.EndPage.ToString(),
                    CurrentPage = Pager.CurrentPage.ToString(),
                    TotalPage = Pager.TotalPages.ToString(),
                    Showing = dt.AsEnumerable().Max(r => r["Seq"]).ToString(),
                    Entries = total.ToString()
                });
            }
            //AdminDev type = 4


            #region
            //AxdSalesDaily axdSODaily = new AxdSalesDaily();
            //List<SalesStore> store = new List<SalesStore>();
            //DataTable dtStore = new DataTable();
            ////List<DataRow> rows = new List<DataRow>();
            //int totalEntries = 0;

            ////--------------------- Get Store by User ------------------------
            //store = Stored.Get(user.Username);

            //string storeId = "";
            //foreach (var item in store)
            //{
            //    storeId += item.StoreId + ",";
            //}
            //if (storeId != "")
            //    storeId = storeId.Remove(storeId.Length - 1, 1);

            ////--------------------- Get Data ------------------------
            //    dtStore = QueryData.FindV2(
            //        "STMSalesDaily",
            //        "StmSalesSoDaily",
            //        "StmStoreId",
            //        storeId,
            //        Convert.ToInt32(user.Page),
            //        Convert.ToInt32(user.PageSize)
            //        ).Tables["STMSalesDaily"];

            //if (!string.IsNullOrEmpty(user.TextSearch))
            //{
            //    //dtStore = QueryData.Search(
            //    //   "STMSalesDaily",
            //    //   "StmSalesSoDaily",
            //    //   "StmStoreId",
            //    //   storeId,
            //    //   Convert.ToInt32(user.Page),
            //    //   Convert.ToInt32(user.PageSize)
            //    //   ).Tables["STMSalesDaily"];

            //    dtStore = (from a in dtStore.AsEnumerable()
            //            orderby a.Field<long>("RecId") descending
            //            where a.Field<string>("StmStoreId").ToLower().Contains(user.TextSearch.ToLower()) ||
            //                    a.Field<string>("SalesPoolId").ToLower().Contains(user.TextSearch.ToLower()) ||
            //                    a.Field<string>("PurchId").ToString().ToLower().Contains(user.TextSearch.ToLower()) ||
            //                    a.Field<string>("SalesId").ToString().ToLower().Contains(user.TextSearch.ToLower()) ||
            //                    a.Field<string>("SalesName").ToString().ToLower().Contains(user.TextSearch.ToLower()) ||
            //                    a.Field<double>("SalesQty").ToString().ToLower().Contains(user.TextSearch.ToLower()) ||
            //                    a.Field<double>("SalesAmount").ToString().ToLower().Contains(user.TextSearch.ToLower())
            //            select a)
            //            .CopyToDataTable();

            //    totalEntries = dtStore.Rows.Count;
            //}
            //else
            //{
            //    //--------------------- Get Page ------------------------
            //    #region cal page
            //    var callContext = new CallContext()
            //    {
            //        MessageId = Guid.NewGuid().ToString(),
            //        LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
            //        Language = "en-us"
            //    };

            //    CriteriaElement elem = new CriteriaElement();
            //    elem.DataSourceName = "StmSalesSoDaily";
            //    elem.FieldName = "StmStoreId";
            //    elem.Operator = user.UserType == "4" ? Operator.NotEqual: Operator.Equal;
            //    elem.Value1 = user.UserType == "4"? "" : storeId;

            //    QueryCriteria crit = new QueryCriteria();
            //    crit.CriteriaElement = new CriteriaElement[] { elem };

            //    using (SalesDailyServiceClient client = new SalesDailyServiceClient())
            //    {
            //        client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
            //        client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
            //        client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
            //        axdSODaily = client.find(callContext, crit);
            //    }
            //    totalEntries = axdSODaily.StmSalesSoDaily.Length;
            //}

            //var pager = new Pager(totalEntries, Convert.ToInt32(user.Page), Convert.ToInt32(user.PageSize));

            //var totalPages = Pager.TotalPages;// (int)Math.Ceiling((decimal)totalEntries / (decimal)Convert.ToInt32(user.PageSize));//จำนวนหน้าทั้งหมด
            //var currentPage = Pager.CurrentPage;// Convert.ToInt32(user.Page) != null ? Convert.ToInt32(user.Page) : 1;//หน้าปัจจุบัน
            //var startPage = Pager.StartPage;// currentPage - 5;//แสดงปุ่มแรก
            //var endPage = Pager.EndPage;// currentPage + 4;//แสดงปุ่มสุดท้าย


            //int no = Convert.ToInt32(user.Page) == 0 ? 1 :  Convert.ToInt32(user.Page) * Convert.ToInt32(user.PageSize) - (Convert.ToInt32(user.PageSize) - 1);
            ////record in last page > record in page
            //int show = (Convert.ToInt32(user.Page) * Convert.ToInt32(user.PageSize)) > dtStore.Rows.Count ? totalEntries : Convert.ToInt32(user.Page) * Convert.ToInt32(user.PageSize);

            //foreach (DataRow row in dtStore.Rows)
            //{
            //    //For count image 
            //    DataSet dsimage = QueryData.Find(
            //              "STMSalesImage",
            //              "StmSalesImage",
            //              "RefRecId",
            //              row["RecId"].ToString()
            //              );
            //    //For count Line
            //    DataSet ds = QueryData.Find(
            //           "STMSalesSODailyLine",
            //           "StmSalesSoDailyLine",
            //           "SalesOrderDaily",
            //           row["RecId"].ToString()
            //           );

            //    sodailyList.Add(new SODaily
            //    {
            //        No = no++,
            //        RecId = row["RecId"].ToString(),
            //        StoreId = row["StmStoreId"].ToString(),
            //        Pool = row["SalesPoolId"].ToString(),
            //        Qty = Convert.ToDouble(row["SalesQty"]),
            //        Amount = Convert.ToDouble(row["SalesAmount"]),
            //        Date = dtStore.Locale.Name == "th-TH" ? Convert.ToDateTime(row["SalesDate"].ToString()).ToString("dd/MM/yyyy", new CultureInfo("en-US")) : row["SalesDate"].ToString(),
            //        DueDate = Convert.ToDateTime(row["DueDate"]),
            //        ConfirmDate = dtStore.Locale.Name == "th-TH" ? Convert.ToDateTime(row["ConfirmDate"].ToString()).ToString("dd/MM/yyyy", new CultureInfo("en-US")) : row["ConfirmDate"].ToString(),
            //        PurchId = row["PurchId"].ToString(),
            //        SalesId = row["SalesId"].ToString(),
            //        CustName = row["SalesName"].ToString(),
            //        LineCount = ds.Tables["StmSalesSoDailyLine"].Rows.Count,
            //        ImageCount = dsimage.Tables["STMSalesImage"].Rows.Count,

            //        StartPage = startPage.ToString(),
            //        EndPage = endPage.ToString(),
            //        CurrentPage = currentPage.ToString(),
            //        TotalPage = totalPages.ToString(),
            //        Showing = show.ToString(),
            //        Entries = totalEntries.ToString()
            //    });
            //}
            //#endregion
            #endregion
            return Json(sodailyList);
        }

        [Route("SalesLine")]
        [HttpPost]
        public IHttpActionResult SalesLine(SODaily soDaily)
        {
            List<SODaily> sodailyList = new List<SODaily>();

            string sql = string.Format(@"SELECT ROW_NUMBER() over(order by RECID desc) as Seq
                                              ,[RECID]
                                              ,[MODEL]
                                              ,[SALESAMOUNT]
                                              ,[SALESDATE]
                                              ,[SALESPOOLID]
                                              ,[SALESQTY]
                                              ,[SINK]
                                              ,[SERIES]
                                              ,[STMSTOREID]
                                              ,[SALESORDERDAILY]
                                          FROM [dbo].[STMSALESSODAILYLINE]
                                          WHERE SALESORDERDAILY = '{0}'", soDaily.RecId);
            DataTable dtStore = STM.QuerySelect(sql);
            
            //List<SalesStore> store = new List<SalesStore>();
            //var encodeDataAsBytes = System.Convert.FromBase64String(user.Password);
            //var decode = Encoding.ASCII.GetString(encodeDataAsBytes);
            #region
            //DataTable tableUser = ExecuteStaticQuery.Get("STMSalesUser").Tables["StmSalesUser"];
            //var getUser = (from a in tableUser.AsEnumerable()
            //               where a.Field<string>("StmUserName") == soDaily.UserName
            //               //&&  a.Field<string>("StmPassword") == decode
            //               select a);
            //EnumerableRowCollection<DataRow> result = null;
            //int count = getUser.AsDataView().Count;
            //DataTable tableStore = ExecuteStaticQuery.Get("STMSalesStore").Tables["StmSalesStore"];
            //if (count > 0)
            //{
            //    foreach (DataRow row in getUser)
            //    {
            //        switch (Convert.ToInt64(row["StmSalesStoreType"]))
            //        {
            //            case 1: //Sales
            //                result = from a in tableStore.AsEnumerable()
            //                         where a.Field<string>("Sales") == row["StmName"].ToString()
            //                         select a;
            //                break;
            //            case 2: //Manager
            //                result = from a in tableStore.AsEnumerable()
            //                         where a.Field<string>("SalesManager") == row["StmName"].ToString()
            //                         select a;
            //                break;
            //            case 3://Area
            //                result = from a in tableStore.AsEnumerable()
            //                         where a.Field<string>("AreaManager") == row["StmName"].ToString()
            //                         select a;
            //                break;
            //            case 4://Admin
            //                result = from a in tableStore.AsEnumerable()
            //                         where a.Field<string>("KeyAcManager") == row["StmName"].ToString()
            //                         select a;
            //                break;
            //        }

            //        if (result != null)
            //        {
            //            foreach (DataRow row1 in result)
            //            {
            //                store.Add(
            //                    new SalesStore
            //                    {
            //                        StoreId = row1["StmStoreId"].ToString(),
            //                        StoreName = row1["StmStoreName"].ToString()
            //                    });
            //            }
            //        }
            //        //else if (soDaily.UserName == "admin")
            //        //{
            //        //    result = from a in tableStore.AsEnumerable() select a;
            //        //    foreach (DataRow row2 in result)
            //        //    {
            //        //        store.Add(
            //        //            new SalesStore
            //        //            {
            //        //                StoreId = row2["StmStoreId"].ToString(),
            //        //                StoreName = row2["StmStoreName"].ToString()
            //        //            });
            //        //    }
            //        //}
            //    }
            //}
            //else if (soDaily.UserName == "admin")
            //{
            //    result = from a in tableStore.AsEnumerable() select a;
            //    foreach (DataRow row2 in result)
            //    {
            //        store.Add(
            //            new SalesStore
            //            {
            //                StoreId = row2["StmStoreId"].ToString(),
            //                StoreName = row2["StmStoreName"].ToString()
            //            });
            //    }
            //}
            #endregion

            //string storeId = "";
            //foreach (var item in store)
            //{
            //    storeId += item.StoreId + ",";
            //}
            //if (storeId != "")
            //    storeId = storeId.Remove(storeId.Length - 1, 1);
            //int no = 1;
            //DataTable dtStore = QueryData.Find(
            //         "STMSalesSODailyLine",
            //         "StmSalesSoDailyLine",
            //         "SalesOrderDaily",
            //         soDaily.RecId
            //         ).Tables["StmSalesSoDailyLine"];

            foreach (DataRow row in dtStore.Rows)
            {
                //DataSet ds = QueryData.Find(
                //         "STMSalesImage",
                //         "StmSalesImage",
                //         "RefRecId",
                //         row["RecId"].ToString()
                //         );

                //var storeName = from a in tableStore.AsEnumerable() where a.Field<string>("StmStoreId") == row["StmStoreId"].ToString() select a.Field<string>("StmStoreName").ToString();
                //string name = "";
                //foreach (var item in storeName)
                //{
                //    name = item;
                //}
                sodailyList.Add(new SODaily
                {
                    No = Convert.ToInt32(row["Seq"]),
                    RecId = row["RecId"].ToString(),
                    StoreId = row["StmStoreId"].ToString(),
                    Pool = row["SalesPoolId"].ToString(),
                    Qty = Convert.ToDouble(row["SalesQty"]),
                    Amount = Convert.ToDouble(row["SalesAmount"]),
                    Date = dtStore.Locale.Name == "th-TH" ? Convert.ToDateTime(row["SalesDate"].ToString()).ToString("dd/MM/yyyy", new CultureInfo("en-US")) : row["SalesDate"].ToString(),
                    Series = row["Series"].ToString(),
                    Model = row["Model"].ToString(),
                    Sink = row["Sink"].ToString(),
                    Top = row["StmStoreId"].ToString(),
                    //ImageCount = ds.Tables["StmSalesImage"].Rows.Count,
                    //StoreName = name
                });
            }
            //if (dtStore.Rows.Count == 0)
            //{
            //    foreach (DataRow item in result)
            //    {
            //        sodailyList.Add(new SODaily
            //        {
            //            No = no++,
            //            StoreId = item["StmStoreId"].ToString(),
            //            StoreName = item["StmStoreName"].ToString()
            //        });
            //    }
            //}

            return Json(sodailyList);
        }

        [Route("UserAccount")]
        [HttpGet]
        public IHttpActionResult UserAccount(SODaily soDaily)
        {
            List<SODaily> sodailyList = new List<SODaily>();

            string sql = @"SELECT ROW_NUMBER() over(order by u.STMSALESSTORETYPE, STMSTOREID) as Seq
	                            ,store.STMSTOREID
	                            ,store.STMSTORENAME
                                ,[STMNAME]
	                            ,[STMUSERNAME]
                                ,[STMPASSWORD]
                                ,[STMSALESSTORETYPE]
                                ,(SELECT COUNT(*) FROM dbo.STMSALESSODAILY WHERE STMSTOREID = store.STMSTOREID) AS CountOrder
                        FROM [dbo].[STMSALESUSER] u
                        LEFT JOIN dbo.STMSALESSTORE store
	                        ON u.STMNAME = CASE
						                        WHEN u.STMSALESSTORETYPE = 4 THEN store.KEYACMANAGER
						                        WHEN u.STMSALESSTORETYPE = 3 THEN store.AREAMANAGER
						                        WHEN u.STMSALESSTORETYPE = 2 THEN store.SALESMANAGER
						                        WHEN u.STMSALESSTORETYPE = 1 THEN store.SALES
					                        END
                            ORDER BY STMSALESSTORETYPE, STMSTOREID";
            DataTable dtStore = STM.QuerySelect(sql);

            foreach (DataRow row in dtStore.Rows)
            {
                string storeType = "";
                switch (row["StmSalesStoreType"])
                {
                    case 1: storeType = "Sales";
                        break;
                    case 2:
                        storeType = "Manager";
                        break;
                    case 3:
                        storeType = "Area";
                        break;
                    case 4:
                        storeType = "Admin";
                        break;
                    default:
                        break;
                }

                sodailyList.Add(new SODaily
                {
                    No = Convert.ToInt32(row["Seq"]),
                    StoreId = row["StmStoreId"].ToString(),
                    StoreName = row["StmStoreName"].ToString(),
                    StoreType = storeType,
                    Name = row["StmName"].ToString(),
                    UserName = row["StmUserName"].ToString(),
                    Password = row["StmPassword"].ToString(),
                    LineCount = Convert.ToInt32(row["CountOrder"])
                });
            }
            
            return Json(sodailyList);
        }

        [Route("CalendarDetail")]
        [HttpPost]
        public IHttpActionResult FindCalendarDetail(SODaily soDaily)
        {
            List<SODaily> sodailyList = new List<SODaily>();
            AxdSalesDaily axdSoDaily = new AxdSalesDaily();

            DataSet ds = QueryData.Find(
                "STMSalesDaily",
                "StmSalesSoDaily",
                "ConfirmDate",
                soDaily.ConfirmDate,
                "SalesPoolId",
                soDaily.Pool
                );

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                sodailyList.Add(new SODaily
                {
                    Date = row["SalesDate"].ToString(),
                    StoreId = row["StmStoreId"].ToString(),
                    Pool = row["SalesPoolId"].ToString(),
                    Qty = Convert.ToDouble(row["SalesQty"]),
                    Amount = Convert.ToDouble(row["SalesAmount"]),
                    CustName = row["SalesName"].ToString(),
                    DueDate = Convert.ToDateTime(row["DueDate"]),
                    ConfirmDate = row["ConfirmDate"].ToString(),
                    PurchId = row["PurchId"].ToString(),
                    SalesId = row["SalesId"].ToString()
                });
            }

            return Json(sodailyList);
        }

        [Route("create")]
        [HttpPost]
        public string Post(SODaily soDaily)
        {
            /// Response JSON 

            //{
            //    "StoreId": "Test",
            //    "Pool": "DIY",
            //    "Qty": 4,
            //    "Amount": 2500
            //}
            DateTime dtTime = Convert.ToDateTime(soDaily.Date);

            string msg = "";
            #region Create sales daily 
            EntityKey[] entitykey = new EntityKey[1];
            try
            {
                var callContext = new CallContext()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                    Language = "en-us"
                };

                string duedate = soDaily.DueDate.ToShortDateString();
                //string confirmdate = soDaily.ConfirmDate.ToShortDateString();
                //string orderDate = soDaily.Date.ToShortDateString();

                AxdEntity_StmSalesSoDaily order = new AxdEntity_StmSalesSoDaily();
                if (soDaily.DueDate != DateTime.MinValue)
                {
                    order.DueDate = Convert.ToDateTime(Convert.ToDateTime(soDaily.DueDate).ToString("dd/MM/yyyy", new CultureInfo("en-US"))); //DateTime.ParseExact(duedate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    order.DueDateSpecified = true;
                }
                if(!string.IsNullOrEmpty(soDaily.ConfirmDate))//if (soDaily.ConfirmDate != DateTime.MinValue)
                {
                    order.ConfirmDate = DateTime.ParseExact(soDaily.ConfirmDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    order.ConfirmDateSpecified = true;
                }
                if(!string.IsNullOrEmpty(soDaily.Date))//if (soDaily.Date != DateTime.MinValue)
                {
                    order.SalesDate = DateTime.ParseExact(soDaily.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    order.SalesDateSpecified = true;
                }

                //order.SalesDate = DateTime.Now;
                //order.SalesDateSpecified = true;
                order.StmStoreId = soDaily.StoreId;
                order.SalesId = soDaily.SalesId;
                order.SalesName = soDaily.CustName;
                order.PurchId = soDaily.PurchId;

                order.SalesPoolId = soDaily.Pool;
                order.SalesQty = Convert.ToDecimal(soDaily.Qty);
                order.SalesQtySpecified = true;
                order.SalesAmount = Convert.ToDecimal(soDaily.Amount);
                order.SalesAmountSpecified = true;

                var orderList = new AxdSalesDaily()
                {
                    StmSalesSoDaily = new AxdEntity_StmSalesSoDaily[1] { order }
                };

                using (var client = new SalesDailyServiceClient())
                {
                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                    entitykey = client.create(callContext, orderList);
                }

                long refRecId = Convert.ToInt64(entitykey[0].KeyData[0].Value);

                #region Create Confirm
                if (soDaily.DueDate != DateTime.MinValue)
                {
                    var callContextConfirm = new AxSalesOrderDailyConfirm.CallContext()
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                        Language = "en-us"
                    };

                    var confirmEntity = new AxSalesOrderDailyConfirm.AxdEntity_StmSalesSoDailyConfirm();
                    confirmEntity.Remark = soDaily.Remark;
                    confirmEntity.SalesOrderDaily = refRecId;
                    confirmEntity.SalesOrderDailySpecified = true;


                    var confirm = new AxSalesOrderDailyConfirm.AxdSTMSalesConfirm()
                    {
                        StmSalesSoDailyConfirm = new AxSalesOrderDailyConfirm.AxdEntity_StmSalesSoDailyConfirm[1] { confirmEntity }
                    };

                    using (var client = new AxSalesOrderDailyConfirm.STMSalesConfirmServiceClient())
                    {
                        client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                        client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                        client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                        client.create(callContextConfirm, confirm);
                    }
                }

                #endregion

                if (!string.IsNullOrEmpty(soDaily.Series))
                {
                    var callContextLine = new AxSalesOrderDailyLine.CallContext()
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                        Language = "en-us"
                    };

                    var lineEntity = new AxSalesOrderDailyLine.AxdEntity_StmSalesSoDailyLine();

                    lineEntity.Series = soDaily.Series;
                    lineEntity.Model = soDaily.Model;
                    lineEntity.Sink = soDaily.Sink;

                    if (!string.IsNullOrEmpty(soDaily.Date))
                    {
                        lineEntity.SalesDate = DateTime.ParseExact(soDaily.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        lineEntity.SalesDateSpecified = true;
                    }

                    //lineEntity.SalesDate = orderDate;
                    //lineEntity.SalesDateSpecified = true;
                    lineEntity.StmStoreId = soDaily.StoreId;
                    lineEntity.SalesPoolId = soDaily.Pool;
                    lineEntity.SalesQty = Convert.ToDecimal(soDaily.Qty);
                    lineEntity.SalesQtySpecified = true;
                    lineEntity.SalesAmount = Convert.ToDecimal(soDaily.Amount);
                    lineEntity.SalesAmountSpecified = true;
                    lineEntity.SalesOrderDaily = Convert.ToInt64(refRecId);
                    lineEntity.SalesOrderDailySpecified = true;

                    var soLine = new AxSalesOrderDailyLine.AxdSTMSODailyLine()
                    {
                        StmSalesSoDailyLine = new AxSalesOrderDailyLine.AxdEntity_StmSalesSoDailyLine[1] { lineEntity }
                    };

                    using (var client = new AxSalesOrderDailyLine.STMSODailyLineServiceClient())
                    {
                        client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                        client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                        client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                        client.create(callContextLine, soLine);
                    }


                }
                #region Create Log
                SalesDailyLog.Create(refRecId, soDaily.UserName);
                #endregion
                msg = "สร้างรายการสำเร็จ!";
            }
            catch (Exception ex)
            {
                msg = ex.Message;

            }


            #endregion
            return msg;
        }

        [Route("CreateLine")]
        [HttpPost]
        public void CreateLine(SODaily soDaily)
        {
            AxSalesOrderDailyLine.EntityKey[] entitykey = new AxSalesOrderDailyLine.EntityKey[1];
            if (!string.IsNullOrEmpty(soDaily.Series))
            {
                //string orderDate = soDaily.Date.ToShortDateString();

                var callContextLine = new AxSalesOrderDailyLine.CallContext()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                    Language = "en-us"
                };

                var lineEntity = new AxSalesOrderDailyLine.AxdEntity_StmSalesSoDailyLine();

                lineEntity.Series = soDaily.Series;
                lineEntity.Model = soDaily.Model;
                lineEntity.Sink = soDaily.Sink;

                if(!string.IsNullOrEmpty(soDaily.Date))//if (soDaily.Date != DateTime.MinValue)
                {
                    lineEntity.SalesDate = DateTime.ParseExact(soDaily.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    lineEntity.SalesDateSpecified = true;
                }

                //lineEntity.SalesDate = orderDate;
                //lineEntity.SalesDateSpecified = true;
                lineEntity.StmStoreId = soDaily.StoreId;//ใช้เก็บข้อมูล Top แทน
                lineEntity.SalesPoolId = soDaily.Pool;
                lineEntity.SalesQty = Convert.ToDecimal(soDaily.Qty);
                lineEntity.SalesQtySpecified = true;
                lineEntity.SalesAmount = Convert.ToDecimal(soDaily.Amount);
                lineEntity.SalesAmountSpecified = true;
                lineEntity.SalesOrderDaily = Convert.ToInt64(soDaily.RecIdHeader);
                lineEntity.SalesOrderDailySpecified = true;

                var soLine = new AxSalesOrderDailyLine.AxdSTMSODailyLine()
                {
                    StmSalesSoDailyLine = new AxSalesOrderDailyLine.AxdEntity_StmSalesSoDailyLine[1] { lineEntity }
                };

                using (var client = new AxSalesOrderDailyLine.STMSODailyLineServiceClient())
                {
                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                    entitykey = client.create(callContextLine, soLine);
                }
                long refRecId = Convert.ToInt64(entitykey[0].KeyData[0].Value);
                SalesDailyLog.Create(refRecId, soDaily.UserName);
            }
        }

        [Route("GetImage")]
        [HttpPost]
        public IHttpActionResult GetImage(SODaily soDaily)
        {
            string sql = string.Format(@"SELECT [IMAGE]
                                              ,[NAME]
                                              ,[REFRECID]
                                              ,[RECID]
                                          FROM [dbo].[STMSALESIMAGE]
                                          WHERE REFRECID = '{0}'", soDaily.RecId);
            DataTable dtImage = STM.QuerySelect(sql);
            List<SODaily> soList = new List<SODaily>();
            //try
            //{
            //DataTable dtImage = QueryData.Find(
            //        "STMSalesImage",
            //        "StmSalesImage",
            //        "RefRecId",
            //        soDaily.RecId
            //        ).Tables[0];

            #region Get image
            foreach (DataRow row in dtImage.Rows)
            {
                //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "5637153588_LOGOSTM.png");
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", row["Name"].ToString());
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                var bytes = default(byte[]);
                string FileExtension = Path.GetExtension(row["Name"].ToString());
                //if (FileExtension == ".pdf")
                //{
                //    bytes = File.ReadAllBytes(path);
                //    File.WriteAllBytes(path, bytes);
                //}
                //else
                //{
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    Image image = Image.FromStream(stream);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        image.Save(memoryStream, ImageFormat.Jpeg);
                        result.Content = new ByteArrayContent(memoryStream.ToArray());
                        result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                        bytes = memoryStream.ToArray();
                    }
                }
                //}

                soList.Add(new SODaily
                {
                    RecId = row["RecId"].ToString(),
                    ImageName = row["Name"].ToString().Substring(0, row["Name"].ToString().Length - 4),
                    Base64 = "data:image/png;base64," + Convert.ToBase64String(bytes, 0, bytes.Length)
                });
            }
            //}
            //catch (Exception e)
            //{

            //    throw;
            //}
            #endregion

            return Json(soList);
        }

        [Route("UploadFile")]
        [HttpPost]
        public async Task<string> UploadFile()
        {
            /// Path เก็บรูป 10.11.0.12

            var ctx = HttpContext.Current;
            var user = HttpContext.Current.Request.Params["User"];
            var recid = HttpContext.Current.Request.Params["RecId"];
            AxSalesOrderDailyImage.EntityKey[] entitykey = new AxSalesOrderDailyImage.EntityKey[1];
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/App_Data")))
            {
                // Try to create the directory.
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/App_Data"));
            }

            var root = ctx.Server.MapPath("~/App_Data");
            var provider = new MultipartFileStreamProvider(root);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
                foreach (var file in provider.FileData)
                {
                    var name = file.Headers.ContentDisposition.FileName;
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = recid.ToString() + "_" + name.Trim('"');

                        var localFileName = file.LocalFileName;
                        string filePath = Path.Combine(root, name);
                        File.Move(localFileName, filePath);

                        string filep = root + "\'" + name;

                        #region
                        AxSalesOrderDailyImage.CallContext callContext = new AxSalesOrderDailyImage.CallContext()
                        {
                            MessageId = Guid.NewGuid().ToString(),
                            LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                            Language = "en-us"
                        };

                        AxSalesOrderDailyImage.AxdEntity_StmSalesImage imageEntity = new AxSalesOrderDailyImage.AxdEntity_StmSalesImage();
                        imageEntity.Image = @"D:\inetpub\wwwroot\AxAIFService\App_Data\";//"" + filePath.Replace(@"\","PP").Replace(":","OO").Replace(".","KK") + ""; //<==== ERROR ความยาวตัวอักษร
                        imageEntity.Name = name;
                        imageEntity.RefRecId = Convert.ToInt64(recid);
                        imageEntity.RefRecIdSpecified = true;

                        AxSalesOrderDailyImage.AxdSTM_SalesImage image = new AxSalesOrderDailyImage.AxdSTM_SalesImage()
                        {
                            StmSalesImage = new AxSalesOrderDailyImage.AxdEntity_StmSalesImage[1] { imageEntity }
                        };

                        AxSalesOrderDailyImage.STM_SalesImageServiceClient client = new AxSalesOrderDailyImage.STM_SalesImageServiceClient();
                        client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                        client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                        client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                        client.create(callContext, image);

                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                return $"Error: {e.Message}";
            }

            return "File uploaded!";
        }

        [Route("Edit")]
        [HttpPost]
        public void Put(SODaily soDaily)
        {
            /// response JSON
            /// 
            /// {
            ///    "RecId": "5637147579",
            ///    "Pool": "WARDROBE"
            /// }
            try
            {
                EntityKey[] entitykey = new EntityKey[1];
                AxdSalesDaily axdSoDaily = new AxdSalesDaily();
                var callContext = new CallContext()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                    Language = "en-us"
                };

                using (var client = new SalesDailyServiceClient())
                {
                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                    axdSoDaily = client.read(callContext, ReadCritera.read(soDaily.RecId));
                }

                string duedate = soDaily.DueDate.ToShortDateString();
                //string confirmdate = soDaily.ConfirmDate.ToShortDateString();
                //string orderDate = soDaily.Date.ToShortDateString();

                using (var client = new SalesDailyServiceClient())
                {
                    AxdEntity_StmSalesSoDaily soDailyEntity = axdSoDaily.StmSalesSoDaily[0];
                    var axdSODaily2 = new AxdSalesDaily()
                    {
                        ClearNilFieldsOnUpdate = axdSoDaily.ClearNilFieldsOnUpdate,
                        ClearNilFieldsOnUpdateSpecified = true,
                        DocPurpose = axdSoDaily.DocPurpose,
                        DocPurposeSpecified = true,
                        SenderId = axdSoDaily.SenderId
                    };

                    var soDailyEntityNew = new AxdEntity_StmSalesSoDaily();
                    soDailyEntityNew._DocumentHash = soDailyEntity._DocumentHash; ///for update method
                    soDailyEntityNew.RecId = Convert.ToInt64(soDaily.RecId);
                    soDailyEntityNew.RecIdSpecified = true;
                    if (soDaily.DueDate != DateTime.MinValue)
                    {
                        soDailyEntityNew.DueDate = soDaily.DueDate;//DateTime.ParseExact(duedate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        soDailyEntityNew.DueDateSpecified = true;
                    }

                    if(!string.IsNullOrEmpty(soDaily.ConfirmDate))//if (soDaily.ConfirmDate != DateTime.MinValue)
                    {
                        soDailyEntityNew.ConfirmDate = DateTime.ParseExact(soDaily.ConfirmDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        soDailyEntityNew.ConfirmDateSpecified = true;
                    }

                    if(!string.IsNullOrEmpty(soDaily.Date))//if (soDaily.Date != DateTime.MinValue)
                    {
                        soDailyEntityNew.SalesDate = DateTime.ParseExact(soDaily.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        soDailyEntityNew.SalesDate = soDailyEntity.SalesDate;
                    }

                    soDailyEntityNew.SalesDateSpecified = true;
                    soDailyEntityNew.SalesPoolId = soDaily.Pool;// string.IsNullOrEmpty(soDaily.Pool) ? soDailyEntity.SalesPoolId : soDaily.Pool;
                    soDailyEntityNew.PurchId = soDaily.PurchId;
                    soDailyEntityNew.SalesId = soDaily.SalesId;
                    soDailyEntityNew.SalesName = soDaily.CustName;
                    soDailyEntityNew.SalesQty = Convert.ToDecimal(soDaily.Qty);// string.IsNullOrEmpty(soDaily.Qty.ToString()) ? Convert.ToDecimal(soDailyEntity.SalesQty) : Convert.ToDecimal(soDaily.Qty);
                    soDailyEntityNew.SalesQtySpecified = true;
                    soDailyEntityNew.StmStoreId = soDaily.StoreId;//string.IsNullOrEmpty(soDaily.StoreId) ? soDailyEntity.StmStoreId : soDaily.StoreId;
                    soDailyEntityNew.SalesAmount = Convert.ToDecimal(soDaily.Amount);// Convert.ToDecimal(soDaily.Amount) == soDailyEntity.SalesAmount ? Convert.ToDecimal(soDailyEntity.SalesAmount) : Convert.ToDecimal(soDaily.Amount);
                    soDailyEntityNew.SalesAmountSpecified = true;

                    axdSODaily2.StmSalesSoDaily = new AxdEntity_StmSalesSoDaily[1] { soDailyEntityNew };

                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                    client.update(callContext, ReadCritera.read(soDaily.RecId), axdSODaily2);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        [Route("EditLine")]
        [HttpPost]
        public void EditLine(SODaily soDaily)
        {
            /// response JSON
            /// 
            /// {
            ///    "RecId": "5637147579",
            ///    "Pool": "WARDROBE"
            /// }

           
            AxSalesOrderDailyLine.EntityKey[] entitykey = new AxSalesOrderDailyLine.EntityKey[1];
            AxSalesOrderDailyLine.AxdSTMSODailyLine axdSoLine = new AxSalesOrderDailyLine.AxdSTMSODailyLine();
            var callContext = new AxSalesOrderDailyLine.CallContext()
            {
                MessageId = Guid.NewGuid().ToString(),
                LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                Language = "en-us"
            };

            AxSalesOrderDailyLine.KeyField keyField = new AxSalesOrderDailyLine.KeyField() { Field = "RecId", Value = soDaily.RecId };
            AxSalesOrderDailyLine.EntityKey entityKey = new AxSalesOrderDailyLine.EntityKey();

            entityKey.KeyData = new AxSalesOrderDailyLine.KeyField[1] { keyField };

            AxSalesOrderDailyLine.EntityKey[] entityKeys = new AxSalesOrderDailyLine.EntityKey[1] { entityKey };

            using (var client = new AxSalesOrderDailyLine.STMSODailyLineServiceClient())
            {
                client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                axdSoLine = client.read(callContext, entityKeys);
            }

            string duedate = soDaily.DueDate.ToShortDateString();
            //string confirmdate = soDaily.ConfirmDate.ToShortDateString();
            //string orderDate = soDaily.Date.ToShortDateString();
           //DateTime dttt = Convert.ToDateTime( Convert.ToDateTime(soDaily.Date).ToString("dd/MM/yyyy",ci));

            using (var client = new AxSalesOrderDailyLine.STMSODailyLineServiceClient())
            {
                AxSalesOrderDailyLine.AxdEntity_StmSalesSoDailyLine soLineEntity = axdSoLine.StmSalesSoDailyLine[0];
                var axdsoline2 = new AxSalesOrderDailyLine.AxdSTMSODailyLine()
                {
                    ClearNilFieldsOnUpdate = axdSoLine.ClearNilFieldsOnUpdate,
                    ClearNilFieldsOnUpdateSpecified = true,
                    DocPurpose = axdSoLine.DocPurpose,
                    DocPurposeSpecified = true,
                    SenderId = axdSoLine.SenderId

                };

                var solineEntityNew = new AxSalesOrderDailyLine.AxdEntity_StmSalesSoDailyLine();
                solineEntityNew._DocumentHash = soLineEntity._DocumentHash; ///for update method
                solineEntityNew.RecId = Convert.ToInt64(soDaily.RecId);
                solineEntityNew.RecIdSpecified = true;

                if(!string.IsNullOrEmpty(soDaily.Date))//if (soDaily.Date != DateTime.MinValue)
                {
                    solineEntityNew.SalesDate = DateTime.ParseExact(soDaily.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    solineEntityNew.SalesDate = soLineEntity.SalesDate;
                }

                solineEntityNew.SalesDateSpecified = true;
                //solineEntityNew.SalesPoolId = soDaily.Pool;
                solineEntityNew.Series = soDaily.Series;
                solineEntityNew.Model = soDaily.Model;
                solineEntityNew.Sink = soDaily.Sink;
                solineEntityNew.SalesQty = Convert.ToDecimal(soDaily.Qty);
                solineEntityNew.SalesQtySpecified = true;
                solineEntityNew.StmStoreId = soDaily.StoreId;
                solineEntityNew.SalesAmount = Convert.ToDecimal(soDaily.Amount);
                solineEntityNew.SalesAmountSpecified = true;
                solineEntityNew.SalesOrderDaily = Convert.ToInt64(soDaily.RecIdHeader);
                solineEntityNew.SalesOrderDailySpecified = true;
                axdsoline2.StmSalesSoDailyLine = new AxSalesOrderDailyLine.AxdEntity_StmSalesSoDailyLine[1] { solineEntityNew };

                client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                client.update(callContext, entityKeys, axdsoline2);
            }

        }

        [Route("Delete")]
        [HttpPost]
        public void Delete(SODaily soDaily)
        {
            //{
            //    "RecId": "5637147579"
            //}
            try
            {
                var callContext = new CallContext()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                    Language = "en-us"
                };

                using (SalesDailyServiceClient client = new SalesDailyServiceClient())
                {
                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                    client.delete(callContext, ReadCritera.read(soDaily.RecId));
                }

                #region Delete lines

                DataTable dt = QueryData.Find(
                       "STMSalesSODailyLine",
                       "StmSalesSoDailyLine",
                       "SalesOrderDaily",
                       soDaily.RecId
                       ).Tables["StmSalesSoDailyLine"];

                var callContextline = new AxSalesOrderDailyLine.CallContext()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                    Language = "en-us"
                };

                foreach (DataRow row in dt.Rows)
                {
                    AxSalesOrderDailyLine.KeyField keyField = new AxSalesOrderDailyLine.KeyField() { Field = "RecId", Value = row["RecId"].ToString() };
                    AxSalesOrderDailyLine.EntityKey entityKey = new AxSalesOrderDailyLine.EntityKey();

                    entityKey.KeyData = new AxSalesOrderDailyLine.KeyField[1] { keyField };

                    AxSalesOrderDailyLine.EntityKey[] entityKeys = new AxSalesOrderDailyLine.EntityKey[1] { entityKey };

                    using (AxSalesOrderDailyLine.STMSODailyLineServiceClient client = new AxSalesOrderDailyLine.STMSODailyLineServiceClient())
                    {
                        client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                        client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                        client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                        client.delete(callContextline, entityKeys);
                    }
                }

                #endregion

                #region Delete Image

                DataTable dtImage = QueryData.Find(
                            "STMSalesImage",
                            "StmSalesImage",
                            "RefRecId",
                            soDaily.RecId
                            ).Tables[0];

                var callContextimage = new AxSalesOrderDailyImage.CallContext()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                    Language = "en-us"
                };

                foreach (DataRow row in dtImage.Rows)
                {
                    AxSalesOrderDailyImage.KeyField keyFieldimage = new AxSalesOrderDailyImage.KeyField() { Field = "RecId", Value = row["RecId"].ToString() };
                    AxSalesOrderDailyImage.EntityKey entityKeyimage = new AxSalesOrderDailyImage.EntityKey();

                    entityKeyimage.KeyData = new AxSalesOrderDailyImage.KeyField[1] { keyFieldimage };

                    AxSalesOrderDailyImage.EntityKey[] entityKeysimage = new AxSalesOrderDailyImage.EntityKey[1] { entityKeyimage };

                    using (AxSalesOrderDailyImage.STM_SalesImageServiceClient client = new AxSalesOrderDailyImage.STM_SalesImageServiceClient())
                    {
                        client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                        client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                        client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                        client.delete(callContextimage, entityKeysimage);
                    }

                    #region Delete file image in server
                    string path = string.Format(@"{0}{1}", row["Image"].ToString(), row["Name"].ToString());
                    File.Delete(path);
                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {

                throw;
            }
          
        }

        [Route("DeleteLine")]
        [HttpPost]
        public void DeleteLine(SODaily soDaily)
        {
            //{
            //    "RecId": "5637147579"
            //}

            var callContext = new AxSalesOrderDailyLine.CallContext()
            {
                MessageId = Guid.NewGuid().ToString(),
                LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                Language = "en-us"
            };

            AxSalesOrderDailyLine.KeyField keyField = new AxSalesOrderDailyLine.KeyField() { Field = "RecId", Value = soDaily.RecId };
            AxSalesOrderDailyLine.EntityKey entityKey = new AxSalesOrderDailyLine.EntityKey();

            entityKey.KeyData = new AxSalesOrderDailyLine.KeyField[1] { keyField };

            AxSalesOrderDailyLine.EntityKey[] entityKeys = new AxSalesOrderDailyLine.EntityKey[1] { entityKey };

            using (AxSalesOrderDailyLine.STMSODailyLineServiceClient client = new AxSalesOrderDailyLine.STMSODailyLineServiceClient())
            {
                client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                client.delete(callContext, entityKeys);
            }
        }

        [Route("DeleteImage/{id}")]
        [HttpGet]
        public string DeleteImage(string id)
        {
            try
            {
                DataTable dtImage = QueryData.Find(
                        "STMSalesImage",
                        "StmSalesImage",
                        "Name",
                        id + "*"
                        ).Tables[0];

                var callContextimage = new AxSalesOrderDailyImage.CallContext()
                {
                    MessageId = Guid.NewGuid().ToString(),
                    LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                    Language = "en-us"
                };

                foreach (DataRow row in dtImage.Rows)
                {
                    AxSalesOrderDailyImage.KeyField keyFieldimage = new AxSalesOrderDailyImage.KeyField() { Field = "RecId", Value = row["RecId"].ToString() };
                    AxSalesOrderDailyImage.EntityKey entityKeyimage = new AxSalesOrderDailyImage.EntityKey();

                    entityKeyimage.KeyData = new AxSalesOrderDailyImage.KeyField[1] { keyFieldimage };

                    AxSalesOrderDailyImage.EntityKey[] entityKeysimage = new AxSalesOrderDailyImage.EntityKey[1] { entityKeyimage };

                    using (AxSalesOrderDailyImage.STM_SalesImageServiceClient client = new AxSalesOrderDailyImage.STM_SalesImageServiceClient())
                    {
                        client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                        client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                        client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                        client.delete(callContextimage, entityKeysimage);
                    }

                    #region Delete file image in server
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", row["Name"].ToString());
                    //string path = string.Format(@"{0}{1}", row["Image"].ToString(), row["Name"].ToString());
                    File.Delete(path);
                    #endregion
                }
            }
            catch (Exception e)
            {
                return $"Error: {e.Message}";
            }

            return "Delete completed!";
        }

    }
}
