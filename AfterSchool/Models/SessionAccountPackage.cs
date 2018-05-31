using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary;

namespace AfterSchool.Models
{
    [Serializable]
    public class SessionAccountPackage
    {
        public int ID { get; set; }
        public int AccountId { get; set; }
        public int PackageId { get; set; }
        public decimal Price { get; set; }
        public bool PackageHasAttendance { get; set; }
        public bool PackageHasCNameUrl { get; set; }
        public bool PackageHasMessaging { get; set; }
        public bool PackageHasPayments { get; set; }
        public bool PackageHasWaitingList { get; set; }
        public int PackageMaxActivities { get; set; }
        public int PackageMaxRegistrants { get; set; }
        public int PackageMaxUploadSpace { get; set; }
        public int PackageMaxUsers { get; set; }
        public string PackageName { get; set; }
        public decimal PackagePrice { get; set; }

        public SessionAccountPackage(AccountPackage ap)
        {
            ID = ap.ID.Value;
            AccountId = ap.AccountId.Value;
            PackageId = ap.PackageId.Value;
            Price = ap.Price.Value;
            PackageHasAttendance = ap.PackageHasAttendance;
            PackageHasCNameUrl = ap.PackageHasCNameUrl;
            PackageHasMessaging = ap.PackageHasMessaging;
            PackageHasPayments = ap.PackageHasPayments;
            PackageHasWaitingList = ap.PackageHasWaitingList;
            PackageMaxActivities = ap.PackageMaxActivities.Value;
            PackageMaxRegistrants = ap.PackageMaxRegistrants.Value;
            PackageMaxUploadSpace = ap.PackageMaxUploadSpace.Value;
            PackageMaxUsers = ap.PackageMaxUsers.Value;
            PackageName = ap.PackageName;
            PackagePrice = ap.PackagePrice.Value;
        }

        public bool IsFree
        {
            get
            {
                return Price == 0;
            }
        }

        public AccountPackage ToAccountPackage(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return AccountPackage.Load(this.ID, dc);
        }
    }
}