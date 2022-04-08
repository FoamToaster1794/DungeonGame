Imports System.Console
Imports System.IO
Imports System.Convert
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.SqlServer.Server

Module Module1
    Const johnson = "fileNames.txt"
    Const wall = "██"
    Const floor = "  "
    Const wallSize = 2
    Const initialFontWeight = 400
    Const initialFontSize = 36
    Const initialFontName = "Lucida Sans Typewriter"
    
    Const defaultMazeWidth = 101
    Const defaultMazeHeight = 101
    Const roomTryCount As Short = 400
    Const extraConnectorChance As Byte = 35
    Const roomExtraSize As Short = 1
    Const windingPercent As Byte = 50
    Const showMazeGen = True
    
    Sub Main()
        Dim mainMenuChoice As Integer
        Dim currentMaze As maze
        
        '          max for 1080p:943,  325
        Dim mazeSize = New vec(101, 101)
        Dim roomTryCount As Short = 32767
        Dim extraConnectorChance As Byte = 20
        Dim roomExtraSize As Short = 0
        Dim windingPercent As Byte = 60
        Dim showMazeGen = True
        
        
        MaximiseConsole()
        Do 
            Clear()
            SetupConsole(initialFontWeight, initialFontSize, initialFontName)
            Randomize()
            MainMenu(mainMenuChoice)
            Select Case mainMenuChoice
                Case 0
                    LoadMaze(currentMaze)
'                    Dim maze As maze = LoadGrid("blank.txt")
'                    If maze.Size.x > 0 Then
'                         DisplayMaze(maze)
'                    End If
                Case 1
                    Dim fontSize As Byte = CalculateFontSize(mazeSize)
                    MsgBox(fontSize)
                    SetupConsole(100, fontSize, "Consolas")
                    Dim generatedMaze As maze = GenerateMaze(mazeSize, roomTryCount, extraConnectorChance, roomExtraSize,
                                                             windingPercent, showMazeGen)
                    SetCursorPosition(0, 0)
                    DisplayMaze(generatedMaze)
                    ReadLine()
                    Clear()
                Case 2
                    GenerationMenu(mazeSize, roomTryCount, extraConnectorChance, roomExtraSize, windingPercent, showMazeGen)
            End Select
        Loop Until mainMenuChoice = 3
    End Sub
    
    

    Private Sub MainMenu(ByRef position As Integer)
        Dim keypressed As Integer
        Dim topposition = 0
        Dim bottomspot = 3
        position = 0
        WriteLine(" Load maze")
        WriteLine(" Generate new maze")
        WriteLine(" Change generation parameters")
        WriteLine(" Exit")
        Do
            SetCursorPosition(0, position)
            Write(">")
            keypressed = ReadKey(True).Key
            Select Case keypressed
                Case Is = ConsoleKey.DownArrow
                    CursorLeft -= 1
                    Write(" ")
                    If position < bottomspot Then
                        position += 1
                    End If
                Case Is = ConsoleKey.UpArrow
                    CursorLeft -= 1
                    Write(" ")
                    If position > topposition Then
                        position -= 1
                    End If
            End Select
        Loop Until keypressed = ConsoleKey.Enter
        Clear()
    End Sub
    
    Private Sub LoadMaze(ByRef maze As maze)
        If Not File.Exists(johnson)
            WriteLine("No files to load")
            ReadLine()
            Return
        End If
        Dim fileNames() As String = File.ReadAllLines(johnson)
        If fileNames.Length = 0
            WriteLine("No files to load")
            ReadLine()
            Return
        End If
        
        'menu code
        Dim keypressed As Integer
        Const topPos As Byte = 0
        Dim bottomPos As Byte = fileNames.Length
        Dim position = 0
        WriteLine(" Back to main menu")
        For Each fileName As String In fileNames
            WriteLine(" " & fileName)
        Next
        Do
            SetCursorPosition(0, position)
            Write(">")
            keypressed = ReadKey(True).Key
            Select Case keypressed
                Case Is = ConsoleKey.DownArrow
                    CursorLeft -= 1
                    Write(" ")
                    If position < bottomPos Then
                        position += 1
                    End If
                Case Is = ConsoleKey.UpArrow
                    CursorLeft -= 1
                    Write(" ")
                    If position > topPos Then
                        position -= 1
                    End If
            End Select
        Loop Until keypressed = ConsoleKey.Enter
        If position = 0
            Return
        End If
        Clear()
        Dim mazeName As String = fileNames(position)
        maze = LoadMazeFromFile(mazeName)
    End Sub

    Private Function LoadMazeFromFile(fileName As String) As maze
        Dim lines() As String = File.ReadAllLines(fileName)
        Dim input As String = lines(0).Remove(0, 8)
        Dim tempHeight, tempWidth As Integer
        If IsNumeric(input) AndAlso input > 0 AndAlso input Mod 1 = 0 Then
            tempHeight = input
        Else
            WriteLine("Error has occured when loading grid")
            WriteLine("Height is not valid")
            Return New maze()
        End If
        input = lines(1).Remove(0, 7)
        If IsNumeric(input) AndAlso input > 0 AndAlso input Mod 1 = 0 Then
            tempWidth = input
        Else
            WriteLine("Error has occured when loading grid")
            WriteLine("Width is not valid")
            Return New maze()
        End If
        Dim maze = New maze(New vec(tempWidth, tempHeight))
        For y = 0 To maze.Size.y - 1
            For x = 0 To lines(y + 2).Length - 1
                Dim character As Char = lines(y + 2)(x)
                If Not Char.IsNumber(character) OrElse ToInt16(character.ToString()) > 1 Then
                    WriteLine("Error has occured when loading grid")
                    WriteLine("Grid cell at position (" & x & ", " & y & ") is not valid")
                    Return New maze()
                End If
                maze.Cells(x, y) = ToInt16(character.ToString())
            Next
        Next
        Return maze
    End Function

    Private Sub SaveMaze(maze As maze, fileName As String)
        FileOpen(0, fileName, OpenMode.Output)
        FileSystem.WriteLine(0, "Height: " & maze.Size.y)
        FileSystem.WriteLine(0, "Width: " & maze.Size.x)
        For y = 0 To maze.Size.y
            For x = 0 To maze.Size.x
                FileSystem.Write(0, maze.Cells(x, y))
            Next
            FileSystem.WriteLine(0, "")
        Next
        FileClose(0)
    End Sub
    
    Private Sub GenerationMenu(ByRef mazeSize As vec, ByRef roomTryCount As Short, ByRef extraConnectorChance As Byte,
                             ByRef roomExtraSize As Short, ByRef windingPercent As Byte, ByRef showMazeGen As Boolean)
        Dim keypressed As Integer
        Const topPos As Byte = 2
        Const bottomPos As Byte = 6
        Dim position = 0
        Dim lines() As String = {"Maze width (odd integer 21-325): ", "Maze height (odd integer 21-943): ",
                                 "No. of attempts to place a room (0-1000 recommended): ",
                                 "Percentage chance for extra room connections (0-100): ",
                                 "Extra room size (0-10 recommended): ",
                                 "Percentage chance for paths to wind (0-100): ",
                                 "Instant generation (True(1)-False(0)): "}
        
        WriteLine("Generation Settings (think of these like difficulty settings)")
        WriteLine("Press Enter to save settings")
        For x = 0 To lines.Length - 1
            WriteLine(lines(x))
        Next
        Dim errorMessage As String
        Dim input(bottomPos + 1) As String
        For x = 0 To bottomPos
            input(x) = ""
        Next
        Do
            Dim isInputting = False
            Dim inputLength As Byte
            Do
                If Not isInputting
                    SetCursorPosition(lines(position).Length - 1, position + topPos)
                    Write(">")
                End If
                keypressed = ReadKey(True).Key
                Select Case keypressed
                    Case ConsoleKey.DownArrow
                        CursorLeft = lines(position).Length - 1
                        Write(" ")
                        If position < bottomPos Then
                            position += 1
                        End If
                        isInputting = False
                        inputLength = 0
                    Case ConsoleKey.UpArrow
                        CursorLeft = lines(position).Length - 1
                        Write(" ")
                        If position > 0 Then
                            position -= 1
                        End If
                        isInputting = False
                        inputLength = 0
                    Case Else
                        If inputLength < 11
                            isInputting = True
                            If inputLength = 0 AndAlso input(position).Length > 0
                                Write("           ")
                                CursorLeft -= 11
                                input(position) = ""
                            End If
                            inputLength += 1
                            If keypressed <> ConsoleKey.Enter
                                input(position) &= ChrW(keypressed)
                                Write(ChrW(keypressed))
                            End If
                        End If
                End Select
            Loop Until keypressed = ConsoleKey.Enter
            For x = 0 To bottomPos
                errorMessage = input(x).IsValidInput(x)
                If errorMessage.Length > 0 Then Exit For
            Next
            SetCursorPosition(0, bottomPos + topPos + 2)
            WriteLine(errorMessage)
        Loop Until errorMessage.Length = 0
        
        'save settings code
        
        
    End Sub
    
    <Extension>
    Private Function IsValidInput(input As String, position As Byte) As String
        Select Case position
            Case 0
                If NOT IsNumeric(input)
                    Return "Maze width is not a number"
                End If
                If input Mod 1 <> 0
                    Return "Maze width is not an integer"
                End If
                If input Mod 2 <> 1
                    Return "Maze width is not odd"
                End If
                If input < 21 OrElse input > 325
                    Return "Maze width is out of range"
                End If
            Case 1
                If NOT IsNumeric(input)
                    Return "Maze height is not a number"
                End If
                If input Mod 1 <> 0
                    Return "Maze height is not an integer"
                End If
                If input Mod 2 <> 1
                    Return "Maze height is not odd"
                End If
                If input < 21 OrElse input > 943
                    Return "Maze height is out of range"
                End If
            Case 2
                If NOT IsNumeric(input)
                    Return "No. of tries to place a room is not a number"
                End If
                If input Mod 1 <> 0
                    Return "No. of tries to place a room is not an integer"
                End If
                If input < 0 Or input > 32767
                    Return "No. of tries to place a room is out of range"
                End If
            Case 3
                If NOT IsNumeric(input)
                    Return "Extra room connector chance is not a number"
                End If
                If input Mod 1 <> 0
                    Return "Extra room connector chance is not an integer"
                End If
                If input < 0 OrElse input > 100
                    Return "Extra room connector chance is out of range"
                End If
            Case 4
                If NOT IsNumeric(input)
                    Return "Extra room size is not a number"
                End If
                If input Mod 1 <> 0
                    Return "Extra room size is not an integer"
                End If
                If input < 0 OrElse input > 32767
                    Return "Extra room size is out of range"
                End If
            Case 5
                If NOT IsNumeric(input)
                    Return "Path winding chance is not a number"
                End If
                If input Mod 1 <> 0
                    Return "Path winding chance is not an integer"
                End If
                If input < 0 OrElse input > 100
                    Return "Path winding chance is out of range"
                End If
            Case 6
                If NOT IsNumeric(input) OrElse input < 0 OrElse input > 1
                    Return "Instant generation is not 1 or 0"
                End If
        End Select
        Return ""
    End Function
    
    Private Function CalculateFontSize(mazeSize As vec) As Byte
        Dim fontSize As Byte
        fontSize = 17
        Do
            fontSize -= 1
        Loop Until (fontSize * 2 * (mazeSize.y + 2)) < 1070 AndAlso (fontSize * 2 * (mazeSize.x + 2)) < 1910
        Return fontSize
    End Function
    
    Private Function GenerateMaze(mazeSize As vec, roomTryCount As Short, extraConnectorChance As Byte,
                                  roomExtraSize As Short, windingPercent As Byte, showMazeGen As Boolean) As maze
        Randomize()
        WriteLine("press enter to see maze generation")
        ReadLine()
        Dim maze = New maze(mazeSize)
        Dim roomList = New List(Of room)()
        Dim regionAtPos(mazeSize.x - 1, mazeSize.y - 1) As Integer
        Dim currentRegion As Short = -1
        
        DisplayMaze(maze)
        
        'Add rooms
        
        Dim modCount = 1
        Dim sleepTime = 0
        If showMazeGen
            modCount = ((roomTryCount ^ 2) * 30) \ 700 + 1
            sleepTime = 100 \ roomTryCount
        End If
        MsgBox("modCount: " & modCount & " sleepTime: " & sleepTime)
        
        Dim cellCount = 0
        For z = 0 To roomTryCount - 1
            Dim size As Short = GetRnd(1, 3 + roomExtraSize) * 2 + 1
            Dim rectangularity As Integer = GetRnd(0, 1 + size \ 2) * 2
            Dim roomSize = New vec(size, size)

            If (GetRnd(1, 2) = 2) Then
                roomSize.x += rectangularity
            Else
                roomSize.y += rectangularity
            End If

            Dim newRoomPos = New vec(GetRnd(0, (mazeSize.x - roomSize.x) \ 2) * 2,
                                            GetRnd(0, (mazeSize.y - roomSize.y) \ 2) * 2)
            'checks if it overlaps an existing room
            If roomList.Any(Function(r) newRoomPos.x <= r.Pos.x + r.Size.x + 2 AndAlso
                                        newRoomPos.x + roomSize.x + 2 >= r.Pos.x AndAlso
                                        newRoomPos.y <= r.Pos.y + r.Size.y + 2 AndAlso
                                        newRoomPos.y + roomSize.y + 2 >= r.Pos.y) Then Continue For
            Dim newRoom = New room(newRoomPos, roomSize)
            roomList.Add(newRoom)
            'start region
            currentRegion += 1
            'carving
            For x = 0 To newRoom.Size.x - 1
                For y = 0 to newRoom.Size.y - 1
                    Dim pos = New vec(newRoomPos.x + x, newRoomPos.y + y)
                    pos.Carve(currentRegion, regionAtPos, maze, True)
                    cellCount += 1
                    If modCount > 0 AndAlso cellCount Mod modCount = 0
                        Threading.Thread.Sleep(sleepTime)
                    End If
                Next
            Next
        Next
        
        SetCursorPosition(0, 0)
        DisplayMaze(maze)
        ReadLine()
        
        'maze generation
        
        modCount = 1
        sleepTime = 0
        If showMazeGen
            modCount = (mazeSize.x * mazeSize.y * 1.4) \ 1000 + 1
            sleepTime = 900 \ (mazeSize.x + mazeSize.y)
        End If
        MsgBox("modCount: " & modCount & " sleepTime: " & sleepTime)
        
        Dim regionCount = 0
        For y = 0 To mazeSize.y Step 2
            For x = 0 To mazeSize.x Step 2
                Dim pos = New vec(x, y)
                If maze.Cells(x, y) <> 0 Then Continue For
                
                regionCount += 1
                'grow maze
                Dim cells = New List(Of vec)
                Dim lastDir As Integer = -1
                'start region
                currentRegion += 1
                'carve
                pos.Carve(currentRegion, regionAtPos, maze)
                
                cells.Add(pos)
                While cells.Count > 0
                    Dim cell = cells.Last()
                    Dim unmadeCells = New List(Of Integer)
                    For z = 0 To 3
                        Dim cellAdded As vec = cell.AddDirection(z, 2)
                        If cellAdded.y < mazeSize.y AndAlso cellAdded.y > -1 AndAlso 
                           cellAdded.x < mazeSize.x AndAlso cellAdded.x > - 1 AndAlso 
                           maze.Cells(cellAdded.x, cellAdded.y) = 0
                            unmadeCells.Add(z)
                        End If
                    Next
                    If unmadeCells.Count > 0
                        'applying windiness
                        Dim dir As Integer
                        If unmadeCells.Contains(lastDir) AndAlso GetRnd(1, 100) > windingPercent
                            dir = lastDir
                        Else
                            dir = unmadeCells(GetRnd(0, unmadeCells.Count - 1))
                        End If
                        'carving
                        Dim cell1 As vec = cell.AddDirection(dir)
                        Dim cell2 As vec = cell.AddDirection(dir, 2)
                        cell1.Carve(currentRegion, regionAtPos, maze, True)
                        cell2.Carve(currentRegion, regionAtPos, maze, True)
                        cells.Add(cell2)
                        lastDir = dir
                        If modCount > 0 AndAlso Cells.Count Mod modCount = 0
                            Threading.Thread.Sleep(sleepTime)
                        End If
                    Else
                        cells.RemoveAt(cells.Count - 1)
                        lastDir = -1
                    End If
                End While
            Next
        Next
        MsgBox("region count: " & regionCount)
        
        'connect regions

        Dim connectorregions = New Dictionary(Of vec, LinkedList(Of Integer))
        'tuans space ill get it eventually 
        
        ReadLine()
        
        'remove dead ends
        
        modCount = 1
        sleepTime = 0
        If showMazeGen
            modCount = (mazeSize.x * mazeSize.y * 1.4) \ 1000 + 1
            sleepTime = 900 \ (mazeSize.x + mazeSize.y)
        End If
        MsgBox("modCount: " & modCount & " sleepTime: " & sleepTime)
        
        Dim removedCount = 0
        For y = 0 To mazeSize.y - 1
            For x = 0 To mazeSize.x - 1
                If maze.Cells(x, y) = 0 Then Continue For
                Dim pos = New vec(x, y)
                Dim exits = New List(Of Byte)
                While True
                    exits.Clear()
                    For z = 0 To 3
                        Dim addPos = pos.AddDirection(z)
                        If addPos.y < mazeSize.y AndAlso addPos.y > - 1 AndAlso
                           addPos.x < mazeSize.x AndAlso addPos.x > - 1 AndAlso maze.Cells(addPos.x, addPos.y) = 1
                            exits.Add(z)
                        End If
                    Next
                    If exits.Count > 1 Then Exit While
                    maze.Cells(pos.x, pos.y) = 0
                    SetCursorPosition((pos.x + 1) * 2, pos.y + 1)
                    Write(wall)
                    If exits.Count = 0 Then Exit While
                    pos = pos.AddDirection(exits.First())
                    removedCount += 1
                    If modCount > 0 AndAlso removedCount Mod modCount = 0
                        Threading.Thread.Sleep(sleepTime)
                    End If
                End While
            Next
        Next
        
        Return maze
    End Function
    
    <Extension>
    Private Sub Carve(pos As vec, currentRegion As Integer, ByRef regionAtPos(,) As Integer, ByRef maze As maze,
                      Optional displayChanges As Boolean = False)
        If pos.x < maze.Size.x AndAlso pos.x > -1 AndAlso pos.y < maze.Size.y AndAlso pos.y > - 1
            regionAtPos(pos.x, pos.y) = currentRegion
            maze.Cells(pos.x, pos.y) = 1
            If displayChanges
                SetCursorPosition((pos.x + 1) * 2, pos.y + 1)
                Write(floor)
            End If
        End If
    End Sub
    
    <Extension>
    Private Function AddDirection(pos As vec, direction As Integer, Optional amount As Integer = 1) As vec
        Select Case direction
            Case 0
                Return New vec(pos.x, pos.y - amount)
            Case 1
                Return New vec(pos.x + amount, pos.y)
            Case 2
                Return New vec(pos.x, pos.y + amount)
            Case 3
                Return New vec(pos.x - amount, pos.y)
        End Select
        WriteLine("Function add direction has received an invalid direction")
        Return pos
    End Function

    Private Sub DisplayMaze(maze As maze)
        Dim line = New StringBuilder
        'For x = 0 To maze.Size.x + 1
            'Write(wall)
        'Next
        line.Append("█", (maze.Size.x + 2) * wallSize)
        'WriteLine("")
        line.AppendLine()
        For y = 0 To maze.Size.y - 1
            'Write(wall)
            line.Append(wall)
            For x = 0 To maze.Size.x - 1
                'Write(maze.Cells(x, y).ToChar())
                line.Append(maze.Cells(x, y).ToChar())
            Next
            'WriteLine(wall)
            line.AppendLine(wall)
        Next
        'For x = 0 To maze.Size.x + 1
            'Write(wall)
        'Next
        line.Append("█", (maze.Size.x + 2) * wallSize)
        'WriteLine("")
        line.AppendLine()
        
        WriteLine(line)
    End Sub

    <Extension>
    Private Function ToChar(num As Integer) As String
        Select Case num
            Case 0
                Return wall
            Case 1
                Return floor
        End Select
        Return ""
    End Function

    Private Structure maze
        Dim Size As vec
        Dim Cells(,) As Integer '0 is wall 1 is floor
        Sub New(mazeSize As vec)
            ReDim Cells(mazeSize.x - 1, mazeSize.y - 1)
            Size = mazeSize
        End Sub
    End Structure
    
    Private Structure room
        Dim Pos As vec
        Dim Size As vec

        Sub New(position As vec, roomSize As vec)
            Pos = position
            Size = roomSize
        End Sub
    End Structure

    Private Structure vec
        Dim x, y As Integer
        Sub New(xPos As Integer, yPos As Integer)
            x = xPos
            y = yPos
        End Sub
    End Structure

    Private Function GetRnd(min As Integer, max As Integer) 'its inclusive on both ends
        Return CInt(Math.Floor((max - min + 1) * Rnd())) + min
    End Function
    
    'stuff for changing font size
    
    Private Const STD_OUTPUT_HANDLE = -11
    Private Sub SetupConsole(fontWeight As Short, fontSize As Short, Optional fontName As String = "Raster Fonts")
        Dim hHandle As IntPtr = GetStdHandle(CType(STD_OUTPUT_HANDLE, IntPtr))
        If (hHandle <> CType(- 1, IntPtr)) Then
            Dim fontInfoex = New CONSOLE_FONT_INFOEX()
            fontInfoex.cbSize = CUInt(Marshal.SizeOf(fontInfoex))
            GetCurrentConsoleFontEx(hHandle, False, fontInfoex)
            fontInfoex.FontWeight = fontWeight
            fontInfoex.FaceName = fontName
            fontInfoex.dwFontSize = New Coord(fontSize, fontSize)
            SetCurrentConsoleFontEx(hHandle, False, fontInfoex)
        End If
        
        SetWindowSize(LargestWindowWidth, LargestWindowHeight)
        SetBufferSize(WindowWidth, WindowHeight)
        'MsgBox("window width: " & WindowWidth & " window height: " & WindowHeight)
    End Sub
    
    <DllImport("Kernel32.dll", SetLastError := True)>
    Private Function SetCurrentConsoleFontEx(hConsoleOutput As IntPtr, bMaximumWindow As Boolean,
                                            ByRef lpConsoleCurrentFontEx As CONSOLE_FONT_INFOEX) As Boolean
    End Function

    <DllImport("Kernel32.dll", SetLastError := True)>
    Private Function GetCurrentConsoleFontEx(hConsoleOutput As IntPtr, bMaximumWindow As Boolean,
                                            ByRef lpConsoleCurrentFontEx As CONSOLE_FONT_INFOEX) As Boolean
    End Function

    Private Const LfFacesize = 32

    <StructLayout(LayoutKind.Sequential, CharSet := CharSet.Unicode)>
    Private Structure CONSOLE_FONT_INFOEX
        Public cbSize As UInteger
        Private ReadOnly nFont As Integer
        Public dwFontSize As Coord
        Private ReadOnly FontFamily As UInteger
        Public FontWeight As UInteger
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst := LfFacesize)> Public FaceName As String
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure Coord
        Private ReadOnly X As Short
        Private ReadOnly Y As Short

        Public Sub New(x As Short, y As Short)
            Me.X = x
            Me.Y = y
        End Sub
    End Structure

    <DllImport("Kernel32.dll", SetLastError := True)>
    Private Function GetStdHandle(nStdHandle As IntPtr) As IntPtr
    End Function
    
    'stuff to maximise window at runtime
    
    Private Sub MaximiseConsole()
        ShowWindow(GetConsoleWindow(), SW_SHOWMAXIMIZED)
        
        SetBufferSize(WindowWidth, WindowHeight)
    End Sub
    
    <DllImport("kernel32.dll", ExactSpelling := True)>
    Private Function GetConsoleWindow() As IntPtr
    End Function

    Private Const SW_SHOWMAXIMIZED As Integer = 3

    <DllImport("user32.dll", CharSet := CharSet.Auto, SetLastError := True)>
    Private Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

End Module