using AxRetailSO.AxSalesOrderDailyLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AxRetailSO.Classes
{
    public class SalesDailyLog
    {
        public static void Create(long refRecId, string createBy)
        {
            var callContextLog = new CallContext()
            {
                MessageId = Guid.NewGuid().ToString(),
                LogonAsUser = string.Format(@"{0}\{1}", Logon.Domain, Logon.DefaultUser),
                Language = "en-us"
            };

            var log = new AxdEntity_StmSalesSoLog()
            {
                RefRecId = refRecId,
                RefRecIdSpecified = true,
                CreateBy = createBy
            };

            //var logList = new AxdAxdSTMSalesSolog() //UAT
            var logList = new AxdAxdSTMSalesSoLog()   //LIVE
            {
                StmSalesSoLog = new AxdEntity_StmSalesSoLog[1] { log }
            };

            //using (AxdSTMSalesSologServiceClient client = new AxdSTMSalesSologServiceClient()) //UAT
            using (AxdSTMSalesSoLogServiceClient client = new AxdSTMSalesSoLogServiceClient()) //LIVE
            {
                client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;
                client.create(callContextLog, logList);
            }
        }
    }
}