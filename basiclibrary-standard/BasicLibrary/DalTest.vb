Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Linq
Imports System.Reflection
Imports System.Text

Namespace DataAccess

	<AttributeUsage(System.AttributeTargets.Class Or System.AttributeTargets.Property)>
	Public Class DalTestSkip
		Inherits System.Attribute
	End Class

	Public Class DalTest
		Public Shared Function TestDatabase(Optional ByVal assembly As Assembly = Nothing) As SystemMessages
			Dim msgs As SystemMessages = New SystemMessages()
            If assembly Is Nothing Then
                assembly = System.Reflection.Assembly.GetExecutingAssembly()
            End If

            For Each type As Type In assembly.GetTypes().Where(
				Function(t)
					Return t.IsSubclassOf(GetType(BasicObject)) AndAlso t.GetCustomAttributes(GetType(DalTestSkip), True).Length < 1
				End Function).OrderBy(
				Function(t)
					Return t.FullName
				End Function).ToList()

				msgs.AddRange(TestClass(type))
			Next

			Return msgs
		End Function

		Private Shared Function TestClass(ByVal type As Type) As SystemMessages
			Dim msgs As SystemMessages = New SystemMessages()
			msgs.Add("Testing " + type.FullName, MessageType.Notice)

			Dim dc As DatabaseConnection = New DatabaseConnection(Utilities.ConnectionString)
			dc.Open()
			dc.BeginTransaction()

			Try
				' Test CRUD methods
				Dim obj As BasicObject = TestClassInit(type)
				msgs.AddRange(TestClassCreate(obj, dc))
				If Not msgs.HasAny(SystemMessages.ErrorTypes) Then
					msgs.AddRange(TestClassRead(obj, dc))
				End If
				If Not msgs.HasAny(SystemMessages.ErrorTypes) Then
					msgs.AddRange(TestClassUpdate(obj, dc))
				End If
				If Not msgs.HasAny(SystemMessages.ErrorTypes) Then
					msgs.AddRange(TestClassDelete(obj, dc))
				End If
			Catch ex As Exception
				msgs.Add("Exception: " & ex.ToString(), MessageType.Error)
			Finally
				dc.RollbackTransaction()
				dc.Close()
				dc.Dispose()
				dc = Nothing
			End Try

			msgs.Clear(MessageType.Success)
			If msgs.HasOnly(MessageType.Notice) Then
				msgs.Clear(MessageType.Notice)
			End If
			If Not msgs.HasAny(SystemMessages.ErrorTypes) Then
				msgs.Add(type.FullName & " - OK", MessageType.Success)
			End If

			Return msgs
		End Function

		Private Shared Function TestClassCreate(ByRef obj As BasicObject, ByRef dc As DatabaseConnection) As SystemMessages
			Dim msgs As SystemMessages = New SystemMessages()
			msgs.Add("Creating", MessageType.Notice)

            Dim saveMethod As MethodInfo = obj.GetType().GetMethod("Save", New Type() {GetType(DatabaseConnection)})
            If saveMethod Is Nothing Then
                msgs.Add("No Save method defined", MessageType.Error)
            Else
                msgs.AddRange(CType(saveMethod.Invoke(obj, New Object() {dc}), SystemMessages))
            End If

            If Not obj.ID.HasValue Or obj.ID.Value < 1 Then
                msgs.Add("Create failed", MessageType.Error)
            End If

            Return msgs
        End Function

        Private Shared Function TestClassRead(ByRef obj As BasicObject, ByRef dc As DatabaseConnection) As SystemMessages
            Dim msgs As SystemMessages = New SystemMessages()
            msgs.Add("Reading", MessageType.Notice)

            Dim obj2 As BasicObject = Nothing
            Dim loadMethod As MethodInfo = obj.GetType().GetMethod("Load", New Type() {GetType(Integer), GetType(DatabaseConnection)})
            If loadMethod Is Nothing Then
                msgs.Add("No Load method defined", MessageType.Error)
            Else
                obj2 = CType(loadMethod.Invoke(obj, New Object() {obj.ID.Value, dc}), BasicObject)
            End If

            If Not msgs.HasAny(SystemMessages.ErrorTypes) AndAlso obj2 Is Nothing Then
                msgs.Add("Read failed", MessageType.Error)
            End If
            If Not msgs.HasAny(SystemMessages.ErrorTypes) Then
                msgs.AddRange(TestClassCompare(obj, obj2))
            End If
            If msgs.HasAny(SystemMessages.ErrorTypes) Then
                msgs.Add("Read failed", MessageType.Error)
            End If

            Return msgs
        End Function

        Private Shared Function TestClassUpdate(ByRef obj As BasicObject, ByRef dc As DatabaseConnection) As SystemMessages
            Dim msgs As SystemMessages = New SystemMessages()
            msgs.Add("Updating", MessageType.Notice)

            Dim type As Type = obj.GetType()
            Dim obj2 As BasicObject = TestClassInit(type)
            obj2.ID = obj.ID
            Dim saveMethod As MethodInfo = obj.GetType().GetMethod("Save", New Type() {GetType(DatabaseConnection)})
            If saveMethod Is Nothing Then
                msgs.Add("No Save method defined", MessageType.Error)
            Else
                msgs.AddRange(CType(saveMethod.Invoke(obj2, New Object() {dc}), SystemMessages))
			End If
			If Not msgs.HasAny(SystemMessages.ErrorTypes) Then
				Dim loadMethod As MethodInfo = obj.GetType().GetMethod("Load", New Type() {GetType(Integer), GetType(DatabaseConnection)})
                If loadMethod Is Nothing Then
                    msgs.Add("No Load method defined", MessageType.Error)
                Else
                    obj = CType(loadMethod.Invoke(obj2, New Object() {obj2.ID.Value, dc}), BasicObject)
				End If

                If Not msgs.HasAny(SystemMessages.ErrorTypes) AndAlso obj Is Nothing Then
                    msgs.Add("Read failed", MessageType.Error)
                End If
                If Not msgs.HasAny(SystemMessages.ErrorTypes) Then
					msgs.AddRange(TestClassCompare(obj, obj2))
				End If
				If msgs.HasAny(SystemMessages.ErrorTypes) Then
					msgs.Add("Update failed", MessageType.Error)
				End If
			End If

			Return msgs
		End Function

		Private Shared Function TestClassDelete(ByRef obj As BasicObject, ByRef dc As DatabaseConnection) As SystemMessages
			Dim msgs As SystemMessages = New SystemMessages()
			msgs.Add("Deleting", MessageType.Notice)

			Dim type As Type = obj.GetType()
			Dim deleteMethod As MethodInfo = obj.GetType().GetMethod("Delete")
            If deleteMethod Is Nothing Then
                msgs.Add("No Delete method defined", MessageType.Error)
            Else
                msgs.AddRange(CType(deleteMethod.Invoke(obj, New Object() {dc}), SystemMessages))
			End If

			If Not msgs.HasAny(SystemMessages.ErrorTypes) Then
				Dim obj2 As BasicObject = Nothing
				Dim loadMethod As MethodInfo = obj.GetType().GetMethod("Load", New Type() {GetType(Integer), GetType(DatabaseConnection)})
                If loadMethod Is Nothing Then
                    msgs.Add("No Load method defined", MessageType.Error)
                Else
                    obj2 = CType(loadMethod.Invoke(obj, New Object() {obj.ID.Value, dc}), BasicObject)
				End If
                If Not obj2 Is Nothing Then
                    msgs.Add("Delete failed", MessageType.Error)
                End If
            End If

			Return msgs
		End Function

		Private Shared Function TestClassCompare(ByRef obj As BasicObject, ByRef obj2 As BasicObject) As SystemMessages
			Dim msgs As SystemMessages = New SystemMessages()
			Dim type As Type = obj.GetType()

			For Each prop As PropertyInfo In type.GetProperties().Where(
				Function(p)
					Return p.CanWrite AndAlso Not IsExcludedProperty(p.Name)
				End Function)

				If prop.PropertyType.Equals(GetType(String)) Then
					If (CType(prop.GetValue(obj, Nothing), String) <> CType(prop.GetValue(obj2, Nothing), String)) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
					End If
				ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Integer))) Then
					If (CType(prop.GetValue(obj, Nothing), Nullable(Of Integer)) <> CType(prop.GetValue(obj2, Nothing), Nullable(Of Integer))) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
					End If
				ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Double))) Then
					If (CType(prop.GetValue(obj, Nothing), Nullable(Of Double)) <> CType(prop.GetValue(obj2, Nothing), Nullable(Of Double))) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
                    End If

                ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Single))) Then
                    If (CType(prop.GetValue(obj, Nothing), Nullable(Of Single)) <> CType(prop.GetValue(obj2, Nothing), Nullable(Of Single))) Then
                        msgs.Add(prop.Name & " not equals", MessageType.Error)
                    End If

                ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Decimal))) Then
                    If (CType(prop.GetValue(obj, Nothing), Nullable(Of Decimal)) <> CType(prop.GetValue(obj2, Nothing), Nullable(Of Decimal))) Then
                        msgs.Add(prop.Name & " not equals", MessageType.Error)
                    End If

				ElseIf prop.PropertyType.Equals(GetType(DateTime)) Then
					If (CType(prop.GetValue(obj, Nothing), DateTime) <> CType(prop.GetValue(obj2, Nothing), DateTime)) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
					End If
				ElseIf prop.PropertyType.Equals(GetType(Nullable(Of DateTime))) Then
					If (CType(prop.GetValue(obj, Nothing), Nullable(Of DateTime)) <> CType(prop.GetValue(obj2, Nothing), Nullable(Of DateTime))) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
					End If
				ElseIf prop.PropertyType.Equals(GetType(Boolean)) Then
					If (CType(prop.GetValue(obj, Nothing), Boolean) <> CType(prop.GetValue(obj2, Nothing), Boolean)) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
					End If
				ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Boolean))) Then
					If (CType(prop.GetValue(obj, Nothing), Nullable(Of Boolean)) <> CType(prop.GetValue(obj2, Nothing), Nullable(Of Boolean))) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
					End If
				ElseIf prop.PropertyType.Equals(GetType(Guid)) Then
					If (CType(prop.GetValue(obj, Nothing), Guid) <> CType(prop.GetValue(obj2, Nothing), Guid)) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
					End If
				ElseIf prop.PropertyType.IsEnum Then
					If Not prop.GetValue(obj, Nothing).Equals(prop.GetValue(obj2, Nothing)) Then
						msgs.Add(prop.Name & " not equals", MessageType.Error)
					End If
				Else
					Throw New NotSupportedException("Cannot compare class, because of unknown data type: " & prop.PropertyType.FullName)
				End If
			Next

			Return msgs
		End Function

		Private Shared Function TestClassInit(ByRef type As Type) As BasicObject
			Dim r As Random = New Random(DateTime.Now.Millisecond)
			Dim obj As BasicObject = CType(Activator.CreateInstance(type), BasicObject)
			For Each prop As PropertyInfo In type.GetProperties().Where(
				Function(p)
					Return p.CanWrite AndAlso Not IsExcludedProperty(p.Name) AndAlso p.GetCustomAttributes(GetType(DalTestSkip), True).Length < 1
				End Function)

				If (prop.PropertyType.Equals(GetType(String))) Then
					prop.SetValue(obj, New String(r.Next(1000, 1000000).ToString()), Nothing)
				ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Integer))) Then
					prop.SetValue(obj, r.Next(Integer.MaxValue), Nothing)
				ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Double))) Then
                    prop.SetValue(obj, r.NextDouble(), Nothing)

                ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Decimal))) Then
                    prop.SetValue(obj, r.NextDouble().ToDecimal(), Nothing)

                ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Single))) Then
                    prop.SetValue(obj, r.NextDouble().ToSingle(), Nothing)

				ElseIf prop.PropertyType.Equals(GetType(DateTime)) Then
					prop.SetValue(obj, New DateTime(2014, r.Next(1, 13), r.Next(1, 20), r.Next(24), r.Next(60), r.Next(60)), Nothing)
				ElseIf prop.PropertyType.Equals(GetType(Nullable(Of DateTime))) Then
					prop.SetValue(obj, New DateTime(2014, r.Next(1, 13), r.Next(1, 20), r.Next(24), r.Next(60), r.Next(60)), Nothing)
				ElseIf prop.PropertyType.Equals(GetType(Boolean)) Then
					prop.SetValue(obj, (r.Next(0, 2).Equals(1)), Nothing)
				ElseIf prop.PropertyType.Equals(GetType(Nullable(Of Boolean))) Then
					prop.SetValue(obj, New Nullable(Of Boolean)(r.Next(0, 2).Equals(1)), Nothing)
				ElseIf prop.PropertyType.Equals(GetType(Guid)) Then
					prop.SetValue(obj, Guid.NewGuid(), Nothing)
				ElseIf prop.PropertyType.IsEnum Then
					Dim enums As Array = prop.PropertyType.GetEnumValues()
					prop.SetValue(obj, enums.GetValue(r.Next(enums.Length)), Nothing)
				Else
					Throw New NotSupportedException("Cannot init class, because of unknown data type: " & prop.PropertyType.FullName)
				End If
			Next

			Return obj
		End Function

		Private Shared Function IsExcludedProperty(ByVal name As String) As Boolean
			Dim excludeProps As String() = {"ObjectMap", "ID"}
			Return excludeProps.ContainsCaseInsensitive(name)
		End Function

	End Class
End Namespace