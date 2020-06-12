using AxRetailSO.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AxRetailSO.Classes
{
    public class Stored
    {
        public static List<SalesStore> Get(string user)
        {
            List<SODaily> sodailyList = new List<SODaily>();
            List<SalesStore> store = new List<SalesStore>();
            DataTable tableUser = ExecuteStaticQuery.Get("STMSalesUser").Tables["StmSalesUser"];
            var getUser = (from a in tableUser.AsEnumerable()
                           where a.Field<string>("StmUserName") == user
                           //&&  a.Field<string>("StmPassword") == decode
                           select a);
            int count = getUser.AsDataView().Count;
            DataTable tableStore = ExecuteStaticQuery.Get("STMSalesStore").Tables["StmSalesStore"];
            if (count > 0)
            {
                foreach (DataRow row in getUser)
                {
                    switch (Convert.ToInt64(row["StmSalesStoreType"]))
                    {
                        case 1: //Sales
                            store = (from a in tableStore.AsEnumerable()
                                     orderby a.Field<string>("StmStoreId")
                                     where a.Field<string>("Sales") == row["StmName"].ToString()
                                     select new SalesStore
                                     {
                                         StoreId = a.Field<string>("StmStoreId"),
                                         StoreName = a.Field<string>("StmStoreName")
                                     }).ToList();
                            break;
                        case 2: //Manager
                            store = (from a in tableStore.AsEnumerable()
                                     orderby a.Field<string>("StmStoreId")
                                     where a.Field<string>("SalesManager") == row["StmName"].ToString()
                                     select new SalesStore
                                     {
                                         StoreId = a.Field<string>("StmStoreId"),
                                         StoreName = a.Field<string>("StmStoreName")
                                     }).ToList();
                            break;
                        case 3://Area
                            store = (from a in tableStore.AsEnumerable()
                                     orderby a.Field<string>("StmStoreId")
                                     where a.Field<string>("AreaManager") == row["StmName"].ToString()
                                     select new SalesStore
                                     {
                                         StoreId = a.Field<string>("StmStoreId"),
                                         StoreName = a.Field<string>("StmStoreName")
                                     }).ToList();
                            break;
                        case 4://Admin
                            store = (from a in tableStore.AsEnumerable()
                                     orderby a.Field<string>("StmStoreId")
                                     where a.Field<string>("KeyAcManager") == row["StmName"].ToString()
                                     select new SalesStore
                                     {
                                         StoreId = a.Field<string>("StmStoreId"),
                                         StoreName = a.Field<string>("StmStoreName")
                                     }).ToList();
                            break;

                    }
                }
            }
            else if (count == 0 || user == "admin")
            {
                store = (from a in tableStore.AsEnumerable()
                         orderby a.Field<string>("StmStoreId")
                         select new SalesStore
                         {
                             StoreId = a.Field<string>("StmStoreId"),
                             StoreName = a.Field<string>("StmStoreName")
                         }).ToList();
            }

            return store;
        }
    }
}