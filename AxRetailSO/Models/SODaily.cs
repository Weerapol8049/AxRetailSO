using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AxRetailSO.Models
{
    public class SODaily
    {
        [JsonProperty(PropertyName = "No")]
        public int No { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "UserName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "Password")]
        public string Password { get; set; }


        [JsonProperty(PropertyName = "RecId")]
        public string RecId { get; set; }

        [JsonProperty(PropertyName = "RecIdHeader")]
        public string RecIdHeader { get; set; }

        [JsonProperty(PropertyName = "SalesId")]
        public string SalesId { get; set; }

        [JsonProperty(PropertyName = "PurchId")]
        public string PurchId { get; set; }

        [JsonProperty(PropertyName = "StoreId")]
        public string StoreId { get; set; }

        [JsonProperty(PropertyName = "StoreName")]
        public string StoreName { get; set; }

        [JsonProperty(PropertyName = "StoreType")]
        public string StoreType { get; set; }

        [JsonProperty(PropertyName = "CustName")]
        public string CustName { get; set; }

        [JsonProperty(PropertyName = "Pool")]
        public string Pool { get; set; }

        [JsonProperty(PropertyName = "Qty")]
        public double Qty { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public string Date { get; set; }

        [JsonProperty(PropertyName = "DueDate")]
        public DateTime DueDate { get; set; }

        [JsonProperty(PropertyName = "ConfirmDate")]
        public string ConfirmDate { get; set; }

        [JsonProperty(PropertyName = "CreatedDate")]
        public string CreatedDate { get; set; }

        [JsonProperty(PropertyName = "LineCount")]
        public int LineCount { get; set; }

        //========= Confirm ========
        [JsonProperty(PropertyName = "Remark")]
        public string Remark { get; set; }

        //========= PO =============
        [JsonProperty(PropertyName = "Series")]
        public string Series { get; set; }

        [JsonProperty(PropertyName = "Model")]
        public string Model { get; set; }

        [JsonProperty(PropertyName = "Sink")]
        public string Sink { get; set; }

        [JsonProperty(PropertyName = "Top")]
        public string Top { get; set; }

        //---------- Image --------------
        [JsonProperty(PropertyName = "ImageName")]
        public string ImageName { get; set; }

        [JsonProperty(PropertyName = "Path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "ImageCount")]
        public int ImageCount { get; set; }

        [JsonProperty(PropertyName = "Base64")]
        public string Base64 { get; set; }

        //---------- Paging --------------
        [JsonProperty(PropertyName = "EndPage")]
        public string EndPage { get; set; }

        [JsonProperty(PropertyName = "CurrentPage")]
        public string CurrentPage { get; set; }

        [JsonProperty(PropertyName = "StartPage")]
        public string StartPage { get; set; }

        [JsonProperty(PropertyName = "TotalPage")]
        public string TotalPage { get; set; }

        [JsonProperty(PropertyName = "Showing")]
        public string Showing { get; set; }

        [JsonProperty(PropertyName = "Entries")]
        public string Entries { get; set; }



        [JsonProperty(PropertyName = "Error")]
        public bool Error { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }
    }
}