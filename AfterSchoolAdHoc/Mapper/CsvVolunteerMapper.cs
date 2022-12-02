using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AfterSchoolAdHoc.DTOs;
using TinyCsvParser.Mapping;

namespace AfterSchoolAdHoc.Mapper
{
    public class CsvVolunteerMapper : CsvMapping<VolunteerDto>
    {
        public CsvVolunteerMapper()
        : base()
        {
            MapProperty(0, x => x.status);
            MapProperty(1, x => x.dateReviewed);
            MapProperty(2, x => x.legalFirstName);
            MapProperty(3, x => x.preferedName);
            MapProperty(4, x => x.legalSecondName);
            MapProperty(9, x => x.email);
            MapProperty(10, x => x.phoneNumber);
        }
    }
}
