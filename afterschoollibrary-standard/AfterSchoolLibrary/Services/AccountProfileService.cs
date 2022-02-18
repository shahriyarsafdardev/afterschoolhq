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
                Title = account.Name,
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

        public static void UpdateProfile(this AccountProfile profile, string title = null, string headline = null, string about = null, string website = null, bool opencovid = false, bool elearningcenter = false, bool isvirtualprogram = false, string mingrade = null, string maxgrade = null, string agestart = null, string ageend = null, string costlow = null, string costhigh = null, string twitter = null, string facebook = null, string instagram = null, string linkedin = null, string youtube = null, string calendar = null, string genderrestriction = null, string profileemail = null, string profilephone = null, NameValueCollection savedAwards = null, NameValueCollection newAwards = null, NameValueCollection savedCertifications = null, NameValueCollection newCertifications = null, NameValueCollection servicesOffered = null, NameValueCollection timesOffered = null, NameValueCollection pricingOptions = null, NameValueCollection specialNeedsOfferings = null, string classifications = null, string softskills = null, string academicsubjects = null, string teachingmethods = null, string programsalldayduringworkweek = null, string ahqapproved = null, string videoUrl = null, WebUser loggedInUser = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = profile.GetAccount(msgs, dc);

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // ..update account name if title has been changed for profile
                    if (account.IsNotEmpty() && title.IsNotEmpty() && profile.Title != title)
                    {
                        account.Name = title;
                        account.Save(adc);
                    }

                    if (title != null)
                        profile.Title = title.MaxChars(50);

                    if (headline != null)
                        profile.Headline = headline.MaxChars(400);

                    if (about != null)
                        profile.About = about.MaxChars(4000);

                    if (website != null)
                        profile.Website = website.MaxChars(400);

                    if (twitter != null)
                        profile.Twitter = twitter;

                    if (instagram != null)
                        profile.Instagram = instagram;

                    if (facebook != null)
                        profile.Facebook = facebook;

                    if (linkedin != null)
                        profile.LinkedIn = linkedin;

                    if (youtube != null)
                        profile.YouTube = youtube;

                    if (calendar != null)
                        profile.CalendarBookingLink = calendar;

                    if (profileemail != null)
                        profile.ProfileEmail = profileemail;

                    if (profilephone != null)
                        profile.ProfilePhone = profilephone;

                    if (genderrestriction.IsInteger())
                        profile.GenderRestriction = (GenderRestrictionType)genderrestriction.ToInteger();

                    if (mingrade.IsInteger())
                    {
                        if (mingrade.ToInteger() < 0)
                            profile.GradeLevelStart = null;
                        else
                        {
                            profile.GradeLevelStart = mingrade.ToInteger();
                            profile.AgeStart = null;
                        }

                    }

                    if (maxgrade.IsInteger())
                    {
                        if (maxgrade.ToInteger() < 0)
                            profile.GradeLevelEnd = null;
                        else
                        {
                            profile.GradeLevelEnd = maxgrade.ToInteger();
                            profile.AgeEnd = null;
                        }
                    }

                    if (agestart.IsInteger())
                    {
                        if (agestart.ToInteger() < 0)
                            profile.AgeStart = null;
                        else
                        {
                            profile.AgeStart = agestart.ToInteger();
                            profile.GradeLevelStart = null;
                        }

                    }

                    if (ageend.IsInteger())
                    {
                        if (ageend.ToInteger() < 0)
                            profile.AgeEnd = null;
                        else
                        {
                            profile.AgeEnd = ageend.ToInteger();
                            profile.GradeLevelEnd = null;
                        }

                    }

                    if (costhigh.IsNotEmpty())
                        profile.CostHigh = costhigh.ToDecimal();

                    if (costlow.IsNotEmpty())
                        profile.CostLow = costlow.ToDecimal();

                    profile.IsOpenDuringCovid = opencovid;
                    profile.IsELearningCenter = elearningcenter;
                    profile.IsVirtualProgram = isvirtualprogram;

                    if (ahqapproved.IsNumber())
                        profile.IsAHQApproved = ahqapproved.ToBoolean();

                    if (videoUrl != null)
                        profile.VideoUrl = videoUrl;

                    amsgs.AddRange(profile.Save(adc));

                    if (savedAwards.IsNotEmpty())
                        UpdateProperties(savedAwards, loggedInUser, amsgs, adc);

                    if (savedCertifications.IsNotEmpty())
                        UpdateProperties(savedCertifications, loggedInUser, amsgs, adc);

                    if (newAwards.IsNotEmpty())
                        profile.AddProperties(newAwards, AccountPropertyType.Award, loggedInUser, amsgs, adc);

                    if (newCertifications.IsNotEmpty())
                        profile.AddProperties(newCertifications, AccountPropertyType.Certification, loggedInUser, amsgs, adc);

                    if (servicesOffered.IsNotEmpty())
                        profile.ManageProperties(servicesOffered, AccountPropertyType.ServiceOffered, loggedInUser, amsgs, adc);

                    if (timesOffered.IsNotEmpty())
                        profile.ManageProperties(timesOffered, AccountPropertyType.TimeOffered, loggedInUser, amsgs, adc);

                    if (pricingOptions.IsNotEmpty())
                        profile.ManageProperties(pricingOptions, AccountPropertyType.PricingOption, loggedInUser, amsgs, adc);

                    if (specialNeedsOfferings.IsNotEmpty())
                        profile.ManageProperties(specialNeedsOfferings, AccountPropertyType.SpecialNeedsOffering, loggedInUser, amsgs, adc);
                                     
                    profile.ManageProperties(classifications.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.Classifications, loggedInUser, amsgs, adc);
                    profile.ManageProperties(programsalldayduringworkweek.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.ProgramsAllDayDuringWorkWeek, loggedInUser, msgs, dc);

                    profile.ManageProperties(softskills.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.SoftSkills, loggedInUser, amsgs, adc);

                   
                    profile.ManageProperties(academicsubjects.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.AcademicSubjects, loggedInUser, amsgs, adc);

                    
                    profile.ManageProperties(teachingmethods.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.TeachingMethods, loggedInUser, amsgs, adc);

                    profile.LastModifiedBy = loggedInUser.ID.Value;
                    profile.LastModifiedDate = DateTime.UtcNow;

                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }             
        
        public static void UpdateProfileLinks(this AccountProfile profile, string website, string twitter, string facebook, string instagram, string linkedin, string youtube, string calendar, WebUser loggedInUser = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            profile.Website = website.MaxChars(400);
            profile.Twitter = twitter;
            profile.Instagram = instagram;
            profile.Facebook = facebook;
            profile.LinkedIn = linkedin;
            profile.YouTube = youtube;
            profile.CalendarBookingLink = calendar;
            profile.LastModifiedBy = loggedInUser.ID.Value;
            profile.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(profile.Save(dc));
        }

        public static void UpdateProfileContactinfo(this AccountProfile profile, WebUser loggedInUser, string profileemail = null, string profilephone = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            profile.ProfileEmail = profileemail;
            profile.ProfilePhone = profilephone;
            profile.LastModifiedBy = loggedInUser.ID.Value;
            profile.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(profile.Save(dc));
        }

        public static void UpdateProfileProgramInfo(this AccountProfile profile, string title = null, string headline = null, string genderrestriction = null, WebUser loggedInUser = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {    
            profile.Title = title.MaxChars(50);
            profile.Headline = headline.MaxChars(400);
            profile.GenderRestriction = (GenderRestrictionType)genderrestriction.ToInteger();
            profile.LastModifiedBy = loggedInUser.ID.Value;
            profile.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(profile.Save(dc));
        }      

        public static void UpdateProfileServicesOffered(this AccountProfile profile, string classifications = null, string softskills = null, NameValueCollection servicesOffered = null, NameValueCollection timesOffered = null, NameValueCollection pricingOptions = null, NameValueCollection specialNeedsOfferings = null, string academicsubjects = null, string teachingmethods = null, string programsalldayduringworkweek = null, WebUser loggedInUser = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            profile.ManageProperties(servicesOffered, AccountPropertyType.ServiceOffered, loggedInUser, msgs, dc);            
            profile.ManageProperties(timesOffered, AccountPropertyType.TimeOffered, loggedInUser, msgs, dc);
            profile.ManageProperties(pricingOptions, AccountPropertyType.PricingOption, loggedInUser, msgs, dc);
            profile.ManageProperties(specialNeedsOfferings, AccountPropertyType.SpecialNeedsOffering, loggedInUser, msgs, dc);
            profile.ManageProperties(programsalldayduringworkweek.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.ProgramsAllDayDuringWorkWeek, loggedInUser, msgs, dc);
            profile.ManageProperties(classifications.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.Classifications, loggedInUser, msgs, dc);
            profile.ManageProperties(softskills.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.SoftSkills, loggedInUser, msgs, dc);
            profile.ManageProperties(academicsubjects.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.AcademicSubjects, loggedInUser, msgs, dc);
            profile.ManageProperties(teachingmethods.Split(',').Select(x => x.Trim().ToProperCase()), AccountPropertyType.TeachingMethods, loggedInUser, msgs, dc);
            profile.LastModifiedBy = loggedInUser.ID.Value;
            profile.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(profile.Save(dc));
        }

        public static void UpdateProfileAdditionalInfo(this AccountProfile profile, string mingrade, string maxgrade, string agestart, string ageend, string costhigh, string costlow, bool opencovid = false, bool elearningcenter = false, bool isvirtualprogram = false, WebUser loggedInUser = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            profile.GradeLevelStart = mingrade.IsInteger() ? mingrade.ToInteger() : new int?();
            profile.GradeLevelEnd = maxgrade.IsInteger() ? maxgrade.ToInteger() : new int?();
            profile.AgeStart = agestart.IsInteger() ? agestart.ToInteger() : new int?();
            profile.AgeEnd = ageend.IsInteger() ? ageend.ToInteger() : new int?();
            profile.IsOpenDuringCovid = opencovid;
            profile.IsELearningCenter = elearningcenter;
            profile.IsVirtualProgram = isvirtualprogram;
            profile.LastModifiedBy = loggedInUser.ID.Value;
            profile.LastModifiedDate = DateTime.UtcNow;

            if(costhigh.IsNotEmpty())
               profile.CostHigh = costhigh.ToDecimal();

            if (costlow.IsNotEmpty())
                profile.CostLow = costlow.ToDecimal();

            msgs.AddRange(profile.Save(dc)); 
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

        public static List<AccountProperty> GetProperties(this AccountProfile profile, AccountPropertyType propertyType, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static void RemoveProperty(this AccountProfile profile, int id, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var property = AccountProperty.Load(id, msgs, dc);
            if (property != null)
                msgs.AddRange(property.Delete(dc: dc));
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

        public static Location GetLocation(this AccountProfile profile, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (profile.LocationId.HasValue)
                return Location.Load(profile.LocationId.Value, msgs, dc);

            return null;
        }

        public static Location AddNewLocation(this AccountProfile profile, string locationName, string streetNumber, string streetName, string address2, string neighborhood, string city, string county, string state, string postalCode, string country, 
            string googlePlaceId, decimal latitude, decimal longitude,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var location = LocationService.CreateOrUpdateFromGoogle(locationName, streetNumber, streetName, address2, neighborhood, city, county, state, postalCode, country, googlePlaceId, latitude, longitude, loggedInUser, msgs, dc);

            profile.SetLocation(location, loggedInUser, msgs, dc);

            return location;
        }

        public static void SetLocation(this AccountProfile profile, Location location, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            profile.LocationId = location.ID;
            profile.LastModifiedBy = loggedInUser?.ID;
            profile.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(profile.Save(dc));
        }

        public static int? GetAgeStart(this AccountProfile profile)
        {
            if (profile.AgeStart.HasValue)
                return profile.AgeStart;

            return null;
        }

        public static int? GetAgeEnd(this AccountProfile profile)
        {
            if (profile.AgeEnd.HasValue)
                return profile.AgeEnd;

            return null;
        }

        public static bool RestrictByGrade(this AccountProfile profile)
        {
            return profile.AgeStart.IsEmpty();
        }

        public static bool IsRestrictByGrade(this AccountProfile profile)
        {
            return profile.GradeLevelStart.IsNotEmpty();
        }

        public static string PrintGenderRestrictions(this AccountProfile profile)
        {
            switch (profile.GenderRestriction)
            {
                case GenderRestrictionType.FemaleOnly:
                    return "Girls";
                case GenderRestrictionType.MaleOnly:
                    return "Boys";
            }
            return "Co-Ed";
        }
    }
}
