using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Data;

namespace AfterSchoolAdHoc;

internal class StudentImportCSV
{
    public static void Run(SystemMessages msgs, DatabaseConnection dc)
    {
        Database.ExecuteTransaction
        (
            (amsgs, adc) =>
            {
                const string sql =
                    @"select s.ID,s.first_name,s.last_name,s.email, s.DOB, s.Gender,s.Student_street, s.Student_zip, r.ID as RegistrantId from Registrant as r 
                      inner join StudentImportCSV as s on r.FirstName = s.first_name and r.LastName = s.last_name";

                var dq = new DatabaseQuery();
                dq.Connection = adc;
                var dt = dq.ExecuteSelect(sql, amsgs);

                var studentList = (from DataRow row in dt.Rows
                    select new StudentImportDto
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        FirstName = row["first_name"].ToString(),
                        LastName = row["last_name"].ToString(),
                        Email = row["email"].ToString(),
                        DOB = row["DOB"].ToString(),
                        Gender = (int) StudentImportDto.GetGenderType(row["Gender"].ToString().ToLower()),
                        Address = row["Student_street"].ToString(),
                        ZipCode = row["Student_zip"].ToString(),
                        RegistrantId = (int) row["RegistrantId"],
                    }).ToList();

                studentList.ForEach(student =>
                {
                    if (student.RegistrantId.IsNotEmpty())
                    {
                        var matchedColumns = $"{student.FirstName}, {student.LastName}";
                        
                        var answerByDob = FormQuestionAnswer.Load(
                            new Hashtable()
                            {
                                {"RegistrantId", student.RegistrantId},
                                {"QuestionText", "Date of Birth"},
                                {"QuestionDataTypeId", (int) QuestionDataType.RegistrantDateOfBirth},
                                {"Text", student.DOB},
                            },
                            msgs: amsgs,
                            dc: adc
                        ).FirstOrDefault();
                        
                        var registrantByGender = Registrant.Load(
                            new Hashtable()
                            {
                                {"ID", student.RegistrantId},
                                {"GenderId", student.Gender},
                            },
                            msgs: amsgs,
                            dc: adc
                        ).FirstOrDefault();
                        
                        var answerByPostalCode = FormQuestionAnswer.Load(
                            new Hashtable()
                            {
                                {"RegistrantId", student.RegistrantId},
                                {"QuestionText", "Zip Code"},
                                {"QuestionDataTypeId", (int) QuestionDataType.RegistrantPostalCode},
                                {"Text", student.ZipCode},
                            },
                            msgs: amsgs,
                            dc: adc
                        ).FirstOrDefault();

                        var answerByAddress = FormQuestionAnswer.Load(
                            new Hashtable()
                            {
                                {"RegistrantId", student.RegistrantId},
                                {"QuestionText", "Street Address"},
                                {"QuestionDataTypeId", (int) QuestionDataType.RegistrantAddress1},
                                {"Text", student.Address},
                            },
                            msgs: amsgs,
                            dc: adc
                        ).FirstOrDefault();

                        if (answerByDob.IsNotEmpty())
                            matchedColumns = string.Concat(matchedColumns, $", {student.DOB}");
                        
                        if (registrantByGender.IsNotEmpty())
                            matchedColumns = string.Concat(matchedColumns, $", {student.Gender}");
                        
                        if (answerByPostalCode.IsNotEmpty())
                            matchedColumns = string.Concat(matchedColumns, $", {student.ZipCode}");

                        if (answerByAddress.IsNotEmpty())
                            matchedColumns = string.Concat(matchedColumns, $", {student.Address}");

                        var sqlInsert = $"insert into StudentRegistrant(RegistrantId,StudentId,MatchedColumns) " +
                                        $"values('{student.RegistrantId}','{student.ID}','{matchedColumns}')";
                        
                        amsgs.AddRange(dq.ExecuteSql(sqlInsert));
                    }
                });
            },
            (smsgs) => msgs.AddRange(smsgs),
            (fmsgs) => { msgs.AddRange(fmsgs); },
            (emsgs) => { AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs); },
            dc
        );
    }
}