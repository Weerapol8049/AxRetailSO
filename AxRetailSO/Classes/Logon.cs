using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace AxRetailSO.Classes
{
    public class Logon
    {
        public static string DefaultUser
        {
            get { return "1011405"; }
        }
        public static string DefaultPassword
        {
            get { return "Weeku8049"; }
        }
        public static string Domain
        {
            get { return "STMM.LOCAL"; }
        }

        public static string User { get; set; }
        public static string Password { get; set; }
        public static void GetCredential()
        {
            Credential = new NetworkCredential(User, Password, Domain);
        }
        public static void GetDefaultCredential()
        {

            DefaultCredential = new NetworkCredential(DefaultUser, DefaultPassword, Domain);
        }


        public static NetworkCredential Credential { get; set; }
        public static NetworkCredential DefaultCredential { get; set; }

    }
}