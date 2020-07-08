using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Linq;

namespace AfterSchoolLibrary.Services
{
    public static class AccountProfileService
    {
        public static AccountProfile CreateNewAccountProfile(Account account, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var profile = new AccountProfile()
            {
                AccountId = account.ID.Value,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),                  
            };

            msgs.AddRange(profile.Save(dc));

            return profile;
        }

        public static void UploadLogo(this AccountProfile profile, Account account, System.IO.Stream stream, string filename, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ah = Helpers.Utilities.GetNewAmazonHelper();
            var amazonLocation = $"{account.UniqueName}/logo/{filename}";
            msgs.AddRange(ah.AddFile(stream, amazonLocation, true));

            if (msgs.HasNoErrors())
            {
                profile.Logo = amazonLocation;
                profile.LastModifiedBy = loggedInUser.ID;
                profile.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(profile.Save(dc));
            }
        }

        public static Account GetAccount(this AccountProfile profile, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (profile.AccountId.HasValue)
                return Account.Load(profile.AccountId.Value, msgs, dc);

            return null;
        }

        public static void UploadHeaderImage(this AccountProfile profile, Account account, System.IO.Stream stream, string filename, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ah = Helpers.Utilities.GetNewAmazonHelper();
            var amazonLocation = BuildAmazonLocation(account, "headerimage", filename);
            msgs.AddRange(ah.AddFile(stream, amazonLocation, true));

            if (msgs.HasNoErrors())
            {
                profile.HeaderImage = amazonLocation;
                profile.LastModifiedBy = loggedInUser.ID;
                profile.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(profile.Save(dc));
            }
        }

        public static void UploadGalleryPhoto(this AccountProfile profile, Account account, System.IO.Stream stream, string filename, string photoId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ah = Helpers.Utilities.GetNewAmazonHelper();
            var amazonLocation = BuildAmazonLocation(account, "photos", filename);
            msgs.AddRange(ah.AddFile(stream, amazonLocation, true));

            if (msgs.HasNoErrors())
            {
                if (photoId.IsNotEmpty())
                    UpdateProperty(photoId.ToInteger(), amazonLocation, loggedInUser, msgs, dc);
                else
                    profile.AddProperty(amazonLocation, AccountPropertyType.Photo, loggedInUser, msgs, dc);                
            }
        }

        private static string BuildAmazonLocation(Account account, string folder, string filename)
        {
            var ah = Helpers.Utilities.GetNewAmazonHelper();
            return $"{account.UniqueName}/{folder}/{filename}";
        }

        public static string GetHeaderImageUrl(this AccountProfile profile, SystemMessages msgs = null)
        {
            if (profile.HeaderImage.IsNotBlank())
                return Helpers.Utilities.GetNewAmazonHelper().GetFileLink(profile.HeaderImage, true, null, true);

            return null;
        }

        public static string GetLogoUrl(this AccountProfile profile, SystemMessages msgs = null)
        {
            if (profile.Logo.IsNotBlank())
                return Helpers.Utilities.GetNewAmazonHelper().GetFileLink(profile.Logo, true, null, true);

            return null;
        }

        public static List<AccountProperty> GetAwards(this AccountProfile profile, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(profile.AccountId.Value, AccountPropertyType.Award, msgs, dc);
        }

        public static List<AccountProperty> GetCertifications(this AccountProfile profile, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(profile.AccountId.Value, AccountPropertyType.Certification, msgs, dc);
        }

        public static List<AccountProperty> GetPhotos(this AccountProfile profile, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(profile.AccountId.Value, AccountPropertyType.Photo, msgs, dc);
        }

        public static void UpdateProfile(this AccountProfile profile, string headline, string about, string primaryColor, string website, string mingrade, string maxgrade, string twitter, string facebook, string instagram, string linkedin, string youtube, string genderrestriction, NameValueCollection savedAwards, NameValueCollection newAwards, NameValueCollection savedCertifications, NameValueCollection newCertifications, NameValueCollection servicesOffered, NameValueCollection timesOffered, NameValueCollection pricingOptions, string classifications, string softskills, string academicsubjects, string teachingmethods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (headline.IsNotBlank())
                        profile.Headline = headline.MaxChars(400);

                    if (about.IsNotBlank())
                        profile.About = about.MaxChars(4000);

                    if (primaryColor.IsNotBlank())
                        profile.PrimaryColor = primaryColor.MaxChars(20);

                    if (website.IsNotBlank())
                        profile.Website = website.MaxChars(400);

                    if (mingrade.IsInteger())
                        profile.GradeLevelStart = mingrade.ToInteger();

                    if (maxgrade.IsInteger())
                        profile.GradeLevelEnd = maxgrade.ToInteger();

                    if (twitter.IsNotEmpty())
                        profile.Twitter = twitter;

                    if (instagram.IsNotEmpty())
                        profile.Instagram = instagram;

                    if (facebook.IsNotEmpty())
                        profile.Facebook = facebook;

                    if (linkedin.IsNotEmpty())
                        profile.LinkedIn = linkedin;

                    if (youtube.IsNotEmpty())
                        profile.YouTube = youtube;

                    if (genderrestriction.IsInteger())
                        profile.GenderRestriction = (GenderRestrictionType)genderrestriction.ToInteger();

                    amsgs.AddRange(profile.Save(adc));

                    UpdateProperties(savedAwards, loggedInUser, amsgs, adc);
                    UpdateProperties(savedCertifications, loggedInUser, amsgs, adc);

                    profile.AddProperties(newAwards, AccountPropertyType.Award, loggedInUser, amsgs, adc);
                    profile.AddProperties(newCertifications, AccountPropertyType.Certification, loggedInUser, amsgs, adc);

                    profile.ManageProperties(servicesOffered, AccountPropertyType.ServiceOffered, loggedInUser, amsgs, adc);
                    profile.ManageProperties(timesOffered, AccountPropertyType.TimeOffered, loggedInUser, amsgs, adc);
                    profile.ManageProperties(pricingOptions, AccountPropertyType.PricingOption, loggedInUser, amsgs, adc);

                    profile.ManageProperties(classifications.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.Classifications, loggedInUser, amsgs, adc);
                    profile.ManageProperties(softskills.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.SoftSkills, loggedInUser, amsgs, adc);
                    profile.ManageProperties(academicsubjects.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.AcademicSubjects, loggedInUser, amsgs, adc);
                    profile.ManageProperties(teachingmethods.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.TeachingMethods, loggedInUser, amsgs, adc);

                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void ManageProperties(this AccountProfile profile, IEnumerable<string> properties, AccountPropertyType propertyType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var existingProperties = profile.GetProperties(propertyType, msgs, dc);

            existingProperties.ForEach(existingProperty => {
                if (existingProperty.Value.NotIn(properties))
                    msgs.AddRange(existingProperty.Delete(dc));
            });

            profile.AddProperties(properties, propertyType, loggedInUser, msgs, dc);
        }

        public static void ManageProperties(this AccountProfile profile, NameValueCollection properties, AccountPropertyType propertyType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var existingProperties = profile.GetProperties(propertyType, msgs, dc);
            var newValues = new List<string>();

            foreach (var key in properties.AllKeys)
                newValues.Add(properties[key]);

            existingProperties.ForEach(existingProperty => {
                if (existingProperty.Value.NotIn(newValues))
                    msgs.AddRange(existingProperty.Delete(dc));
            });

            profile.AddProperties(properties, propertyType, loggedInUser, msgs, dc);
        }

        public static List<AccountProperty> GetProperties(this AccountProfile profile, AccountPropertyType propertyType, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(profile.AccountId.Value, propertyType, msgs, dc);
        }

        public static void AddProperties(this AccountProfile profile, IEnumerable<string> properties, AccountPropertyType propertyType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            foreach (var value in properties)
                profile.AddProperty(value, propertyType, loggedInUser, msgs, dc);
        }

        public static void AddProperties(this AccountProfile profile, NameValueCollection properties, AccountPropertyType propertyType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            foreach (var key in properties.AllKeys)
                profile.AddProperty(properties[key], propertyType, loggedInUser, msgs, dc);
        }

        public static void AddProperty(this AccountProfile profile, string value, AccountPropertyType propertyType, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (value.IsNotEmpty())
            {
                var property = AccountProperty.LoadByAccountIdByAccountPropertyTypeIdByValue(profile.AccountId.Value, propertyType, value, msgs, dc);

                if (property.IsEmpty())
                {
                    property = new AccountProperty()
                    {
                        AccountId = profile.AccountId,
                        CreatedBy = loggedInUser.ID,
                        CreateDate = DateTime.UtcNow,
                        LastModifiedBy = loggedInUser.ID,
                        LastModifiedDate = DateTime.UtcNow,
                        Value = value.Trim(),
                        AccountPropertyTypeId = propertyType,
                    };

                    msgs.AddRange(property.Save(dc));
                }
            }
        }

        public static void UpdateProperties(NameValueCollection properties, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            foreach (var key in properties.AllKeys)
                UpdateProperty(key.Split('-')[2].ToInteger(), properties[key], loggedInUser, msgs, dc);
        }

        public static void UpdateProperty(int id, string value, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var property = AccountProperty.Load(id, msgs, dc);

            if (value.IsNotBlank())
            {
                property.Value = value.Trim();
                property.LastModifiedBy = loggedInUser.ID;
                property.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(property.Save(dc));
            }
            else
            {
                msgs.AddRange(property.Delete(dc));
            }
        }

        public static GradeLevel GetGradeLevelStart(this AccountProfile profile, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (profile.GradeLevelStart.HasValue)
                return GradeLevel.Load(profile.GradeLevelStart.Value, msgs, dc);

            return GradeLevel.Load(msgs: msgs, dc: dc).FirstOrDefault();
        }

        public static GradeLevel GetGradeLevelEnd(this AccountProfile profile, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (profile.GradeLevelEnd.HasValue)
                return GradeLevel.Load(profile.GradeLevelEnd.Value, msgs, dc);

            return GradeLevel.Load(msgs: msgs, dc: dc).LastOrDefault();
        }
    }
}
