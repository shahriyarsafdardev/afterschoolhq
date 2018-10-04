using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class GradeLevelFactory
    {
        public static GradeLevel Create(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new GradeLevel() { Name = "whatever" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
