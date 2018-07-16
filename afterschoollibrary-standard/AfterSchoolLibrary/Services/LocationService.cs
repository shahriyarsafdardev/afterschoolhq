using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class LocationService
    {
        public static List<Account> GetVisitorAccounts(this Location location, DatabaseConnection dc = null)
        {
            return AccountRelationshipLocation.LoadByLocationId(location.ID.Value, dc: dc)
                .Select(x => x.GetVisitorAccount(dc: dc))
                .ToList();
        }
        
        public static Account GetHostAcount(this Location location, DatabaseConnection dc = null)
        {
            return Account.Load(location.HostAccountId.Value, dc: dc);
        }

        public static Location CreateNew(string locationName, string address1, string address2, string city, string state, string postalCode, string country, int accountId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var location = new Location()
            {
                Address1 = address1,
                Address2 = address2,
                City = city,
                State = state,
                PostalCode = postalCode,
                Country = country,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                HostAccountId = accountId,
                Name = locationName
            };

            msgs.AddRange(location.Save(dc));

            return location;
        }

        public static bool DoesLocationAlreadyExist(string locationName, string address1, string address2, string city, string state, string postalCode, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Location.GetCount(
                new Hashtable()
                {
                    { "Name", locationName.Trim() },
                    { "Address1", address1.Trim() },
                    { "Address2", address2.Trim() },
                    { "City", city.Trim() },
                    { "State", state.Trim() },
                    { "PostalCode", postalCode.Trim() },
                },
                msgs: msgs,
                dc: dc) > 0;
        }

        public static List<Location> LoadByAddress(string locationName, string address1, string address2, string city, string state, string postalCode, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Location.Load(
                new Hashtable()
                {
                    { "Name", locationName.Trim() },
                    { "Address1", address1.Trim() },
                    { "Address1", address2.Trim() },
                    { "City", city.Trim() },
                    { "State", state.Trim() },
                    { "PostalCode", postalCode.Trim() },
                },
                msgs: msgs,
                dc: dc);
        }
    }
}
