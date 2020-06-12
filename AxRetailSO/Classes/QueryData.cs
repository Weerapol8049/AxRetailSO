using AxRetailSO.AxQueryService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AxRetailSO.Classes
{
    public class QueryData
    {
        public static DataSet Find(string queryName, string tableName, string fieldName, string value, string fieldName2 = "", string value2 = "")
        {
            DataSet ds = new DataSet();
            QueryServiceClient client = new QueryServiceClient();

            //Set up paging so that 1000 records are retrieved at a time

            Paging paging = new ValueBasedPaging() { RecordLimit = 1000 };
            QueryMetadata query;
            QueryDataSourceMetadata customerDataSource;
            QueryDataRangeMetadata range, range2;
            QueryDataOrderByMetadata sort;
            try
            {
                query = new QueryMetadata();
                // Set the properties of the query.
                //query.QueryType = QueryType.Join;
                query.DataSources = new QueryDataSourceMetadata[1];
                // Set the properties of the Customers data source.
                customerDataSource = new QueryDataSourceMetadata();
                customerDataSource.Name = queryName;
                customerDataSource.Enabled = true;
                customerDataSource.Table = tableName;
                //Add the data source to the query.
                query.DataSources[0] = customerDataSource;
                // Setting DynamicFieldList property to false so I can specify only a few fields
                customerDataSource.DynamicFieldList = true;

                range = new QueryDataRangeMetadata();
                range.TableName = tableName;
                range.FieldName = fieldName;
                range.Value = value;
                range.Enabled = true;

                range2 = new QueryDataRangeMetadata();
                range2.TableName = tableName;
                range2.FieldName = fieldName2;
                range2.Value = value2;
                range2.Enabled = true;

                customerDataSource.Ranges = new QueryDataRangeMetadata[fieldName2 == "" ? 1 : 2];
                customerDataSource.Ranges[0] = range;
                if (fieldName2 != "")
                    customerDataSource.Ranges[1] = range2;

                //sort = new QueryDataOrderByMetadata();
                //sort.DataSource = queryName;
                //sort.FieldName = "RecId";
                //sort.SortOrder = SortOrder.Descending;
                //query.OrderByFields = new QueryOrderByMetadata[1];
                //query.OrderByFields[0] = sort;

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
                ds = client.ExecuteQuery(query, ref paging);
            }
            catch (Exception ex)
            {

                throw;
            }
           
            return ds;
        }

        public static DataSet Search(string queryName, string tableName, string fieldName, string value, int page, int pageSize)
        {


            int i = 0;
            DataSet ds = new DataSet();
            QueryServiceClient client = new QueryServiceClient();

            //Set up paging so that 1000 records are retrieved at a time

            Paging paging = new ValueBasedPaging() { RecordLimit = page == 0 ? 1000 : pageSize };
            QueryMetadata query;
            QueryDataSourceMetadata customerDataSource;
            QueryDataRangeMetadata range;
            QueryDataOrderByMetadata sort;
            QueryDataFilterMetadata filter, salesId;

            try
            {
                do
                {
                    query = new QueryMetadata();
                    // Set the properties of the query.
                    //query.QueryType = QueryType.Join;
                    query.DataSources = new QueryDataSourceMetadata[1];
                    // Set the properties of the Customers data source.
                    customerDataSource = new QueryDataSourceMetadata();
                    customerDataSource.Name = queryName;
                    customerDataSource.Enabled = true;
                    customerDataSource.Table = tableName;
                    //Add the data source to the query.
                    query.DataSources[0] = customerDataSource;
                    // Setting DynamicFieldList property to false so I can specify only a few fields
                    customerDataSource.DynamicFieldList = true;

                    range = new QueryDataRangeMetadata();
                    range.TableName = tableName;
                    range.FieldName = fieldName;
                    range.Value = value;
                    range.Enabled = true;

                    customerDataSource.Ranges = new QueryDataRangeMetadata[1];
                    customerDataSource.Ranges[0] = range;

                    sort = new QueryDataOrderByMetadata();
                    sort.DataSource = queryName;
                    sort.FieldName = "RecId";
                    sort.SortOrder = SortOrder.Descending;

                    query.OrderByFields = new QueryOrderByMetadata[1];
                    query.OrderByFields[0] = sort;

                    filter = new QueryDataFilterMetadata();
                    filter.DataSourceName = queryName;
                    filter.FieldName = "StmStoreId";
                    //filter.Type = QueryRangeType.FullText;
                    filter.Value = "RST";

                    //filter = new QueryDataFilterMetadata();
                    //filter.DataSourceName = queryName;
                    //filter.FieldName = "SalesId";
                    //filter.Value = "RST";


                    query.Filters = new QueryFilterMetadata[1];
                    query.Filters[0] = filter;
                    //query.Filters[1] = salesId;

                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;

                    ds = client.ExecuteQuery(query, ref paging);

                    i++;
                    if (i == page && page > 0)
                    {
                        return ds;
                    }

                }

                while (((ValueBasedPaging)paging).Bookmark != null);


            }
            catch (Exception ex)
            {

                throw;
            }

            return ds;
        }

        public static DataSet FindV2(string queryName, string tableName, string fieldName, string value, int page, int pageSize )
        {
            
            int i = 0;
            DataSet ds = new DataSet();
            QueryServiceClient client = new QueryServiceClient();

            //Set up paging so that 1000 records are retrieved at a time

            Paging paging = new ValueBasedPaging() { RecordLimit = page == 0 ? 1000:pageSize };
            QueryMetadata query;
            QueryDataSourceMetadata customerDataSource;
            QueryDataRangeMetadata range;
            QueryDataOrderByMetadata sort;
            try
            {
                do
                {
                    query = new QueryMetadata();
                    // Set the properties of the query.
                    //query.QueryType = QueryType.Join;
                    query.DataSources = new QueryDataSourceMetadata[1];
                    // Set the properties of the Customers data source.
                    customerDataSource = new QueryDataSourceMetadata();
                    customerDataSource.Name = queryName;
                    customerDataSource.Enabled = true;
                    customerDataSource.Table = tableName;
                    //Add the data source to the query.
                    query.DataSources[0] = customerDataSource;
                    // Setting DynamicFieldList property to false so I can specify only a few fields
                    customerDataSource.DynamicFieldList = true;

                    range = new QueryDataRangeMetadata();
                    range.TableName = tableName;
                    range.FieldName = fieldName;
                    range.Value = value;
                    range.Enabled = true;

                    customerDataSource.Ranges = new QueryDataRangeMetadata[1];
                    customerDataSource.Ranges[0] = range;

                    sort = new QueryDataOrderByMetadata();
                    sort.DataSource = queryName;
                    sort.FieldName = "RecId";
                    sort.SortOrder = SortOrder.Descending;
                    query.OrderByFields = new QueryOrderByMetadata[1];
                    query.OrderByFields[0] = sort;

                    client.ClientCredentials.Windows.ClientCredential.Domain = Logon.Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = Logon.DefaultUser;
                    client.ClientCredentials.Windows.ClientCredential.Password = Logon.DefaultPassword;

                    ds = client.ExecuteQuery(query, ref paging);

                    i++;
                    if (i == page && page > 0)
                    {
                        return ds;
                    }
                 
                }

                while (((ValueBasedPaging)paging).Bookmark != null);

               
            }
            catch (Exception ex)
            {

                throw;
            }

            return ds;
        }

        public static DataSet Paging()
        {
            QueryServiceClient client = new QueryServiceClient();

            Paging paging = new ValueBasedPaging() { RecordLimit = 20};

            DataSet dataset;
            int page = 10;
            int i = 0;

            do
            {
               
                dataset = client.ExecuteStaticQuery("STMSalesDaily", ref paging);
                DataRow row = dataset.Tables[0].Rows[0];

                i++;
                if (i == page)
                {
                    return dataset ;
                }
            }

            while (((ValueBasedPaging)paging).Bookmark != null);

            return dataset;
        }
    }
}