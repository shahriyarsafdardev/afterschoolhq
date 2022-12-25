Imports System.Data
Imports System.IO
Imports BasicLibrary.DataAccess

<Serializable>
Public Class BaseObject
    <NonSerialized> Private _baseObjectMap As DatabaseParameters

    Protected Property BaseObjectMap As DatabaseParameters
        Get
            Return _baseObjectMap
        End Get
        Set
            _baseObjectMap = value
        End Set
    End Property

    <NonSerialized> Private _BaseDatabaseQuery As DatabaseQuery

    Protected Property BaseDatabaseQuery As DatabaseQuery
        Get
            Return _BaseDatabaseQuery
        End Get
        Set
            _BaseDatabaseQuery = value
        End Set
    End Property

    <NonSerialized> Private _DatabaseTableName As String

    Protected Property DatabaseTableName As String
        Get
            Return _DatabaseTableName
        End Get
        Set
            _DatabaseTableName = value
        End Set
    End Property

    <NonSerialized> Private _PrimaryKeyFieldName As String

    Protected Property PrimaryKeyFieldName As String
        Get
            Return _PrimaryKeyFieldName
        End Get
        Set
            _PrimaryKeyFieldName = value
        End Set
    End Property

    <NonSerialized> Private _ObjectConverter As Converter(Of DataRow, Object)

    Protected Property ObjectConverter As Converter(Of DataRow, Object)
        Get
            Return _ObjectConverter
        End Get
        Set
            _ObjectConverter = value
        End Set
    End Property

    Private _originalDataValues As DatabaseParameterValues

    Protected ReadOnly Property OriginalDataValues As DatabaseParameterValues
        Get
            Return _originalDataValues
        End Get
    End Property

    Private _modifiedDataValues As DatabaseParameterValues

    Protected ReadOnly Property ModifiedDataValues As DatabaseParameterValues
        Get
            Return _modifiedDataValues
        End Get
    End Property

    Protected ReadOnly Property ODV As DatabaseParameterValues
        Get
            Return OriginalDataValues
        End Get
    End Property

    Protected ReadOnly Property MDV As DatabaseParameterValues
        Get
            Return ModifiedDataValues
        End Get
    End Property

    <NonSerialized> Private _Prop As DatabaseParameterDictionary

    Public ReadOnly Property Prop As DatabaseParameterDictionary
        Get
            If _Prop Is Nothing Then
                _Prop = New DatabaseParameterDictionary(ModifiedDataValues)
            End If
            Return _Prop
        End Get
    End Property

    Public Sub New()
        MyBase.New()
        Me.ResetDataValues()
    End Sub

    Protected Sub ResetDataValues()
        _originalDataValues = New DatabaseParameterValues()
        _modifiedDataValues = New DatabaseParameterValues()
    End Sub

    Protected Sub CopyDataValues(originalDataValues As DatabaseParameterValues,
                                 modifiedDataValues As DatabaseParameterValues)
        _originalDataValues = originalDataValues.Clone()

        For i = 0 To _originalDataValues.Count - 1
            _originalDataValues(i).Value = Nothing
        Next

        _modifiedDataValues = modifiedDataValues.Clone()
    End Sub

    Protected Sub ResetObjectAfterSave()
        _originalDataValues = _modifiedDataValues.Clone()
    End Sub

    Public Function CompareObjectProperties (Of T As BaseObject)(ObjectMap As DatabaseParameters, obj As T) _
        As SystemMessages

        Dim msgs As New SystemMessages
        Dim isMatch = True

        For Each param As DatabaseParameter In ObjectMap
            isMatch = True

            If Me.GetPropertyValue(param.PropertyName) = obj.GetPropertyValue(param.PropertyName) Then

                If _
                    Me.GetPropertyValue(param.PropertyName) IsNot Nothing AndAlso
                    (Me.GetPropertyValue(param.PropertyName).GetType.IsValueType Or
                     Me.GetPropertyValue(param.PropertyName).GetType Is GetType(String)) Then
                    If Me.GetPropertyValue(param.PropertyName) <> obj.GetPropertyValue(param.PropertyName) Then
                        isMatch = False
                    Else
                        Dim errCount = 0
                        Try
                            Me.GetPropertyValue(param.PropertyName).ToString()
                        Catch ex As Exception
                            errCount += 1
                        End Try
                        Try
                            obj.GetPropertyValue(param.PropertyName).ToString()
                        Catch ex As Exception
                            errCount += 1
                        End Try
                        If errCount = 1 Then
                            isMatch = False
                        End If
                    End If
                Else
                    If Me.GetPropertyValue(param.PropertyName) IsNot obj.GetPropertyValue(param.PropertyName) Then
                        isMatch = False
                    End If
                End If
            End If

            If (isMatch = False) Then
                msgs.Add(param.PropertyName.ToString & " does not match", MessageType.Error)
            End If
        Next
        Return msgs
    End Function

    Protected Sub Config(objMap As DatabaseParameters, dq As DatabaseQuery, primaryKeyFieldName As String)
        Config(objMap, dq, dq.Joins.FromTableName, primaryKeyFieldName, Nothing)
    End Sub

    Protected Sub Config(objMap As DatabaseParameters, dq As DatabaseQuery, databaseTableName As String,
                         primaryKeyFieldName As String)
        Config(objMap, dq, databaseTableName, primaryKeyFieldName, Nothing)
    End Sub

    Protected Sub Config(objMap As DatabaseParameters, dq As DatabaseQuery, databaseTableName As String,
                         primaryKeyFieldName As String, converter As Converter(Of DataRow, Object))
        Me.BaseObjectMap = objMap
        Me.BaseDatabaseQuery = dq
        Me.DatabaseTableName = databaseTableName
        Me.PrimaryKeyFieldName = primaryKeyFieldName
        Me.ObjectConverter = converter
    End Sub

    Protected Sub LoadObjectValues(objMap As DatabaseParameters)
        LoadObjectValues(objMap, Nothing)
    End Sub

    Protected Sub LoadObjectValues(objMap As DatabaseParameters, dataRow As DataRow)
        Me.OriginalDataValues.Clear()
        Me.ModifiedDataValues.Clear()
        For j = 0 To objMap.Count - 1
            If _
                dataRow Is Nothing OrElse dataRow.Table.Columns.Contains(objMap(j).PropertyName) = False OrElse
                dataRow(objMap(j).PropertyName) Is Nothing OrElse dataRow(objMap(j).PropertyName) Is DBNull.Value Then
                Me.OriginalDataValues.Add(objMap(j), Nothing)
                Me.ModifiedDataValues.Add(objMap(j), Nothing)
            Else
                Me.OriginalDataValues.Add(objMap(j), dataRow(objMap(j).PropertyName))
                Me.ModifiedDataValues.Add(objMap(j), dataRow(objMap(j).PropertyName))
            End If
        Next
    End Sub

    Protected Function GetModifiedValues() As DatabaseParameterValues
        Dim temp As New DatabaseParameterValues()
        For i = 0 To OriginalDataValues.Count - 1
            Dim doAdd = False
            If _
                ModifiedDataValues(i).Value IsNot Nothing AndAlso
                (ModifiedDataValues(i).Value.GetType.IsValueType Or
                 ModifiedDataValues(i).Value.GetType Is GetType(String)) Then
                If OriginalDataValues(i).Value <> ModifiedDataValues(i).Value Then
                    doAdd = True
                Else
                    Dim errCount = 0

                    If OriginalDataValues(i) Is Nothing OrElse OriginalDataValues(i).Value Is Nothing Then
                        errCount += 1
                    End If

                    If ModifiedDataValues(i) Is Nothing OrElse ModifiedDataValues(i).Value Is Nothing Then
                        errCount += 1
                    End If

                    If errCount = 1 Then
                        doAdd = True
                    End If
                End If
            Else
                If OriginalDataValues(i).Value IsNot ModifiedDataValues(i).Value Then
                    doAdd = True
                End If
            End If

            If doAdd Then
                temp.Add(ModifiedDataValues(i))
            End If
        Next
        Return temp
    End Function

    Public Shared ReadOnly Property DatabaseQuery(om As DatabaseParameters) As DatabaseQuery
        Get
            Dim _databaseQuery As New DatabaseQuery()
            _databaseQuery.Connection = New DatabaseConnection(Utilities.ConnectionString)
            _databaseQuery.SelectParameters.AddRange(om)
            Return _databaseQuery
        End Get
    End Property

    Public Shared ReadOnly Property DatabaseQuery(om As DatabaseParameters, dc As DatabaseConnection) As DatabaseQuery
        Get
            Dim _databaseQuery As New DatabaseQuery()
            _databaseQuery.Connection = dc
            _databaseQuery.SelectParameters.AddRange(om)
            Return _databaseQuery
        End Get
    End Property

    Public Shared ReadOnly Property DatabaseQuery(om As DatabaseParameters, fromTableName As String) As DatabaseQuery
        Get
            Dim _databaseQuery As New DatabaseQuery()
            _databaseQuery.Connection = New DatabaseConnection(Utilities.ConnectionString)
            _databaseQuery.SelectParameters.AddRange(om)
            _databaseQuery.Joins.FromTableName = fromTableName
            Return _databaseQuery
        End Get
    End Property


    Public Function GetPropertyValue(name As String) As Object
        Return ModifiedDataValues(name).Value
    End Function

    Public Sub SetProperyValue(name As String, value As Object)
        ModifiedDataValues(name).Value = value
    End Sub

    Public Function Add() As SystemMessages
        Return Add(Nothing)
    End Function

    Public Function Add(dc As DatabaseConnection) As SystemMessages
        Dim values As DatabaseParameterValues
        Dim msgs As New SystemMessages()
        Me.BaseDatabaseQuery.Connection = dc

        values = Me.GetModifiedValues()

        If values.Count > 0 Then

            Me.BaseDatabaseQuery.InsertParameters.Clear()
            Me.BaseDatabaseQuery.InsertParameters.AddRange(values)
            Dim newId As Integer = Me.BaseDatabaseQuery.Insert(Me.DatabaseTableName, msgs)
            If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                Me.ResetObjectAfterSave()
            End If

            If newId > 0 And PrimaryKeyFieldName.IsEmpty = False Then
                Me.MDV(Me.PrimaryKeyFieldName).Value = newId
            End If

        End If

        Return msgs
    End Function

    Protected Shared Function BaseLoad(criterias As Hashtable, ObjectMap As DatabaseParameters,
                                       DatabaseQuery As DatabaseQuery) As DataTable
        Return BaseLoad(criterias, ObjectMap, DatabaseQuery, Nothing)
    End Function

    Protected Shared Function BaseLoad(criterias As Hashtable, ObjectMap As DatabaseParameters,
                                       DatabaseQuery As DatabaseQuery, msgs As SystemMessages) As DataTable

        If msgs Is Nothing Then
            msgs = New SystemMessages()
        End If

        If criterias IsNot Nothing Then
            Dim criterias2 As New DatabaseCriterias
            For i = 0 To criterias.Count - 1
                If criterias.Values(i) Is DBNull.Value Then
                    criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i),
                                                        DatabaseCriteriaOperatorEnum.Is))
                    DatabaseQuery.UseParameterizedQuery = False
                ElseIf _
                    criterias.Values(i).GetType IsNot GetType(String) AndAlso
                    TryCast(criterias.Values(i), IEnumerable) IsNot Nothing Then
                    criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i),
                                                        DatabaseCriteriaOperatorEnum.In))
                Else
                    criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i)))
                End If
            Next

            DatabaseQuery.Criterias.Clear()
            DatabaseQuery.Criterias.AddRange(criterias2)
        End If

        Return DatabaseQuery.Select(msgs)
    End Function
    Protected Shared Function BaseLoad(criterias As Hashtable, orderBys As List(Of String), ObjectMap As DatabaseParameters,
                                       DatabaseQuery As DatabaseQuery, msgs As SystemMessages) As DataTable

        If msgs Is Nothing Then
            msgs = New SystemMessages()
        End If

        If criterias IsNot Nothing Then
            Dim criterias2 As New DatabaseCriterias
            For i = 0 To criterias.Count - 1
                If criterias.Values(i) Is DBNull.Value Then
                    criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i),
                                                        DatabaseCriteriaOperatorEnum.Is))
                    DatabaseQuery.UseParameterizedQuery = False
                ElseIf _
                    criterias.Values(i).GetType IsNot GetType(String) AndAlso
                    TryCast(criterias.Values(i), IEnumerable) IsNot Nothing Then
                    criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i),
                                                        DatabaseCriteriaOperatorEnum.In))
                Else
                    criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i)))
                End If
            Next

            DatabaseQuery.Criterias.Clear()
            DatabaseQuery.Criterias.AddRange(criterias2)
        End If

        If orderBys IsNot Nothing Then
            Dim orderBys2 As New DatabaseOrderBys
            For i = 0 To orderBys.Count - 1

                orderBys2.Add(New DatabaseOrderBy(orderBys(i)))

            Next
        End If

        Return DatabaseQuery.Select(msgs)
    End Function

    Protected Shared Function BaseGetCount(criterias As Hashtable, ObjectMap As DatabaseParameters,
                                           DatabaseQuery As DatabaseQuery) As Integer
        Return BaseGetCount(criterias, ObjectMap, DatabaseQuery, Nothing)
    End Function

    Protected Shared Function BaseGetCount(criterias As Hashtable, ObjectMap As DatabaseParameters,
                                           DatabaseQuery As DatabaseQuery, msgs As SystemMessages) As Integer

        If msgs Is Nothing Then
            msgs = New SystemMessages()
        End If

        If criterias IsNot Nothing Then
            Dim criterias2 As New DatabaseCriterias
            For i = 0 To criterias.Count - 1
                If criterias.Values(i) Is DBNull.Value Then
                    criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i),
                                                        DatabaseCriteriaOperatorEnum.Is))
                    DatabaseQuery.UseParameterizedQuery = False
                Else
                    criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i)))
                End If
            Next

            DatabaseQuery.Criterias.Clear()
            DatabaseQuery.Criterias.AddRange(criterias2)
        End If

        Return DatabaseQuery.GetCount(msgs)
    End Function

    Public Function Update() As SystemMessages
        Return Update(New Hashtable(), Nothing)
    End Function

    Public Function Update(criterias As Hashtable) As SystemMessages
        Return Update(criterias, Nothing)
    End Function

    Public Function Update(criterias As Hashtable, dc As DatabaseConnection) As SystemMessages
        Dim values As DatabaseParameterValues
        Dim msgs As New SystemMessages()
        Me.BaseDatabaseQuery.Connection = dc

        Dim criterias2 As New DatabaseCriterias

        If PrimaryKeyFieldName.IsEmpty = False Then
            criterias2.Add(BaseObjectMap(PrimaryKeyFieldName), MDV(PrimaryKeyFieldName).Value)
        End If

        If criterias IsNot Nothing Then
            For i = 0 To criterias.Count - 1
                If criterias.Values(i) Is DBNull.Value Then
                    criterias2.Add(New DatabaseCriteria(BaseObjectMap(criterias.Keys(i)), criterias.Values(i),
                                                        DatabaseCriteriaOperatorEnum.Is))
                Else
                    criterias2.Add(New DatabaseCriteria(BaseObjectMap(criterias.Keys(i)), criterias.Values(i)))
                End If
            Next
        End If

        values = Me.GetModifiedValues()

        If values.Count > 0 Then
            Me.BaseDatabaseQuery.UpdateParameters.Clear()
            Me.BaseDatabaseQuery.UpdateParameters.AddRange(values)
            Me.BaseDatabaseQuery.Criterias.Clear()
            Me.BaseDatabaseQuery.Criterias.AddRange(criterias2)
            Me.BaseDatabaseQuery.Update(Me.DatabaseTableName, msgs)
            If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                Me.ResetObjectAfterSave()
            End If

        End If

        Return msgs
    End Function

    Protected Shared Function _Create(dq As DatabaseQuery, dc As DatabaseConnection, ObjectMap As DatabaseParameters) _
        As SystemMessages
        Dim msgs As New SystemMessages
        dq.Connection = dc
        dq.Create(msgs, ObjectMap)
        Return msgs
    End Function

    Protected Shared Function _Alter(dq As DatabaseQuery, dc As DatabaseConnection, ObjectMap As DatabaseParameters) _
        As SystemMessages
        Dim msgs As New SystemMessages
        dq.Connection = dc
        dq.Alter(msgs, ObjectMap)
        Return msgs
    End Function

    Public Overloads Function Delete() As SystemMessages
        Return Delete(New Hashtable(), Nothing)
    End Function

    Public Overloads Function Delete(criterias As Hashtable) As SystemMessages
        Return Delete(criterias, Nothing)
    End Function

    Public Overloads Function Delete(criterias As Hashtable, dc As DatabaseConnection) As SystemMessages
        Dim values As DatabaseParameterValues
        Dim msgs As New SystemMessages()
        Me.BaseDatabaseQuery.Connection = dc

        Dim criterias2 As New DatabaseCriterias

        If PrimaryKeyFieldName.IsEmpty = False Then
            criterias2.Add(BaseObjectMap(PrimaryKeyFieldName), MDV(PrimaryKeyFieldName).Value)
        End If

        If criterias IsNot Nothing Then
            For i = 0 To criterias.Count - 1
                If criterias.Values(i) Is DBNull.Value Then
                    criterias2.Add(New DatabaseCriteria(BaseObjectMap(criterias.Keys(i)), criterias.Values(i),
                                                        DatabaseCriteriaOperatorEnum.Is))
                Else
                    criterias2.Add(New DatabaseCriteria(BaseObjectMap(criterias.Keys(i)), criterias.Values(i)))
                End If
            Next
        End If

        Me.BaseDatabaseQuery.Criterias.Clear()
        Me.BaseDatabaseQuery.Criterias.AddRange(criterias2)
        Me.BaseDatabaseQuery.Delete(Me.DatabaseTableName, msgs)

        Return msgs
    End Function

    Public Shared Function Insert(insertValues As Hashtable, ObjectMap As DatabaseParameters) As SystemMessages
        Return Insert(insertValues, ObjectMap, Nothing)
    End Function

    Public Shared Function Insert(insertValues As Hashtable, ObjectMap As DatabaseParameters, dc As DatabaseConnection) _
        As SystemMessages
        Dim values As New DatabaseParameterValues
        For i = 0 To insertValues.Count - 1
            values.Add(New DatabaseParameterValue(ObjectMap(insertValues.Keys(i)), insertValues.Values(i)))
        Next
        Return Insert(values, dc)
    End Function

    Public Shared Function Insert(insertValues As DatabaseParameterValues) As SystemMessages
        Return Insert(insertValues, Nothing)
    End Function

    Public Shared Function Insert(insertValues As DatabaseParameterValues, dc As DatabaseConnection) As SystemMessages
        Dim msgs As New SystemMessages()
        Dim dq As New DatabaseQuery()
        dq.Connection = dc
        dq.InsertParameters.AddRange(insertValues)
        dq.Insert(insertValues(0).Parameter.TableName, msgs)
        Return msgs
    End Function

    Public Shared Function Update(updateValues As Hashtable, criterias As Hashtable, ObjectMap As DatabaseParameters) _
        As SystemMessages
        Return Update(updateValues, criterias, ObjectMap, Nothing)
    End Function

    Public Shared Function Update(updateValues As Hashtable, criterias As Hashtable, ObjectMap As DatabaseParameters,
                                  dc As DatabaseConnection) As SystemMessages
        Dim values As New DatabaseParameterValues
        Dim criterias2 As New DatabaseCriterias
        For i = 0 To updateValues.Count - 1
            values.Add(New DatabaseParameterValue(ObjectMap(updateValues.Keys(i)), updateValues.Values(i)))
        Next
        For i = 0 To criterias.Count - 1
            If criterias.Values(i) Is DBNull.Value Then
                criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i),
                                                    DatabaseCriteriaOperatorEnum.Is))
            Else
                criterias2.Add(New DatabaseCriteria(ObjectMap(criterias.Keys(i)), criterias.Values(i)))
            End If
        Next
        Return Update(values, criterias2, dc)
    End Function

    Public Shared Function Update(updateValues As DatabaseParameterValues, criterias As DatabaseCriterias) _
        As SystemMessages
        Return Update(updateValues, criterias, Nothing)
    End Function

    Public Shared Function Update(updateValues As DatabaseParameterValues, criterias As DatabaseCriterias,
                                  dc As DatabaseConnection) As SystemMessages
        Dim msgs As New SystemMessages()
        Dim dq As New DatabaseQuery()
        dq.Connection = dc
        dq.UpdateParameters.AddRange(updateValues)
        dq.Criterias.AddRange(criterias)

        If dq.Criterias.Where(Function(x) x.Value Is DBNull.Value).Count > 0 Then
            dq.UseParameterizedQuery = False
        End If

        dq.Update(updateValues(0).Parameter.TableName, msgs)
        Return msgs
    End Function
End Class

<Serializable>
Public MustInherit Class BasicIDObject
    Inherits BaseObject

    Private _id As Integer?

    Public Property ID As Integer?
        Get
            If ModifiedDataValues("ID") IsNot Nothing AndAlso ModifiedDataValues("ID").Value IsNot Nothing Then
                Return ModifiedDataValues("ID").Value
            End If
            Return _id
        End Get
        Set
            _id = value
            If ModifiedDataValues("ID") IsNot Nothing Then ModifiedDataValues("ID").Value = value
        End Set
    End Property

    Public Sub ResetID()
        Me.ID = New Integer ?
    End Sub

    Public Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = New DatabaseParameters
            _databaseParameters.Add(New DatabaseParameter(className, "ID", tableName, SqlDbType.Int, True, True, True))
            Return _databaseParameters
        End Get
    End Property

    Protected Overridable Function _Save2(dq As DatabaseQuery, dc As DatabaseConnection, ObjectMap As DatabaseParameters,
                                          saveTableName As String) As SystemMessages
        Return _Save2(dq, dc, ObjectMap, saveTableName, SaveType.NotSet)
    End Function

    Protected Overridable Function _Save2(dq As DatabaseQuery, dc As DatabaseConnection, ObjectMap As DatabaseParameters,
                                          saveTableName As String, setAllProperties As Boolean) As SystemMessages
        Return _Save2(dq, dc, ObjectMap, saveTableName, SaveType.NotSet, setAllProperties)
    End Function

    Protected Overridable Function _Save2(dq As DatabaseQuery, dc As DatabaseConnection, ObjectMap As DatabaseParameters,
                                          saveTableName As String, saveType As SaveType, setAllProperties As Boolean) _
        As SystemMessages
        Dim values As DatabaseParameterValues
        Dim msgs As New SystemMessages()
        dq.Connection = dc

        If setAllProperties = True Then
            values = MDV
        Else
            values = GetModifiedValues()
        End If

        If values.Count > 0 Then

            If saveType = SaveType.NotSet Then
                If ID.HasValue Then
                    saveType = SaveType.Update
                Else
                    saveType = SaveType.Insert
                End If
            End If

            If saveType = SaveType.Update Then
                dq.UpdateParameters.AddRange(values)
                dq.Criterias.Add(ObjectMap("id"), ID.Value)
                dq.Update(saveTableName, msgs)
            Else
                dq.InsertParameters.AddRange(values)
                Dim newId As Integer = dq.Insert(saveTableName, msgs)
                If ID.HasValue = False Then
                    Me.ID = newId
                End If
            End If

            If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                Me.ResetObjectAfterSave()
            End If

        End If

        Return msgs
    End Function

    Protected Overridable Function _Delete2(dq As DatabaseQuery, dc As DatabaseConnection,
                                            ObjectMap As DatabaseParameters, deleteTableName As String) _
        As SystemMessages
        Dim msgs As New SystemMessages()
        dq.Connection = dc
        dq.Criterias.Add(ObjectMap("id"), ID.Value)
        dq.Delete(deleteTableName, msgs)
        'If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
        '    Me.ID = New Integer?
        'End If
        Return msgs
    End Function
    'Protected Shared Function _Create(ByVal dq As DataAccess.DatabaseQuery, ByVal dc As DataAccess.DatabaseConnection, ByVal ObjectMap As DataAccess.DatabaseParameters) As SystemMessages
    '    Dim msgs As New SystemMessages
    '    dq.Connection = dc
    '    dq.Create(msgs, ObjectMap)
    '    Return msgs
    'End Function

    'Protected Shared Function _Alter(ByVal dq As DataAccess.DatabaseQuery, ByVal dc As DataAccess.DatabaseConnection, ByVal ObjectMap As DataAccess.DatabaseParameters) As SystemMessages
    '    Dim msgs As New SystemMessages
    '    dq.Connection = dc
    '    dq.Alter(msgs, ObjectMap)
    '    Return msgs
    'End Function


    Protected Shared Function GetNextId(dq As DatabaseQuery, dc As DatabaseConnection, idParam As DatabaseParameter) _
        As Integer
        dq.Connection = dc
        idParam.Function = "max(" & DataAccess.DatabaseQuery.FunctionReplaceValue & ")"

        dq.SelectParameters.Clear()
        dq.SelectParameters.Add(idParam)

        Dim results As String = dq.Select().Rows(0)(0).ToString
        If results.IsNumber Then
            Return (results.ToInteger + 1)
        Else
            Return 1
        End If
    End Function
End Class

<Serializable>
Public MustInherit Class BasicObject
    Inherits BasicIDObject

    Private _createDate As Date?

    Public Property CreateDate As Date?
        Get
            If _
                ModifiedDataValues("CreateDate") IsNot Nothing AndAlso
                ModifiedDataValues("CreateDate").Value IsNot Nothing Then
                Return ModifiedDataValues("CreateDate").Value
            End If
            Return _createDate
        End Get
        Set
            _createDate = value
            If ModifiedDataValues("CreateDate") IsNot Nothing Then ModifiedDataValues("CreateDate").Value = value
        End Set
    End Property

    Private _createdBy As Integer?

    Public Property CreatedBy As Integer?
        Get
            If ModifiedDataValues("CreatedBy") IsNot Nothing AndAlso ModifiedDataValues("CreatedBy").Value IsNot Nothing _
                Then
                Return ModifiedDataValues("CreatedBy").Value
            End If
            Return _createdBy
        End Get
        Set
            _createdBy = value
            If ModifiedDataValues("CreatedBy") IsNot Nothing Then ModifiedDataValues("CreatedBy").Value = value
        End Set
    End Property

    Public Sub New()
        Me.CreateDate = DateTime.UtcNow
    End Sub

    Public Shared Shadows ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Shared Shadows ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasicIDObject.BaseObjectMap(className, tableName)
            _databaseParameters.Add(New DatabaseParameter(className, "CreateDate", tableName, SqlDbType.DateTime))
            _databaseParameters.Add(New DatabaseParameter(className, "CreatedBy", tableName, SqlDbType.Int))
            Return _databaseParameters
        End Get
    End Property
End Class

<Serializable>
Public MustInherit Class BasicObjectWithModified
    Inherits BasicObject

    Private _lastModifiedDate As Date?

    Public Property LastModifiedDate As Date?
        Get
            If _
                ModifiedDataValues("LastModifiedDate") IsNot Nothing AndAlso
                ModifiedDataValues("LastModifiedDate").Value IsNot Nothing Then
                Return ModifiedDataValues("LastModifiedDate").Value
            End If
            Return _lastModifiedDate
        End Get
        Set
            _lastModifiedDate = value
            If ModifiedDataValues("LastModifiedDate") IsNot Nothing Then _
                ModifiedDataValues("LastModifiedDate").Value = value
        End Set
    End Property

    Private _lastModifiedBy As Integer?

    Public Property LastModifiedBy As Integer?
        Get
            If _
                ModifiedDataValues("LastModifiedBy") IsNot Nothing AndAlso
                ModifiedDataValues("LastModifiedBy").Value IsNot Nothing Then
                Return ModifiedDataValues("LastModifiedBy").Value
            End If
            Return _lastModifiedBy
        End Get
        Set
            _lastModifiedBy = value
            If ModifiedDataValues("LastModifiedBy") IsNot Nothing Then _
                ModifiedDataValues("LastModifiedBy").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasicObject.BaseObjectMap(className, tableName)
            _databaseParameters.Add(New DatabaseParameter(className, "LastModifiedDate", tableName, SqlDbType.DateTime))
            _databaseParameters.Add(New DatabaseParameter(className, "LastModifiedBy", tableName, SqlDbType.Int))
            Return _databaseParameters
        End Get
    End Property

    Public Sub New()
        Me.LastModifiedDate = DateTime.UtcNow
    End Sub
End Class

<Serializable>
Public MustInherit Class BasicLookUpObject
    Inherits BasicObjectWithModified

    Private _name As String

    Public Property Name As String
        Get
            If ModifiedDataValues("Name") IsNot Nothing AndAlso ModifiedDataValues("Name").Value IsNot Nothing Then
                Return ModifiedDataValues("Name").Value
            End If
            Return _name
        End Get
        Set
            _name = value
            If ModifiedDataValues("Name") IsNot Nothing Then ModifiedDataValues("Name").Value = value
        End Set
    End Property

    Private _description As String

    Public Property Description As String
        Get
            If _
                ModifiedDataValues("Description") IsNot Nothing AndAlso
                ModifiedDataValues("Description").Value IsNot Nothing Then
                Return ModifiedDataValues("Description").Value
            End If
            Return _description
        End Get
        Set
            _description = value
            If ModifiedDataValues("Description") IsNot Nothing Then ModifiedDataValues("Description").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasicObjectWithModified.BaseObjectMap(className, tableName)
            _databaseParameters.Add(New DatabaseParameter(className, "Name", tableName, SqlDbType.NVarChar, "75"))
            _databaseParameters.Add(New DatabaseParameter(className, "Description", tableName, SqlDbType.NVarChar, "400"))
            Return _databaseParameters
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class

<Serializable>
Public MustInherit Class BaseAddress
    Inherits BasicObjectWithModified

    Public Property Address1 As String
        Get
            Return MDV("Address1").Value
        End Get
        Set
            MDV("Address1").Value = value
        End Set
    End Property

    Public Property Address2 As String
        Get
            Return MDV("Address2").Value
        End Get
        Set
            MDV("Address2").Value = value
        End Set
    End Property

    Public Property Neighborhood As String
        Get
            If MDV("Neighborhood").IsNotEmpty() Then
                Return MDV("Neighborhood").Value
            End If

            Return Nothing
        End Get
        Set
            MDV("Neighborhood").Value = value
        End Set
    End Property

    Public Property City As String
        Get
            Return MDV("City").Value
        End Get
        Set
            MDV("City").Value = value
        End Set
    End Property

    Public Property County As String
        Get
            If MDV("County").IsNotEmpty() Then
                Return MDV("County").Value
            End If

            Return Nothing
        End Get
        Set
            MDV("County").Value = value
        End Set
    End Property

    Public Property State As String
        Get
            Return MDV("State").Value
        End Get
        Set
            MDV("State").Value = value
        End Set
    End Property

    Public Property PostalCode As String
        Get
            Return MDV("PostalCode").Value
        End Get
        Set
            MDV("PostalCode").Value = value
        End Set
    End Property

    Public Property Country As String
        Get
            Return MDV("Country").Value
        End Get
        Set
            MDV("Country").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasicObjectWithModified.BaseObjectMap(className, tableName)
            _databaseParameters.Add(New DatabaseParameter(className, "Address1", tableName, SqlDbType.NVarChar, "100"))
            _databaseParameters.Add(New DatabaseParameter(className, "Address2", tableName, SqlDbType.NVarChar, "100"))
            _databaseParameters.Add(New DatabaseParameter(className, "City", tableName, SqlDbType.NVarChar, "100"))
            _databaseParameters.Add(New DatabaseParameter(className, "State", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "PostalCode", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Country", tableName, SqlDbType.NVarChar, "100"))
            Return _databaseParameters
        End Get
    End Property

    Public Shared Function GetGoogleMapsLink(address1 As String, address2 As String, city As String, county As String,
                                             state As String, postalCode As String, country As String) As String
        Dim address As String =
                FormatAddress(address1, address2, city, county, state, postalCode, country).Replace(vbCrLf, " ").Replace(
                    " ", "+")
        Return "https://www.google.com/maps/place/" + address
    End Function

    Public Overridable Function GetGoogleMapsLink() As String
        Return GetGoogleMapsLink(Address1, Address2, City, County, State, PostalCode, Country)
    End Function

    Public Overridable Function GetAddress() As String
        Return FormatAddress()
    End Function

    Public Overridable Function FormatAddress(Optional ByVal useLineBreaks As Boolean = True) As String
        Return FormatAddress(Address1, Address2, City, County, State, PostalCode, Country, useLineBreaks)
    End Function

    Public Shared Function FormatAddress(address1 As String, address2 As String, city As String, county As String,
                                         state As String, postalCode As String, country As String,
                                         Optional ByVal useLineBreaks As Boolean = True) As String

        Dim address = ""

        If address1.IsEmpty() = False Then
            address &= address1
        End If

        If address2.IsEmpty() = False Then
            If address.IsEmpty() = False Then
                If useLineBreaks Then
                    address &= vbCrLf
                Else
                    address &= " "
                End If
            End If
            address &= address2
        End If

        Dim cityLine = FormatCityLine(city, county, state, postalCode, useLineBreaks)

        If cityLine.IsNotEmpty() Then

            If address.IsEmpty() = False Then
                If useLineBreaks Then
                    address &= vbCrLf
                Else
                    address &= ", "
                End If
            End If

            address &= cityLine
        End If

        If country.IsEmpty = False Then
            If address.IsEmpty = False Then
                If useLineBreaks Then
                    address &= vbCrLf
                Else
                    address &= " "
                End If
            End If
            address &= country
        End If

        Return address
    End Function

    Public Shared Function FormatImpreciseAddress(neighborhood As String, city As String, county As String,
                                                  state As String, postalCode As String, country As String,
                                                  Optional ByVal useLineBreaks As Boolean = True) As String

        Dim address = ""

        If neighborhood.IsEmpty() = False Then
            If address.IsEmpty() = False Then
                If useLineBreaks Then
                    address &= vbCrLf
                Else
                    address &= ", "
                End If
            End If
            address &= neighborhood
        End If

        Dim cityLine = FormatCityLine(city, county, state, postalCode, useLineBreaks)

        If cityLine.IsNotEmpty() Then

            If address.IsEmpty() = False Then
                If useLineBreaks Then
                    address &= vbCrLf
                Else
                    address &= ", "
                End If
            End If

            address &= cityLine
        End If

        If country.IsEmpty = False Then
            If address.IsEmpty = False Then
                If useLineBreaks Then
                    address &= vbCrLf
                Else
                    address &= " "
                End If
            End If
            address &= country
        End If

        Return address
    End Function

    Private Shared Function FormatCityLine(city As String, county As String, state As String, postalCode As String,
                                           Optional ByVal useLineBreaks As Boolean = True) As String

        Dim cityLine = ""

        If city.IsEmpty() = False Or county.IsEmpty() = False Or state.IsEmpty() = False Or postalCode.IsEmpty = False _
            Then

            If city.IsEmpty() = False Then
                cityLine &= city
            End If

            If city.IsEmpty() And county.IsNotEmpty() Then
                cityLine &= county
            End If

            If city.IsNotEmpty() Or county.IsNotEmpty() Then
                If cityLine.IsEmpty() = False Then
                    cityLine &= ", "
                End If
                cityLine &= state
            End If

            If city.IsNotEmpty() Or county.IsNotEmpty() Then
                If cityLine.IsEmpty() = False Then
                    cityLine &= " "
                End If
                cityLine &= postalCode
            End If

        End If

        Return cityLine
    End Function

    Public Shared Function FormatCityStateZipCode(city As String, county As String, state As String,
                                                  postalCode As String, Optional ByVal useLineBreaks As Boolean = True) _
        As String

        Return FormatCityLine(city, county, state, postalCode, useLineBreaks)
    End Function

    Public Overrides Function ToString() As String
        Return FormatAddress(False)
    End Function
End Class

<Serializable>
Public MustInherit Class BaseLocation
    Inherits BaseAddress

    Public Property Name As String
        Get
            Return MDV("Name").Value
        End Get
        Set
            MDV("Name").Value = value
        End Set
    End Property

    Public Property Description As String
        Get
            Return MDV("Description").Value
        End Get
        Set
            MDV("Description").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BaseAddress.BaseObjectMap(className, tableName)
            _databaseParameters.Add(New DatabaseParameter(className, "Name", tableName, SqlDbType.NVarChar, "500"))
            _databaseParameters.Add(New DatabaseParameter(className, "Description", tableName, SqlDbType.NVarChar, "100"))
            Return _databaseParameters
        End Get
    End Property

    Public Overrides Function FormatAddress(Optional ByVal useLineBreaks As Boolean = True) As String

        Dim rtn = ""

        If Name.IsEmpty = False And Name <> City And Name <> County And Name <> Address1 Then
            rtn &= Name
            If useLineBreaks Then
                rtn &= vbCrLf
            Else
                rtn &= ", "
            End If
        End If

        Return rtn & MyBase.FormatAddress(useLineBreaks)
    End Function

    Public Function FormatAddressWithoutName(Optional ByVal useLineBreaks As Boolean = True) As String
        Return MyBase.FormatAddress(useLineBreaks)
    End Function

    Public Overloads Function FormatImpreciseAddress(Optional ByVal useLineBreaks As Boolean = True) As String
        Return FormatImpreciseAddress(Neighborhood, City, County, State, PostalCode, Country, useLineBreaks)
    End Function

    Public Overloads Function FormatCityStateZipcode(Optional ByVal useLineBreaks As Boolean = True) As String
        Return FormatCityStateZipCode(City, County, State, PostalCode, useLineBreaks)
    End Function

    Public Overloads Function FormatCityState(Optional ByVal useLineBreaks As Boolean = True) As String
        Return FormatCityStateZipCode(City, County, State, "", useLineBreaks)
    End Function

    Public Overrides Function ToString() As String
        Return FormatAddress(False)
    End Function
End Class

<Serializable>
Public MustInherit Class BaseCompany
    Inherits BaseAddress

    Public Property Name As String
        Get
            Return MDV("Name").Value
        End Get
        Set
            MDV("Name").Value = value
        End Set
    End Property

    Public Property Phone As String
        Get
            Return MDV("Phone").Value
        End Get
        Set
            MDV("Phone").Value = value
        End Set
    End Property

    Public Property Email As String
        Get
            Return MDV("Email").Value
        End Get
        Set
            MDV("Email").Value = value
        End Set
    End Property

    Public Property Fax As String
        Get
            Return MDV("Fax").Value
        End Get
        Set
            MDV("Fax").Value = value
        End Set
    End Property

    Public Property Website As String
        Get
            Return MDV("Website").Value
        End Get
        Set
            MDV("Website").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BaseAddress.BaseObjectMap(className, tableName)
            _databaseParameters.Add(New DatabaseParameter(className, "Name", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Phone", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Email", tableName, SqlDbType.NVarChar, "100"))
            _databaseParameters.Add(New DatabaseParameter(className, "Fax", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Website", tableName, SqlDbType.NVarChar, "100"))
            Return _databaseParameters
        End Get
    End Property

    Public Overrides Function FormatAddress(Optional ByVal useLineBreaks As Boolean = True) As String

        Dim rtn = ""

        If Name.IsEmpty = False Then
            rtn &= Name
            If useLineBreaks Then
                rtn &= vbCrLf
            Else
                rtn &= " "
            End If
        End If

        Return rtn & MyBase.FormatAddress(useLineBreaks)
    End Function

    Public Function FormatAddressWithoutName(Optional ByVal useLineBreaks As Boolean = True) As String
        Return MyBase.FormatAddress(useLineBreaks)
    End Function

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class

<Serializable>
Public MustInherit Class BasePerson
    Inherits BasicObjectWithModified

    Public Property FirstName As String
        Get
            Return MDV("FirstName").Value
        End Get
        Set
            MDV("FirstName").Value = value
        End Set
    End Property

    Public Property LastName As String
        Get
            Return MDV("LastName").Value
        End Get
        Set
            MDV("LastName").Value = value
        End Set
    End Property

    Public Property Email As String
        Get
            Return MDV("Email").Value
        End Get
        Set
            MDV("Email").Value = value
        End Set
    End Property

    Public Property GenderId As GenderType
        Get
            Return MDV("GenderId").Value
        End Get
        Set
            MDV("GenderId").Value = value
        End Set
    End Property

    Public Property Phone As String
        Get
            Return MDV("Phone").Value
        End Get
        Set
            MDV("Phone").Value = value
        End Set
    End Property

    Public Property DateOfBirth As Date?
        Get
            Return MDV("DateOfBirth").Value
        End Get
        Set
            MDV("DateOfBirth").Value = value
        End Set
    End Property

    Public ReadOnly Property DisplayName As String
        Get
            Return BuildDisplayName(FirstName, LastName).IfEmpty(Email)
        End Get
    End Property

    Public ReadOnly Property Initials As String
        Get
            Return GetInitials(FirstName, LastName)
        End Get
    End Property

    Public ReadOnly Property FullName As String
        Get
            Dim str = ""
            If LastName.IsEmpty = False Then
                str &= LastName
            End If
            If FirstName.IsEmpty = False Then
                If str.IsEmpty = False Then str &= ", "
                str &= FirstName
            End If
            Return str
        End Get
    End Property

    Public Shared Function BuildDisplayName(firstName As String, lastName As String) As String
        Dim str = ""
        If firstName.IsEmpty = False Then
            str &= firstName
        End If
        If lastName.IsEmpty = False Then
            If str.IsEmpty = False Then str &= " "
            str &= lastName
        End If

        Return str
    End Function

    Public Shared Function GetInitials(firstName As String, lastName As String) As String
        Dim str = ""
        If firstName.IsEmpty = False Then
            str &= firstName.ToCharArray()(0).ToString.ToUpper
        End If
        If lastName.IsEmpty = False Then
            str &= lastName.ToCharArray()(0).ToString.ToUpper
        End If
        Return str
    End Function

    Public Shared Function GetAbbreviation(firstName As String, lastName As String, firstNameLen As Integer?,
                                           lastNameLen As Integer?) As String

        Dim str = ""
        If firstName.IsEmpty = False Then
            If firstNameLen.HasValue AndAlso firstName.Length >= firstNameLen.Value Then
                str &= firstName.Substring(0, firstNameLen.Value)
            Else
                str &= firstName
            End If
        End If
        If lastName.IsEmpty = False Then

            If str.Length > 0 Then
                str &= " "
            End If

            If lastNameLen.HasValue AndAlso lastName.Length >= lastNameLen.Value Then
                str &= lastName.Substring(0, lastNameLen.Value)
            Else
                str &= lastName
            End If
        End If

        Return str
    End Function

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasicObjectWithModified.BaseObjectMap(className)
            _databaseParameters.Add(New DatabaseParameter(className, "FirstName", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "LastName", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Email", tableName, SqlDbType.NVarChar, "100"))
            _databaseParameters.Add(New DatabaseParameter(className, "Phone", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "DateOfBirth", tableName, SqlDbType.DateTime))
            _databaseParameters.Add(New DatabaseParameter(className, "GenderId", tableName, SqlDbType.Int))
            Return _databaseParameters
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return DisplayName.IfEmpty(Email)
    End Function
End Class

<Serializable>
Public MustInherit Class BaseWebUser
    Inherits BasePerson

    Public Property Username As String
        Get
            Return MDV("Username").Value
        End Get
        Set
            MDV("Username").Value = value
        End Set
    End Property

    Public Property Password As String
        Get
            Return MDV("Password").Value
        End Get
        Set
            MDV("Password").Value = value
        End Set
    End Property

    Public Property ImageUrl As String
        Get
            Return MDV("ImageUrl").Value
        End Get
        Set
            MDV("ImageUrl").Value = value
        End Set
    End Property

    Public Property OpenId As String
        Get
            Return MDV("OpenId").Value
        End Get
        Set
            MDV("OpenId").Value = value
        End Set
    End Property

    Public Property OpenIdProviderUrl As String
        Get
            Return MDV("OpenIdProviderUrl").Value
        End Get
        Set
            MDV("OpenIdProviderUrl").Value = value
        End Set
    End Property

    Public Property LastLogin As Date?
        Get
            Return MDV("LastLogin").Value
        End Get
        Set
            MDV("LastLogin").Value = value
        End Set
    End Property

    Public Property TimeZoneId As Double?
        Get
            Return MDV("TimeZoneId").Value
        End Get
        Set
            MDV("TimeZoneId").Value = value
        End Set
    End Property

    Public Property ProfileUrl As String
        Get
            Return MDV("ProfileUrl").Value
        End Get
        Set
            MDV("ProfileUrl").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasePerson.BaseObjectMap(className)
            _databaseParameters.Add(New DatabaseParameter(className, "Username", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Password", tableName, SqlDbType.NVarChar, "60"))
            _databaseParameters.Add(New DatabaseParameter(className, "ImageUrl", tableName, SqlDbType.NVarChar, "400"))
            _databaseParameters.Add(New DatabaseParameter(className, "OpenId", tableName, SqlDbType.NVarChar, "400"))
            _databaseParameters.Add(New DatabaseParameter(className, "OpenIdProviderUrl", tableName, SqlDbType.NVarChar,
                                                          "400"))
            _databaseParameters.Add(New DatabaseParameter(className, "LastLogin", tableName, SqlDbType.DateTime))
            _databaseParameters.Add(New DatabaseParameter(className, "TimeZoneId", tableName, SqlDbType.Float))
            _databaseParameters.Add(New DatabaseParameter(className, "ProfileUrl", tableName, SqlDbType.NVarChar, "400"))
            Return _databaseParameters
        End Get
    End Property
End Class

<Serializable>
Public MustInherit Class BaseContact
    Inherits BasePerson

    Public Property Prefix As String
        Get
            Return MDV("Prefix").Value
        End Get
        Set
            MDV("Prefix").Value = value
        End Set
    End Property

    Public Property MiddleName As String
        Get
            Return MDV("MiddleName").Value
        End Get
        Set
            MDV("MiddleName").Value = value
        End Set
    End Property

    Public Property Suffix As String
        Get
            Return MDV("Suffix").Value
        End Get
        Set
            MDV("Suffix").Value = value
        End Set
    End Property

    Public Property Website As String
        Get
            Return MDV("Website").Value
        End Get
        Set
            MDV("Website").Value = value
        End Set
    End Property

    Public Property Company As String
        Get
            Return MDV("Company").Value
        End Get
        Set
            MDV("Company").Value = value
        End Set
    End Property

    Public Property JobTitle As String
        Get
            Return MDV("JobTitle").Value
        End Get
        Set
            MDV("JobTitle").Value = value
        End Set
    End Property

    Public Property Notes As String
        Get
            Return MDV("Notes").Value
        End Get
        Set
            MDV("Notes").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasePerson.BaseObjectMap(className)
            _databaseParameters.Add(New DatabaseParameter(className, "Prefix", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "MiddleName", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Suffix", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Website", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Company", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "JobTitle", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Notes", tableName, SqlDbType.NVarChar, "400"))
            Return _databaseParameters
        End Get
    End Property
End Class

<Serializable>
Public MustInherit Class BaseContactWithAddress
    Inherits BaseContact

    Public Property Address1 As String
        Get
            Return MDV("Address1").Value
        End Get
        Set
            MDV("Address1").Value = value
        End Set
    End Property

    Public Property Address2 As String
        Get
            Return MDV("Address2").Value
        End Get
        Set
            MDV("Address2").Value = value
        End Set
    End Property

    Public Property City As String
        Get
            Return MDV("City").Value
        End Get
        Set
            MDV("City").Value = value
        End Set
    End Property

    Public Property County As String
        Get
            Return MDV("County").Value
        End Get
        Set
            MDV("County").Value = value
        End Set
    End Property

    Public Property State As String
        Get
            Return MDV("State").Value
        End Get
        Set
            MDV("State").Value = value
        End Set
    End Property

    Public Property PostalCode As String
        Get
            Return MDV("PostalCode").Value
        End Get
        Set
            MDV("PostalCode").Value = value
        End Set
    End Property

    Public Property Country As String
        Get
            Return MDV("Country").Value
        End Get
        Set
            MDV("Country").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BaseContact.BaseObjectMap(className, tableName)
            _databaseParameters.Add(New DatabaseParameter(className, "Address1", tableName, SqlDbType.NVarChar, "100"))
            _databaseParameters.Add(New DatabaseParameter(className, "Address2", tableName, SqlDbType.NVarChar, "100"))
            _databaseParameters.Add(New DatabaseParameter(className, "City", tableName, SqlDbType.NVarChar, "100"))
            _databaseParameters.Add(New DatabaseParameter(className, "State", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "PostalCode", tableName, SqlDbType.NVarChar, "50"))
            _databaseParameters.Add(New DatabaseParameter(className, "Country", tableName, SqlDbType.NVarChar, "100"))
            Return _databaseParameters
        End Get
    End Property

    Public Shared Function GetGoogleMapsLink(address1 As String, address2 As String, city As String, county As String,
                                             state As String, postalCode As String, country As String) As String
        Dim address As String =
                BaseAddress.FormatAddress(address1, address2, city, county, state, postalCode, country).Replace(vbCrLf,
                                                                                                                " ").
                Replace(" ", "+")
        Return "https://www.google.com/maps/place/" + address
    End Function

    Public Overridable Function GetGoogleMapsLink() As String
        Return GetGoogleMapsLink(Address1, Address2, City, County, State, PostalCode, Country)
    End Function

    Public Overridable Function GetAddress() As String
        Return FormatAddress()
    End Function

    Public Overridable Function FormatAddress() As String
        Return BaseAddress.FormatAddress(Address1, Address2, City, County, State, PostalCode, Country)
    End Function
End Class

<Serializable>
Public MustInherit Class BaseFile
    Inherits BasicObjectWithModified

    Public Property Filename As String
        Get
            Return MDV("Filename").Value
        End Get
        Set
            MDV("Filename").Value = value
        End Set
    End Property

    Public Property FileLocation As String
        Get
            Return MDV("FileLocation").Value
        End Get
        Set
            MDV("FileLocation").Value = value
        End Set
    End Property

    Public Property FileSizeInBytes As Integer?
        Get
            Return MDV("FileSizeInBytes").Value
        End Get
        Set
            MDV("FileSizeInBytes").Value = value
        End Set
    End Property

    Public ReadOnly Property FileExtension As String
        Get
            Return Path.GetExtension(Filename)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasicObjectWithModified.BaseObjectMap(className)
            _databaseParameters.Add(New DatabaseParameter(className, "Filename", tableName, SqlDbType.NVarChar, "400"))
            _databaseParameters.Add(New DatabaseParameter(className, "FileLocation", tableName, SqlDbType.NVarChar,
                                                          "1000"))
            _databaseParameters.Add(New DatabaseParameter(className, "FileSizeInBytes", tableName, SqlDbType.Int))
            Return _databaseParameters
        End Get
    End Property

    Public Function RemoveFromAmazon(helper As AmazonHelper) As SystemMessages
        Dim msgs = New SystemMessages()
        msgs.AddRange(helper.RemoveFile(Me.FileLocation))
        Return msgs
    End Function

    Public Function AddToAmazon(helper As AmazonHelper, folderName As String, inputStream As Stream,
                                makePublic As Boolean, Optional ByVal msgs As SystemMessages = Nothing,
                                Optional ByVal dc As DatabaseConnection = Nothing) As String

        If msgs Is Nothing Then
            msgs = New SystemMessages()
        End If

        Dim fullFilePath As String = "uploads/" + folderName + "/" + Me.ID.Value.ToString() + "-" + Filename
        msgs.AddRange(helper.AddFile(inputStream, fullFilePath, makePublic))

        Return fullFilePath
    End Function

    Public Function AddToAmazon(helper As AmazonHelper, folderName As String, filename As String, fileLocation As String,
                                makePublic As Boolean, Optional ByVal msgs As SystemMessages = Nothing,
                                Optional ByVal dc As DatabaseConnection = Nothing) As String

        If msgs Is Nothing Then
            msgs = New SystemMessages()
        End If
        Dim fullFilePath As String = "uploads/" + folderName + "/" + Me.ID.Value.ToString() + "-" + filename
        msgs.AddRange(helper.AddFile(fileLocation, fullFilePath, makePublic))

        Return fullFilePath
    End Function

    Public Overrides Function ToString() As String
        Return Filename
    End Function
End Class

<Serializable>
Public MustInherit Class BaseEvent
    Inherits BasicLookUpObject

    Public Property StartDateTime As Date?
        Get
            Return MDV("StartDateTime").Value
        End Get
        Set
            MDV("StartDateTime").Value = value
        End Set
    End Property

    Public Property EndDateTime As Date?
        Get
            Return MDV("EndDateTime").Value
        End Get
        Set
            MDV("EndDateTime").Value = value
        End Set
    End Property

    Public Property Location As String
        Get
            Return MDV("Location").Value
        End Get
        Set
            MDV("Location").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BasicLookUpObject.BaseObjectMap(className)
            _databaseParameters.Add(New DatabaseParameter(className, "StartDateTime", tableName, SqlDbType.DateTime))
            _databaseParameters.Add(New DatabaseParameter(className, "EndDateTime", tableName, SqlDbType.DateTime))
            _databaseParameters.Add(New DatabaseParameter(className, "Location", tableName, SqlDbType.NVarChar, "400"))
            Return _databaseParameters
        End Get
    End Property
End Class


<Serializable>
Public MustInherit Class BaseEventWithAddress
    Inherits BaseCompany

    Public Property StartDateTime As Date?
        Get
            Return MDV("StartDateTime").Value
        End Get
        Set
            MDV("StartDateTime").Value = value
        End Set
    End Property

    Public Property EndDateTime As Date?
        Get
            Return MDV("EndDateTime").Value
        End Get
        Set
            MDV("EndDateTime").Value = value
        End Set
    End Property

    Public Property Description As String
        Get
            Return MDV("Description").Value
        End Get
        Set
            MDV("Description").Value = value
        End Set
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String) As DatabaseParameters
        Get
            Return BaseObjectMap(className, className)
        End Get
    End Property

    Public Overloads Shared ReadOnly Property BaseObjectMap(className As String, tableName As String) _
        As DatabaseParameters
        Get
            Dim _databaseParameters As DatabaseParameters = Nothing
            _databaseParameters = BaseCompany.BaseObjectMap(className)
            _databaseParameters.Add(New DatabaseParameter(className, "StartDateTime", tableName, SqlDbType.DateTime))
            _databaseParameters.Add(New DatabaseParameter(className, "EndDateTime", tableName, SqlDbType.DateTime))
            _databaseParameters.Add(New DatabaseParameter(className, "Description", tableName, SqlDbType.NVarChar,
                                                          "1000"))
            Return _databaseParameters
        End Get
    End Property
End Class


Public Class BasicLibraryEventArgs
    Inherits EventArgs

    Private _messages As SystemMessages

    Public Property Messages As SystemMessages
        Get
            Return _messages
        End Get
        Set
            _messages = value
        End Set
    End Property


    Public Sub New()
        MyBase.New()
        Me.Messages = New SystemMessages()
    End Sub

    Public Sub New(msgs As SystemMessages)
        MyBase.New()
        Me.Messages = msgs
    End Sub
End Class

Public MustInherit Class BaseTestObject
    Protected Sub SetBaseProperties (Of T As BaseObject)(ObjectMap As DatabaseParameters, obj As T, savetype As SaveType)

        Dim d As Date
        Dim i As Integer

        If savetype = SaveType.Insert Then
            d = DateTime.UtcNow.AddMonths(- 1)
            i = 4
        Else
            d = DateTime.UtcNow
            i = 25
        End If

        For Each Param As DatabaseParameter In ObjectMap
            Select Case Param.PropertyName.ToLower
                Case "createdate"
                    obj.SetProperyValue("createdate", d)
                Case "createdby"
                    obj.SetProperyValue("createdby", i)
                Case "lastmodifieddate"
                    obj.SetProperyValue("lastmodifieddate", d)
                Case "lastmodifiedby"
                    obj.SetProperyValue("lastmodifiedby", i)
            End Select
        Next
    End Sub
End Class


