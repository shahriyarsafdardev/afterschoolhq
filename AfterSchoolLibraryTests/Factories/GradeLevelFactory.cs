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

            gradeLevels.Add(Create("Pre - K", "P", msgs, dc));
            gradeLevels.Add(Create("Kindergarten", "K", msgs, dc));
            gradeLevels.Add(Create("1st", "1", msgs, dc));
            gradeLevels.Add(Create("2nd", "2", msgs, dc));
            gradeLevels.Add(Create("3rd", "3", msgs, dc));
            gradeLevels.Add(Create("4th", "4", msgs, dc));
            gradeLevels.Add(Create("4th", "4", msgs, dc));
            gradeLevels.Add(Create("5th", "5", msgs, dc));
            gradeLevels.Add(Create("6th", "6", msgs, dc));
            gradeLevels.Add(Create("7th", "7", msgs, dc));
            gradeLevels.Add(Create("8th", "8", msgs, dc));
            gradeLevels.Add(Create("9th", "9", msgs, dc));
            gradeLevels.Add(Create("10th", "10", msgs, dc));
            gradeLevels.Add(Create("11th", "11", msgs, dc));
            gradeLevels.Add(Create("12th", "12", msgs, dc));

            return gradeLevels;
        }
    }
}
