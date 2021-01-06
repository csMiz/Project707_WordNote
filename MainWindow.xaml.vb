Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Threading

Class MainWindow

    Private Rnd As New Random

    Public ProgressColor(1) As Color
    Public ColorDist As Integer()

    Public WordBuffer As List(Of Word)

    Public DictateBuffer As List(Of Word)

    Public CurrentWord As Word = Nothing

    Public CurrentWordHint As Boolean = False

    Public SearchResult As New List(Of String)
    Public SearchRefreshCount As Integer = 0

    Public Search_Timer As New DispatcherTimer

    Public AllWordBuffer As Word()


    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        WordBuffer = New List(Of Word)

        LoadWordList()

        'LoadAllWords()

        AddHandler Search_Timer.Tick, AddressOf RefreshSearchResult
        Search_Timer.Interval = TimeSpan.FromMilliseconds(500)

    End Sub

    Public Sub LoadAllWords()

        Dim tmpAllWordList As New List(Of Word)
        For Each tmpItem As ListViewItem In list1.Items
            Dim path As String = tmpItem.Tag

            Dim file As New FileStream(path, FileMode.Open)
            Dim content As String = vbNullString
            Using sr As New StreamReader(file)
                content = sr.ReadToEnd
            End Using
            file.Close()
            file.Dispose()

            Dim lines As String() = Regex.Split(content, vbCrLf)
            For Each line As String In lines
                If line.Trim.Length > 0 Then
                    Dim segs As String() = line.Split(vbTab)
                    Dim tmpWord As New Word
                    With tmpWord
                        .Spelling = segs(0)
                        .Chinese = segs(1)
                    End With
                    tmpAllWordList.Add(tmpWord)
                End If
            Next

        Next

        AllWordBuffer = tmpAllWordList.ToArray

    End Sub

    ''' <summary>
    ''' 加载所有单词列表
    ''' </summary>
    Public Sub LoadWordList()

        Dim default_font As FontFamily = New FontFamily("Mirosoft YaHei")

        Dim root_path As String = AppDomain.CurrentDomain.BaseDirectory & "\words\"
        Dim folder As DirectoryInfo = New DirectoryInfo(root_path)
        For Each onefile As FileInfo In folder.GetFiles()
            Dim tmpItem As New ListViewItem
            With tmpItem
                .Content = onefile.Name
                .Tag = onefile.FullName
                .Height = 36
                .FontSize = 20
                .FontFamily = default_font
            End With
            AddHandler tmpItem.MouseUp, AddressOf WordList_Click
            list1.Items.Add(tmpItem)
        Next

    End Sub

    Public Sub LoadListDetail(path As String)

        ProgressColor(0) = Color.FromArgb(255, Rnd.Next(255), Rnd.Next(255), Rnd.Next(255))
        ProgressColor(1) = Color.FromArgb(255, Rnd.Next(255), Rnd.Next(255), Rnd.Next(255))
        ColorDist = {CInt(ProgressColor(1).R) - CInt(ProgressColor(0).R),
                                            CInt(ProgressColor(1).G) - CInt(ProgressColor(0).G),
                                            CInt(ProgressColor(1).B) - CInt(ProgressColor(0).B)}

        WordBuffer.Clear()

        Dim file As New FileStream(path, FileMode.Open)
        Dim content As String = vbNullString
        Using sr As New StreamReader(file)
            content = sr.ReadToEnd
        End Using
        file.Close()
        file.Dispose()

        Dim lines As String() = Regex.Split(content, vbCrLf)
        For Each line As String In lines
            If line.Trim.Length > 0 Then
                Dim segs As String() = line.Split(vbTab)
                Dim tmpWord As New Word
                With tmpWord
                    .Spelling = segs(0)
                    .Chinese = segs(1)
                End With
                WordBuffer.Add(tmpWord)
            End If
        Next

        If DictateBuffer Is Nothing Then
            DictateBuffer = New List(Of Word)
        Else
            DictateBuffer.Clear()
        End If
        DictateBuffer.AddRange(WordBuffer)

        CurrentWord = DictateBuffer(Rnd.Next(DictateBuffer.Count))
        CurrentWordHint = False
        RefreshWordDisplay()

    End Sub

    Public Sub RefreshWordDisplay()
        If CurrentWord Is Nothing Then Return

        tb_wordContent.Text = CurrentWord.Spelling
        If CurrentWordHint Then
            tb_chinese.Text = CurrentWord.Chinese
            tb_chinese.Visibility = Visibility.Visible
        Else
            tb_chinese.Visibility = Visibility.Collapsed
        End If
        tb_progress.Text = (WordBuffer.Count - DictateBuffer.Count + 1).ToString & " / " & WordBuffer.Count
        Dim passCount As Integer = (WordBuffer.Count - DictateBuffer.Count + 1)
        Dim prog_rate As Single = (passCount * 1.0F / WordBuffer.Count)
        rect_progress.Width = 100 * prog_rate
        rect_progress.Margin = New Thickness(0, 20, 5 + 100 * (1 - prog_rate) - 1, 0)

        Dim tmpColor As Color = Color.FromArgb(255, ProgressColor(0).R + prog_rate * ColorDist(0),
                                               ProgressColor(0).G + prog_rate * ColorDist(1),
                                               ProgressColor(0).B + prog_rate * ColorDist(2))

        rect_progress.Fill = New SolidColorBrush(tmpColor)


    End Sub

    Public Sub QuitDictate()
        MessageBox.Show("all passed")
        ' TODO

    End Sub

    Public Sub SearchWord(key As String)
        list_search.Items.Clear()
        SearchResult.Clear()
        Dim root_path As String = AppDomain.CurrentDomain.BaseDirectory & "\words\"
        Dim folder As DirectoryInfo = New DirectoryInfo(root_path)

        Dim searchInOneFile = Sub(fs As FileStream)
                                  Dim fileContent As String = vbNullString
                                  Using sr As New StreamReader(fs)
                                      fileContent = sr.ReadToEnd
                                  End Using
                                  fs.Close()
                                  fs.Dispose()
                                  Dim lines As String() = Regex.Split(fileContent, vbCrLf)
                                  For Each line As String In lines
                                      If line.Contains(key) Then
                                          SearchResult.Add(line)
                                      End If
                                  Next
                              End Sub

        For Each onefile As FileInfo In folder.GetFiles()
            Dim t As New Threading.Thread(searchInOneFile)
            t.Start(onefile.OpenRead)
        Next

    End Sub



    Public Sub WordList_Click(sender As Object, e As MouseButtonEventArgs)

        LoadListDetail(sender.tag)

        grid_display.Visibility = Visibility.Visible
        grid_home.Visibility = Visibility.Collapsed
    End Sub

    Private Sub btn_know_Click(sender As Object, e As RoutedEventArgs) Handles btn_know.Click
        If CurrentWord IsNot Nothing Then
            If Not CurrentWordHint Then
                CurrentWord.PassCount += 1
                If CurrentWord.PassCount >= 3 Then
                    DictateBuffer.Remove(CurrentWord)
                End If
            End If
        End If
        CurrentWordHint = False
        If DictateBuffer.Count Then
            CurrentWord = DictateBuffer(Rnd.Next(DictateBuffer.Count))
        Else
            QuitDictate()
            Return
        End If
        RefreshWordDisplay()
    End Sub

    Private Sub btn_hint_Click(sender As Object, e As RoutedEventArgs) Handles btn_hint.Click
        CurrentWordHint = True
        RefreshWordDisplay()
    End Sub

    Private Sub RefreshSearchResult()
        list_search.Items.Clear()

        For Each tmpStr As String In SearchResult
            Dim tmpLVI As New ListViewItem With {.Content = tmpStr}
            list_search.Items.Add(tmpLVI)
        Next

        SearchRefreshCount += 1
        If SearchRefreshCount >= 4 Then
            SearchRefreshCount = 0
            Search_Timer.Stop()
        End If

    End Sub

    Private Sub tbox_search_KeyDown(sender As Object, e As KeyEventArgs) Handles tbox_search.KeyDown
        If e.Key = Key.Enter Then
            If tbox_search.Text.Trim = "" Then
                Return
            End If
            SearchWord(tbox_search.Text)
            SearchRefreshCount = 0
            Search_Timer.Start()
        End If

    End Sub
End Class
