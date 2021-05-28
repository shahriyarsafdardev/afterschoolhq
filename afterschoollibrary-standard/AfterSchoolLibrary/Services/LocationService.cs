using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Web;
using AfterSchoolLibrary.Helpers;

namespace AfterSchoolLibrary.Services
{
    public static class LocationService
    {
        /// <summary>
        /// its the same if all the address fields are the same and if the HostAccountIds are the same
        /// Whether host account ids have same value, are both null, or if one if them is null. 
        /// If both are not null, then they must be equal
        /// </summary>
        /// <param name="location"></param>
        /// <param name="otherLocation"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static bool IsSameAs(this Location location, Location otherLocation, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return location.Name.IsSameAs(otherLocation.Name) &&
                location.Address1.IsSameAs(otherLocation.Address1) &&
                location.Address2.IsSameAs(otherLocation.Address2) &&
                location.City.IsSameAs(location.City) &&
                location.State.IsSameAs(location.State) &&
                location.PostalCode.IsSameAs(location.PostalCode) &&
                (
                    location.HostAccountId == otherLocation.HostAccountId || 
                    (location.HostAccountId.HasValue && !otherLocation.HostAccountId.HasValue) ||
                    (!location.HostAccountId.HasValue && otherLocation.HostAccountId.HasValue)
                );
        }

        public static SystemMessages SetHostAccount(this Location location, Account hostAccount, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            location.HostAccountId = hostAccount.ID;
            location.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            location.LastModifiedDate = DateTime.UtcNow;
            return location.Save(dc);
        }

        public static List<Account> GetVisitorAccounts(this Location location, DatabaseConnection dc = null)
        {
            return AccountRelationshipLocation.LoadByLocationId(location.ID.Value, dc: dc)
                .Select(x => x.GetVisitorAccount(dc: dc))
                .ToList();
        }
        
        public static Account GetHostAccount(this Location location, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (location.HostAccountId.HasValue)
                return Account.Load(location.HostAccountId.Value, msgs, dc);

            return null;
        }

        public static Location CreateNewWithValidation(string locationName, string address1, string address2, string city, string state, string postalCode, string country, LocationType locationType, int? hostAccountId, Account visitorAccount, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
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

            if (msgs.HasNoErrors() && !newLocation.IsOwned())
            {
                // if it's a new location and the current account is not the owner and it's a school, 
                // then I should create an inactive Account for the location owner to claim at some point 
                // and then create an account relationship between the Provider and the Host

                var accountType = AccountType.ActivityProvider;

                if (newLocation.LocationTypeId == LocationType.School)
                    accountType = AccountType.School;

                var newHostAccount = AccountService.CreateNewGhostAccount(
                    orgName: newLocation.Name,
                    accountType: accountType,
                    loggedInUser: loggedInUser,
                    isAfterSchool: true,
                    msgs: msgs,
                    dc: dc);

                // set host account
                newLocation.SetHostAccount(newHostAccount, loggedInUser, dc);

                if (visitorAccount.IsNotEmpty())
                {
                    var relationship = AccountRelationshipService.GetOrCreateNew(
                        hostAccount: newHostAccount,
                        visitorAccount: visitorAccount,
                        approvalStatus: ApprovalStatus.Requested,
                        hostPermissionType: HostPermissionType.ApproveOnly,
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);

                    var relationshipLocation = AccountRelationshipLocationService.GetOrCreateNew(
                        accountRelationship: relationship,
                        hostAccount: newHostAccount,
                        visitorAccount: visitorAccount,
                        location: newLocation,
                        loggedInUser: loggedInUser,
                        msgs: msgs,
                        dc: dc);
                }

                // TODO: Send message to internal team about new ghost account

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
                LocationTypeId = locationType,
                Name = locationName
            };

            msgs.AddRange(location.Save(dc));

            return location;
        }

        public static Location CreateOrUpdateFromGoogle(string locationName, string streetNumber, string streetName, string address2, string neighborhood, string city, string county, string state, string postalCode, string country, 
            string googlePlaceId, decimal? latitude, decimal? longitude,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var location = Location.LoadByGooglePlaceId(googlePlaceId, msgs, dc);

            if (location.IsEmpty())
                location = new Location()
                {
                    GooglePlaceId = googlePlaceId,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID,
                };            

            location.Name = locationName;
            location.StreetNumber = streetNumber;
            location.StreetName = streetName;
            location.Address1 = $"{streetNumber} {streetName}";
            location.Address2 = address2;
            location.Neighborhood = neighborhood;
            location.City = city;
            location.County = county;
            location.State = state;
            location.PostalCode = postalCode;
            location.Country = country;
            location.Latitude = latitude;
            location.Longitude = longitude;
            location.LastModifiedDate = DateTime.UtcNow;
            location.LastModifiedBy = loggedInUser?.ID;

            //..co-ordiantes can be negative
            if (longitude.IsNotEmpty() && latitude.IsNotEmpty())
            {
                var distance = DistanceAlgorithm.DistanceBetweenPlaces(Convert.ToDouble(longitude), Convert.ToDouble(latitude), 0, 0);
                if (distance.IsNotEmpty())
                    location.DistanceFromOrigin = Convert.ToDecimal(distance);
            }

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
                    { "Address2", address2.IfBlank("").Trim() },
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

        public static bool IsOwnedBy(this Location location,  Account ownerAccount)
        {
            return location.HostAccountId == ownerAccount.ID;
        }

        public static bool IsOwnedByOrIsNotOwnedAtAll(this Location location, Account ownerAccount)
        {
            return location.IsOwnedBy(ownerAccount) || !location.IsOwned();
        }

        public static void CleanUpDuplicateLocations(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var locations = Location.Load(msgs: msgs, dc: dc).OrderByDescending(location => location.HostAccountId.HasValue).ThenBy(location => location.ID).ToList();
            var processedDuplicates = new List<Location>();
            var masterLocations = new List<Location>();

            locations.ForEach(location => {
                var duplicates = locations.Where(otherLocation => 
                    location.IsSameAs(otherLocation) && 
                    location.ID != otherLocation.ID && 
                    otherLocation.NotIn(processedDuplicates) &&
                    otherLocation.NotIn(masterLocations))
                    .ToList();

                masterLocations.Add(location);

                duplicates.ForEach(duplicateLocation => {

                    // find accounts that have the duplicate set as the default location
                    var accounts = Account.Load(new Hashtable() { { "DefaultLocationId", duplicateLocation.ID.Value } }, null, null, msgs, dc);

                    accounts.ForEach(account => {
                        account.DefaultLocationId = location.ID.Value;
                        msgs.AddRange(account.Save(dc));
                    });

                    // find activities that have the duplicate set as the default location
                    var activities = Activity.Load(new Hashtable() { { "LocationId", duplicateLocation.ID.Value } }, null, null, msgs, dc);

                    activities.ForEach(activity => {
                        activity.LocationId = location.ID.Value;
                        msgs.AddRange(activity.Save(dc));
                    });

                    // find registrants that have the duplicate set as the school location
                    var registrants = Registrant.Load(new Hashtable() { { "SchoolLocationId", duplicateLocation.ID.Value } }, null, null, msgs, dc);

                    registrants.ForEach(registrant => {
                        registrant.SchoolLocationId = location.ID.Value;
                        msgs.AddRange(registrant.Save(dc));
                    });

                    // find AccountRelationshipLocations that have the duplicate set as the locationid
                    var arls = AccountRelationshipLocation.Load(new Hashtable() { { "LocationId", duplicateLocation.ID.Value } }, null, null, msgs, dc);

                    arls.ForEach(arl => {

                        // see if there is already a relationship for the non-duplicate location
                        var otherArl = AccountRelationshipLocation.LoadByHostAccountIdByVisitorAccountIdByLocationId(
                            arl.HostAccountId.Value,
                            arl.VisitorAccountId.Value,
                            location.ID.Value,
                            msgs,
                            dc);

                        // if no existing relationship with non-duplicate location, then update the current record
                        // otherwise delete the existing relationship so we don't create a duplicate relationship entry
                        if (otherArl.IsEmpty())
                        {
                            arl.LocationId = location.ID.Value;
                            msgs.AddRange(arl.Save(dc));
                        }
                        else
                        {
                            msgs.AddRange(arl.Delete(dc));
                        }
                    });

                    // now that everything is done, delete the duplicate
                    msgs.AddRange(duplicateLocation.Delete(dc));

                    processedDuplicates.Add(duplicateLocation);
                });
            });
            
        }

        public static string GetGoogleMapsImageUrl(this Location location, int width, int height)
        {
            return $"https://maps.googleapis.com/maps/api/staticmap?center={HttpUtility.UrlEncode(location.FormatAddressWithoutName(false))}&zoom=13&size={width}x{height}&maptype=roadmap&markers=color:{HttpUtility.UrlEncode("0x00AE83")}|{HttpUtility.UrlEncode(location.FormatAddressWithoutName(false))}&key={Helpers.GoogleHelper.Maps.ApiKey}";
        }
    }
}
