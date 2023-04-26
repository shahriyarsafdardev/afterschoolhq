Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Reflection
Imports System.Web
Imports System.Collections.Specialized
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Data
Imports System.Globalization

Public Module Extensions

    <Extension>
    Public Function KeepInRange(ByVal aDecimal As Decimal, ByVal min As Decimal, ByVal max As Decimal) As Decimal
        Return Math.Min(Math.Max(aDecimal, min), max)
    End Function

    <Extension>
    Public Function IsInRange(ByVal aInteger As Integer, ByVal min As Integer, ByVal max As Integer) As Boolean
        Return aInteger >= min And aInteger <= max
    End Function

    <Extension>
    Public Function IsTrue(ByVal aBool As Boolean?) As Boolean
        Return aBool.HasValue AndAlso aBool.Value
    End Function

    <Extension>
    Public Function IsPhone(ByVal aString As String) As Boolean
        Return aString.IsNotEmpty() AndAlso aString.IsRegExMatch(Utilities.PhoneRegEx)
    End Function

    <Extension>
    Public Function IsFormattedPhone(ByVal aString As String) As Boolean
        Return aString.IsNotEmpty() AndAlso aString.IsRegExMatch(Utilities.FormattedPhoneRegEx)
    End Function

    <Extension>
    Public Function IsEmail(ByVal aString As String) As Boolean
        Return aString.IsNotEmpty() AndAlso aString.IsRegExMatch(Utilities.EmailRegEx)
    End Function

    <Extension>
    Public Function [In](ByVal aObject As String, ByVal list As IEnumerable(Of String), Optional ByVal comparer As StringComparer = Nothing) As Boolean

        If comparer.IsEmpty Then
            comparer = StringComparer.CurrentCultureIgnoreCase
        End If

        Return list.Contains(aObject, comparer)
    End Function

    <Extension>
    Public Function [In](Of T)(ByVal aObject As T, ByVal list As IEnumerable(Of T)) As Boolean
        Return list.Contains(aObject)
    End Function

    <Extension>
    Public Function NotIn(Of T)(ByVal aObject As T, ByVal list As IEnumerable(Of T)) As Boolean
        Return Not list.Contains(aObject)
    End Function

    <Extension>
    Public Function ParseEnum(Of T)(ByVal aString As String) As T
        Return System.Enum.Parse(GetType(T), aString, True)
    End Function

    <Extension>
    Public Function Contains(ByVal aString As String, ByVal strings As IEnumerable(Of String)) As Boolean
        For Each item As String In strings
            If aString.Contains(item) Then
                Return True
            End If
        Next
        Return False
    End Function

    <Extension>
    Public Function LastChars(ByVal aString As String, ByVal length As Integer) As String

        If aString.Length < length Then
            Return aString
        End If
        Return aString.Substring(aString.Length - 4)

    End Function

    <Extension>
    Public Function IsSameAs(ByVal aString As String, ByVal bString As String) As Boolean
        If (aString.IsEmpty() And bString.IsEmpty()) OrElse (aString.IsNotEmpty() AndAlso bString.IsNotEmpty() AndAlso aString.ToLower.Trim = bString.ToLower.Trim) Then
            Return True
        End If
        Return False
    End Function

    <Extension>
    Public Function MonthAsString(ByVal aDate As Date) As String
        Return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(aDate.Month)
    End Function

    <Extension>
    Public Function FirstDateOfWeek(ByVal aDate As Date) As Date
        Dim day As Integer = Convert.ToInt32(aDate.DayOfWeek)
        Return aDate.AddDays(day * -1)
    End Function

    <Extension>
    Public Function LastDateOfWeek(ByVal aDate As Date) As Date
        Dim day As Integer = Convert.ToInt32(aDate.DayOfWeek)
        Return aDate.AddDays(6 - day)
    End Function
    <Extension>
    Public Function FirstDateOfMonth(ByVal aDate As Date) As Date
        Return New DateTime(aDate.Year, aDate.Month, 1)
    End Function

    <Extension>
    Public Function LastDateOfMonth(ByVal aDate As Date) As Date
        Return New DateTime(aDate.Year, aDate.Month, aDate.DaysInMonth(aDate.Year, aDate.Month))
    End Function

    <Extension>
    Public Function AsList(Of T)(ByVal aObj As T) As List(Of T)
        Dim x As New List(Of T)
        x.Add(aObj)
        Return x
    End Function

    <Extension>
    Public Function Remove(ByVal aString As String, ByVal stringToRemove As String) As String
        Return aString.Replace(stringToRemove, "")
    End Function

    <Extension>
    Public Function Any(ByVal aDRC As DataRowCollection) As Boolean
        Return aDRC.Count > 0
    End Function

    <Extension>
    Public Function IsAfter(ByVal aDate As DateTime, ByVal bDate As DateTime) As Boolean
        Return aDate > bDate
    End Function

    <Extension>
    Public Function IsOnOrAfter(ByVal aDate As DateTime, ByVal bDate As DateTime) As Boolean
        Return aDate >= bDate
    End Function

    <Extension>
    Public Function IsBefore(ByVal aDate As DateTime, ByVal bDate As DateTime) As Boolean
        Return aDate < bDate
    End Function

    <Extension>
    Public Function IsOnOrBefore(ByVal aDate As DateTime, ByVal bDate As DateTime) As Boolean
        Return aDate <= bDate
    End Function

    <Extension>
    Public Function IsInRange(ByVal aDate As DateTime, ByVal min As DateTime, ByVal max As DateTime) As Boolean
        Return aDate >= min And aDate <= max
    End Function

    <Extension>
    Public Function ClosestDate(ByVal aDate As DateTime, ByVal list As IEnumerable(Of DateTime)) As DateTime
        Return list.ClosestDate(aDate)
        'Return list.OrderBy(Function(x) Math.Abs((aDate - x).Ticks)).First()
    End Function
    <Extension>
    Public Function ClosestDate(ByVal aList As IEnumerable(Of DateTime), ByVal bDate As DateTime) As DateTime
        Return aList.OrderBy(Function(x) Math.Abs((bDate - x).Ticks)).First()
    End Function
    <Extension>
    Public Function ClosestPastDate(ByVal aList As IEnumerable(Of DateTime), ByVal comparisonDate As DateTime) As DateTime
        Return aList.Where(Function(x) x.IsBefore(comparisonDate)).OrderBy(Function(x) Math.Abs((comparisonDate - x).Ticks)).First()
    End Function
    <Extension>
    Public Function ClosestFutureDate(ByVal aList As IEnumerable(Of DateTime), ByVal bDate As DateTime, ByVal comparisonDate As DateTime) As DateTime
        Return aList.Where(Function(x) bDate.IsAfter(comparisonDate)).OrderBy(Function(x) Math.Abs((bDate - x).Ticks)).First()
    End Function
    <Extension>
    Public Function FarthestDate(ByVal aDate As DateTime, ByVal list As IEnumerable(Of DateTime)) As DateTime
        Return list.OrderBy(Function(x) Math.Abs((aDate - x).Ticks)).Last()
    End Function
    <Extension>
    Public Function FarthestDate(ByVal aList As IEnumerable(Of DateTime), ByVal bDate As DateTime) As DateTime
        Return aList.OrderBy(Function(x) Math.Abs((bDate - x).Ticks)).Last()
    End Function
    <Extension>
    Public Function DatesAreInSameWeek(ByVal aDate As DateTime, ByVal bDate As DateTime) As Boolean
        Return Equals(aDate.AddDays(-1 * aDate.DayOfWeek), bDate.AddDays(-1 * bDate.DayOfWeek))
    End Function


    <Extension()>
    Public Function Shuffle(Of T)(ByVal value As IEnumerable(Of T)) As IEnumerable(Of T)
        Return value.OrderBy(Function(x) Guid.NewGuid())
    End Function

    <Extension()>
    Public Function Filter(ByVal aNameValueCollection As NameValueCollection, ByVal aFilter As String) As NameValueCollection
        Dim col As New NameValueCollection()
        For Each key As String In aNameValueCollection.AllKeys
            If key.Contains(aFilter) Then
                col.Add(key, aNameValueCollection(key))
            End If
        Next
        Return col
    End Function

    <Extension()>
    Public Function ToOrdinal(ByVal value As Integer) As String
        If (value <= 0) Then Return value.ToString()

        Select Case (value)
            Case 11, 12, 13
                Return value.ToString() + "th"
        End Select

        Select Case (value Mod 100)
            Case 11, 12, 13
                Return value.ToString() + "th"
        End Select

        Select Case (value Mod 10)
            Case 1
                Return value.ToString() + "st"
            Case 2
                Return value.ToString() + "nd"
            Case 3
                Return value.ToString() + "rd"
            Case Else
                Return value.ToString() + "th"
        End Select

    End Function

    <Extension()>
    Public Function ContainsCaseInsensitive(ByVal arr() As String, ByVal value As String) As Boolean

        If value.IsEmpty = False Then
            For Each str As String In arr
                If str.ToLower = value.ToLower Then
                    Return True
                End If
            Next
        End If

        Return False

    End Function
    <Extension()>
    Public Function AsString(ByVal aMemoryStream As System.IO.MemoryStream) As String
        Dim sw As New System.IO.StreamWriter(aMemoryStream)
        sw.Flush()
        aMemoryStream.Position = 0
        Dim sr As New System.IO.StreamReader(aMemoryStream)
        Return sr.ReadToEnd()
    End Function
    <Extension()>
    Public Function Trim(ByVal aDate As Date, ByVal roundTicks As Long) As Date
        Return New DateTime(aDate.Ticks - aDate.Ticks Mod roundTicks)
    End Function
    <Extension()>
    Public Function ToRfc3339String(ByVal aDate As Date) As String
        Return Rfc3339DateTime.ToRfc3339String(aDate.ToUniversalTime)
    End Function

    <Extension()>
    Public Function ToMemoryStream(ByVal aByte As Byte()) As System.IO.MemoryStream
        Return New System.IO.MemoryStream(aByte)
    End Function

    <Extension()>
    Public Function ToEnum(Of T)(ByVal aString As String) As T
        Return CType([Enum].Parse(GetType(T), aString, True), T)
    End Function

    <Extension()>
    Public Function ToEnum(Of T)(ByVal aInt As Integer) As T
        Return CType(CType(aInt, Object), T)
    End Function

    <Extension()>
    Public Function ToUTF8String(ByVal aByte As Byte()) As String
        Return System.Text.Encoding.UTF8.GetString(aByte)
    End Function

    <Extension()>
    Public Function FromUTF8String(ByVal aString As String) As Byte()
        Return System.Text.Encoding.UTF8.GetBytes(aString)
    End Function

    <Extension()>
    Public Function ToBase64String(ByVal aByte As Byte()) As String
        Return Convert.ToBase64String(aByte)
    End Function

    <Extension()>
    Public Function FromBase64String(ByVal aString As String) As Byte()
        Return Convert.FromBase64String(aString)
    End Function

    <Extension()>
    Public Function Zip(ByVal aByte As Byte()) As Byte()
        Dim msi As System.IO.MemoryStream = New IO.MemoryStream(aByte)
        Dim mso As System.IO.MemoryStream = New IO.MemoryStream()
        Dim gs As System.IO.Compression.GZipStream = New IO.Compression.GZipStream(mso, IO.Compression.CompressionMode.Compress)

        msi.CopyTo(gs)

        msi.Dispose()
        gs.Dispose()

        msi = Nothing
        gs = Nothing

        Return mso.ToArray()
    End Function

    <Extension()>
    Public Function Zip(ByVal aString As String) As Byte()
        Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(aString)

        Return Zip(bytes)
    End Function

    <Extension()>
    Public Function UnZip(ByVal aByte As Byte()) As String

        Dim msi As System.IO.MemoryStream = New IO.MemoryStream(aByte)
        Dim mso As New IO.MemoryStream()
        Dim gs As System.IO.Compression.GZipStream = New IO.Compression.GZipStream(msi, IO.Compression.CompressionMode.Decompress)

        gs.CopyTo(mso)

        msi.Dispose()
        gs.Dispose()

        msi = Nothing
        gs = Nothing

        Return Text.Encoding.UTF8.GetString(mso.ToArray())

    End Function

    <Extension()>
    Public Function ToByteArray(ByVal input As System.IO.Stream) As Byte()
        Dim buffer As Byte() = New Byte(input.Length) {}
        Dim ms As New System.IO.MemoryStream()
        Dim read As Integer = input.Read(buffer, 0, buffer.Length)
        While (read > 0)
            ms.Write(buffer, 0, read)
            read = input.Read(buffer, 0, buffer.Length)
        End While
        Return ms.ToArray()
    End Function

    <Extension()>
    Public Function UrlEncode(ByVal aString As String) As String
        Return HttpUtility.UrlEncode(aString)
    End Function

    <Extension()>
    Public Function UrlDecode(ByVal aString As String) As String
        Return HttpUtility.UrlDecode(aString)
    End Function

    <Extension()>
    Public Function SerializeAsXElement(ByRef xs As XmlSerializer, ByRef o As Object) As XElement
        Dim d As XDocument = New XDocument()
        Using w As XmlWriter = d.CreateWriter()
            xs.Serialize(w, o)
        End Using
        Dim e As XElement = d.Root
        e.Remove()
        Return e
    End Function
    <Extension()>
    Public Function DeserializeAsObject(ByRef xs As XmlSerializer, ByRef xe As XElement) As Object
        Dim d As XDocument = New XDocument()
        Dim o As Object
        Dim w As XmlReader = xe.CreateReader()
        o = xs.Deserialize(w)
        Return o
    End Function

    Private Function ArrayConvertAllIntegerToString(ByVal p As Integer) As String
        Return p.ToString()
    End Function
    Private Function ArrayConvertAllStringToInteger(ByVal p As String) As Integer
        Return p.ToInteger()
    End Function
    Private Function ArrayConvertAllStringToDate(ByVal p As String) As Date
        Return p.ToDateTime()
    End Function

    <Extension()>
    Public Function ToIntegerArray(ByVal aArray() As String) As Integer()
        Return Array.ConvertAll(Of String, Integer)(aArray, AddressOf ArrayConvertAllStringToInteger)
    End Function

    <Extension()>
    Public Function ToListOfInteger(ByVal aArray() As Integer) As List(Of Integer)
        Dim list As New List(Of Integer)
        For Each i As Integer In aArray
            list.Add(i)
        Next
        Return list
    End Function

    <Extension()>
    Public Function IfThen(ByVal aDouble As Double, ByVal condition As Boolean, ByVal value As Double) As Double
        If condition Then
            Return value
        Else
            Return aDouble
        End If
    End Function
    <Extension()>
    Public Function IfThen(ByVal aString As String, ByVal conditionValue As String, ByVal value As String) As String
        If aString = conditionValue Then
            Return value
        Else
            Return aString
        End If
    End Function

    <Extension()>
    Public Function IsRegExMatch(ByVal aString As String, ByVal regularExpression As String) As Boolean
        Return aString.IsRegExMatch(New Text.RegularExpressions.Regex(regularExpression))
    End Function
    <Extension()>
    Public Function IsRegExMatch(ByVal aString As String, ByVal regularExpression As Text.RegularExpressions.Regex) As Boolean
        Return regularExpression.IsMatch(aString)
    End Function

    <Extension()>
    Public Function IsNotEmpty(ByVal aObj As Object) As Boolean
        Return Not IsEmpty(aObj)
    End Function

    <Extension()>
    Public Function IsBlank(ByVal aObj As Object) As Boolean
        Return IsEmpty(aObj)
    End Function

    <Extension()>
    Public Function IsNotBlank(ByVal aString As String) As Boolean
        Return Not IsBlank(aString)
    End Function

    <Extension()>
    Public Function IsEmpty(ByVal aObj As Object) As Boolean
        If aObj Is Nothing Then
            Return True
        ElseIf aObj Is DBNull.Value Then
            Return True
        ElseIf aObj.GetType Is GetType(String) AndAlso aObj.ToString.IsEmpty Then
            Return True
        Else
            Return False
        End If
    End Function

    <Extension()>
    Public Function IsEmpty(ByVal aString As String) As Boolean
        If aString Is Nothing Then
            Return True
        ElseIf aString = String.Empty Then
            Return True
        ElseIf aString.Trim.Length = 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    <Extension()>
    Public Function ReturnNullIfEmpty(ByVal aString As String)
        If aString.IsEmpty Then
            Return Nothing
        Else
            Return aString
        End If
    End Function

    <Extension()>
    Public Function IsBlank(ByVal aString As String) As Boolean
        Return aString.IsEmpty
    End Function

    <Extension()>
    Public Function DateDiff(ByVal aDateTime As Date, ByVal compareDate As Date, ByVal unit As String, Optional ByVal useFractions As Boolean = False) As Double
        Select Case unit.ToLower()
            Case "d"
                Dim ts As TimeSpan = aDateTime - compareDate
                If useFractions Then
                    Return ts.TotalDays
                Else
                    Return ts.Days
                End If
            Case "w"
                Dim ts As TimeSpan = aDateTime - compareDate
                If useFractions Then
                    Return ts.TotalDays / 7
                Else
                    Return Math.Floor(ts.TotalDays / 7)
                End If
            Case "m"
                Return ((aDateTime.Year * 12 + aDateTime.Month) - (compareDate.Year * 12 + compareDate.Month))
            Case "y"
                Return aDateTime.Year - compareDate.Year
        End Select
    End Function

    <Extension()>
    Public Function AddWeeks(ByVal aDateTime As Date, ByVal weeks As Integer) As Date
        Return aDateTime.AddDays(weeks * 7)
    End Function

    <Extension()>
    Public Function AddBiWeeks(ByVal aDateTime As Date, ByVal weeks As Integer) As Date
        Return aDateTime.AddDays(weeks * 14)
    End Function

    <Extension()>
    Public Function NextDayOfWeek(ByVal aStartDate As Date, ByVal day As DayOfWeek) As Date
        Dim daysToAdd As Integer = (Convert.ToInt32(day) - Convert.ToInt32(aStartDate.DayOfWeek) + 7) Mod 7
        Return aStartDate.AddDays(daysToAdd)
    End Function

    <Extension()>
    Public Function Quarter(ByVal aDateTime As Date) As Integer
        Dim q As Integer = 0
        Dim month As Integer = CInt(aDateTime.Month)

        If month >= 1 And month <= 3 Then
            q = 1
        ElseIf month >= 4 And month <= 6 Then
            q = 2
        ElseIf month >= 7 And month <= 9 Then
            q = 3
        ElseIf month >= 10 And month <= 12 Then
            q = 4
        End If

        Return q
    End Function

    <Extension()>
    Public Function AddQuarters(ByVal aDateTime As Date, quarters As Integer) As DateTime
        Return aDateTime.AddMonths(quarters * 3)
    End Function

    <Extension()>
    Public Function IsGuid(ByVal aString As String) As Boolean
        Try
            Dim uid As Guid = New Guid(aString)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    <Extension()>
    Public Function IsNotGuid(ByVal aString As String) As Boolean
        Try
            Return Not IsGuid(aString)
        Catch ex As Exception
            Return False
        End Try
    End Function

    <Extension()>
    Public Function ToGuid(ByVal aObj As Object) As Guid
        Return New Guid(aObj.ToString())
    End Function

    <Extension()>
    Public Function ToGuid(ByVal aString As String) As Guid
        Return New Guid(aString)
    End Function

    <Extension()>
    Public Function ToStringArray(ByVal aArray() As Integer) As String()
        Return aArray.ToStrings().ToArray()
    End Function

    <Extension()>
    Public Function ToStrings(ByVal aList As IEnumerable(Of Integer)) As IEnumerable(Of String)
        Return aList.Select(Function(item) item.ToString())
    End Function

    <Extension()>
    Public Function ToDateArray(ByVal aArray() As String) As Date()
        Return Array.ConvertAll(Of String, Date)(aArray, AddressOf ArrayConvertAllStringToDate)
    End Function

    <Extension()>
    Public Function ToStringList(ByVal aArray() As String, Optional ByVal separator As String = ",", Optional ByVal includeTrailing As Boolean = False) As String
        Return String.Join(separator, aArray)
    End Function

    <Extension()>
    Public Function ToStringList(ByVal aList As IEnumerable(Of String), Optional ByVal separator As String = ",", Optional finalSeparator As String = "") As String
        If finalSeparator.IsNotBlank And aList.Count > 2 Then
            Dim x As List(Of String) = New List(Of String)()
            Dim y As List(Of String) = New List(Of String)()
            y.Add(aList.Last())
            x.AddRange(aList.Except(y))

            Return String.Join(separator, x.ToArray()) & finalSeparator & aList.Last().Replace("  ", " ")
        ElseIf finalSeparator.IsNotBlank And aList.Count = 2 Then
            Return String.Join(finalSeparator, aList.ToArray())
        Else
            Return String.Join(separator, aList.ToArray())
        End If
    End Function

    <Extension()>
    Public Function ToDateTime(ByVal aString As String) As Date
        Return Convert.ToDateTime(aString)
    End Function
    <Extension()>
    Public Function IsDate(ByVal aString As String) As Boolean
        Return DateTime.TryParse(aString, Nothing)
    End Function
    <Extension()>
    Public Function IsDateTime(ByVal aString As String) As Boolean
        Return IsDate(aString)
    End Function
    <Extension()>
    Public Function IsTime(ByVal aString As String) As Boolean
        Dim dateStr As String = DateTime.UtcNow.ToShortDateString() & " " & aString
        Return IsDate(dateStr)
    End Function
    <Extension()>
    Public Function IsNumber(ByVal aObject As Object) As Boolean
        Return Double.TryParse(aObject, Nothing)
    End Function
    <Extension()>
    Public Function IsNumber(ByVal aString As String) As Boolean
        Return Double.TryParse(aString, Nothing)
    End Function
    <Extension()>
    Public Function IsInteger(ByVal aString As String) As Boolean
        If IsNumber(aString) Then
            Dim result As Integer
            If Integer.TryParse(aString, result) Then
                Return True
            End If
        End If
        Return False
    End Function
    <Extension()>
    Public Function IsIntegerInRange(ByVal aString As String, ByVal minValue As Integer, ByVal maxValue As Integer) As Boolean
        If IsNumber(aString) Then
            Dim result As Integer
            If Integer.TryParse(aString, result) Then
                If result >= minValue And result <= maxValue Then
                    Return True
                End If
            End If
        End If
        Return False
    End Function
    <Extension()>
    Public Function IsLong(ByVal aString As String) As Boolean
        If IsNumber(aString) Then
            Dim result As Long
            If Long.TryParse(aString, result) Then
                Return True
            End If
        End If
        Return False
    End Function
    <Extension()>
    Public Function IsDouble(ByVal aString As String) As Boolean
        Return IsNumber(aString)
    End Function

    <Extension()>
    Public Function ToCurrency(ByVal aInteger As Integer, Optional ByVal useDecimals As Boolean = True) As String
        Dim dec As String = "0"
        If useDecimals Then dec = "2"
        Return aInteger.ToString("c" & dec)
    End Function
    <Extension()>
    Public Function ToCurrency(ByVal aDouble As Double, Optional ByVal useDecimals As Boolean = True) As String
        Dim dec As String = "0"
        If useDecimals Then dec = "2"
        Return aDouble.ToString("c" & dec)
    End Function
    <Extension()>
    Public Function ToCurrency(ByVal aDecimal As Decimal, Optional ByVal useDecimals As Boolean = True) As String
        Dim dec As String = "0"
        If useDecimals Then dec = "2"
        Return aDecimal.ToString("c" & dec)
    End Function
    <Extension()>
    Public Function ToCurrency(ByVal aDecimal As Decimal?, Optional ByVal useDecimals As Boolean = True) As String
        Dim dec As String = "0"
        If useDecimals Then dec = "2"
        If aDecimal.HasValue Then
            Return aDecimal.Value.ToString("c" & dec)
        Else
            Return (0).ToCurrency()
        End If
    End Function
    <Extension()>
    Public Function ToCurrency(ByVal aSingle As Single, Optional ByVal useDecimals As Boolean = True) As String
        Dim dec As String = "0"
        If useDecimals Then dec = "2"
        Return aSingle.ToString("c" & dec)
    End Function
    <Extension()>
    Public Function ToCurrency(ByVal aDouble As Double?, Optional ByVal useDecimals As Boolean = True) As String
        Dim dec As String = "0"
        If useDecimals Then dec = "2"
        If aDouble.HasValue Then
            Return aDouble.Value.ToString("c" & dec)
        Else
            Return (0).ToCurrency()
        End If
    End Function
    <Extension()>
    Public Function ToCurrency(ByVal aSingle As Single?, Optional ByVal useDecimals As Boolean = True) As String
        Dim dec As String = "0"
        If useDecimals Then dec = "2"
        If aSingle.HasValue Then
            Return aSingle.Value.ToString("c" & dec)
        Else
            Return (0).ToCurrency()
        End If
    End Function
    <Extension()>
    Public Function HoursToTime(ByVal hours As Double) As String

        Dim hoursWorked As String = Math.Floor(hours).ToInteger().ToString()
        Dim minsWorked As String = ((hours - hoursWorked.ToInteger()) * 60).ToInteger().ToString().PadLeft(2, "0"c)

        If minsWorked.ToInteger = 60 Then
            hoursWorked = (hoursWorked.ToInteger() + 1).ToString()
            minsWorked = "00"
        End If

        Return hoursWorked + ":" + minsWorked
    End Function
    <Extension()>
    Public Function HoursToTimeLong(ByVal hours As Double) As String

        Dim hoursTemp As Double = Math.Floor(hours).ToInteger()

        Dim hoursWorked As String = hoursTemp.ToString("#,###,##0")
        Dim minsWorked As String = ((hours - hoursTemp) * 60).ToInteger().ToString()

        If minsWorked.ToInteger = 60 Then
            hoursWorked = (hoursTemp + 1).ToString("#,###,##0")
            minsWorked = "0"
        End If

        Dim rtn As String = ""

        If hoursWorked <> "0" Or minsWorked = "0" Then
            rtn &= hoursWorked + " hrs "
        End If
        If minsWorked <> "0" Then
            rtn &= minsWorked + " mins"
        End If

        Return rtn

    End Function
    <Extension()>
    Public Function SplitByUpperCase(ByVal aString As String) As String
        Dim rtn As String = ""

        If aString IsNot Nothing AndAlso aString <> String.Empty Then
            aString = aString.Replace(" ", "")

            For i As Integer = 0 To aString.Length - 1

                Dim isNotFirstChar = i > 0
                Dim isCharUpperCase = isNotFirstChar AndAlso Char.IsUpper(aString.Chars(i))
                Dim isCharNumber = isNotFirstChar AndAlso Char.IsNumber(aString.Chars(i))
                Dim isPrevCharLowerCase = isNotFirstChar AndAlso Char.IsLower(aString.Chars(i - 1))
                Dim isNotLastChar = isNotFirstChar AndAlso i < aString.Length - 1
                Dim isNextCharLowerCase = isNotFirstChar AndAlso isNotLastChar AndAlso Char.IsLower(aString.Chars(i + 1))
                Dim isNextCharNumber = isNotFirstChar AndAlso isNotLastChar AndAlso Char.IsNumber(aString.Chars(i + 1))

                If (isCharUpperCase Or isCharNumber) AndAlso (isPrevCharLowerCase Or isNextCharLowerCase) Then
                    rtn &= " " & aString.Chars(i).ToString
                Else
                    rtn &= aString.Chars(i).ToString
                End If
            Next

        End If

        Return rtn
    End Function

    <Extension()>
    Public Function FirstLetterOfEachWord(ByVal aString As String) As String
        Dim rtn As String = ""
        If aString IsNot Nothing AndAlso aString <> String.Empty Then
            Dim words = aString.Split(" ")
            For Each word As String In words
                If word.First.ToString().IsNumber() = False Then
                    rtn &= word.First()
                End If
            Next
        End If
        Return rtn
    End Function

    <Extension()>
    Public Function ToProperCase(ByVal aString As String) As String
        Dim rtn As String = String.Empty

        If aString.IsEmpty = False Then
            rtn = aString.ToLower
            If aString.Contains(" ") Then
                Dim reg As New Text.RegularExpressions.Regex("[^\s\(\)\[\]\{\}""'\<\>\/\\]+", Text.RegularExpressions.RegexOptions.IgnoreCase Or Text.RegularExpressions.RegexOptions.Singleline)
                Dim matches As Text.RegularExpressions.MatchCollection = reg.Matches(aString)

                For i As Integer = 0 To matches.Count - 1
                    Dim doUpperCase As Boolean = True
                    If matches(i).Index > 0 Then
                        Select Case matches(i).Value.ToLower
                            Case "and", "or", "to", "for", "a", "the"
                                doUpperCase = False
                        End Select
                    End If
                    If doUpperCase = True Then
                        rtn = rtn.ToUpperAt(matches(i).Index)
                    End If
                Next
            Else
                rtn = rtn.ToUpperAt(0)
            End If
        End If
        Return rtn
    End Function
    <Extension()>
    Public Function ToUpperAt(ByVal aString As String, ByVal index As Integer) As String
        Dim rtn As String = aString

        If aString IsNot Nothing AndAlso aString <> String.Empty Then

            Dim firstHalf As String = ""
            Dim character As String = ""
            Dim lastHalf As String = ""

            If index > 0 Then
                firstHalf = aString.Substring(0, index)
            End If

            character = aString.Substring(index, 1).ToUpper

            If index < aString.Length - 1 Then
                lastHalf = aString.Substring(index + 1)
            End If

            rtn = firstHalf & character & lastHalf
        End If

        Return rtn

    End Function
    <Extension>
    Public Function MaskString(ByVal aString As String, Optional ByVal unMaskedBeginningLength As Integer = 0, Optional ByVal unMaskedEndLength As Integer = 0) As String
        Return aString.Substring(0, unMaskedBeginningLength) & New String("*"c, aString.Length - unMaskedBeginningLength - unMaskedEndLength) & aString.Substring(aString.Length - unMaskedEndLength, unMaskedEndLength)
    End Function
    <Extension()>
    Public Function ToMaskedData(ByVal aString As String) As String
        Dim isEmail As Boolean = aString.IsEmail()


        If isEmail Then
            Return ToMaskedEmail(aString)
        End If

        Return MaskString(aString)
    End Function

    <Extension>
    Public Function ToMaskedEmail(ByVal aString As String) As String
        Dim rtn As String = aString
        Dim firstTwoCharacters As String = aString.Substring(0, 2)
        Dim emailSplit As String() = aString.Split("@")
        Dim emailName As String = emailSplit.ElementAt(0)
        emailName = emailName.Substring(0, emailName.Length - 1)
        Dim domain = "@" & emailSplit.ElementAtOrDefault(1)
        Dim tldIndex = domain.Length - domain.Split(".").ElementAtOrDefault(0).Length - 1
        Return MaskString(emailName, 2) & MaskString(domain, 2, tldIndex)
    End Function

    <Extension()>
    Public Function ToInteger(ByVal aString As String) As Integer
        Return Convert.ToInt32(aString)
    End Function
    <Extension()>
    Public Function ToInteger(ByVal aDecimal As Decimal) As Integer
        Return Convert.ToInt32(aDecimal)
    End Function
    <Extension()>
    Public Function ToInteger(ByVal aDouble As Double) As Integer
        Return Convert.ToInt32(aDouble)
    End Function
    <Extension()>
    Public Function ToInteger(ByVal aSingle As Single) As Integer
        Return Convert.ToInt32(aSingle)
    End Function
    <Extension()>
    Public Function ToInteger(ByVal aBoolean As Boolean) As Integer
        If aBoolean Then
            Return 1
        Else
            Return 0
        End If
    End Function

    <Extension()>
    Public Function ToLong(ByVal aString As String) As Long
        Return Convert.ToInt64(aString)
    End Function
    <Extension()>
    Public Function ToLong(ByVal aDouble As Double) As Long
        Return Convert.ToInt64(aDouble)
    End Function
    <Extension()>
    Public Function ToLong(ByVal aSingle As Single) As Long
        Return Convert.ToInt64(aSingle)
    End Function

    <Extension()>
    Public Function ToSingle(ByVal aString As String) As Single
        Return Convert.ToSingle(aString)
    End Function
    <Extension()>
    Public Function ToSingle(ByVal aDouble As Double) As Single
        Return Convert.ToSingle(aDouble)
    End Function
    <Extension()>
    Public Function ToSingle(ByVal aInteger As Integer) As Single
        Return Convert.ToSingle(aInteger)
    End Function

    <Extension()>
    Public Function ToDecimal(ByVal aSingle As Single) As Decimal
        Return Convert.ToDecimal(aSingle)
    End Function
    <Extension()>
    Public Function ToDecimal(ByVal aInteger As Integer) As Decimal
        Return Convert.ToDecimal(aInteger)
    End Function
    <Extension()>
    Public Function ToDecimal(ByVal aDouble As Double) As Decimal
        Return Convert.ToDecimal(aDouble)
    End Function
    <Extension()>
    Public Function ToDecimal(ByVal aString As String) As Decimal
        Return Convert.ToDecimal(aString)
    End Function
    <Extension()>
    Public Function ToDouble(ByVal aSingle As Single) As Double
        Return Convert.ToDouble(aSingle)
    End Function
    <Extension()>
    Public Function ToDouble(ByVal aInteger As Integer) As Double
        Return Convert.ToDouble(aInteger)
    End Function
    <Extension()>
    Public Function ToDouble(ByVal aDouble As Double) As Double
        Return Convert.ToDouble(aDouble)
    End Function
    <Extension()>
    Public Function ToDouble(ByVal aString As String) As Double
        Return Convert.ToDouble(aString)
    End Function
    <Extension()>
    Public Function ToFloat(ByVal aInteger As Integer) As Single
        Return Convert.ToSingle(aInteger)
    End Function
    <Extension()>
    Public Function ToFloat(ByVal aDouble As Double) As Single
        Return Convert.ToSingle(aDouble)
    End Function
    <Extension()>
    Public Function ToFloat(ByVal aString As String) As Single
        Return Convert.ToSingle(aString)
    End Function

    <Extension()>
    Public Function ToLongDateTimeString(ByVal aDate As Date) As String
        Return aDate.ToLongDateString & " " & aDate.ToLongTimeString
    End Function
    <Extension()>
    Public Function ToLongDateTimeString(ByVal aDate As Date?) As String
        Return ToLongDateTimeString(aDate, "")
    End Function
    <Extension()>
    Public Function ToLongDateTimeString(ByVal aDate As Date?, ByVal subValue As String) As String
        If aDate.HasValue Then
            Return aDate.Value.ToLongDateTimeString()
        Else
            Return subValue
        End If
    End Function
    <Extension()>
    Public Function ToLongDateString(ByVal aDate? As Date) As String
        If aDate.HasValue Then
            Return aDate.Value.ToLongDateString()
        End If
        
        Return ""
        
    End Function

    <Extension()>
    Public Function ToShortDateTimeString(ByVal aDate As Date) As String
        Return aDate.ToShortDateString & " " & aDate.ToShortTimeString
    End Function
    <Extension()>
    Public Function ToShortDateTimeString(ByVal aDate As Date?) As String
        Return ToShortDateTimeString(aDate, "")
    End Function
    <Extension()>
    Public Function ToShortDateTimeString(ByVal aDate As Date?, ByVal subValue As String) As String
        If aDate.HasValue Then
            Return aDate.Value.ToShortDateTimeString()
        Else
            Return subValue
        End If
    End Function
    <Extension()>
    Public Function ToShortDateString(ByVal aDate As Date?) As String
        Return ToShortDateString(aDate, "")
    End Function
    <Extension()>
    Public Function ToShortDateString(ByVal aDate As Date?, ByVal subValue As String) As String
        If aDate.HasValue Then
            Return aDate.Value.ToShortDateString()
        Else
            Return subValue
        End If
    End Function
    <Extension()>
    Public Function ToShortTimeString(ByVal aDate As Date?) As String
        Return ToShortTimeString(aDate, "")
    End Function
    <Extension()>
    Public Function ToShortTimeString(ByVal aDate As Date?, ByVal subValue As String) As String
        If aDate.HasValue Then
            Return aDate.Value.ToShortTimeString()
        Else
            Return subValue
        End If
    End Function

    <Extension()>
    Public Function IsBoolean(ByVal aString As String) As Boolean

        If aString.IsBlank() Then
            Return False
        End If

        Try
            Dim b As Boolean = aString.ToBoolean()
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function
    <Extension()>
    Public Function ToBoolean(ByVal aInteger As Integer) As Boolean
        If aInteger = 0 Then
            Return False
        ElseIf aInteger = 1 Then
            Return True
        Else
            Throw New Exception("value cannot be converted to a boolean")
        End If
    End Function
    <Extension()>
    Public Function ToBoolean(ByVal aString As String) As Boolean
        Try
            Select Case aString.ToLower
                Case "1", "y", "yes", "true", "t"
                    Return True
                Case "0", "n", "no", "false", "f"
                    Return False
            End Select
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    <Extension()>
    Public Function ToYesNo(ByVal aBoolean As Boolean) As String
        If aBoolean Then
            Return "Yes"
        Else
            Return "No"
        End If
    End Function
    <Extension()>
    Public Function MaxChars(ByVal aStr As String, ByVal totalCharCount As Integer, Optional ByVal appendChars As String = "") As String
        Dim temp As String = ""

        If aStr.IsEmpty = False Then
            temp = aStr
            If temp.Length > totalCharCount Then
                temp = temp.Substring(0, totalCharCount - appendChars.Length) & appendChars
            End If
        End If

        Return temp
    End Function

    <Extension()>
    Public Function Round(ByVal aSingle As Single, ByVal digits As Integer) As Double
        Return Math.Round(aSingle, digits)
    End Function

    <Extension()>
    Public Function Round(ByVal aDecimal As Decimal, ByVal digits As Integer) As Decimal
        Return Math.Round(aDecimal, digits)
    End Function
    <Extension()>
    Public Function Round(ByVal aDouble As Double, ByVal digits As Integer) As Double
        Return Math.Round(aDouble, digits)
    End Function
    <Extension()>
    Public Function Round(ByVal aInteger As Integer, ByVal digits As Integer) As Double
        Return Math.Round(aInteger, digits)
    End Function

    <Extension()>
    Public Function Repeat(ByVal aString As String, ByVal times As Integer) As String
        Dim str As String = ""
        For i As Integer = 1 To times
            str &= aString
        Next
        Return str
    End Function

    <Extension()>
    Public Function IfBlank(ByVal aString As String, ByVal alternateText As String) As String
        If aString.IsBlank Then
            Return alternateText
        Else
            Return aString
        End If
    End Function

    <Extension()>
    Public Function IfEmpty(ByVal aString As String, ByVal alternateText As String) As String
        If aString.IsEmpty Then
            Return alternateText
        Else
            Return aString
        End If
    End Function

    <Extension()>
    Public Function IfEmpty(ByVal aObject As Object, ByVal alternateText As String) As String
        If aObject Is Nothing Then
            Return alternateText
        Else
            Return aObject.ToString
        End If
    End Function

    <Extension()>
    Public Function IfEmpty(ByVal aInteger As Integer?, ByVal alternateNumber As Integer) As Integer
        If aInteger.IsEmpty OrElse aInteger.HasValue = False Then
            Return alternateNumber
        Else
            Return aInteger.Value
        End If
    End Function

    <Extension()>
    Public Function IfEmpty(ByVal aDateTime As Date?, ByVal alternateDate As Date) As Date
        If aDateTime.IsEmpty OrElse aDateTime.HasValue = False Then
            Return alternateDate
        Else
            Return aDateTime.Value
        End If
    End Function

    <Extension()>
    Public Function IfEmpty(ByVal aDecimal As Decimal?, ByVal alternateNumber As Decimal) As Decimal
        If aDecimal.IsEmpty OrElse aDecimal.HasValue = False Then
            Return alternateNumber
        Else
            Return aDecimal.Value
        End If
    End Function

    <Extension()>
    Public Function IfEmpty(ByVal aDouble As Double?, ByVal alternateNumber As Double) As Double
        If aDouble.IsEmpty OrElse aDouble.HasValue = False Then
            Return alternateNumber
        Else
            Return aDouble.Value
        End If
    End Function
    <Extension()>
    Public Function IfEmpty(ByVal aSingle As Single?, ByVal alternateNumber As Single) As Single
        If aSingle.IsEmpty OrElse aSingle.HasValue = False Then
            Return alternateNumber
        Else
            Return aSingle.Value
        End If
    End Function
    <Extension()>
    Public Function Replace(ByVal aString As String, ByVal regEx As Text.RegularExpressions.Regex, ByVal replacement As String) As String
        If aString.IsEmpty = False Then
            Return regEx.Replace(aString, replacement)
        Else
            Return aString
        End If
    End Function

    <Extension()>
    Public Function ToJSON(ByVal aObject As Object) As String
        If aObject Is Nothing Then
            Return "{}"
        End If
        Return Newtonsoft.Json.JsonConvert.SerializeObject(aObject)
    End Function

    <Extension()>
    Public Function FromJSON(ByVal aString As String) As Object
        If aString.IsEmpty Then
            Return Nothing
        End If
        Return Newtonsoft.Json.JsonConvert.DeserializeObject(aString)
    End Function

    <Extension()>
    Public Function ToTimeSpanString(ByVal aDate As DateTime, SinceDate As DateTime) As String

        Dim timeSinceCreation As String = "Just Now"
        Dim ts As TimeSpan = SinceDate - aDate

        Dim monthDiff As Integer = ((SinceDate.Year * 12 + SinceDate.Month) - (aDate.Year * 12 + aDate.Month))

        If monthDiff > 0 Then

            timeSinceCreation = monthDiff.ToString() + "mo"

        ElseIf (ts.Days > 7) Then

            timeSinceCreation = Math.Floor(ts.Days.ToDouble() / 7).ToString() + "w"

        ElseIf (ts.Days > 0) Then

            timeSinceCreation = ts.Days.ToString() + "d"

        ElseIf (ts.Hours > 0) Then

            timeSinceCreation = ts.Hours.ToString() + "h"

        ElseIf (ts.Minutes > 0) Then

            timeSinceCreation = ts.Minutes.ToString() + "m"

        ElseIf (ts.Seconds > 0) Then

            timeSinceCreation = ts.Seconds.ToString() + "s"
        End If

        Return timeSinceCreation

    End Function

    <Extension()>
    Public Function ToText(ByVal aDate As DateTime) As String

        Dim ts As TimeSpan = DateTime.Now - aDate
        Dim timePassed As String = ""
        If (ts.TotalDays.ToInteger() >= 31) Then

            timePassed = "Over a month ago"

        ElseIf (ts.TotalDays.ToInteger() > 28) Then

            timePassed = "About a month ago"

        ElseIf (ts.TotalDays.ToInteger() = 28) Then

            timePassed = "About four weeks ago"

        ElseIf (ts.TotalDays.ToInteger() > 21) Then

            timePassed = "About " + ts.TotalDays.ToInteger().ToString() + " days ago"

        ElseIf (ts.TotalDays.ToInteger() = 21) Then

            timePassed = "About three weeks ago"

        ElseIf (ts.TotalDays.ToInteger() > 14) Then

            timePassed = "About " + ts.TotalDays.ToInteger().ToString() + " days ago"

        ElseIf (ts.TotalDays.ToInteger() = 14) Then

            timePassed = "About two weeks ago"

        ElseIf (ts.TotalDays.ToInteger() > 7) Then

            timePassed = "About " + ts.TotalDays.ToInteger().ToString().ToString() + " days ago"

        ElseIf (ts.TotalDays.ToInteger() = 7) Then

            timePassed = "About a week ago"

        ElseIf (ts.TotalDays.ToInteger() > 1) Then

            timePassed = "About " + ts.TotalDays.ToInteger().ToString().ToString() + " days ago"

        ElseIf (ts.TotalDays >= 1) Then

            timePassed = "About a day ago"

        ElseIf (ts.TotalHours.ToInteger() > 1) Then

            timePassed = "About " + ts.TotalHours.ToInteger().ToString().ToString() + " hours ago"

        ElseIf (ts.TotalHours >= 1) Then

            timePassed = "About an hour ago"

        ElseIf (ts.TotalMinutes.ToInteger() > 1) Then

            timePassed = "About " + ts.TotalMinutes.ToInteger().ToString().ToString() + " minutes ago"

        ElseIf (ts.TotalMinutes.ToInteger() = 1) Then

            timePassed = "About a minute ago"

        ElseIf (ts.TotalSeconds.ToInteger() > 1) Then

            timePassed = "About " + ts.TotalSeconds.ToInteger().ToString().ToString() + " seconds ago"

        ElseIf (ts.TotalSeconds.ToInteger() = 1) Then

            timePassed = "About a second ago"

        End If

        Return timePassed

    End Function

    <Extension()>
    Public Function ToText(ByVal aDouble As Double) As String
        Return ToText(aDouble.ToInteger())
    End Function
    <Extension()>
    Public Function ToText(ByVal aInteger As Integer) As String

        If (aInteger < 0) Then

            Return "Minus " + (-aInteger).ToText

        ElseIf aInteger = 0 Then

            Return "Zero"

        ElseIf (aInteger <= 19) Then

            Return New String() {"One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"}(aInteger - 1) + " "

        ElseIf (aInteger <= 99) Then

            Dim temp As Integer = aInteger Mod 10
            Dim textToAdd As String = ""
            If temp > 0 Then textToAdd = " " & temp.ToText
            Return New String() {"Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"}(Math.Floor((aInteger / 10 - 2).Round(1)).ToInteger) + textToAdd

        ElseIf (aInteger <= 199) Then

            Return "One Hundred " + (aInteger Mod 100).ToText

        ElseIf (aInteger <= 999) Then

            Dim temp As Integer = aInteger Mod 100
            Dim textToAdd As String = ""
            If temp > 0 Then textToAdd = " " & temp.ToText
            Return Math.Floor(aInteger / 100).ToInteger.ToText + "Hundred " + textToAdd

        ElseIf (aInteger <= 1999) Then

            Return "One Thousand " + (aInteger Mod 1000).ToText

        ElseIf (aInteger <= 999999) Then

            Dim temp As Integer = aInteger Mod 1000
            Dim textToAdd As String = ""
            If temp > 0 Then textToAdd = " " & temp.ToText
            Return Math.Floor(aInteger / 1000).ToInteger.ToText + "Thousand " + textToAdd

        ElseIf (aInteger <= 1999999) Then

            Return "One Million " + (aInteger Mod 1000000).ToText

        ElseIf (aInteger <= 999999999) Then

            Dim temp As Integer = aInteger Mod 1000000
            Dim textToAdd As String = ""
            If temp > 0 Then textToAdd = " " & temp.ToText
            Return Math.Floor(aInteger / 1000000).ToInteger.ToText + "Million " + textToAdd

        ElseIf (aInteger <= 1999999999) Then

            Return "One Billion " + (aInteger Mod 1000000000).ToText

        Else

            Dim temp As Integer = aInteger Mod 1000000000
            Dim textToAdd As String = ""
            If temp > 0 Then textToAdd = " " & temp.ToText
            Return Math.Floor(aInteger / 1000000000).ToInteger.ToText + "Billion " + textToAdd

        End If

    End Function

    <Extension()>
    Public Function ToXml(ByVal obj As Object, Optional ByVal xdoc As XDocument = Nothing, Optional ByVal xe As XElement = Nothing) As XDocument

        If xe Is Nothing Then
            xe = New XElement("results")
            If xdoc Is Nothing Then
                xdoc = New XDocument(New XDeclaration("1.0", "utf-8", "yes"), xe)
            End If
        End If

        ' Get all properyInfo
        Dim pis As System.Reflection.PropertyInfo() = obj.GetType().GetProperties(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Instance)

        For Each pi As System.Reflection.PropertyInfo In pis

            ' Get the property value
            Dim o As Object = pi.GetValue(obj, Nothing)

            If o IsNot Nothing Then

                Dim t As Type = o.GetType()

                ' If the property value is an array, retrieve each object value inside
                If t.IsArray Then

                    '// Build the element with the property's name
                    Dim newElement As XElement = New XElement(pi.Name)
                    Dim arr As Object() = DirectCast(o, Object())
                    For i As Integer = 0 To arr.Length - 1
                        ToXml(arr(i), xdoc, newElement)
                        'arr(i).ToXml(xdoc, newElement)
                    Next
                    xe.Add(newElement)
                Else

                    ' For the anonymous type and other entity class type
                    If t.IsClass And t.Name <> "String" Then

                        Dim newElement As XElement = New XElement(pi.Name)
                        ToXml(o, xdoc, newElement)
                        'o.ToXml(xdoc, newElement)
                        xe.Add(newElement)

                        '// For other value type and string type, build the XML element
                    Else

                        Dim newElement As XElement = New XElement(pi.Name, o.ToString())
                        xe.Add(newElement)

                    End If
                End If
            End If
        Next

        Return xdoc
    End Function

    <Extension()>
    Public Function DistinctBy(Of T, TKey)(ByVal source As IEnumerable(Of T), ByVal selector As Func(Of T, TKey)) As IEnumerable(Of T)

        Dim seenKeys As HashSet(Of TKey) = New HashSet(Of TKey)()
        Dim list As List(Of T) = New List(Of T)
        For Each element As T In source
            If seenKeys.Add(selector(element)) Then
                'yield return element;
                list.Add(element)
            End If
        Next

        Return list
    End Function

    <Extension()>
    Public Function GetDescription(ByVal en As System.Enum) As String
        Dim type As Type = en.GetType()
        Dim memInfo As MemberInfo() = type.GetMember(en.ToString())
        If (memInfo IsNot Nothing And memInfo.Length > 0) Then
            Dim attrs As Object() = memInfo(0).GetCustomAttributes(GetType(DescriptionAttribute), False)
            If (attrs IsNot Nothing And attrs.Length > 0) Then
                Return DirectCast(attrs(0), DescriptionAttribute).Description
            End If
        End If
        Return en.ToString()
    End Function

    <Extension()>
    Public Function IsLocked(ByVal aFileInfo As System.IO.FileInfo) As Boolean

        If aFileInfo.Exists Then
            Dim stream As System.IO.FileStream = Nothing
            Try

                stream = aFileInfo.Open(System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None)

            Catch ex As System.IO.IOException

                '//the file is unavailable because it is:
                '//still being written to
                '//or being processed by another thread
                '//or does not exist (has already been processed)
                Return True

            Finally
                If stream IsNot Nothing Then
                    stream.Close()
                End If

            End Try
        End If
        '//file is not locked
        Return False
    End Function

    <Extension()>
    Public Function GetAgeInYears(ByVal dateOfBirth As DateTime) As Integer
        Dim today As DateTime = DateTime.Today
        If dateOfBirth > today Then
            Return 0
        End If
        Dim a As Integer = (today.Year * 100 + today.Month) * 100 + today.Day
        Dim b As Integer = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day

        Return (a - b) / 10000
    End Function

    <Extension()>
    Public Function IsPositive(ByVal aInt As Integer?) As Boolean
        If aInt.HasValue AndAlso aInt.Value > 0 Then
            Return True
        End If

        Return False
    End Function

    <Extension()>
    Public Function IsPositive(ByVal aInt As Integer) As Boolean
        If aInt > 0 Then
            Return True
        End If

        Return False
    End Function

    <Extension()>
    Public Function IsNegative(ByVal aInt As Integer?) As Boolean
        If aInt.HasValue AndAlso aInt.Value < 0 Then
            Return True
        End If

        Return False
    End Function

    <Extension()>
    Public Function IsNegative(ByVal aInt As Integer) As Boolean
        If aInt < 0 Then
            Return True
        End If

        Return False
    End Function
End Module