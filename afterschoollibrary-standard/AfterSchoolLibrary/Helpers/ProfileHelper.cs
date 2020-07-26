using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Helpers
{
    public static class ProfileHelper
    {
        public static string[] ServicesOfferings
        {
            get
            {
                return new string[] { "Classes", "Events", "Camps", "Field Trips", "Workshops", "Virtual Learning", "Tutoring", "Mentoring", "Jobs", "Counseling", "Resources", "Curriculum" };
            }
        }
        
        public static string[] OfferingTimes
        {
            get
            {
                return new string[] { "After-school programs", "Before-school programs", "During-school programs", "Evening programs", "Weekend programs", "Summer programs" };
            }
        }

        public static string[] PricingOptions
        {
            get
            {
                return new string[] { "Free Activities available", "Scholarships available" };
            }
        }
    }
}
