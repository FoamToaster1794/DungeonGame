Imports System.Console
Imports System.IO
Imports System.Convert
Imports System.Runtime.CompilerServices

Module Module1
    Sub Main()
        Dim maze As maze = LoadGrid("blank.txt")
        Randomize()
        Dim generatedMaze As maze = GenerateMaze(New vec(33, 33), 50, 10, 0, 90)
        DisplayMaze(generatedMaze)
        WriteLine()
        ReadLine()
        If maze.Size.x > 0 Then
            DisplayMaze(maze)
        End If
        ReadLine()
    End Sub

    Private Sub MainMenu()
        Clear()
        WriteLine("")

    End Sub

    Private Function LoadGrid(fileName As String) As maze
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

    Private Sub SaveGrid(maze As maze, fileName As String)
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

    Private Structure room
        Dim Pos As vec
        Dim Size As vec
        Sub New(position As vec, roomSize As vec)
            Pos = position
            Size = roomSize
        End Sub
    End Structure
    Private Function GenerateMaze(mazeSize As vec, noOfRoomTries As Integer, extraConnectorChance As Integer, roomExtraSize As Integer, windingPercent As Integer)
        Randomize()
        Dim maze = New maze(mazeSize)
        Dim roomList = New List(Of room)()
        Dim regionAtPos(mazeSize.x - 1, mazeSize.y - 1) As Integer
        Dim currentRegion As Integer = -1

        'Add rooms
        For z = 0 To noOfRoomTries - 1
            Dim size As Integer = GetRnd(1, 3 + roomExtraSize) * 2 + 1
            Dim rectangularity As Integer = GetRnd(0, 1 + size / 2) * 2
            Dim roomSize = New vec(size, size)

            If (GetRnd(1, 2) = 2) Then
                roomSize.x += rectangularity
            Else
                roomSize.y += rectangularity
            End If

            Dim newRoomPos = New vec(GetRnd(0, ((mazeSize.x - roomSize.x) / 2) * 2 + 1),
                                            GetRnd(0, ((mazeSize.y - roomSize.y) / 2) * 2 + 1))
            'checks if it overlaps an existing room
            If roomList.Any(Function(r) newRoomPos.x <= r.Pos.x + r.Size.x AndAlso
                                        newRoomPos.x + roomSize.x >= r.Pos.x AndAlso
                                        newRoomPos.y <= r.Pos.y + r.Size.y AndAlso
                                        newRoomPos.y + roomSize.y >= r.Pos.y) Then Continue For
            Dim newRoom = New room(newRoomPos, roomSize)
            roomList.Add(newRoom)
            'start region
            currentRegion += 1
            'carving
            For x = 0 To newRoom.Size.x - 1
                For y = 0 to newRoom.Size.y - 1
                    Dim pos = New vec(newRoomPos.x + x, newRoomPos.y + y)
                    pos.Carve(currentRegion, regionAtPos, maze)
                Next
            Next
        Next
        DisplayMaze(maze)
        WriteLine("")
        
        'maze generation
        For y = 1 To mazeSize.y - 1 Step 2
            For x = 1 To mazeSize.x - 1 Step 2
                Dim pos = New vec(x, y)
                If maze.Cells(x, y) <> 0 Then Continue For
                
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
                    Dim unmadeCells() As Integer = 'expression to check if cell can be carved
                            Enumerable.Range(0, 4).Where(Function(dir) _ 
                                cell.AddDirection(dir).x < mazeSize.x AndAlso cell.AddDirection(dir).x > - 1 AndAlso
                                cell.AddDirection(dir).y < mazeSize.y AndAlso cell.AddDirection(dir).y > - 1 AndAlso
                                maze.Cells(cell.AddDirection(dir).x, cell.AddDirection(dir).y) = 0).ToArray()
                    If unmadeCells.Length > 0
                        'applying windiness
                        Dim dir As Integer
                        If unmadeCells.Contains(lastDir) AndAlso GetRnd(1, 100) > windingPercent
                            dir = lastDir
                        Else
                            dir = unmadeCells(GetRnd(0, unmadeCells.Length - 1))
                        End If
                        'carving
                        cell.AddDirection(dir).Carve(currentRegion, regionAtPos, maze)
                        cell.AddDirection(dir, 2).Carve(currentRegion, regionAtPos, maze)
                        cells.Add(cell.AddDirection(dir, 2))
                        lastDir = dir 
                    Else 
                        cells.RemoveAt(cells.Count - 1)
                        lastDir = -1
                    End If
                End While
            Next
        Next
        
        'connect regions

        Return maze
    End Function
    
    <Extension>
    Private Sub Carve(pos As vec, currentRegion As Integer, ByRef regionAtPos(,) As Integer, ByRef maze As maze)
        If pos.x < maze.Size.x AndAlso pos.x > -1 AndAlso pos.y < maze.Size.y AndAlso pos.y > - 1
            regionAtPos(pos.x, pos.y) = currentRegion
            maze.Cells(pos.x, pos.y) = 1
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
        For x = 0 To maze.Size.x + 1
            Write("██")
        Next
        WriteLine("")
        For y = 0 To maze.Size.y - 1
            Write("██")
            For x = 0 To maze.Size.x - 1
                Write(maze.Cells(x, y).ToChar())
            Next
            WriteLine("██")
        Next
        For x = 0 To maze.Size.x + 1
            Write("██")
        Next
        WriteLine("")
    End Sub

    <Extension>
    Private Function ToChar(num As Integer) As String
        Select Case num
            Case 0
                Return "██"
            Case 1
                Return "  "
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

    Private Structure vec
        Dim x, y As Integer
        Sub New(xPos As Integer, yPos As Integer)
            x = xPos
            y = yPos
        End Sub
    End Structure

    Private Function GetRnd(min, max) 'its inclusive on both ends
        Return CInt(Math.Floor((max - min + 1) * Rnd())) + min
    End Function
End Module
