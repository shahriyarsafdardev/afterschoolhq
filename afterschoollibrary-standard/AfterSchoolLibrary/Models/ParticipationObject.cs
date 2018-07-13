using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Models
{
    public class ParticipationObject
    {
        public int? ObjectID { get; set; }
        public ParticipationObjectType ObjectType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ActivitySchedule Schedule { get; set; }
        public Location Location { get; set; }

        
    }
}
