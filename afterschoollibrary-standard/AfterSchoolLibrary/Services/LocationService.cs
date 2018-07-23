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

        public static Location CreateNewWithValidation(string locationName, string address1, string address2, string city, string state, string postalCode, string country, LocationType locationType, int? hostAccountId, Account loggedInAccount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var doesExist = DoesLocationAlreadyExist(
                   locationName,
                   address1,
                   address2,
                   city,
                   state,
                   postalCode.MaxChars(5),
                   country,
                   msgs,
                   dc);

            if (doesExist)
            {
                msgs.Add("Location already exists", MessageType.UserError);
                return null;
            }

            var newLocation =  CreateNew(
                locationName, 
                address1, 
                address2, 
                city, 
                state, 
                postalCode, 
                country, 
                locationType, 
                hostAccountId, 
                loggedInUser, 
                msgs, 
                dc);

            if (msgs.HasNoErrors() && !newLocation.IsOwned() && newLocation.LocationTypeId == LocationType.School)
            {
                // if it's a new location and the current account is not the owner and it's a school, 
                // then I should create an inactive Account for the location owner to claim at some point 
                // and then create an account relationship between the Provider and the Host

                var newHostAccount = AccountService.CreateNew(
                    orgName: newLocation.Name,
                    email: null,
                    phone: null,
                    isActive: false,
                    accountType: AccountType.School,
                    accountOwner: null,
                    packageId: null,
                    packageBillingPeriod: null,
                    loggedInUser: loggedInUser,
                    isAfterSchool: true,
                    msgs: msgs,
                    dc: dc);

                var relationship = AccountRelationshipService.CreateNew(
                    hostAccount: newHostAccount,
                    visitorAccount: loggedInAccount,
                    approvalStatus: ApprovalStatus.Requested,
                    hostPermissionType: HostPermissionType.ApproveOnly,
                    loggedInUser: loggedInUser,
                    msgs: msgs,
                    dc: dc);

                var relationshipLocation = AccountRelationshipLocationService.CreateNew(
                    accountRelationship: relationship,
                    hostAccount: newHostAccount,
                    visitorAccount: loggedInAccount,
                    location: newLocation,
                    loggedInUser: loggedInUser,
                    msgs: msgs,
                    dc: dc);

            }

            return newLocation;
            
        }

        public static Location CreateNew(string locationName, string address1, string address2, string city, string state, string postalCode, string country, LocationType locationType, int? hostAccountId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
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
                HostAccountId = hostAccountId,
                Name = locationName
            };

            msgs.AddRange(location.Save(dc));

            return location;
        }

        public static bool DoesLocationAlreadyExist(string locationName, string address1, string address2, string city, string state, string postalCode, string country, SystemMessages msgs, DatabaseConnection dc = null)
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
                    { "Country", country.Trim() }
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
                    { "Address2", address2.Trim() },
                    { "City", city.Trim() },
                    { "State", state.Trim() },
                    { "PostalCode", postalCode.Trim() },
                },
                msgs: msgs,
                dc: dc);
        }

        public static bool IsOwned(this Location location)
        {
            return location.HostAccountId.HasValue;
        }

        public static bool IsOwnedBy(this Location location,  Account account)
        {
            return location.HostAccountId == account.ID;
        }

        public static bool IsOwnedByOrIsNotOwned(this Location location, Account account)
        {
            return location.IsOwnedBy(account) || !location.IsOwned();
        }
    }
}
