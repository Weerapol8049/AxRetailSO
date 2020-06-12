using AxRetailSO.AxQueryService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AxRetailSO.Classes
{
    public class ExecuteStaticQuery
    {
        public static DataSet Get(string tableQuery)
        {
            QueryServiceClient client = new QueryServiceClient();
            DataSet dataSet = new DataSet();
            Paging paging = null;
            try
            {
                if (string.IsNullOrEmpty(Logon.User) && string.IsNullOrEmpty(Logon.Password))
                {
                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                }
                else
                {
                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.User;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.Password;
                }

                dataSet = client.ExecuteStaticQuery(tableQuery, ref paging);

            }
            catch (Exception ex)
            {

                string msg = ex.Message;
            }

            return dataSet;


        }
    }
}