using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Data;
using AfterSchoolLibrary.Services;

namespace AfterSchoolAdHoc;

internal class StudentImportCSV
{
    public static void Run(SystemMessages msgs, DatabaseConnection dc)
    {
        var dt = RegistrantService.GetAllStudents(msgs,dc);
        
        var studentList = (from DataRow row in dt.Rows
            select new StudentImportDto
            {
                ID = Convert.ToInt32(row["ID"]),
                FirstName = row["first_name"].ToString(),
                LastName = row["last_name"].ToString(),
                Email = row["email"].ToString(),
                DOB = row["DOB"].ToString().ToDateTime(),
                Gender = (int) StudentImportDto.GetGenderType(row["Gender"].ToString().ToLower()),
                Address = row["Student_street"].ToString(),
                ZipCode = row["Student_zip"].ToString(),
                RegistrantId = (int) row["RegistrantId"],
            }).ToList();

        studentList.ForEach(student =>
            {
                var recordExist = RegistrantService.CheckStudentRegistrant(student.RegistrantId, student.ID, msgs, dc);
                if (student.RegistrantId.IsNotEmpty() && !recordExist)
                {
                    var matchedColumns = $"FirstName: {student.FirstName}, LastName: {student.LastName}";

                    var registrantByDateOfBirth = FormQuestionAnswer.Load(
                        new Hashtable()
                        {
                            {"RegistrantId", student.RegistrantId},
                            {"QuestionText", "Date of Birth"},
                            {"QuestionDataTypeId", (int) QuestionDataType.RegistrantDateOfBirth},
                        },
                        msgs: msgs,
                        dc: dc
                    ).FirstOrDefault();

                    var registrantByGender = Registrant.Load(
                        new Hashtable()
                        {
                            {"ID", student.RegistrantId},
                            {"GenderId", student.Gender},
                        },
                        msgs: msgs,
                        dc: dc
                    ).FirstOrDefault();

                    var registrantByPostalCode = FormQuestionAnswer.Load(
                        new Hashtable()
                        {
                            {"RegistrantId", student.RegistrantId},
                            {"QuestionText", "Zip Code"},
                            {"QuestionDataTypeId", (int) QuestionDataType.RegistrantPostalCode},
                            {"Text", student.ZipCode},
                        },
                        msgs: msgs,
                        dc: dc
                    ).FirstOrDefault();

                    var registrantByAddress = FormQuestionAnswer.Load(
                        new Hashtable()
                        {
                            {"RegistrantId", student.RegistrantId},
                            {"QuestionText", "Street Address"},
                            {"QuestionDataTypeId", (int) QuestionDataType.RegistrantAddress1},
                            {"Text", student.Address},
                        },
                        msgs: msgs,
                        dc: dc
                    ).FirstOrDefault();

                    if (registrantByDateOfBirth.IsNotEmpty() && registrantByDateOfBirth.Text.IsNotEmpty() && 
                        student.DOB == registrantByDateOfBirth.Text.ToDateTime())
                        matchedColumns = string.Concat(matchedColumns, $", Date of Birth: {student.DOB}");

                    if (registrantByGender.IsNotEmpty())
                        matchedColumns = string.Concat(matchedColumns, $", Gender: {(GenderType) student.Gender}");

                    if (registrantByPostalCode.IsNotEmpty())
                        matchedColumns = string.Concat(matchedColumns, $", ZipCode: {student.ZipCode}");

                    if (registrantByAddress.IsNotEmpty())
                        matchedColumns = string.Concat(matchedColumns, $", Address: {student.Address}");

                    matchedColumns = matchedColumns.Replace("'","''"); // for avoiding single quotes in matchedColumns during insertion

                    if (registrantByDateOfBirth.IsNotEmpty() && registrantByDateOfBirth.Text.IsNotEmpty() && 
                        student.DOB == registrantByDateOfBirth.Text.ToDateTime())
                        RegistrantService.SaveMatchedStudentRegistrant(student.RegistrantId, student.ID, matchedColumns, msgs, dc);
                }
            }
        );
    }
}