' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: Word
' Author: Miz
' Date: 2019/7/10 20:06:59
' -----------------------------------------

Public Class Word

    Public Spelling As String = vbNullString

    Public Chinese As String = vbNullString

    Public PassCount As Integer = 0

    ''' <summary>
    ''' 用于动态规划来计算字符串编辑距离
    ''' </summary>
    Public Chars As Char() = Nothing

    Public Shared Function GetDistance(a As String, b As String)

    End Function

End Class


