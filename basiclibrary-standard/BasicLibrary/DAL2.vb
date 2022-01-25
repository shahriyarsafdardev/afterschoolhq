Option Strict On

Imports System.Collections
Imports System.Collections.Generic
Imports System.Text
Imports System.Data
Imports Microsoft.Data.SqlClient

Namespace DataAccess
    <Serializable()> _
    Public Class DatabaseQuery
        Implements IDisposable

        Private _DebugMode As Boolean
        Public Property DebugMode() As Boolean
            Get
                Return _DebugMode
            End Get
            Set(ByVal value As Boolean)
                _DebugMode = value
            End Set
        End Property

        Private _ThrowExceptions As Boolean
        Public Property ThrowExceptions() As Boolean
            Get
                Return _ThrowExceptions
            End Get
            Set(ByVal value As Boolean)
                _ThrowExceptions = value
            End Set
        End Property

        Public Shared ReadOnly Property FunctionReplaceValue As String
            Get
                Return "<[value]>"
            End Get
        End Property

        Private _useParameterizedQuery As Boolean
        Public Property UseParameterizedQuery() As Boolean
            Get
                Return _useParameterizedQuery
            End Get
            Set(ByVal value As Boolean)
                _useParameterizedQuery = value
            End Set
        End Property

        Private _useDistinct As Boolean
        Public Property UseDistinct() As Boolean
            Get
                Return _useDistinct
            End Get
            Set(ByVal value As Boolean)
                _useDistinct = value
            End Set
        End Property

        Private _useFullParameterNames As Boolean
        Public Property UseFullParameterNames() As Boolean
            Get
                Return _useFullParameterNames
            End Get
            Set(ByVal value As Boolean)
                _useFullParameterNames = value
            End Set
        End Property

        Private _useEscapedParameters As Boolean
        Public Property UseEscapedParameters() As Boolean
            Get
                Return _useEscapedParameters
            End Get
            Set(ByVal value As Boolean)
                _useEscapedParameters = value
            End Set
        End Property


        Private _timeout As Integer?
        Public Property Timeout() As Integer?
            Get
                Return _timeout
            End Get
            Set(ByVal value As Integer?)
                _timeout = value
            End Set
        End Property

        Private _parentCriteria As DatabaseCriteria
        Friend Property ParentCriteria() As DatabaseCriteria
            Get
                Return _parentCriteria
            End Get
            Set(ByVal value As DatabaseCriteria)
                _parentCriteria = value
            End Set
        End Property

        Private _connection As DatabaseConnection
        Public Property Connection() As DatabaseConnection
            Get
                Return _connection
            End Get
            Set(ByVal value As DatabaseConnection)
                _connection = value
            End Set
        End Property

        Private _selectParameters As SelectParameters
        Public ReadOnly Property SelectParameters() As SelectParameters
            Get
                Return _selectParameters
            End Get
        End Property

        Private _joins As DatabaseJoins
        Public ReadOnly Property Joins() As DatabaseJoins
            Get
                Return _joins
            End Get
        End Property

        Private _criterias As DatabaseCriterias
        Public ReadOnly Property Criterias() As DatabaseCriterias
            Get
                Return _criterias
            End Get
        End Property

        Private _InsertParameters As DatabaseParameterValues
        Public ReadOnly Property InsertParameters() As DatabaseParameterValues
            Get
                Return _InsertParameters
            End Get
        End Property

        Private _UpdateParameters As DatabaseParameterValues
        Public ReadOnly Property UpdateParameters() As DatabaseParameterValues
            Get
                Return _InsertParameters
            End Get
        End Property

        Private _orderBys As DatabaseOrderBys
        Public ReadOnly Property OrderBys() As DatabaseOrderBys
            Get
                Return _orderBys
            End Get
        End Property

        Private _groupBys As DatabaseGroupBys
        Public ReadOnly Property GroupBys() As DatabaseGroupBys
            Get
                Return _groupBys
            End Get
        End Property

        Private _selectRange As StartAndLength
        Public Property SelectRange() As StartAndLength
            Get
                Return _selectRange
            End Get
            Set(ByVal value As StartAndLength)
                _selectRange = value
            End Set
        End Property

        Private _tableSample As TableSample
        Public Property TableSample() As TableSample
            Get
                Return _tableSample
            End Get
            Set(ByVal value As TableSample)
                _tableSample = value
            End Set
        End Property

        Private hasSelectRange As Boolean
        Private isSelfCreatedConnection As Boolean

        Public Sub New()
            _selectParameters = New SelectParameters()
            _joins = New DatabaseJoins()
            _criterias = New DatabaseCriterias()
            _InsertParameters = New DatabaseParameterValues()
            _UpdateParameters = New DatabaseParameterValues()
            _orderBys = New DatabaseOrderBys()
            _groupBys = New DatabaseGroupBys()
            UseParameterizedQuery = True
            SelectRange = New StartAndLength()
            UseDistinct = True
            DebugMode = False
            UseFullParameterNames = True
            UseEscapedParameters = True
            ThrowExceptions = False
        End Sub

        Public Function ExecuteSql(ByVal sql As String) As SystemMessages
            Dim msgs As New SystemMessages()
            Dim closeConnection As Boolean

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen

            Connection.Open()
            Connection.CreateSqlCommand(sql.ToString)
            If Timeout.HasValue Then
                Connection.GetSqlCommand.CommandTimeout = Timeout.Value
            End If

            Try
                Connection.GetSqlCommand.ExecuteNonQuery()
            Catch ex As Exception
                If ThrowExceptions Then
                    Throw ex
                Else
                    msgs.Add(ex.Message, MessageType.Error, ex)
                End If
            Finally
                If closeConnection Then
                    Connection.Close()
                    If isSelfCreatedConnection Then
                        Connection.Dispose()
                    End If
                End If
            End Try

            Return msgs
        End Function

        Public Function CreateDatabase(ByVal databaseDirectory As String, ByVal databaseName As String) As SystemMessages
            Dim msgs As New SystemMessages()
            Dim closeConnection As Boolean

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen

            Dim sql As String = ""
            sql &= "USE [Master]"
            sql &= "GO"
            sql &= "CREATE DATABASE [" & databaseName & "] ON  PRIMARY "
            sql &= "( NAME = N'" & databaseName & "', FILENAME = N'" & databaseDirectory & databaseName & ".mdf' , SIZE = 2048KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )"
            sql &= "LOG ON "
            sql &= "( NAME = N'" & databaseName & "_log', FILENAME = N'" & databaseDirectory & databaseName & "_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)"
            sql &= "Go"

            Connection.Open()
            Connection.CreateSqlCommand(sql.ToString)
            If Timeout.HasValue Then
                Connection.GetSqlCommand.CommandTimeout = Timeout.Value
            End If

            Connection.GetSqlCommand.ExecuteNonQuery()

            If closeConnection Then
                Connection.Close()
                If isSelfCreatedConnection Then
                    Connection.Dispose()
                End If
            End If

            Return msgs
        End Function

        Public Sub Create(ByRef msgs As SystemMessages, ByVal objectMap As DatabaseParameters)

            Dim sql As New StringBuilder()
            Dim closeConnection As Boolean

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen

            If msgs Is Nothing Then
                msgs = New SystemMessages()
            End If

            Try
                sql.Append("Create Table [")
                sql.Append(Joins.FromTableName)
                sql.Append("](")

                Dim firstParam As Boolean = True
                Dim autoIDField As String = ""
                For i As Integer = 0 To objectMap.Count - 1
                    If objectMap(i).TableName = Joins.FromTableName Then

                        If firstParam = False Then
                            sql.Append(",")
                        End If

                        sql.Append(objectMap(i).ParameterNameEscaped)
                        sql.Append(" [")
                        sql.Append(objectMap(i).ParameterType.ToString)
                        sql.Append("]")
                        If objectMap(i).Length.IsNotEmpty Then
                            sql.Append("(")
                            sql.Append(objectMap(i).Length)
                            sql.Append(")")
                        End If
                        If objectMap(i).IsPrimaryKey Then
                            autoIDField = objectMap(i).ParameterName
                            sql.Append(" IDENTITY(1,1) NOT NULL ")
                            If Connection.SqlDatabaseType = SqlDatabaseType.SqlCE Then
                                sql.Append(" PRIMARY KEY ")
                            End If
                        Else
                            sql.Append(" NULL ")
                        End If

                        firstParam = False

                    End If
                Next

                If Connection.SqlDatabaseType = SqlDatabaseType.SqlServer Then
                    sql.Append(", ")
                    sql.Append("CONSTRAINT [PK_" & Joins.FromTableName & "] PRIMARY KEY CLUSTERED ([" & autoIDField & "] Asc)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY]")
                Else
                    sql.Append(")")
                End If

                Connection.Open()

                Select Case Connection.SqlDatabaseType
                    'Case SqlDatabaseType.SqlCE
                    '    Connection.CreateSqlCeCommand(sql.ToString).ExecuteNonQuery()
                    Case Else
                        Connection.CreateSqlCommand(sql.ToString).ExecuteNonQuery()
                End Select

            Catch ex As Exception

                If ThrowExceptions Then
                    Throw ex
                Else
                    msgs.Add(ex.Message, MessageType.Error, ex)
                End If

            Finally

                If closeConnection Then
                    Connection.Close()
                    If isSelfCreatedConnection Then
                        Connection.Dispose()
                    End If
                End If
            End Try

        End Sub

        Public Sub Alter(ByRef msgs As SystemMessages, ByVal objectMap As DatabaseParameters)
            Dim sql As New StringBuilder()
            Dim closeConnection As Boolean

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen

            If msgs Is Nothing Then
                msgs = New SystemMessages()
            End If

            Try
                sql.Append("Alter Table [")
                sql.Append(Joins.FromTableName)
                sql.Append("] Add ")

                Dim firstParam As Boolean = True

                For i As Integer = 0 To objectMap.Count - 1
                    If objectMap(i).TableName = Joins.FromTableName Then

                        If firstParam = False Then
                            sql.Append(",")
                        End If

                        sql.Append(objectMap(i).ParameterNameEscaped)
                        sql.Append(" ")
                        sql.Append(objectMap(i).ParameterType.ToString)
                        If objectMap(i).Length.IsNotEmpty Then
                            sql.Append("(")
                            sql.Append(objectMap(i).Length)
                            sql.Append(")")
                        End If

                        firstParam = False

                    End If
                Next

                Connection.Open()

                Select Case Connection.SqlDatabaseType
                    'Case SqlDatabaseType.SqlCE
                    '    Connection.CreateSqlCeCommand(sql.ToString).ExecuteNonQuery()
                    Case Else
                        Connection.CreateSqlCommand(sql.ToString).ExecuteNonQuery()
                End Select

            Catch ex As Exception

                If ThrowExceptions Then
                    Throw ex
                Else
                    msgs.Add(ex.Message, MessageType.Error, ex)
                End If

            Finally

                If closeConnection Then
                    Connection.Close()
                    If isSelfCreatedConnection Then
                        Connection.Dispose()
                    End If
                End If
            End Try
        End Sub

        Private Sub BuildSelect(ByRef sql As StringBuilder)
            BuildSelect(sql, False)
        End Sub
        Private Sub BuildSelect(ByRef sql As StringBuilder, ByVal getCountOnly As Boolean)

            sql.Append("Select ")

            If getCountOnly Then
                sql.Append(" count(")

                If SelectParameters.Count = 1 Then

                    If UseDistinct Then
                        sql.Append("DISTINCT ")
                    End If

                    If UseFullParameterNames Then
                        sql.Append(SelectParameters(0).Parameter.ParameterFullName)
                    Else
                        sql.Append(SelectParameters(0).Parameter.ParameterName)
                    End If
                Else
                    sql.Append("*")
                End If
                sql.Append(") ")
            Else

                If UseDistinct And SelectParameters.HasBinaryTextField = False And Me.ParentCriteria Is Nothing Then
                    sql.Append(" DISTINCT ")
                End If

                If (SelectRange.Start.HasValue And SelectRange.Length.HasValue) AndAlso SelectRange.Start.Value = 0 AndAlso SelectRange.Length.Value > 0 Then
                    sql.Append(" TOP ")
                    sql.Append(SelectRange.Length.Value)
                    sql.Append(" ")
                End If

                For i As Integer = 0 To SelectParameters.Count - 1
                    If i > 0 Then
                        sql.Append(",")
                    End If
                    If SelectParameters(i).Function.IsEmpty() = False Then
                        If SelectParameters(i).Parameter Is Nothing And SelectParameters(i).Function.IsNotEmpty() Then
                            sql.Append(SelectParameters(i).Function)
                        Else
                            sql.Append(SelectParameters(i).Function.ToLower.Replace(FunctionReplaceValue, SelectParameters(i).Parameter.ParameterFullName))
                        End If
                    ElseIf SelectParameters(i).Parameter.Function.IsEmpty() = False Then
                        sql.Append(SelectParameters(i).Parameter.Function.ToLower.Replace(FunctionReplaceValue, SelectParameters(i).Parameter.ParameterFullName))
                    Else
                        If UseFullParameterNames Then
                            sql.Append(SelectParameters(i).Parameter.ParameterFullName)
                        Else
                            sql.Append(SelectParameters(i).Parameter.ParameterName)
                        End If
                    End If
                    sql.Append(" as [")
                    If SelectParameters(i).Alias IsNot Nothing Then
                        sql.Append(SelectParameters(i).Alias)
                    Else
                        sql.Append(SelectParameters(i).Parameter.PropertyName)
                    End If
                    sql.Append("] ")
                Next
            End If

            sql.Append(" FROM ")
            If UseEscapedParameters Then
                sql.Append("[")
            End If
            sql.Append(Joins.FromTableName)
            If UseEscapedParameters Then
                sql.Append("]")
            End If
            sql.Append(" ")

            For i As Integer = 0 To Joins.Count - 1

                Dim join As DatabaseJoin = Joins(i)
                Dim joinOnTableAlias As String = Joins(i).JoinOnParameter.TableNameEscaped

                Dim otherJoinTo As DatabaseJoin = Joins.Where(Function(x) join.JoinOnParameter.TableName = x.JoinToParameter.TableName).FirstOrDefault()
                If otherJoinTo.IsNotEmpty() AndAlso otherJoinTo.Alias.IsNotBlank() Then
                    joinOnTableAlias = "[" & otherJoinTo.Alias & "]"
                End If

                sql.Append(" ")
                sql.Append(Joins(i).JoinType.ToString)
                sql.Append(" [")
                sql.Append(Joins(i).JoinToParameter.TableName)
                sql.Append("] ")
                If Joins(i).Alias.IsEmpty() = False Then
                    sql.Append(" [")
                    sql.Append(Joins(i).Alias)
                    sql.Append("] ")
                End If
                sql.Append(" ON ")
                sql.Append(joinOnTableAlias)
                sql.Append(".")
                sql.Append(Joins(i).JoinOnParameter.ParameterNameEscaped)
                sql.Append(" = ")
                If Joins(i).Alias.IsEmpty() = False Then
                    sql.Append(" [")
                    sql.Append(Joins(i).Alias)
                    sql.Append("]")
                    sql.Append(".")
                    sql.Append(Joins(i).JoinToParameter.ParameterNameEscaped)
                Else
                    sql.Append(Joins(i).JoinToParameter.ParameterFullName)
                End If
                sql.Append(" ")
            Next

            If Criterias.HasAnyCriterias Then
                sql.Append(" WHERE ")
                GenerateCriterias(sql, Criterias)
            End If

            If getCountOnly = False Then
                If ParentCriteria Is Nothing Then
                    BuildGroupBys(sql)
                End If
                BuildOrderBys(sql)
            End If

            If TableSample.IsNotEmpty() Then
                sql.Append(" ")
                sql.Append(TableSample.ToString())
                sql.Append(" ")
            End If

            If SelectRange.Length.HasValue Then
                sql.Append(" option (fast ")
                sql.Append(SelectRange.Length.ToString())
                sql.Append(") ")
            End If

        End Sub
        Private Sub BuildGroupBys(ByRef sql As StringBuilder)
            If GroupBys.Count > 0 Then
                sql.Append(" GROUP BY ")
                For i As Integer = 0 To GroupBys.Count - 1
                    If i > 0 Then
                        sql.Append(",")
                    End If
                    sql.Append(GroupBys(i).ParameterFullName)
                Next
            End If
        End Sub
        Private Sub BuildOrderBys(ByRef sql As StringBuilder)
            If OrderBys.Count > 0 Then
                sql.Append(" ORDER BY ")
                For i As Integer = 0 To OrderBys.Count - 1
                    If i > 0 Then
                        sql.Append(",")
                    End If
                    sql.Append(OrderBys(i).ParameterFullName)
                    sql.Append(" ")
                    sql.Append(OrderBys(i).Direction.ToString)
                Next
            End If
        End Sub

        Public Function GetCount() As Integer
            Return GetCount(Nothing)
        End Function

        Public Function GetCount(ByVal msgs As SystemMessages) As Integer
            Dim reader As SqlDataReader = Nothing
            Dim command As SqlCommand
            Dim count As Integer = 0
            Dim closeConnection As Boolean
            Dim sql As New StringBuilder

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen

            If msgs Is Nothing Then
                msgs = New SystemMessages()
            End If

            Try
                BuildSelect(sql, True)

                Connection.Open()
                command = Connection.CreateSqlCommand(sql.ToString)
                If UseParameterizedQuery Then
                    GenerateCriteriaParameters(command, Criterias)
                End If

                If Timeout.HasValue Then
                    command.CommandTimeout = Timeout.Value
                End If

                count = Convert.ToInt32(command.ExecuteScalar())

                'reader = command.ExecuteReader()
                'If reader.HasRows = True Then
                '   reader.Read()
                '   count = reader.Item(0).ToString.ToInteger()
                'End If

            Catch ex As Exception

                'msgs.Add(ex.Message, MessageType.Error, ex)
                'msgs.Add(sql.ToString(), MessageType.Error)
                If ThrowExceptions Then
                    Throw ex
                Else
                    msgs.Add(ex.Message, MessageType.Error, ex)
                End If

            Finally

                If DebugMode Then
                    msgs.Add(sql.ToString(), MessageType.Notice)
                End If

                If reader IsNot Nothing AndAlso reader.IsClosed = False Then
                    reader.Close()
                End If
                reader = Nothing

                If closeConnection Then
                    Connection.Close()
                    If isSelfCreatedConnection Then
                        Connection.Dispose()
                    End If
                End If
            End Try

            Return count
        End Function

        Public Function [Select]() As DataTable
            Return Me.Select(Nothing)
        End Function
        Public Function [Select](ByVal msgs As SystemMessages) As DataTable
            Dim sql As New StringBuilder

            If msgs Is Nothing Then
                msgs = New SystemMessages()
            End If

            Try
                BuildSelect(sql)
            Catch ex As Exception

                If ThrowExceptions Then
                    Throw ex
                Else
                    msgs.Add(ex.Message, MessageType.Error, ex)
                End If

            End Try

            Return ExecuteSelect(sql.ToString(), msgs)

        End Function

        Public Function ExecuteSelect(ByVal sql As String) As DataTable
            Return ExecuteSelect(sql, Nothing)
        End Function
        Public Function ExecuteSelect(ByVal sql As String, ByVal msgs As SystemMessages) As DataTable
            Dim closeConnection As Boolean
            Dim dt As DataTable = Nothing
            Dim daSql As SqlDataAdapter = Nothing

            If msgs Is Nothing Then
                msgs = New SystemMessages()
            End If

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen

            Try
                If SelectRange.Start.HasValue And SelectRange.Length.HasValue Then
                    hasSelectRange = True
                End If

                Connection.Open()

                dt = New DataTable

                Connection.CreateSqlCommand(sql.ToString)
                If Timeout.HasValue Then
                    Connection.GetSqlCommand.CommandTimeout = Timeout.Value
                End If

                If UseParameterizedQuery Then
                    GenerateCriteriaParameters(Connection.GetSqlCommand, Criterias)
                End If

                daSql = New SqlDataAdapter(Connection.GetSqlCommand)
                If hasSelectRange Then
                    daSql.Fill(SelectRange.Start.Value, SelectRange.Length.Value, dt)
                Else
                    daSql.Fill(dt)
                End If

            Catch ex As Exception

                msgs.Add(ex.Message, MessageType.Error, ex)
                'msgs.Add(sql.ToString(), MessageType.Error)

            Finally
                If DebugMode Then
                    msgs.Add(sql.ToString(), MessageType.Notice)
                End If

                If daSql IsNot Nothing Then
                    daSql.Dispose()
                End If

                If closeConnection Then
                    Connection.Close()
                    If isSelfCreatedConnection Then
                        Connection.Dispose()
                    End If
                End If
            End Try

            Return dt
        End Function

        Public Function Insert(ByVal insertTableName As String) As Integer
            Return Insert(insertTableName, Nothing)
        End Function
        Public Function Insert(ByVal insertTableName As String, ByVal msgs As SystemMessages) As Integer
            Dim sql As New StringBuilder()
            Dim addedParams As Integer = 0
            Dim closeConnection As Boolean
            Dim doCommit As Boolean
            Dim newId As Integer = -1
            Dim tempObj As Object

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen
            doCommit = Not Connection.IsTransactionStarted

            If msgs Is Nothing Then
                msgs = New SystemMessages()
            End If

            Try

                sql.Append("INSERT INTO [")
                sql.Append(insertTableName)
                sql.Append("] ")

                If InsertParameters.Count = 0 Then
                    msgs.Add("No parameters have been sent to be inserted", MessageType.UserError)
                Else
                    sql.Append("(")
                    For i As Integer = 0 To InsertParameters.Count - 1
                        If InsertParameters(i).Parameter.DoNotInsert = False And InsertParameters(i).Parameter.TableName.ToLower = insertTableName.ToLower Then
                            If addedParams > 0 Then
                                sql.Append(",")
                            End If
                            sql.Append(InsertParameters(i).Parameter.ParameterNameEscaped)
                            addedParams += 1
                        End If
                    Next
                    sql.Append(") VALUES (")

                    addedParams = 0
                    For i As Integer = 0 To InsertParameters.Count - 1
                        If InsertParameters(i).Parameter.DoNotInsert = False And InsertParameters(i).Parameter.TableName.ToLower = insertTableName.ToLower Then
                            If addedParams > 0 Then
                                sql.Append(",")
                            End If

                            If UseParameterizedQuery Then
                                sql.Append("@")
                                sql.Append(InsertParameters(i).Parameter.ParameterName)
                            Else
                                GetPreparedParameterValue(sql, InsertParameters(i).Value, InsertParameters(i).Parameter.ParameterType)
                            End If

                            addedParams += 1
                        End If
                    Next
                    sql.Append(")")
                End If

                If Connection.SqlDatabaseType = SqlDatabaseType.SqlServer Then
                    sql.Append(";SELECT SCOPE_IDENTITY();")
                End If

                Connection.Open()
                Connection.BeginTransaction()

                Select Case Connection.SqlDatabaseType
                    'Case SqlDatabaseType.SqlCE
                    '    Connection.CreateSqlCeCommand(sql.ToString)
                    '    If UseParameterizedQuery Then
                    '        For i As Integer = 0 To InsertParameters.Count - 1
                    '            If InsertParameters(i).Parameter.DoNotInsert = False And InsertParameters(i).Parameter.TableName.ToLower = insertTableName.ToLower Then
                    '                AddParameter(Connection.GetSqlCeCommand, InsertParameters(i))
                    '            End If
                    '        Next
                    '    End If
                    '    tempObj = Connection.GetSqlCeCommand.ExecuteScalar()
					Case SqlDatabaseType.OleDB
						Throw New NotImplementedException()
					Case Else
						Connection.CreateSqlCommand(sql.ToString)
						If UseParameterizedQuery Then
							For i As Integer = 0 To InsertParameters.Count - 1
								If InsertParameters(i).Parameter.DoNotInsert = False And InsertParameters(i).Parameter.TableName.ToLower = insertTableName.ToLower Then
									AddParameter(Connection.GetSqlCommand, InsertParameters(i))
								End If
							Next
						End If
						tempObj = Connection.GetSqlCommand.ExecuteScalar()

				End Select

                'If Connection.SqlDatabaseType = SqlDatabaseType.SqlCE Then
                '    Connection.CreateSqlCeCommand("select @@identity")
                '    tempObj = Connection.GetSqlCeCommand.ExecuteScalar()
                'End If

                If tempObj IsNot Nothing AndAlso tempObj.ToString.IsNumber Then
                    newId = tempObj.ToString.ToInteger
                End If

            Catch ex As Exception

                If ThrowExceptions Then
                    Throw ex
                Else
                    msgs.Add(ex.Message, MessageType.Error, ex)
                End If

            Finally

                If doCommit Then
                    If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                        Connection.CommitTransaction()
                    Else
                        Connection.RollbackTransaction()
                    End If
                End If

                If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                    'msgs.Add("Success", MessageType.Success)
                End If

                If closeConnection Then
                    Connection.Close()
                    If isSelfCreatedConnection Then
                        Connection.Dispose()
                    End If
                End If
            End Try

            Return newId
        End Function

        Public Function Update(ByVal updateTableName As String) As Integer
            Return Update(updateTableName, Nothing)
        End Function
        Public Function Update(ByVal updateTableName As String, ByVal msgs As SystemMessages) As Integer
            Dim sql As New StringBuilder()
            Dim closeConnection As Boolean
            Dim doCommit As Boolean
            Dim addedParams As Integer = 0
            Dim recordsAffected As Integer = 0

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen
            doCommit = Not Connection.IsTransactionStarted

            If msgs Is Nothing Then
                msgs = New SystemMessages()
            End If

            Try

                Dim tableNames As New List(Of String)
                tableNames.Add(updateTableName)

                'For i As Integer = 0 To UpdateParameters.Count - 1
                '    If tableNames.Contains(UpdateParameters(i).Parameter.TableName) = False Then
                '        tableNames.Add(UpdateParameters(i).Parameter.TableName)
                '    End If
                'Next

                For j As Integer = 0 To tableNames.Count - 1

                    sql.Append("UPDATE [")
                    sql.Append(tableNames(j))
                    sql.Append("] ")

                    If UpdateParameters.Count = 0 Then
                        msgs.Add("No parameters have been sent to be updated", MessageType.Warning)
                    Else

                        sql.Append(" SET ")

                        For i As Integer = 0 To UpdateParameters.Count - 1
                            If UpdateParameters(i).Parameter.DoNotUpdate = False And UpdateParameters(i).Parameter.TableName.ToLower = tableNames(j).ToLower Then
                                If addedParams > 0 Then
                                    sql.Append(",")
                                End If

                                sql.Append(UpdateParameters(i).Parameter.ParameterNameEscaped)
                                sql.Append(" = ")

                                If UseParameterizedQuery Then
                                    sql.Append("@")
                                    sql.Append(UpdateParameters(i).Parameter.ParameterName)
                                Else
                                    GetPreparedParameterValue(sql, InsertParameters(i).Value, InsertParameters(i).Parameter.ParameterType)
                                End If

                                addedParams += 1
                            End If
                        Next
                    End If

                    If Criterias.HasAnyCriterias Then
                        sql.Append(" WHERE ")
                        GenerateCriterias(sql, Criterias)
                    End If
                Next

                Connection.Open()
                Connection.BeginTransaction()

                Select Case Connection.SqlDatabaseType
                    'Case SqlDatabaseType.SqlCE
                    '    Connection.CreateSqlCeCommand(sql.ToString)
                    '    If UseParameterizedQuery Then
                    '        For i As Integer = 0 To UpdateParameters.Count - 1
                    '            If UpdateParameters(i).Parameter.DoNotUpdate = False Then
                    '                AddParameter(Connection.GetSqlCeCommand, UpdateParameters(i))
                    '            End If
                    '        Next
                    '        GenerateCriteriaParameters(Connection.GetSqlCeCommand, Criterias)
                    '    End If
                    '    recordsAffected = Connection.GetSqlCeCommand.ExecuteNonQuery()
					Case SqlDatabaseType.OleDB
						Throw New NotImplementedException()
					Case Else
						Connection.CreateSqlCommand(sql.ToString)
						If UseParameterizedQuery Then
							For i As Integer = 0 To UpdateParameters.Count - 1
								If UpdateParameters(i).Parameter.DoNotUpdate = False Then
									AddParameter(Connection.GetSqlCommand, UpdateParameters(i))
								End If
							Next
							GenerateCriteriaParameters(Connection.GetSqlCommand, Criterias)
						End If
						recordsAffected = Connection.GetSqlCommand.ExecuteNonQuery()
				End Select

            Catch ex As Exception

                If ThrowExceptions Then
                    Throw ex
                Else
                    msgs.Add(ex.Message, MessageType.Error, ex)
                End If

            Finally
                If doCommit Then
                    If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                        Connection.CommitTransaction()
                    Else
                        Connection.RollbackTransaction()
                    End If
                End If

                If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                    'msgs.Add("Success", MessageType.Success)
                End If

                If closeConnection Then
                    Connection.Close()
                    If isSelfCreatedConnection Then
                        Connection.Dispose()
                    End If
                End If
            End Try

            Return recordsAffected
        End Function

        Public Function Delete(ByVal deleteTableName As String) As Integer
            Return Delete(deleteTableName, Nothing)
        End Function
        Public Function Delete(ByVal deleteTableName As String, ByVal msgs As SystemMessages) As Integer
            Dim sql As New StringBuilder()
            Dim closeConnection As Boolean
            Dim doCommit As Boolean
            Dim recordsAffected As Integer = 0

            If Connection Is Nothing Then
                Connection = New DatabaseConnection(BasicLibrary.Utilities.ConnectionString)
                isSelfCreatedConnection = True
            End If

            closeConnection = Not Connection.IsOpen
            doCommit = Not Connection.IsTransactionStarted

            If msgs Is Nothing Then
                msgs = New SystemMessages()
            End If

            Try

                sql.Append("DELETE FROM [")
                sql.Append(deleteTableName)
                sql.Append("] ")

                If Criterias.HasAnyCriterias Then
                    sql.Append(" WHERE ")
                    GenerateCriterias(sql, Criterias)
                End If

                Connection.Open()
                Connection.BeginTransaction()

                Select Case Connection.SqlDatabaseType
                    'Case SqlDatabaseType.SqlCE
                    '    Connection.CreateSqlCeCommand(sql.ToString)
                    '    If UseParameterizedQuery Then
                    '        GenerateCriteriaParameters(Connection.GetSqlCeCommand, Criterias)
                    '    End If
                    '    recordsAffected = Connection.GetSqlCeCommand.ExecuteNonQuery()
					Case SqlDatabaseType.OleDB
						Throw New NotImplementedException()
					Case Else
						Connection.CreateSqlCommand(sql.ToString)
						If UseParameterizedQuery Then
							GenerateCriteriaParameters(Connection.GetSqlCommand, Criterias)
						End If
						recordsAffected = Connection.GetSqlCommand.ExecuteNonQuery()
				End Select

            Catch ex As Exception

                If ThrowExceptions Then
                    Throw ex
                Else
                    msgs.Add(ex.Message, MessageType.Error, ex)
                End If

            Finally
                If doCommit Then
                    If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                        Connection.CommitTransaction()
                    Else
                        Connection.RollbackTransaction()
                    End If
                End If

                If msgs.HasAny(SystemMessages.ErrorTypes) = False Then
                    'msgs.Add("Success", MessageType.Success)
                End If

                If closeConnection Then
                    Connection.Close()
                    If isSelfCreatedConnection Then
                        Connection.Dispose()
                    End If
                End If
            End Try

            Return recordsAffected
        End Function

        Private Sub GenerateCriterias(ByRef sql As StringBuilder, ByVal criterias As DatabaseCriterias)

            If criterias.HasAnyCriterias Then
                sql.Append("(")

                If criterias.Count > 0 Then
                    For i As Integer = 0 To criterias.Count - 1

                        If i > 0 Then
                            sql.Append(" ")
                            sql.Append(criterias.LogicalOperator.ToString)
                            sql.Append(" ")
                        End If

                        If criterias(i).Parameter.GetType Is GetType(DatabaseParameter) Then
                            Dim dbParam As DatabaseParameter = DirectCast(criterias(i).Parameter, DatabaseParameter)
                            Dim paramFullName As String = dbParam.ParameterFullName
                            If UseFullParameterNames = False Then
                                paramFullName = dbParam.ParameterName
                            End If
                            If dbParam.Function.IsEmpty = False Then
                                paramFullName = dbParam.Function.Replace(FunctionReplaceValue, paramFullName)
                            End If
                            sql.Append(paramFullName)
                        ElseIf criterias(i).Parameter.GetType Is GetType(DatabaseQuery) Then

                            sql.Append(" (")
                            DirectCast(criterias(i).Parameter, DatabaseQuery).BuildSelect(sql)
                            sql.Append(") ")

                        ElseIf criterias(i).Parameter.GetType Is GetType(String) Then

                            sql.Append("'")
                            sql.Append(criterias(i).Parameter)
                            sql.Append("'")

                        Else
                            sql.Append(criterias(i).Parameter)
                        End If


                        sql.Append(" ")
                        sql.Append(criterias(i).Operator.ToString)

                        If criterias(i).Value Is Nothing Then

                            GetPreparedParameterValue(sql, criterias(i).Value, criterias(i).CriteriaType, criterias(i).Operator)

                        ElseIf criterias(i).Value.GetType Is GetType(DatabaseParameter) Then

                            sql.Append(" ")
                            Dim valueParameter As DatabaseParameter = DirectCast(criterias(i).Value, DatabaseParameter)
                            Dim valueParamFullName As String = valueParameter.ParameterFullName
                            If valueParameter.Function.IsEmpty = False Then
                                valueParamFullName = valueParameter.Function.Replace(FunctionReplaceValue, valueParamFullName)
                            End If
                            sql.Append(valueParamFullName)
                            sql.Append(" ")

                        ElseIf criterias(i).Value.GetType Is GetType(DatabaseQuery) Then
                            sql.Append(" (")
                            DirectCast(criterias(i).Value, DatabaseQuery).BuildSelect(sql)
                            sql.Append(") ")

                        ElseIf TryCast(criterias(i).Value, IEnumerable) IsNot Nothing AndAlso criterias(i).Value.GetType IsNot GetType(String) Then
                            Dim eof As IEnumerable = DirectCast(criterias(i).Value, IEnumerable)
                            Dim es As IEnumerator = eof.GetEnumerator()
                            Dim counter As Integer = 0

                            es.Reset()
                            While es.MoveNext()
                                If counter = 0 Then
                                    sql.Append(" (")
                                Else
                                    sql.Append(",")
                                End If

                                If UseParameterizedQuery Then
                                    sql.Append(" @")
                                    sql.Append(criterias(i).UniqueName)
                                    sql.Append("_")
                                    sql.Append(counter.ToString)
                                    sql.Append(" ")
                                Else
                                    Me.GetPreparedParameterValue(sql, es.Current, criterias(i).CriteriaType, criterias(i).Operator)
                                End If

                                counter += 1
                            End While

                            If counter > 0 Then
                                sql.Append(") ")
                            End If
                        Else
                            If UseParameterizedQuery Then
                                sql.Append(" @")
                                sql.Append(criterias(i).UniqueName)
                                sql.Append(" ")
                            Else
                                If criterias(i).Value IsNot Nothing Then
                                    GetPreparedParameterValue(sql, criterias(i).Value, criterias(i).CriteriaType, criterias(i).Operator, criterias(i).Function)
                                End If
                            End If
                        End If
                    Next
                End If

                If criterias.GroupCount > 0 Then
                    For i As Integer = 0 To criterias.GroupCount - 1

                        If i > 0 Or criterias.Count > 0 Then
                            sql.Append(" ")
                            sql.Append(criterias.LogicalOperator.ToString)
                            sql.Append(" ")
                        End If

                        'recursion
                        GenerateCriterias(sql, criterias.ItemGroups(i))
                    Next
                End If

                sql.Append(")")
            End If
        End Sub

        Private Sub GenerateCriteriaParameters(ByRef command As SqlCommand, ByVal criterias As DatabaseCriterias)
            For i As Integer = 0 To criterias.Count - 1
                If criterias(i).Value IsNot Nothing Then
                    If criterias(i).Value.GetType Is GetType(DatabaseQuery) Then
                        Dim dq As DatabaseQuery = DirectCast(criterias(i).Value, DatabaseQuery)
                        dq.GenerateCriteriaParameters(command, dq.Criterias)
                    ElseIf TryCast(criterias(i).Value, IEnumerable) IsNot Nothing AndAlso criterias(i).Value.GetType IsNot GetType(String) Then
                        Dim eof As IEnumerable = DirectCast(criterias(i).Value, IEnumerable)
                        Dim es As IEnumerator = eof.GetEnumerator()
                        Dim counter As Integer = 0

                        es.Reset()
                        While es.MoveNext()
                            command.Parameters.Add("@" & criterias(i).UniqueName & "_" & counter.ToString, criterias(i).CriteriaType).Value = es.Current
                            counter += 1
                        End While
                    Else
                        Select Case criterias(i).Operator.Value
                            Case DatabaseCriteriaOperatorEnum.Like
                                command.Parameters.Add("@" & criterias(i).UniqueName, criterias(i).CriteriaType).Value = "%" & criterias(i).Value.ToString & "%"
                            Case DatabaseCriteriaOperatorEnum.LikeLeft
                                command.Parameters.Add("@" & criterias(i).UniqueName, criterias(i).CriteriaType).Value = "%" & criterias(i).Value.ToString
                            Case DatabaseCriteriaOperatorEnum.LikeRight
                                command.Parameters.Add("@" & criterias(i).UniqueName, criterias(i).CriteriaType).Value = criterias(i).Value.ToString & "%"
                            Case DatabaseCriteriaOperatorEnum.LikeWordMatch
                                command.Parameters.Add("@" & criterias(i).UniqueName, criterias(i).CriteriaType).Value = "%" & Utilities.NoAlphaNumericCharactersRegEx & criterias(i).Value.ToString & Utilities.NoAlphaNumericCharactersRegEx & "%"
                            Case DatabaseCriteriaOperatorEnum.LikeWordMatchLeft
                                command.Parameters.Add("@" & criterias(i).UniqueName, criterias(i).CriteriaType).Value = "%" & Utilities.NoAlphaNumericCharactersRegEx & criterias(i).Value.ToString
                            Case DatabaseCriteriaOperatorEnum.LikeWordMatchRight
                                command.Parameters.Add("@" & criterias(i).UniqueName, criterias(i).CriteriaType).Value = criterias(i).Value.ToString & Utilities.NoAlphaNumericCharactersRegEx & "%"
                            Case Else
                                command.Parameters.Add("@" & criterias(i).UniqueName, criterias(i).CriteriaType).Value = criterias(i).Value
                        End Select
                    End If
                End If
            Next
            For i As Integer = 0 To criterias.GroupCount - 1
                GenerateCriteriaParameters(command, criterias.ItemGroups(i))
            Next

        End Sub

        Private Sub GetPreparedParameterValue(ByRef sql As Text.StringBuilder, ByVal value As Object, ByVal parameterType As SqlDbType)
            GetPreparedParameterValue(sql, value, parameterType, Nothing, Nothing)
        End Sub
        Private Sub GetPreparedParametervalue(ByRef sql As Text.StringBuilder, ByVal value As Object, ByVal parameterType As SqlDbType, ByVal [operator] As DatabaseCriteriaOperator)
            GetPreparedParameterValue(sql, value, parameterType, [operator], Nothing)
        End Sub
        Private Sub GetPreparedParameterValue(ByRef sql As Text.StringBuilder, ByVal value As Object, ByVal parameterType As SqlDbType, ByVal [operator] As DatabaseCriteriaOperator, ByVal [function] As String)
            If value Is Nothing OrElse value Is DBNull.Value Then
                sql.Append(" NULL ")
            Else

                If parameterType = SqlDbType.Bit Then
                    If value.ToString.IsBoolean Then
                        If value.ToString.ToBoolean = True Then
                            value = "1"
                        Else
                            value = "0"
                        End If
                    End If
                End If

                If [function].IsEmpty() = False Then
                    value = [function].Replace(FunctionReplaceValue, value.ToString)

                    sql.Append(" ")
                    sql.Append(value)
                    sql.Append(" ")

                Else

                    Select Case parameterType
                        Case SqlDbType.BigInt, SqlDbType.Decimal, SqlDbType.Float, SqlDbType.Int, SqlDbType.Money, SqlDbType.Real, SqlDbType.SmallInt, SqlDbType.SmallMoney, SqlDbType.TinyInt, SqlDbType.Bit
                            If value.ToString().IsNumber() Then
                                sql.Append(" ")
                                If value.GetType.IsEnum Then
                                    sql.Append(Convert.ToInt32(value).ToString)
                                Else
                                    sql.Append(value.ToString)
                                End If
                                sql.Append(" ")
                            End If
                        Case Else
                            sql.Append(" '")

                            If [operator] IsNot Nothing Then
                                Select Case [operator].Value
                                    Case DatabaseCriteriaOperatorEnum.Like, DatabaseCriteriaOperatorEnum.LikeLeft, DatabaseCriteriaOperatorEnum.LikeWordMatch, DatabaseCriteriaOperatorEnum.LikeWordMatchLeft
                                        sql.Append("%")
                                End Select

                                Select Case [operator].Value
                                    Case DatabaseCriteriaOperatorEnum.LikeWordMatch, DatabaseCriteriaOperatorEnum.LikeWordMatchLeft
                                        sql.Append(Utilities.NoAlphaNumericCharactersRegEx)
                                End Select
                            End If

                            sql.Append(value.ToString.Replace("'", "''"))

                            If [operator] IsNot Nothing Then
                                Select Case [operator].Value
                                    Case DatabaseCriteriaOperatorEnum.LikeWordMatch, DatabaseCriteriaOperatorEnum.LikeWordMatchRight
                                        sql.Append(Utilities.NoAlphaNumericCharactersRegEx)
                                End Select

                                Select Case [operator].Value
                                    Case DatabaseCriteriaOperatorEnum.Like, DatabaseCriteriaOperatorEnum.LikeRight, DatabaseCriteriaOperatorEnum.LikeWordMatch, DatabaseCriteriaOperatorEnum.LikeWordMatchRight
                                        sql.Append("%")
                                End Select
                            End If

                            sql.Append("' ")
                    End Select

                End If
                End If
        End Sub

        Private Sub AddParameter(ByRef command As SqlCommand, ByVal parameterValue As DatabaseParameterValue)
            If parameterValue.Value Is Nothing Then
                command.Parameters.Add("@" & parameterValue.Parameter.ParameterName, parameterValue.Parameter.ParameterType).Value = DBNull.Value
            Else
                command.Parameters.Add("@" & parameterValue.Parameter.ParameterName, parameterValue.Parameter.ParameterType).Value = parameterValue.Value
            End If
        End Sub
        'Private Sub AddParameter(ByRef command As SqlCeCommand, ByVal parameterValue As DatabaseParameterValue)
        '    If parameterValue.Value Is Nothing Then
        '        command.Parameters.Add("@" & parameterValue.Parameter.ParameterName, parameterValue.Parameter.ParameterType).Value = DBNull.Value
        '    Else
        '        command.Parameters.Add("@" & parameterValue.Parameter.ParameterName, parameterValue.Parameter.ParameterType).Value = parameterValue.Value
        '    End If
        'End Sub

        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
                    If Me.Connection IsNot Nothing And isSelfCreatedConnection Then
                        Me.Connection.Dispose()
                    End If
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

    <Serializable()> _
    Public Class SelectParameters
        Implements IEnumerable(Of SelectParameter)

        Private _items As List(Of SelectParameter)
        Private _keys As List(Of String)

        Private _tempKey As String = ""

        Default Public ReadOnly Property Items(ByVal index As Integer) As SelectParameter
            Get
                Return Me._items(index)
            End Get
        End Property
        Default Public ReadOnly Property Items(ByVal key As String) As SelectParameter
            Get
                _tempKey = key
                Dim index As Integer = _keys.FindIndex(AddressOf _FindKeyIndex)
                If index > -1 Then
                    Return Me._items(index)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Private _hasBinaryTextField As Boolean
        Public ReadOnly Property HasBinaryTextField() As Boolean
            Get
                Return _hasBinaryTextField
            End Get
        End Property

        Private Function _FindKeyIndex(ByVal key As String) As Boolean
            If String.Compare(key, _tempKey, True) = 0 Then
                Return True
            Else
                Return False
            End If
        End Function


        Public ReadOnly Property Count() As Integer
            Get
                Return _items.Count
            End Get
        End Property

        Public Sub New()
            _items = New List(Of SelectParameter)
            _keys = New List(Of String)
        End Sub

        Public Function Add(ByVal parameter As DatabaseParameter) As Integer
            Return Add(New SelectParameter(parameter))
        End Function
        Public Function Add(ByVal parameter As DatabaseParameter, ByVal [alias] As String) As Integer
            Return Add(New SelectParameter(parameter, [alias]))
        End Function
        Public Function Add(ByVal parameter As DatabaseParameter, ByVal [alias] As String, ByVal [function] As String) As Integer
            Return Add(New SelectParameter(parameter, [alias], [function]))
        End Function
        Public Function Add(ByVal [alias] As String, ByVal [function] As String) As Integer
            Return Add(New SelectParameter([alias], [function]))
        End Function

        Public Function Add(ByVal item As SelectParameter) As Integer
            If item.Parameter IsNot Nothing AndAlso (item.Parameter.ParameterType = SqlDbType.Text Or item.Parameter.ParameterType = SqlDbType.NText) Then
                _hasBinaryTextField = True
            End If

            _items.Add(item)

            If item.Parameter IsNot Nothing Then
                _keys.Add(item.Parameter.PropertyName)
            Else
                _keys.Add(item.Alias)
            End If
            Return _items.Count - 1
        End Function

        Public Sub AddRange(ByVal items As SelectParameters)
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub
        Public Sub AddRange(ByVal items As DatabaseParameters)
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub AddRange(ByVal items As List(Of SelectParameter))
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub Clear()
            _items.Clear()
            _keys.Clear()
        End Sub

        Public Sub Remove(ByVal item As SelectParameter)
            _items.Remove(item)
            _keys.Remove(item.Parameter.ParameterName)
        End Sub
        Public Sub RemoveAt(ByVal index As Integer)
            _items.RemoveAt(index)
            _keys.RemoveAt(index)
        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of SelectParameter) Implements System.Collections.Generic.IEnumerable(Of SelectParameter).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function GetDatabaseParameters() As DatabaseParameters
            Dim list As New DatabaseParameters()
            For i As Integer = 0 To Me.Count - 1
                list.Add(Me(i).Parameter)
            Next
            Return list
        End Function
    End Class

    <Serializable()> _
    Public Class SelectParameter

        Private _parameter As DatabaseParameter
        Public Property Parameter() As DatabaseParameter
            Get
                Return _parameter
            End Get
            Set(ByVal value As DatabaseParameter)
                _parameter = value
            End Set
        End Property

        'Private _parameter As Object
        'Public Property Parameter() As Object
        '    Get
        '        Return _parameter
        '    End Get
        '    Set(ByVal value As Object)
        '        _parameter = value
        '    End Set
        'End Property

        Private _alias As String
        Public Property [Alias]() As String
            Get
                Return _alias
            End Get
            Set(ByVal value As String)
                _alias = value
            End Set
        End Property
        Private _function As String
        Public Property [Function]() As String
            Get
                Return _function
            End Get
            Set(ByVal value As String)
                _function = value
            End Set
        End Property
        Public Sub New()

        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter)
            Me.New()
            Me.Parameter = parameter
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal [alias] As String)
            Me.New(parameter)
            Me.Alias = [alias]
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal [alias] As String, ByVal [function] As String)
            Me.New(parameter, [alias])
            Me.Function = [function]
        End Sub
        Public Sub New(ByVal [alias] As String, ByVal [function] As String)
            Me.Parameter = Nothing
            Me.Alias = [alias]
            Me.Function = [function]
        End Sub
        'Public Sub New(ByVal parameter As String, ByVal [alias] As String, ByVal [function] As String)
        '    Me.New()
        '    Me.Parameter = parameter
        '    Me.Alias = [alias]
        '    Me.Function = [function]
        'End Sub
        'Public Sub New(ByVal parameter As String, ByVal [alias] As String)
        '    Me.New()
        '    Me.Parameter = parameter
        '    Me.Alias = [alias]
        '    Me.Function = [function]
        'End Sub

    End Class

    <Serializable()> _
    Public Class DatabaseParameters
        Implements IEnumerable(Of DatabaseParameter)
        Implements ICloneable

        Private _items As List(Of DatabaseParameter)
        Private _keys As List(Of String)

        Private _tempKey As String = ""

        Default Public ReadOnly Property Items(ByVal index As Integer) As DatabaseParameter
            Get
                Return Me._items(index)
            End Get
        End Property
        Default Public ReadOnly Property Items(ByVal key As String) As DatabaseParameter
            Get
                _tempKey = key
                Dim index As Integer = _keys.FindIndex(AddressOf _FindKeyIndex)
                If index > -1 Then
                    Return Me._items(index)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return _items.Count
            End Get
        End Property

        Private Function _FindKeyIndex(ByVal key As String) As Boolean
            If String.Compare(key, _tempKey, True) = 0 Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Sub New()
            _items = New List(Of DatabaseParameter)
            _keys = New List(Of String)
        End Sub

        Public Overridable Function Add(ByVal item As DatabaseParameter) As Integer
            _items.Add(item)
            _keys.Add(item.PropertyName)
            Return _items.Count - 1
        End Function
        Public Overridable Function Add(ByVal className As String, ByVal propertyName As String, ByVal parameterType As SqlDbType) As Integer
            Return Me.Add(New DatabaseParameter(className, propertyName, parameterType))
        End Function
        Public Overridable Function Add(ByVal className As String, ByVal propertyName As String, ByVal parameterType As SqlDbType, ByVal length As String) As Integer
            Return Me.Add(New DatabaseParameter(className, propertyName, parameterType, length))
        End Function
        Public Overridable Function Add(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterName As String, ByVal parameterType As SqlDbType, ByVal length As String) As Integer
            Return Me.Add(New DatabaseParameter(className, propertyName, tableName, parameterName, parameterType, length))
        End Function
        Public Overridable Function Add(ByVal className As String, ByVal propertyName As String, ByVal parameterType As SqlDbType, ByVal length As String, ByVal tableAlias As String) As Integer
            Return Me.Add(New DatabaseParameter(className, propertyName, parameterType, length, tableAlias))
        End Function
        Public Overridable Function Add(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterType As SqlDbType) As Integer
            Return Me.Add(New DatabaseParameter(className, propertyName, tableName, propertyName, parameterType, ""))
        End Function
        Public Overridable Function Add(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterType As SqlDbType, ByVal length As String) As Integer
            Return Me.Add(New DatabaseParameter(className, propertyName, tableName, propertyName, parameterType, length))
        End Function
        Public Overridable Function Add(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterName As String, ByVal parameterType As SqlDbType) As Integer
            Return Me.Add(New DatabaseParameter(className, propertyName, tableName, parameterName, parameterType, ""))
        End Function

        Public Sub AddRange(ByVal items As DatabaseParameters)
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub AddRange(ByVal items As List(Of DatabaseParameter))
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub Clear()
            _items.Clear()
            _keys.Clear()
        End Sub
        Public Sub Remove(ByVal item As DatabaseParameter)
            _items.Remove(item)
            _keys.Remove(item.ParameterName)
        End Sub
        Public Sub RemoveAt(ByVal index As Integer)
            _items.RemoveAt(index)
            _keys.RemoveAt(index)
        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of DatabaseParameter) Implements System.Collections.Generic.IEnumerable(Of DatabaseParameter).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function Clone() As DatabaseParameters
            Return DirectCast(_Clone(), DatabaseParameters)
        End Function
        Private Function _Clone() As Object Implements System.ICloneable.Clone
            Dim dps As New DatabaseParameters()
            For i As Integer = 0 To _items.Count - 1
                dps.Add(_items(i).Clone())
            Next
            Return dps
        End Function
    End Class

    <Serializable()> _
    Public Class DatabaseParameter
        Implements ICloneable

        Private _className As String
        Public Property ClassName() As String
            Get
                Return _className
            End Get
            Set(ByVal value As String)
                _className = value
            End Set
        End Property

        Private _tableAlias As String
        Public Property TableAlias() As String
            Get
                Return _tableAlias
            End Get
            Set(ByVal value As String)
                _tableAlias = value
            End Set
        End Property

        Private _tableName As String
        Public Property TableName() As String
            Get
                Return _tableName
            End Get
            Set(ByVal value As String)
                _tableName = value
            End Set
        End Property

        Private _propertyName As String
        Public Property PropertyName() As String
            Get
                Return _propertyName
            End Get
            Set(ByVal value As String)
                _propertyName = value
            End Set
        End Property

        Private _parameterName As String
        Public Property ParameterName() As String
            Get
                Return _parameterName
            End Get
            Set(ByVal value As String)
                _parameterName = value
            End Set
        End Property

        Private _ParameterType As SqlDbType
        Public Property ParameterType() As SqlDbType
            Get
                Return _ParameterType
            End Get
            Set(ByVal value As SqlDbType)
                _ParameterType = value
            End Set
        End Property

        Private _isPrimaryKey As Boolean
        Public Property IsPrimaryKey() As Boolean
            Get
                Return _isPrimaryKey
            End Get
            Set(ByVal value As Boolean)
                _isPrimaryKey = value
            End Set
        End Property

        Private _doNotInsert As Boolean
        Public Property DoNotInsert() As Boolean
            Get
                Return _doNotInsert
            End Get
            Set(ByVal value As Boolean)
                _doNotInsert = value
            End Set
        End Property

        Private _doNotUpdate As Boolean
        Public Property DoNotUpdate() As Boolean
            Get
                Return _doNotUpdate
            End Get
            Set(ByVal value As Boolean)
                _doNotUpdate = value
            End Set
        End Property

        Private _length As String
        Public Property Length() As String
            Get
                Return _length
            End Get
            Set(ByVal value As String)
                _length = value
            End Set
        End Property

        Private _function As String
        ''' <summary>
        ''' Deprecated, please use SelectParameter.Function
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [Function]() As String
            Get
                Return _function
            End Get
            Set(ByVal value As String)
                _function = value
            End Set
        End Property

        Public ReadOnly Property ParameterFullName() As String
            Get
                Return TableNameEscaped & "." & ParameterNameEscaped
            End Get
        End Property

        Public ReadOnly Property ParameterNameEscaped() As String
            Get
                Return "[" & ParameterName & "]"
            End Get
        End Property
        Public ReadOnly Property TableNameEscaped() As String
            Get
                If TableAlias.IsEmpty Then
                    Return "[" & TableName & "]"
                Else
                    Return "[" & TableAlias & "]"
                End If
            End Get
        End Property

        Public Sub New()

        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterName As String, ByVal parameterType As SqlDbType, ByVal length As String)
            Me.New()
            Me.ClassName = className
            Me.PropertyName = propertyName
            Me.TableName = tableName
            Me.ParameterName = parameterName
            Me.ParameterType = parameterType
            Me.Length = length
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterName As String, ByVal parameterType As SqlDbType, ByVal length As String, ByVal tableAlias As String)
            Me.New(className, propertyName, tableName, parameterName, parameterType, length)
            Me.TableAlias = tableAlias
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterName As String, ByVal parameterType As SqlDbType)
            Me.New()
            Me.ClassName = className
            Me.PropertyName = propertyName
            Me.TableName = tableName
            Me.ParameterName = parameterName
            Me.ParameterType = parameterType
            Me.Length = ""
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterType As SqlDbType, ByVal length As String, ByVal tableAlias As String)
            Me.New(className, propertyName, tableName, parameterType, length)
            Me.TableAlias = tableAlias
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterType As SqlDbType, ByVal length As String)
            Me.New()
            Me.ClassName = className
            Me.PropertyName = propertyName
            Me.TableName = tableName
            Me.ParameterName = propertyName
            Me.ParameterType = parameterType
            Me.Length = length
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterType As SqlDbType)
            Me.New()
            Me.ClassName = className
            Me.PropertyName = propertyName
            Me.TableName = tableName
            Me.ParameterName = propertyName
            Me.ParameterType = parameterType
            Me.Length = ""
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal parameterType As SqlDbType)
            Me.New(className, propertyName, className, propertyName, parameterType, "")
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal parameterType As SqlDbType, ByVal length As String, ByVal tableAlias As String)
            Me.New(className, propertyName, className, propertyName, parameterType, length)
            Me.TableAlias = tableAlias
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal parameterType As SqlDbType, ByVal length As String)
            Me.New(className, propertyName, className, propertyName, parameterType, length)
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal parameterType As SqlDbType, ByVal isPrimaryKey As Boolean)
            Me.New(className, propertyName, className, propertyName, parameterType, isPrimaryKey)
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal parameterType As SqlDbType, ByVal isPrimaryKey As Boolean, ByVal doNotInsert As Boolean, ByVal doNotUpdate As Boolean)
            Me.New(className, propertyName, className, propertyName, parameterType, isPrimaryKey, doNotInsert, doNotUpdate)
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterName As String, ByVal parameterType As SqlDbType, ByVal isPrimaryKey As Boolean)
            Me.New(className, propertyName, tableName, parameterName, parameterType, "")
            Me.IsPrimaryKey = isPrimaryKey
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterType As SqlDbType, ByVal isPrimaryKey As Boolean)
            Me.New(className, propertyName, tableName, propertyName, parameterType, "")
            Me.IsPrimaryKey = isPrimaryKey
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterName As String, ByVal parameterType As SqlDbType, ByVal isPrimaryKey As Boolean, ByVal doNotInsert As Boolean, ByVal doNotUpdate As Boolean)
            Me.New(className, propertyName, tableName, parameterName, parameterType, isPrimaryKey)
            Me.DoNotInsert = doNotInsert
            Me.DoNotUpdate = doNotUpdate
        End Sub
        Public Sub New(ByVal className As String, ByVal propertyName As String, ByVal tableName As String, ByVal parameterType As SqlDbType, ByVal isPrimaryKey As Boolean, ByVal doNotInsert As Boolean, ByVal doNotUpdate As Boolean)
            Me.New(className, propertyName, tableName, propertyName, parameterType, isPrimaryKey)
            Me.DoNotInsert = doNotInsert
            Me.DoNotUpdate = doNotUpdate
        End Sub

        Public Function Clone() As DatabaseParameter
            Return DirectCast(_Clone(), DatabaseParameter)
        End Function
        Private Function _Clone() As Object Implements System.ICloneable.Clone
            Return Me.MemberwiseClone()
        End Function
    End Class

    <Serializable()> _
    Public Class DatabaseParameterValues
        Implements IEnumerable(Of DatabaseParameterValue)
        Implements ICloneable

        Private _items As List(Of DatabaseParameterValue)
        Private _keys As List(Of String)

        Private _tempKey As String = ""

        Default Public ReadOnly Property Items(ByVal index As Integer) As DatabaseParameterValue
            Get
                Return Me._items(index)
            End Get
        End Property
        Default Public ReadOnly Property Items(ByVal key As String) As DatabaseParameterValue
            Get
                _tempKey = key
                Dim index As Integer = _keys.FindIndex(AddressOf _FindKeyIndex)
                If index > -1 Then
                    Return Me._items(index)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Private Function _FindKeyIndex(ByVal key As String) As Boolean
            If String.Compare(key, _tempKey, True) = 0 Then
                Return True
            Else
                Return False
            End If
        End Function


        Public ReadOnly Property Count() As Integer
            Get
                Return _items.Count
            End Get
        End Property

        Public Sub New()
            _items = New List(Of DatabaseParameterValue)
            _keys = New List(Of String)
        End Sub

        Public Function Add(ByVal parameter As DatabaseParameter, ByVal value As Object) As Integer
            Return Add(New DatabaseParameterValue(parameter, value))
        End Function

        Public Function Add(ByVal item As DatabaseParameterValue) As Integer
            _items.Add(item)
            _keys.Add(item.Parameter.PropertyName)
            Return _items.Count - 1
        End Function

        Public Sub AddRange(ByVal items As DatabaseParameterValues)
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub AddRange(ByVal items As List(Of DatabaseParameterValue))
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub Clear()
            _items.Clear()
            _keys.Clear()
        End Sub

        Public Sub Remove(ByVal item As DatabaseParameterValue)
            _items.Remove(item)
            _keys.Remove(item.Parameter.ParameterName)
        End Sub
        Public Sub RemoveAt(ByVal index As Integer)
            _items.RemoveAt(index)
            _keys.RemoveAt(index)
        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of DatabaseParameterValue) Implements System.Collections.Generic.IEnumerable(Of DatabaseParameterValue).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function Clone() As DatabaseParameterValues
            Return DirectCast(_Clone(), DatabaseParameterValues)
        End Function
        Private Function _Clone() As Object Implements System.ICloneable.Clone
            Dim dpvs As New DatabaseParameterValues
            For i As Integer = 0 To _items.Count - 1
                dpvs.Add(_items(i).Clone())
            Next
            Return dpvs
        End Function
    End Class

    <Serializable()> _
    Public Class DatabaseParameterValue
        Implements ICloneable

        Private _parameter As DatabaseParameter
        Public Property Parameter() As DatabaseParameter
            Get
                Return _parameter
            End Get
            Set(ByVal value As DatabaseParameter)
                _parameter = value
            End Set
        End Property

        Private _value As Object
        Public Property Value() As Object
            Get
                Return _value
            End Get
            Set(ByVal value As Object)
                _value = value
            End Set
        End Property

        Public Sub New()

        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal value As Object)
            Me.New()
            Me.Parameter = parameter
            Me.Value = value
        End Sub

        Public Function Clone() As DatabaseParameterValue
            Return DirectCast(_Clone(), DatabaseParameterValue)
        End Function
        Private Function _Clone() As Object Implements System.ICloneable.Clone
            Dim dpv As New DatabaseParameterValue
            dpv.Parameter = Me.Parameter.Clone()
            dpv.Value = Me.Value
            Return dpv
        End Function
    End Class

    <Serializable()> _
    Public Class DatabaseCriterias
        Implements IEnumerable(Of DatabaseCriteria)


        Private _itemGroups As List(Of DatabaseCriterias)

        Private _items As List(Of DatabaseCriteria)
        Private _keys As List(Of String)

        Private _tempKey As String = ""


        Public ReadOnly Property HasAnyCriterias() As Boolean
            Get
                If Me.Count > 0 Or Me.GroupCount > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Private _index As Integer
        Public Property Index() As Integer
            Get
                Return _index
            End Get
            Set(ByVal value As Integer)
                _index = value
            End Set
        End Property

        Private _parent As DatabaseCriterias
        Public Property Parent() As DatabaseCriterias
            Get
                Return _parent
            End Get
            Set(ByVal value As DatabaseCriterias)
                _parent = value
            End Set
        End Property

        Private _logicalOperator As LogicalOperatorType
        Public Property LogicalOperator() As LogicalOperatorType
            Get
                Return _logicalOperator
            End Get
            Set(ByVal value As LogicalOperatorType)
                _logicalOperator = value
            End Set
        End Property

        Default Public ReadOnly Property Items(ByVal index As Integer) As DatabaseCriteria
            Get
                Return Me._items(index)
            End Get
        End Property
        Default Public ReadOnly Property Items(ByVal key As String) As DatabaseCriteria
            Get
                _tempKey = key
                Dim index As Integer = _keys.FindIndex(AddressOf _FindKeyIndex)
                If index > -1 Then
                    Return Me._items(index)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public ReadOnly Property ItemGroups(ByVal index As Integer) As DatabaseCriterias
            Get
                Return Me._itemGroups(index)
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return _items.Count
            End Get
        End Property
        Public ReadOnly Property GroupCount() As Integer
            Get
                Return _itemGroups.Count
            End Get
        End Property

        Public ReadOnly Property UniqueId() As String
            Get
                Dim _tempParent As DatabaseCriterias = Me
                Dim str As New StringBuilder
                While _tempParent IsNot Nothing
                    str.Append("_")
                    str.Append(_tempParent.Index.ToString)
                    _tempParent = _tempParent.Parent
                End While
                Return str.ToString
            End Get
        End Property

        Public Sub New()
            _itemGroups = New List(Of DatabaseCriterias)
            _items = New List(Of DatabaseCriteria)
            _keys = New List(Of String)
        End Sub

        Public Sub New(ByVal logicalOperator As LogicalOperatorType)
            Me.New()
            Me.LogicalOperator = logicalOperator
        End Sub

        Public Function AddGroup(ByVal item As DatabaseCriterias) As Integer
            item.Parent = Me
            item.Index = _itemGroups.Count
            _itemGroups.Add(item)
            Return _itemGroups.Count - 1
        End Function
        Public Sub RemoveGroup(ByVal Item As DatabaseCriterias)
            _itemGroups.Remove(Item)
        End Sub

        Public Function Add(ByVal subQuery As DatabaseQuery, ByVal value As Object, ByVal criteriaType As SqlDbType) As Integer
            Return Add(subQuery, value, criteriaType, DatabaseCriteriaOperatorEnum.Equals, Nothing)
        End Function
        Public Function Add(ByVal subQuery As DatabaseQuery, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperatorEnum) As Integer
            Return Add(subQuery, value, [operator], Nothing)
        End Function
        Public Function Add(ByVal subQuery As DatabaseQuery, ByVal value As Object, ByVal criteriaType As SqlDbType, ByVal [operator] As DatabaseCriteriaOperatorEnum, ByVal [function] As String) As Integer
            Return Add(New DatabaseCriteria(subQuery, value, criteriaType, [operator], [function]))
        End Function

        Public Function Add(ByVal parameter As DatabaseParameter, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperatorEnum, ByVal [function] As String) As Integer
            Return Add(New DatabaseCriteria(parameter, value, [operator], [function]))
        End Function
        Public Function Add(ByVal parameter As DatabaseParameter, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperatorEnum) As Integer
            Return Add(New DatabaseCriteria(parameter, value, [operator]))
        End Function
        Public Function Add(ByVal parameter As DatabaseParameter, ByVal value As Object, ByVal [function] As String) As Integer
            Return Add(New DatabaseCriteria(parameter, value, DatabaseCriteriaOperatorEnum.Equals, [function]))
        End Function
        Public Function Add(ByVal parameter As Object, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperatorEnum, ByVal [function] As String) As Integer
            Return Add(New DatabaseCriteria(parameter, value, [operator], [function]))
        End Function
        Public Function Add(ByVal parameter As Object, ByVal value As Object, ByVal [function] As String) As Integer
            Return Add(New DatabaseCriteria(parameter, value, DatabaseCriteriaOperatorEnum.Equals, [function]))
        End Function
        Public Function Add(ByVal parameter As DatabaseParameter, ByVal value As Object) As Integer
            Return Add(New DatabaseCriteria(parameter, value))
        End Function
        Public Function Add(ByVal item As DatabaseCriterias) As Integer
            Return AddGroup(item)
        End Function

        Public Function Add(ByVal item As DatabaseCriteria) As Integer
            item.Parent = Me
            item.Index = Me.Count

            _items.Add(item)
            _keys.Add(item.Index.ToString)

            Return _items.Count - 1
        End Function

        Public Sub AddRange(ByVal items As DatabaseCriterias)
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub AddRange(ByVal items As List(Of DatabaseCriteria))
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub Clear()
            _items.Clear()
            _keys.Clear()
        End Sub
        Public Sub Remove(ByVal item As DatabaseCriteria)
            _items.Remove(item)
            _keys.Remove(item.Index.ToString)
        End Sub
        Public Sub RemoveAt(ByVal index As Integer)
            _items.RemoveAt(index)
            _keys.RemoveAt(index)
        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of DatabaseCriteria) Implements System.Collections.Generic.IEnumerable(Of DatabaseCriteria).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function
        Private Function _FindKeyIndex(ByVal key As String) As Boolean
            If String.Compare(key, _tempKey, True) = 0 Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class

    <Serializable()> _
    Public Class DatabaseCriteria

        Private _index As Integer
        Public Property Index() As Integer
            Get
                Return _index
            End Get
            Set(ByVal value As Integer)
                _index = value
            End Set
        End Property

        Private _parent As DatabaseCriterias
        Public Property Parent() As DatabaseCriterias
            Get
                Return _parent
            End Get
            Set(ByVal value As DatabaseCriterias)
                _parent = value
            End Set
        End Property

        Public ReadOnly Property UniqueId() As String
            Get
                Return "_" & Index.ToString & Me.Parent.UniqueId
            End Get
        End Property

        Public ReadOnly Property UniqueName() As String
            Get
                Dim rtn As String = ""

                If Parameter.GetType Is GetType(DatabaseParameter) Then
                    rtn &= DirectCast(Parameter, DatabaseParameter).ParameterName
                End If

                Return rtn & UniqueId
            End Get
        End Property

        Private _parameter As Object
        Public Property Parameter() As Object
            Get
                Return _parameter
            End Get
            Set(ByVal value As Object)
                _parameter = value
            End Set
        End Property

        Private _value As Object
        Public Property Value() As Object
            Get
                Return _value
            End Get
            Set(ByVal value As Object)
                If value IsNot Nothing AndAlso value.GetType Is GetType(DatabaseQuery) Then
                    DirectCast(value, DatabaseQuery).ParentCriteria = Me
                End If
                _value = value
            End Set
        End Property

        Private _operator As DatabaseCriteriaOperator
        Public Property [Operator]() As DatabaseCriteriaOperator
            Get
                Return _operator
            End Get
            Set(ByVal value As DatabaseCriteriaOperator)
                _operator = value
            End Set
        End Property

        Private _criteriaType As SqlDbType
        Public Property CriteriaType() As SqlDbType
            Get
                Return _criteriaType
            End Get
            Set(ByVal value As SqlDbType)
                _criteriaType = value
            End Set
        End Property

        Private _function As String
        Public Property [Function]() As String
            Get
                Return _function
            End Get
            Set(ByVal value As String)
                _function = value
            End Set
        End Property

        Public Sub New()

        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperator, ByVal [function] As String)
            Me.New()
            Me.Parameter = parameter
            Me.CriteriaType = parameter.ParameterType
            Me.Value = value
            Me.Operator = [operator]
            Me.Function = [function]
            If Me.Function.IsEmpty() Then
                'Me.Function = parameter.Function
            End If
        End Sub
        Public Sub New(ByVal parameter As Object, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperator, ByVal [function] As String)
            Me.New()
            Me.Parameter = parameter
            'Me.CriteriaType 
            Me.Value = value
            Me.Operator = [operator]
            Me.Function = [function]
            If Me.Function.IsEmpty() Then
                'Me.Function = parameter.Function
            End If
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperatorEnum, ByVal [function] As String)
            Me.New(parameter, value, New DatabaseCriteriaOperator([operator]), [function])
        End Sub
        Public Sub New(ByVal parameter As Object, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperatorEnum, ByVal [function] As String)
            Me.New(parameter, value, New DatabaseCriteriaOperator([operator]), [function])
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperatorEnum)
            Me.New(parameter, value, New DatabaseCriteriaOperator([operator]), Nothing)
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal value As Object, ByVal [operator] As DatabaseCriteriaOperator)
            Me.New(parameter, value, [operator], Nothing)
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal value As Object)
            Me.New(parameter, value, DatabaseCriteriaOperatorEnum.Equals)
        End Sub
        Public Sub New(ByVal parameter As DatabaseQuery, ByVal value As Object, ByVal criteriaType As SqlDbType, ByVal [operator] As DatabaseCriteriaOperatorEnum, ByVal [function] As String)
            Me.New()
            Me.Parameter = parameter
            Me.CriteriaType = criteriaType
            Me.Value = value
            Me.Operator = New DatabaseCriteriaOperator([operator])
            Me.Function = [function]
        End Sub

    End Class

    <Serializable()> _
    Public Class DatabaseJoins
        Implements IEnumerable(Of DatabaseJoin)

        Private _items As List(Of DatabaseJoin)
        'Private _keys As List(Of String)

        'Private _tempKey As String = ""

        Default Public ReadOnly Property Items(ByVal index As Integer) As DatabaseJoin
            Get
                Return Me._items(index)
            End Get
        End Property
        'Default Public ReadOnly Property Items(ByVal key As String) As DatabaseJoin
        '    Get
        '        _tempKey = key
        '        Return Me._items(_keys.FindIndex(AddressOf _FindKeyIndex))
        '    End Get
        'End Property

        'Private Function _FindKeyIndex(ByVal key As String) As Boolean
        '    If key = _tempKey Then
        '        Return True
        '    Else
        '        Return False
        '    End If
        'End Function

        Private _FromTableName As String
        Public Property FromTableName() As String
            Get
                Return _FromTableName
            End Get
            Set(ByVal value As String)
                _FromTableName = value
            End Set
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return _items.Count
            End Get
        End Property

        Public Sub New()
            _items = New List(Of DatabaseJoin)
            '_keys = New List(Of String)
        End Sub
        Public Sub New(ByVal fromTableName As String)
            Me.New()
            Me.FromTableName = fromTableName
        End Sub

        Public Function Add(ByVal joinTo As DatabaseParameter, ByVal joinOn As DatabaseParameter, ByVal joinType As DatabaseJoinTypeEnum, ByVal [alias] As String) As Integer
            Return Add(New DatabaseJoin(joinTo, joinOn, joinType, [alias]))
        End Function
        Public Function Add(ByVal joinTo As DatabaseParameter, ByVal joinOn As DatabaseParameter, ByVal joinType As DatabaseJoinTypeEnum) As Integer
            Return Add(New DatabaseJoin(joinTo, joinOn, joinType))
        End Function
        Public Function Add(ByVal joinTo As DatabaseParameter, ByVal joinOn As DatabaseParameter) As Integer
            Return Add(New DatabaseJoin(joinTo, joinOn))
        End Function

        Public Function Add(ByVal item As DatabaseJoin) As Integer
            _items.Add(item)
            '_keys.Add(item.Parameter.PropertyName)
            Return _items.Count - 1
        End Function

        Public Sub AddRange(ByVal items As DatabaseJoins)
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub AddRange(ByVal items As List(Of DatabaseJoin))
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub Clear()
            _items.Clear()
        End Sub
        Public Sub Remove(ByVal item As DatabaseJoin)
            _items.Remove(item)
            '_keys.Remove(item.Parameter.ParameterName)
        End Sub
        Public Sub RemoveAt(ByVal index As Integer)
            _items.RemoveAt(index)
            '_keys.RemoveAt(index)
        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of DatabaseJoin) Implements System.Collections.Generic.IEnumerable(Of DatabaseJoin).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _items.GetEnumerator
        End Function
    End Class

    <Serializable()> _
    Public Class DatabaseJoin

        Private _parameter1 As DatabaseParameter
        Public Property JoinToParameter() As DatabaseParameter
            Get
                Return _parameter1
            End Get
            Set(ByVal value As DatabaseParameter)
                _parameter1 = value
            End Set
        End Property
        Private _parameter2 As DatabaseParameter
        Public Property JoinOnParameter() As DatabaseParameter
            Get
                Return _parameter2
            End Get
            Set(ByVal value As DatabaseParameter)
                _parameter2 = value
            End Set
        End Property

        Private _joinType As DatabaseJoinType
        Public Property JoinType() As DatabaseJoinType
            Get
                Return _joinType
            End Get
            Set(ByVal value As DatabaseJoinType)
                _joinType = value
            End Set
        End Property

        Private _alias As String
        Public Property [Alias]() As String
            Get
                Return _alias
            End Get
            Set(ByVal value As String)
                _alias = value
            End Set
        End Property

        Public Sub New()

        End Sub

        Public Sub New(ByVal joinToParameter As DatabaseParameter, ByVal joinOnParameter As DatabaseParameter, ByVal joinType As DatabaseJoinTypeEnum, ByVal [alias] As String)
            Me.New(joinToParameter, joinOnParameter, New DatabaseJoinType(joinType), [alias])
        End Sub
        Public Sub New(ByVal joinToParameter As DatabaseParameter, ByVal joinOnParameter As DatabaseParameter, ByVal joinType As DatabaseJoinType, ByVal [alias] As String)
            Me.New()
            Me.JoinToParameter = joinToParameter
            Me.JoinOnParameter = joinOnParameter
            Me.JoinType = joinType
            Me.Alias = [alias]
        End Sub

        Public Sub New(ByVal joinToParameter As DatabaseParameter, ByVal joinOnParameter As DatabaseParameter, ByVal joinType As DatabaseJoinType)
            Me.New()
            Me.JoinToParameter = joinToParameter
            Me.JoinOnParameter = joinOnParameter
            Me.JoinType = joinType
        End Sub
        Public Sub New(ByVal joinToParameter As DatabaseParameter, ByVal joinOnParameter As DatabaseParameter, ByVal joinType As DatabaseJoinTypeEnum)
            Me.New(joinToParameter, joinOnParameter, New DatabaseJoinType(joinType))
        End Sub
        Public Sub New(ByVal joinToParameter As DatabaseParameter, ByVal joinOnParameter As DatabaseParameter)
            Me.New(joinToParameter, joinOnParameter, DatabaseJoinTypeEnum.InnerJoin)
        End Sub
    End Class

    <Serializable()> _
    Public Class DatabaseOrderBys
        Implements IEnumerable(Of DatabaseOrderBy)

        Private _items As List(Of DatabaseOrderBy)
        Private _keys As List(Of String)

        Private _tempKey As String = ""

        Default Public ReadOnly Property Items(ByVal index As Integer) As DatabaseOrderBy
            Get
                Return Me._items(index)
            End Get
        End Property
        Default Public ReadOnly Property Items(ByVal key As String) As DatabaseOrderBy
            Get
                _tempKey = key
                Dim index As Integer = _keys.FindIndex(AddressOf _FindKeyIndex)
                If index > -1 Then
                    Return Me._items(index)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Private Function _FindKeyIndex(ByVal key As String) As Boolean
            If String.Compare(key, _tempKey, True) = 0 Then
                Return True
            Else
                Return False
            End If
        End Function

        Public ReadOnly Property Count() As Integer
            Get
                Return _items.Count
            End Get
        End Property
        Public Sub New()
            _items = New List(Of DatabaseOrderBy)
            _keys = New List(Of String)
        End Sub

        Public Function Add(ByVal parameter As DatabaseParameter, ByVal direction As OrderByDirectionEnum) As Integer
            Dim item As New DatabaseOrderBy(parameter, direction)
            _items.Add(item)
            _keys.Add(item.ParameterFullName)
            Return _items.Count - 1
        End Function
        Public Function Add(ByVal parameterFullName As String, ByVal direction As OrderByDirectionEnum) As Integer
            Dim item As New DatabaseOrderBy(parameterFullName, direction)
            _items.Add(item)
            _keys.Add(item.ParameterFullName)
            Return _items.Count - 1
        End Function
        Public Function Add(ByVal parameter As DatabaseParameter) As Integer
            Return Add(parameter, OrderByDirectionEnum.Ascending)
        End Function
        Public Function Add(ByVal parameter As DatabaseParameter, ByVal direction As OrderByDirection) As Integer
            Dim item As New DatabaseOrderBy(parameter, direction)
            _items.Add(item)
            _keys.Add(item.ParameterFullName)
            Return _items.Count - 1
        End Function
        Public Function Add(ByVal item As DatabaseOrderBy) As Integer
            _items.Add(item)
            _keys.Add(item.ParameterFullName)
            Return _items.Count - 1
        End Function

        Public Sub AddRange(ByVal items As DatabaseOrderBys)
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub AddRange(ByVal items As List(Of DatabaseOrderBy))
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub Clear()
            _items.Clear()
            _keys.Clear()
        End Sub
        Public Sub Remove(ByVal item As DatabaseOrderBy)
            _items.Remove(item)
            _keys.Remove(item.ParameterFullName)
        End Sub
        Public Sub RemoveAt(ByVal index As Integer)
            _items.RemoveAt(index)
            _keys.RemoveAt(index)
        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of DatabaseOrderBy) Implements System.Collections.Generic.IEnumerable(Of DatabaseOrderBy).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function
    End Class

    <Serializable()> _
    Public Class DatabaseOrderBy

        'Private _parameter As DatabaseParameter
        'Public Property Parameter() As DatabaseParameter
        '    Get
        '        Return _parameter
        '    End Get
        '    Set(ByVal value As DatabaseParameter)
        '        _parameter = value
        '    End Set
        'End Property

        Private _ParameterFullName As String
        Public Property ParameterFullName() As String
            Get
                Return _ParameterFullName
            End Get
            Set(ByVal value As String)
                _ParameterFullName = value
            End Set
        End Property

        Private _direction As OrderByDirection
        Public Property Direction() As OrderByDirection
            Get
                Return _direction
            End Get
            Set(ByVal value As OrderByDirection)
                _direction = value
            End Set
        End Property

        Public Sub New()

        End Sub
        Public Sub New(ByVal parameterFullName As String)
            Me.New()
            Me.ParameterFullName = parameterFullName
        End Sub
        Public Sub New(ByVal parameterFullName As String, ByVal direction As OrderByDirection)
            Me.New(parameterFullName)
            Me.Direction = direction
        End Sub
        Public Sub New(ByVal parameterFullName As String, ByVal direction As OrderByDirectionEnum)
            Me.New(parameterFullName, New OrderByDirection(direction))
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal direction As OrderByDirection)
            Me.New(parameter.ParameterFullName, direction)
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter, ByVal direction As OrderByDirectionEnum)
            Me.New(parameter.ParameterFullName, New OrderByDirection(direction))
        End Sub

    End Class

    <Serializable()> _
    Public Class DatabaseGroupBys
        Implements IEnumerable(Of DatabaseGroupBy)

        Private _items As List(Of DatabaseGroupBy)
        Private _keys As List(Of String)

        Private _tempKey As String = ""

        Default Public ReadOnly Property Items(ByVal index As Integer) As DatabaseGroupBy
            Get
                Return Me._items(index)
            End Get
        End Property
        Default Public ReadOnly Property Items(ByVal key As String) As DatabaseGroupBy
            Get
                _tempKey = key
                Dim index As Integer = _keys.FindIndex(AddressOf _FindKeyIndex)
                If index > -1 Then
                    Return Me._items(index)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Private Function _FindKeyIndex(ByVal key As String) As Boolean
            If String.Compare(key, _tempKey, True) = 0 Then
                Return True
            Else
                Return False
            End If
        End Function

        Public ReadOnly Property Count() As Integer
            Get
                Return _items.Count
            End Get
        End Property
        Public Sub New()
            _items = New List(Of DatabaseGroupBy)
            _keys = New List(Of String)
        End Sub
        Public Function Add(ByVal parameterFullName As String) As Integer
            Return Add(New DatabaseGroupBy(parameterFullName))
        End Function
        Public Function Add(ByVal parameter As DatabaseParameter) As Integer
            Return Add(New DatabaseGroupBy(parameter))
        End Function
        Public Function Add(ByVal item As DatabaseGroupBy) As Integer
            _items.Add(item)
            _keys.Add(item.ParameterFullName)
            Return _items.Count - 1
        End Function

        Public Sub AddRange(ByVal items As DatabaseGroupBys)
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub AddRange(ByVal items As List(Of DatabaseGroupBy))
            For i As Integer = 0 To items.Count - 1
                Add(items(i))
            Next
        End Sub

        Public Sub Clear()
            _items.Clear()
            _keys.Clear()
        End Sub
        Public Sub Remove(ByVal item As DatabaseGroupBy)
            _items.Remove(item)
            _keys.Remove(item.ParameterFullName)
        End Sub
        Public Sub RemoveAt(ByVal index As Integer)
            _items.RemoveAt(index)
            _keys.RemoveAt(index)
        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of DatabaseGroupBy) Implements System.Collections.Generic.IEnumerable(Of DatabaseGroupBy).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function
    End Class

    <Serializable()> _
    Public Class DatabaseGroupBy

        Private _ParameterFullName As String
        Public Property ParameterFullName() As String
            Get
                Return _ParameterFullName
            End Get
            Set(ByVal value As String)
                _ParameterFullName = value
            End Set
        End Property

        Public Sub New()

        End Sub
        Public Sub New(ByVal fullParameterName As String)
            Me.New()
            Me.ParameterFullName = fullParameterName
        End Sub
        Public Sub New(ByVal parameter As DatabaseParameter)
            Me.New(parameter.ParameterFullName)
        End Sub

    End Class

    <Serializable()> _
    Public Class DatabaseConnection
        Implements IDisposable

        Private _sqlDatabaseType As SqlDatabaseType
        Public Property SqlDatabaseType() As SqlDatabaseType
            Get
                Return _sqlDatabaseType
            End Get
            Set(ByVal value As SqlDatabaseType)
                _sqlDatabaseType = value
            End Set
        End Property

        'Private _sqlCeConnection As Data.SqlServerCe.SqlCeConnection
        'Public Property SqlCeConnection() As Data.SqlServerCe.SqlCeConnection
        '    Get
        '        Return _sqlCeConnection
        '    End Get
        '    Set(ByVal value As Data.SqlServerCe.SqlCeConnection)
        '        _sqlCeConnection = value
        '    End Set
        'End Property

        'Private _sqlCeTransaction As Data.SqlServerCe.SqlCeTransaction
        'Public Property SqlCeTransaction() As Data.SqlServerCe.SqlCeTransaction
        '    Get
        '        Return _sqlCeTransaction
        '    End Get
        '    Set(ByVal value As Data.SqlServerCe.SqlCeTransaction)
        '        _sqlCeTransaction = value
        '    End Set
        'End Property

        Private _sqlconnection As SqlConnection
        Private Property SqlConnection() As SqlConnection
            Get
                Return _sqlconnection
            End Get
            Set(ByVal value As SqlConnection)
                _sqlconnection = value
            End Set
        End Property

        Private _sqltransaction As SqlTransaction
        Private Property SqlTransaction() As SqlTransaction
            Get
                Return _sqltransaction
            End Get
            Set(ByVal value As SqlTransaction)
                _sqltransaction = value
            End Set
        End Property

        Private _connectionString As String
        Public Property ConnectionString() As String
            Get
                Return _connectionString
            End Get
            Set(ByVal value As String)
                _connectionString = value
            End Set
        End Property

        Public ReadOnly Property IsOpen() As Boolean
            Get
                If Me.SqlConnection IsNot Nothing AndAlso Me.SqlConnection.State = ConnectionState.Open Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Private _isTransactionStarted As Boolean
        Public ReadOnly Property IsTransactionStarted() As Boolean
            Get
                Return _isTransactionStarted
            End Get
        End Property

        Public Sub New()
			Me.SqlDatabaseType = DataAccess.SqlDatabaseType.SqlServer
        End Sub
        Public Sub New(ByVal connectionString As String)
            Me.New()
            Me.ConnectionString = connectionString
            Me.SqlDatabaseType = DataAccess.SqlDatabaseType.SqlServer
        End Sub
        Public Sub New(ByVal connectionString As String, ByVal sqlDatabaseType As SqlDatabaseType)
            Me.New(connectionString)
            Me.SqlDatabaseType = sqlDatabaseType
        End Sub
        Public Sub New(ByVal connection As SqlConnection)
            Me.New()
            Me.SqlConnection = connection
            Me.SqlDatabaseType = DataAccess.SqlDatabaseType.SqlServer
        End Sub
        Public Sub New(ByVal connection As SqlConnection, ByVal transaction As SqlTransaction)
            Me.New(connection)
            Me.SqlTransaction = transaction
            Me._isTransactionStarted = True
        End Sub

        Private _sqlCommand As SqlCommand
        Public Function CreateSqlCommand(ByVal sql As String) As SqlCommand
            If IsOpen Then
                If IsTransactionStarted Then
                    _sqlCommand = New SqlCommand(sql, SqlConnection, SqlTransaction)
                Else
                    _sqlCommand = New SqlCommand(sql, SqlConnection)
                End If
                Return _sqlCommand
            Else
                Throw New Exception("Connection must be opened")
            End If
        End Function
        Public Function GetSqlCommand() As SqlCommand
            Return _sqlCommand
        End Function

        Public Sub Open()
            If Me.SqlConnection Is Nothing Then
                If ConnectionString IsNot Nothing Then
                    Me.SqlConnection = New SqlConnection(ConnectionString)
                Else
                    Throw New Exception("Must set connection string")
                End If
            End If
            If IsOpen = False Then
                Me.SqlConnection.Open()
            End If
        End Sub

        Public Sub Close()
            If IsOpen Then
                Me.SqlConnection.Close()
            End If
        End Sub

        Public Sub BeginTransaction()
            If IsOpen = False Then
                Throw New Exception("Connection must be opened")
            Else
                If IsTransactionStarted = False Then
                    Me.SqlTransaction = Me.SqlConnection.BeginTransaction()
                    _isTransactionStarted = True
                End If
            End If

        End Sub

        Public Sub CommitTransaction()
            If IsOpen = False Then
                Throw New Exception("Connection must be opened")
            Else
                If IsTransactionStarted Then
                    Me.SqlTransaction.Commit()

                    _isTransactionStarted = False
                Else
                    Throw New Exception("Transaction must be started")
                End If
            End If
        End Sub

        Public Sub RollbackTransaction()
            If IsOpen = False Then
                Throw New Exception("Connection must be opened")
            Else
                If IsTransactionStarted Then
                    Me.SqlTransaction.Rollback()

                    _isTransactionStarted = False
                Else
                    Throw New Exception("Transaction must be started")
                End If
            End If
        End Sub

        Public Function TestConnection() As SystemMessages
            Dim msgs As New SystemMessages
            Try
                Me.Open()
            Catch ex As Exception
                msgs.Add(ex.Message, MessageType.Error, ex)
            Finally

                If IsOpen Then
                    Me.Close()
                End If

            End Try

            Return msgs

        End Function

        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    If SqlTransaction IsNot Nothing Then
                        SqlTransaction.Dispose()
                    End If
                    If SqlConnection IsNot Nothing Then
                        If SqlConnection.State = ConnectionState.Open Then
                            SqlConnection.Close()
                        End If
                        SqlConnection.Dispose()
                    End If
                    ' TODO: free other state (managed objects).
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
                SqlTransaction = Nothing
                SqlConnection = Nothing

            End If
            Me.disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

#Region "Helper Objects"

    <Serializable()> _
    Public Class DatabaseCriteriaOperator

        Private Shadows ReadOnly Property Equals() As String
            Get
                Return "="
            End Get
        End Property
        Private ReadOnly Property NotEqualTo() As String
            Get
                Return "!="
            End Get
        End Property
        Private ReadOnly Property LessThan() As String
            Get
                Return "<"
            End Get
        End Property
        Private ReadOnly Property LessThanOrEqualTo() As String
            Get
                Return "<="
            End Get
        End Property
        Private ReadOnly Property GreaterThan() As String
            Get
                Return ">"
            End Get
        End Property
        Private ReadOnly Property GreaterThanOrEqualTo() As String
            Get
                Return ">="
            End Get
        End Property
        Private ReadOnly Property [Like]() As String
            Get
                Return "LIKE"
            End Get
        End Property
        Private ReadOnly Property [In]() As String
            Get
                Return "IN"
            End Get
        End Property
        Private ReadOnly Property NotIn() As String
            Get
                Return "NOT IN"
            End Get
        End Property
        Private ReadOnly Property [Is]() As String
            Get
                Return "IS"
            End Get
        End Property
        Private ReadOnly Property [IsNot]() As String
            Get
                Return "IS NOT"
            End Get
        End Property

        Private _value As DatabaseCriteriaOperatorEnum
        Public Property Value() As DatabaseCriteriaOperatorEnum
            Get
                Return _value
            End Get
            Set(ByVal value As DatabaseCriteriaOperatorEnum)
                Me._value = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Dim rtn As String = ""
            Select Case Me.Value
                Case DatabaseCriteriaOperatorEnum.Equals
                    rtn = Me.Equals
                Case DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter
                    rtn = Me.GreaterThan
                Case DatabaseCriteriaOperatorEnum.DBValueIsLessThanParameter
                    rtn = Me.LessThan
                Case DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter
                    rtn = Me.GreaterThanOrEqualTo
                Case DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter
                    rtn = Me.LessThanOrEqualTo
                Case DatabaseCriteriaOperatorEnum.Like, DatabaseCriteriaOperatorEnum.LikeLeft, DatabaseCriteriaOperatorEnum.LikeRight, DatabaseCriteriaOperatorEnum.LikeWordMatch, DatabaseCriteriaOperatorEnum.LikeWordMatchLeft, DatabaseCriteriaOperatorEnum.LikeWordMatchRight
                    rtn = Me.Like
                Case DatabaseCriteriaOperatorEnum.In
                    rtn = Me.In
                Case DatabaseCriteriaOperatorEnum.NotIn
                    rtn = Me.NotIn
                Case DatabaseCriteriaOperatorEnum.Is
                    rtn = Me.Is
                Case DatabaseCriteriaOperatorEnum.IsNot
                    rtn = Me.IsNot
                Case DatabaseCriteriaOperatorEnum.NotEqualTo
                    rtn = Me.NotEqualTo
            End Select
            Return rtn
        End Function


        Public Sub New()

        End Sub
        Public Sub New(ByVal pDatabaseCriteriaOperatorEnum As DatabaseCriteriaOperatorEnum)
            Me.Value = pDatabaseCriteriaOperatorEnum
        End Sub

    End Class

    <Serializable()> _
    Public Class DatabaseJoinType

        Private _value As DatabaseJoinTypeEnum
        Public Property Value() As DatabaseJoinTypeEnum
            Get
                Return _value
            End Get
            Set(ByVal value As DatabaseJoinTypeEnum)
                _value = value
            End Set
        End Property

        Public Sub New(ByVal joinType As DatabaseJoinTypeEnum)
            Me.Value = joinType
        End Sub

        Public Shadows Function ToString() As String
            Return Value.ToString().SplitByUpperCase.ToUpper
        End Function

    End Class

    <Serializable()> _
    Public Class OrderByDirection

        Private _direction As OrderByDirectionEnum
        Public Property Direction() As OrderByDirectionEnum
            Get
                Return _direction
            End Get
            Set(ByVal value As OrderByDirectionEnum)
                _direction = value
            End Set
        End Property

        Public Sub New()

        End Sub

        Public Sub New(ByVal pOrderByDirection As OrderByDirectionEnum)
            Me.Direction = pOrderByDirection
        End Sub

        Public Overrides Function ToString() As String

            If Direction = OrderByDirectionEnum.Ascending Then
                Return "Asc"
            ElseIf Direction = OrderByDirectionEnum.Descending Then
                Return "Desc"
            Else
                Return ""
            End If

        End Function

    End Class

    <Serializable()>
    Public Class StartAndLength

        Private _start As Integer?
        Public Property Start() As Integer?
            Get
                Return _start
            End Get
            Set(ByVal value As Integer?)
                _start = value
            End Set
        End Property

        Private _length As Integer?
        Public Property Length() As Integer?
            Get
                Return _length
            End Get
            Set(ByVal value As Integer?)
                _length = value
            End Set
        End Property
        Public Sub New()

        End Sub
        Public Sub New(ByVal start As Integer, ByVal length As Integer)
            Me.New()
            Me.Start = start
            Me.Length = length
        End Sub

    End Class

    <Serializable()>
    Public Class TableSample
        Private _RowsDesired As Integer?
        Public ReadOnly Property RowsDesired() As Integer?
            Get
                Return _RowsDesired
            End Get
        End Property

        Private _PercentDesired As Integer?
        Public ReadOnly Property PercentDesired() As Integer?
            Get
                Return _PercentDesired
            End Get
        End Property

        Public Sub New()

        End Sub
        Public Sub New(ByVal amountDesired As Integer, ByVal usePercentage As Boolean)
            Me.New()
            If usePercentage Then
                Me._PercentDesired = amountDesired
            Else
                Me._RowsDesired = amountDesired
            End If
        End Sub

        Public Overrides Function ToString() As String

            Dim str As String = ""

            If RowsDesired.HasValue Then
                str = RowsDesired.Value.ToString() & " ROWS "
            ElseIf PercentDesired.HasValue Then
                str = RowsDesired.Value.ToString() & " PERCENT "
            End If

            If str.IsNotEmpty() Then
                Return "TABLESAMPLE (" & str & ")"
            End If

            Return str
        End Function

    End Class

    <Serializable()> _
    Public Class DatabaseParameterDictionary
        Implements IEnumerable(Of Object)

        Private _dbParameters As DatabaseParameterValues

        Private _tempKey As String = ""

        Default Public Property Items(ByVal index As Integer) As Object
            Get
                Return Me._dbParameters(index).Value
            End Get
            Set(value As Object)
                Me._dbParameters(index).Value = value
            End Set
        End Property
        Default Public Property Items(ByVal key As String) As Object
            Get
                Return Me._dbParameters(key).Value
            End Get
            Set(value As Object)
                Me._dbParameters(key).Value = value
            End Set
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return _dbParameters.Count
            End Get
        End Property

        Public Sub New(ByVal dbParameters As DatabaseParameterValues)
            _dbParameters = dbParameters
        End Sub

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of Object) Implements System.Collections.Generic.IEnumerable(Of Object).GetEnumerator
            Return _dbParameters.GetEnumerator()
        End Function

        Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return _dbParameters.GetEnumerator()
        End Function
    End Class

#End Region

#Region "Enumerations"

    Public Enum SqlDatabaseType
        SqlServer
		SqlCE
		OleDB
    End Enum

    Public Enum DatabaseJoinTypeEnum
        LeftOuterJoin
        RightOuterJoin
        InnerJoin
        FullOuterJoin
    End Enum

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
        NotIn
        [Is]
        [IsNot]
    End Enum

    Public Enum OrderByDirectionEnum
        Ascending
        Descending
    End Enum

    Public Enum LogicalOperatorType
        [And]
        [Or]
    End Enum

    Public Enum SaveType
        NotSet = 0
        Insert = 1
        Update = 2
    End Enum

#End Region
End Namespace