Imports System.IO

Public Class Form1
    Public mintcurr As Integer
    Private Sub PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.Click
        If MsgBox("Are You Sure You Want to Exit??", MsgBoxStyle.YesNo, "IRE Label Print") = MsgBoxResult.Yes Then
            Application.Exit()
        End If
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If ConnectToDB() = False Then
            Exit Sub
        End If
        Dim dt As DataTable = ReturningDataByQ("select COUNT( Code)count  from dbo.Tbl_ProductMaster where Del=0 and CONVERT(DATETIME,CONVERT(varchar,Date ,112))=CONVERT(DATETIME,CONVERT(varchar,GETDATE() ,112))")
        lblCount.Text = dt.Rows(0)(0).ToString()

        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        If screenWidth = 1024 And screenHeight = 768 Then
            Dim cControl As Control
            For Each cControl In Me.Controls
                If (cControl.Name <> "PictureBox2") And (cControl.Name <> "PictureBox1") And (cControl.Name <> "Label1") And (cControl.Name <> "Label2") And (cControl.Name <> "Label4") And (cControl.Name <> "PictureBox4") Then
                    cControl.Location = New Point(cControl.Location.X - 150, cControl.Location.Y)
                End If
            Next cControl
            Panel2.Location = New Point(Panel2.Location.X - 20, Panel2.Location.Y)
        End If
        txtid.Focus()

    End Sub

    Private Sub C1Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C1Button1.Click
        Dim intnum As Integer
        Dim str, strrev, strcheck, strfinal As String
        If txtname.Text = "" Then
            MsgBox("Enter Product Name!", MsgBoxStyle.Critical, "IRE Label Print")
            txtname.Focus()
            Exit Sub
        End If
        If txtQty.Text = "" Then
            MsgBox("Enter Qty to be Print!", MsgBoxStyle.Critical, "IRE Label Print")
            txtQty.Focus()
            Exit Sub
        End If
        If MsgBox("Are You Sure want to print??", MsgBoxStyle.YesNo, "IRE Label Print") = MsgBoxResult.No Then
            Exit Sub
        End If
        ReturningDataByQ("INSERT INTO [Tbl_ProductMaster]([Code],[Name],[Qty],[Date],[Del])VALUES('" & txtid.Text & "','" & txtname.Text & "','" & txtQty.Text & "',getdate(),0)")
        txtid.Text = ""
        txtname.Text = ""
        txtQty.Text = ""
    End Sub
    Public Function DefaultPrinterName() As String
        Dim oPS As New System.Drawing.Printing.PrinterSettings
        Try
            DefaultPrinterName = oPS.PrinterName
        Catch ex As System.Exception
            DefaultPrinterName = ""
        Finally
            oPS = Nothing
        End Try
    End Function

    'Public Function replace(ByVal str As String) As String
    '    Dim strres, strfinalres As String
    '    For intcount = 0 To str.Length - 1
    '        If str(intcount) = "0" Then
    '            strres = "9"
    '        ElseIf str(intcount) = "1" Then
    '            strres = "8"
    '        ElseIf str(intcount) = "2" Then
    '            strres = "7"
    '        ElseIf str(intcount) = "3" Then
    '            strres = "6"
    '        ElseIf str(intcount) = "4" Then
    '            strres = "5"
    '        ElseIf str(intcount) = "5" Then
    '            strres = "4"
    '        ElseIf str(intcount) = "6" Then
    '            strres = "3"
    '        ElseIf str(intcount) = "7" Then
    '            strres = "2"
    '        ElseIf str(intcount) = "8" Then
    '            strres = "1"
    '        ElseIf str(intcount) = "9" Then
    '            strres = "0"
    '        End If
    '        strfinalres = strfinalres + strres
    '    Next
    '    Return strfinalres
    'End Function

    'Public Function checksum(ByVal str As String) As String
    '    Dim intres, intres1, intsumres, intcheck, strlast, strcheck As String
    '    intres = CInt(str.Substring(0, 1)) + CInt(str.Substring(2, 1)) + CInt(str.Substring(4, 1)) + CInt(str.Substring(6, 1)) + CInt(str.Substring(8, 1)) + CInt(str.Substring(10, 1))
    '    intres1 = CInt(str.Substring(1, 1)) + CInt(str.Substring(3, 1)) + CInt(str.Substring(5, 1)) + CInt(str.Substring(7, 1)) + CInt(str.Substring(9, 1)) + CInt(str.Substring(11, 1))
    '    intsumres = intres * 4 + intres1
    '    intcheck = intsumres Mod 10
    '    strlast = intcheck.ToString().Substring(intcheck.Length - 1, 1)
    '    strcheck = 10 - CInt(strlast)
    '    If strcheck = 10 Then
    '        strcheck = 0
    '    End If
    '    Return strcheck
    'End Function

    Private Sub C1Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles C1Button2.Click
        If MsgBox("Are You Sure You Want to Exit??", MsgBoxStyle.YesNo, "IRE Label Print") = MsgBoxResult.Yes Then
            Application.Exit()
        End If
    End Sub

    Private Sub PictureBox4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox4.Click
        If Panel2.Visible = True Then
            Panel2.Visible = False
        Else
            Panel2.Visible = True
        End If
    End Sub

    Private Sub C1Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Panel2.Visible = False
    End Sub

    Private Sub txtid_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtid.KeyDown
        If e.KeyCode = Keys.Enter Then
            txtname.Focus()
        End If
    End Sub

    Private Sub txtname_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtname.KeyDown
        If e.KeyCode = Keys.Enter Then
            txtQty.Focus()
        End If
    End Sub

    Private Sub txtQty_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtQty.KeyDown
        If e.KeyCode = Keys.Enter Then
            C1Button1_Click(sender, e)
        End If
    End Sub
End Class
