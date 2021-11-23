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

        public static GradeLevel Create(string name, string abbreviation, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new GradeLevel() { Name = name, Abbreviation = abbreviation };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static List<GradeLevel> CreateAllGradesLevels(SystemMessages msgs, DatabaseConnection dc)
        {
            List<GradeLevel> gradeLevels = new List<GradeLevel>();

            gradeLevels.Add(Create("Pre-K", "Pre-K", msgs, dc));
            gradeLevels.Add(Create("Kindergarten", "K", msgs, dc));
            gradeLevels.Add(Create("1st", "1st", msgs, dc));
            gradeLevels.Add(Create("2nd", "2nd", msgs, dc));
            gradeLevels.Add(Create("3rd", "3rd", msgs, dc));
            gradeLevels.Add(Create("4th", "4th", msgs, dc));
            gradeLevels.Add(Create("5th", "5th", msgs, dc));
            gradeLevels.Add(Create("6th", "6th", msgs, dc));
            gradeLevels.Add(Create("7th", "7th", msgs, dc));
            gradeLevels.Add(Create("8th", "8th", msgs, dc));
            gradeLevels.Add(Create("9th", "9th", msgs, dc));
            gradeLevels.Add(Create("10th", "10th", msgs, dc));
            gradeLevels.Add(Create("11th", "11th", msgs, dc));
            gradeLevels.Add(Create("12th", "12th", msgs, dc));

            return gradeLevels;
        }
    }
}
