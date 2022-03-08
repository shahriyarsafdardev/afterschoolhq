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
                return new string[] { "Classes", "Events", "Camps", "Field Trips", "Workshops", "Virtual Learning", "Tutoring", "Mentoring", "Youth Employment", "Counseling", "Resources", "Curriculum" };
            }
        }
        
        public static string[] OfferingTimes
        {
            get
            {
                return new string[] { "After-school programs", "Before-school programs", "During-school programs", "Evening programs", "Weekend programs", "Summer programs", "Overnight programs" };
            }
        }

        public static string[] PricingOptions
        {
            get
            {
                return new string[] { "Free Activities available", "Scholarships available", "Payment Plans available" };
            }
        }

        public static string[] PricingOptionsWithoutFreeActivities
        {
            get
            {
                return new string[] { "Scholarships available", "Payment Plans available" };
            }
        }

        public static string[] SpecialNeedsOfferings
        {
            get
            {
                return new string[] { "ADD / ADHD", "Autism", "Blindness", "Deafness", "Down Syndrome", "Dyslexia", "Emotional Disturbance", "Hearing Impairment", "Intellectual Disability", "Multiple Disabilities", "Orthopedic Impairment", "Other Health Impaired", "Specific Learning Disability", "Speech or Language Impairment", "Traumatic Brain Injury", "Visual Impairment" };
            }
        }
    }
}
