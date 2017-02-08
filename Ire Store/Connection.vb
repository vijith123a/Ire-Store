
Imports System.Data.SqlClient
Imports System.Net.Mail
Imports System.IO
Module Connection
    Dim Sql_Connection As New SqlConnection

    Public RptCmd As New SqlCommand
    '--------------------------------
    Public boxprint As Boolean = False
    Public index As Long = 0
    Public Dr As SqlDataReader
    Public Dr1 As SqlDataReader
    '--------------------------------
    Dim MyTran As SqlTransaction
    '--------------------------------
    Dim ServerIP As String = ""
    Dim DBname As String = ""
    Dim DBUserID As String = ""
    Dim DBPassword As String = ""
    Public Loc_Prefix As String
    Public Loc_Code As String
    '-------------------------
    Public User_Code As Integer = 0
    Public CompName As String = "IRE Store"
    '---------------------------------
    Public Null_DT As New Date(1990, 1, 1)
    Public MinSchPeriod As Integer = 10
    Public Port As String = ""
    Public Const COLOR_CAPTIONTEXT = 9
    Declare Function SetSysColors Lib "user32" _
(ByVal nChanges As Long, ByVal lpSysColor As _
Long, ByVal lpColorValues As Long) As Long
    Public Enum EnmPKTypes
        DefectMaster
        UserMaster
        ResourceMaster
        ResourceTypeMaster
        LineMaster
        ShiftMaster
        HolidayMaster
        ProductionSceduleID
        PlanGroupNo
        ProductResourceReqID
        ProductionStdTimeID
        WorkOrderID
        ProgramShiftID
    End Enum
    Function generatenext(ByVal tablename As String, ByVal datafield As String) As Integer
        Dim intnext As Integer
        Try
            ConnectToDB()
            RptCmd = New SqlCommand("select isnull(COUNT(" & datafield & "),0)+ 001 from " & tablename & "", Sql_Connection)
            Dr = RptCmd.ExecuteReader
            While Dr.Read
                intnext = Dr(0).ToString
            End While
            Dr.Close()
            Return intnext
        Catch ex As Exception
            Dr.Close()
        End Try
    End Function

    Public Function ConnectToDB(Optional ByVal Disconnect As Boolean = False) As Boolean
        Try
            If Disconnect = True Then
                If Sql_Connection.State = ConnectionState.Open Then
                    Sql_Connection.Close()
                End If
            End If
            If Sql_Connection.State = ConnectionState.Open Then
                Return True
            End If

            If ReadServerID() = True Then
                Sql_Connection.ConnectionString = "Data Source=" & ServerIP & ";Initial Catalog=" & DBname & ";User Id=" & DBUserID & ";PWD=" & DBPassword & ";MultipleActiveResultSets=True"
                Sql_Connection.Open()
                RptCmd.Connection = Sql_Connection
                ConnectToDB = True
            Else
                Return False
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
            ConnectToDB = False
        End Try
    End Function
    Public Function ReadServerID() As Boolean
        ReadServerID = False
        Try
            Dim path As String = Application.StartupPath & "/Server.txt"
            If File.Exists(path) = False Then
                MsgBox("Server File Missing")
                Exit Function
            End If
            Dim mindex As Integer = 0
            Dim myreader As StreamReader
            myreader = New StreamReader(path)
            While myreader.Peek <> -1
                If mindex = 0 Then
                    ServerIP = myreader.ReadLine
                ElseIf mindex = 1 Then
                    DBname = myreader.ReadLine
                ElseIf mindex = 2 Then
                    DBUserID = myreader.ReadLine
                ElseIf mindex = 3 Then
                    DBPassword = myreader.ReadLine
                ElseIf mindex = 4 Then
                    Loc_Code = myreader.ReadLine
                End If
                mindex += 1
            End While
            myreader.Close()
            If ServerIP = "" Then
                WarningMessage("Invalid IP Address")
            ElseIf DBname = "" Then
                WarningMessage("Invalid DataBase Name")
            ElseIf DBUserID = "" Then
                WarningMessage("Invalid DB User ID")
            ElseIf DBPassword = "" Then
                WarningMessage("Invalid DB Password")
            ElseIf Loc_Code = "" Then
                WarningMessage("Invalid Location Code")
            Else
                ReadServerID = True
            End If
        Catch ex As Exception
            WarningMessage(ex.Message)
        End Try
    End Function
    Public Function FormatToSqlDateTime(ByVal DateVal As DateTime)
        Return DateVal.Year.ToString("D4") & "-" & DateVal.Month.ToString("D2") & "-" & DateVal.Day.ToString("D2") & " " & DateVal.Hour.ToString("D2") & ":" & DateVal.Minute.ToString("D2") & ":" & DateVal.Second.ToString("D2")
    End Function
    Public Sub WarningMessage(ByVal Mess As String)
        MsgBox(Mess, MsgBoxStyle.Critical, CompName)
    End Sub
    Public Function ConfirmMessage(ByVal Mess As String) As MsgBoxResult '  for any Warining Message lie are u sure to delete data
        ConfirmMessage = MsgBox(Mess, MsgBoxStyle.YesNo + MsgBoxStyle.Question, CompName)
    End Function
    Public Sub InfoMessage(ByVal Mess As String) ' for Record Added, Updated, Deleted
        MsgBox(Mess, MsgBoxStyle.Information, CompName)
    End Sub
    Public Function WorkStation() As String
        WorkStation = "'" & Mid(Environment.MachineName, 1, 30) & "'"
    End Function
    Public Sub BeginTransaction()
        MyTran = Sql_Connection.BeginTransaction
    End Sub
    Public Sub RollBackTransaction()
        MyTran.Rollback()
    End Sub
    Public Sub CommitTransaction()
        MyTran.Commit()
    End Sub
    Public Function AssignConnection(ByRef SqlCmd As SqlCommand) As Integer
        SqlCmd.Connection = Sql_Connection
        If Not MyTran Is Nothing Then
            SqlCmd.Transaction = MyTran
        End If
    End Function
    Public Sub LoadCombo(ByVal Cntr As ComboBox, ByVal Qry As String, Optional ByVal defaultval As Boolean = False)
        Dim SQLCmd As New SqlCommand
        Dim SQLDr As SqlDataReader

        AssignConnection(SQLCmd)

        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If


        ' Try
        Cntr.Text = ""
        Cntr.SelectedIndex = -1
        Cntr.Items.Clear()
        If defaultval = True Then
            Cntr.Items.Add(New itemdata(0, "<<NA>>"))
        End If

        If Qry = "" Then
            Exit Sub
        End If

        SQLCmd.CommandText = Qry
        SQLDr = SQLCmd.ExecuteReader
        While SQLDr.Read
            If SQLDr.FieldCount < 2 Then
                Cntr.Items.Add(New itemdata(SQLDr(0), SQLDr(0)))
            Else
                Cntr.Items.Add(New itemdata(SQLDr(0), SQLDr(1)))
            End If

        End While
        SQLDr.Close()

        If defaultval = True Then
            If Cntr.Items.Count > 0 Then
                Cntr.SelectedIndex = 0
            End If
        End If
        Cntr.AutoCompleteSource = AutoCompleteSource.CustomSource
        Cntr.AutoCompleteMode = AutoCompleteMode.Suggest
        SQLCmd.Dispose()

    End Sub
    Public Function GetTranCode(ByVal Operation As EnmPKTypes) As Long
        Select Case Operation
            Case EnmPKTypes.UserMaster
                Return TransactionCode("US_CODE", "11", "", "USMT", "000")
            Case EnmPKTypes.DefectMaster
                Return TransactionCode("DF_ID", "12", "", "DFMT", "000")
            Case EnmPKTypes.LineMaster
                Return TransactionCode("LI_ID", "13", "", "LIMT", "000")
            Case EnmPKTypes.ResourceMaster
                Return TransactionCode("RS_ID", "14", "", "RSMT", "000")
            Case EnmPKTypes.ResourceTypeMaster
                Return TransactionCode("RST_ID", "15", "", "RSTM", "000")
            Case EnmPKTypes.ShiftMaster
                Return TransactionCode("SH_CODE", "16", "", "SHIFT_MASTER", "000")
            Case EnmPKTypes.HolidayMaster
                Return TransactionCode("HL_ID", "17", "", "HLTRSUB", "000")
            Case EnmPKTypes.PlanGroupNo
                Return TransactionCode("PlanGrpNo", "", GetServerDate(3), "PLMT", "000")
            Case EnmPKTypes.ProductResourceReqID
                Return TransactionCode("PRR_ID", "18", "", "PRRM", "000")
            Case EnmPKTypes.ProductionSceduleID
                Return TransactionCode("PRSC_ID", "", GetServerDate(3), "Z_PR_RS_SC", "000")
            Case EnmPKTypes.ProductionStdTimeID
                Return TransactionCode("PRS_ID", "19", "", "PRST", "000")
            Case EnmPKTypes.WorkOrderID
                Return TransactionCode("ID", "", GetServerDate(3), "WOMT", "000")
            Case EnmPKTypes.ProgramShiftID

            Case Else
                MsgBox("Error In GetTran Function Call", MsgBoxStyle.Critical)
                Return 0
        End Select
    End Function
    ''' <summary>
    ''' Return the ServerDate based on the Given Format
    ''' </summary>
    ''' <param name="DtFormat">1.yy,2.yyMM,3.yyMMdd,4.MM</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetServerDate(ByVal DtFormat As Integer) As Long
        Dim SQLCmd As New SqlCommand
        Dim SQLDr As SqlDataReader

        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If


        AssignConnection(SQLCmd)

        Dim SDate As Date
        Dim StFormat As String = ""
        If DtFormat = 1 Then
            StFormat = "yy"
        ElseIf DtFormat = 2 Then
            StFormat = "yyMM"
        ElseIf DtFormat = 3 Then
            StFormat = "yyMMdd"
        ElseIf DtFormat = 4 Then
            StFormat = "MM"
        End If

        SQLCmd.CommandText = "SELECT GETDATE()"
        SQLDr = SQLCmd.ExecuteReader
        If SQLDr.Read Then
            SDate = SQLDr(0)
        End If
        SQLDr.Close()

        GetServerDate = CInt(Format(SDate, StFormat))

        SQLCmd.Dispose()

        ' End If
    End Function
    Private Function TransactionCode(ByVal PrimaryKeyField As String, ByVal Operation As String, ByVal ServerYear As String, ByVal TableName As String, ByVal Zeros As String) As Long

        Dim SQLCmd As New SqlCommand
        Dim SQLDr As SqlDataReader
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        AssignConnection(SQLCmd)
        SQLCmd.CommandText = "select ISNULL (MAX(" & PrimaryKeyField & ")," & Operation & ServerYear.ToString & Zeros & ") + 1 from " & TableName & " where " & PrimaryKeyField & " LIKE '" & Operation & ServerYear.ToString & "%'"
        SQLDr = SQLCmd.ExecuteReader
        SQLDr.Read()
        TransactionCode = SQLDr(0)
        SQLDr.Close()

        SQLCmd.Dispose()

        Return TransactionCode
    End Function
    Public Function Replicationcheck(ByVal SQLQuery As String) As Boolean
        Dim SQLCmd As New SqlCommand
        Dim SQLDr As SqlDataReader

        AssignConnection(SQLCmd)

        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If

        SQLCmd.CommandText = SQLQuery
        SQLDr = SQLCmd.ExecuteReader
        If SQLDr.Read() = False Then
            Replicationcheck = False
        Else
            Replicationcheck = True
        End If
        SQLDr.Close()
        SQLCmd.Dispose()
        Return Replicationcheck
    End Function
    Public Function SaveToDb(ByVal Qry As String) As Boolean
        Dim Resulr As Integer = 0
        Dim cmd As New SqlCommand
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        cmd.CommandText = Qry
        AssignConnection(cmd)
        If Not MyTran Is Nothing Then
            cmd.Transaction = MyTran
        End If
        Resulr = cmd.ExecuteNonQuery()
        If Resulr > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function ReturningDataBySp(ByVal FromDate As DateTime, ByVal ToDate As DateTime, ByVal Flag As Integer, ByVal product As String, ByVal Grp As String, ByVal color As String, ByVal bin As String) As DataTable
        Dim cmd As New SqlCommand("Sp_Report", Sql_Connection)
        AssignConnection(cmd)
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        cmd.CommandType = CommandType.StoredProcedure
        cmd.Connection = Sql_Connection
        cmd.Parameters.Clear()
        cmd.Parameters.Add("@Flag", SqlDbType.BigInt).Value = Flag
        'If Flag = 1 Then
        cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = FromDate
        cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = ToDate
        'End If
        cmd.Parameters.Add("@Product", SqlDbType.VarChar).Value = product
        cmd.Parameters.Add("@Grp", SqlDbType.VarChar).Value = Grp
        cmd.Parameters.Add("@Bin", SqlDbType.VarChar).Value = bin
        cmd.Parameters.Add("@Color", SqlDbType.VarChar).Value = color
        Dim adp As New SqlDataAdapter(cmd)
        Dim dt As New DataTable()
        adp.Fill(dt)
        Return dt
    End Function
    Public Function ReturningDataByQ(ByVal query As String) As DataTable
        Dim cmd As New SqlCommand
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        cmd.CommandText = query
        AssignConnection(cmd)
        If Not MyTran Is Nothing Then
            cmd.Transaction = MyTran
        End If
        Dim adp As New SqlDataAdapter(cmd)
        Dim dt As New DataTable()
        adp.Fill(dt)
        Return dt
    End Function
    Public Sub synctable(ByVal query As String)
        query = query.Replace("'", "|")
        ExecuteQuery("insert into Sync_Master(Sync_Date,Sync_Query,Location_Code,Status) values(getdate(),'" & query & "'," & Loc_Code & ",0)")
    End Sub
    Public Function SelectQuery(ByVal Query As String) As SqlDataReader
        Dim SQLCmd As New SqlCommand
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        AssignConnection(SQLCmd)
        SQLCmd.CommandText = Query
        Dim TmpDr As SqlDataReader
        TmpDr = SQLCmd.ExecuteReader
        SQLCmd.Dispose()
        Return TmpDr
    End Function
    Public Function SelectQueryScalar(ByVal Query As String) As Object
        Dim SQLCmd As New SqlCommand
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        AssignConnection(SQLCmd)
        SQLCmd.CommandText = Query
        SelectQueryScalar = SQLCmd.ExecuteScalar()
        SQLCmd.Dispose()
    End Function
    Public Function SaveDt(ByVal dt As DataTable) As Boolean
        Try
            If Sql_Connection.State = ConnectionState.Closed Then
                If ConnectToDB() = False Then
                    WarningMessage("Connection To DB Failed")
                    End
                End If
            End If
            Dim Resulr As Integer = 0
            Dim cmd As New SqlCommand("BulkSave", Sql_Connection)
            AssignConnection(cmd)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Connection = Sql_Connection
            cmd.Parameters.Clear()
            cmd.Parameters.Add("@Flag", SqlDbType.BigInt).Value = 1
            cmd.Parameters.AddWithValue("@UserDefineTable", dt) ' passing Datatable
            Resulr = cmd.ExecuteNonQuery()
            If Resulr > 0 Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception

        End Try
    End Function
    Public Function ExecuteQuery(ByVal Query As String) As Boolean
        ExecuteQuery = False
        Dim SQLCmd As New SqlCommand
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        AssignConnection(SQLCmd)
        SQLCmd.CommandText = Query
        SQLCmd.CommandTimeout = 0
        SQLCmd.ExecuteNonQuery()
        SQLCmd.Dispose()
        ExecuteQuery = True
    End Function
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Combo"></param>
    ''' <param name="SQLQuery"></param>
    ''' <param name="NofFields"></param>
    ''' <param name="FieldNames">Comma Seperated as DB Name</param>
    ''' <remarks></remarks>
    
    
    Public Function GetSerialNoBoxNoPalletNo(ByVal Operation As String, ByVal LiID As String) As Long
        Dim SQLCmd As New SqlCommand
        Dim SQLDr As SqlDataReader
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        AssignConnection(SQLCmd)

        Dim ServerYear As String = GetServerDate(3)
        Dim PrimaryKeyField As String = ""
        Dim Zeros As String = "00000"

        If Operation = "SERIALNO" Then
            PrimaryKeyField = "SERIALNO"
        ElseIf Operation = "BOXID" Then
            PrimaryKeyField = "BOXID"
        ElseIf Operation = "PALLETID" Then
            PrimaryKeyField = "PALLETID"
        Else
            WarningMessage("Invalid Function Call Serial No Generation")
            Exit Function
        End If

        SQLCmd.CommandText = "select ISNULL (MAX(" & PrimaryKeyField & ")," & ServerYear.ToString & Zeros & ")+1 from PRD_MN where LI_ID=" & LiID & " AND " & PrimaryKeyField & " LIKE '" & ServerYear.ToString & "%'"
        SQLDr = SQLCmd.ExecuteReader
        SQLDr.Read()
        GetSerialNoBoxNoPalletNo = SQLDr(0)
        SQLDr.Close()
        SQLCmd.Dispose()

        Return GetSerialNoBoxNoPalletNo
    End Function
   
    Public Function SelectQuery1(ByVal Query As String) As DataSet
        Dim SQLCmd As New SqlCommand
        Dim SQlAdapter As New SqlDataAdapter
        Dim SQlDataset As New DataSet
        If Sql_Connection.State = ConnectionState.Closed Then
            If ConnectToDB() = False Then
                WarningMessage("Connection To DB Failed")
                End
            End If
        End If
        AssignConnection(SQLCmd)
        SQLCmd.CommandText = Query
        SQLCmd.CommandTimeout = 0
        SQlAdapter.SelectCommand = SQLCmd
        SQlAdapter.Fill(SQlDataset)
        SQLCmd.Dispose()
        Return SQlDataset
    End Function
   
End Module
