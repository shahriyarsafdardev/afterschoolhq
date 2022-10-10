Public Enum DALSaveType
    Insert
    Update
End Enum

Public Enum GenderType
    NotSet = 0
    Male
    Female
    Other
End Enum

Public Enum Month
    January = 1
    February = 2
    March = 3
    April = 4
    May = 5
    June = 6
    July = 7
    August = 8
    September = 9
    October = 10
    November = 11
    December = 12
End Enum

Public Enum MessageType
	[Error]
	Warning
	Confirmation
    Success
    NotSet
    UserError
    Notice
    ApiUsageError
    ApiError
End Enum

Public Enum MessageOutputType
    Html = 1
    Text
    Json
End Enum

Public Enum Priority
    High = 1
    Normal = 2
    Low = 3
End Enum

Namespace DataAccess_OLD
    Public Enum DatabaseCriteriaOperatorEnum
        Equals
        NotEqualTo
        DBValueIsLessThanParameter
        DBValueIsGreaterThanParameter
        DBValueIsLessThanOrEqualToParameter
        DBValueIsGreaterThanOrEqualToParameter
        [Like]
        LikeLeft
        LikeRight
        LikeWordMatch
        LikeWordMatchLeft
        LikeWordMatchRight
        [In]
        [Is]
        [IsNot]
    End Enum

    Public Enum DatabaseJoinTypeEnum
        LeftOuterJoin
        RightOuterJoin
        InnerJoin
        FullOuterJoin
    End Enum
End Namespace

