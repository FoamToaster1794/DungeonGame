Imports System.Console
Imports System.IO
Imports System.Convert

Module Module1
    Sub Main()
        Dim maze As maze = LoadGrid("blank.txt")
        If maze.Size.x > 0 Then
            DisplayGrid(maze)
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
        Dim grid = New maze(tempWidth, tempHeight)
        For y = 0 To grid.Size.y - 1
            For x = 0 To lines(y + 2).Length - 1
                Dim character As Char = lines(y + 2)(x)
                If Not Char.IsNumber(character) OrElse ToInt16(character.ToString()) > 1 Then
                    WriteLine("Error has occured when loading grid")
                    WriteLine("Grid cell at position (" & x & ", " & y & ") is not valid")
                    Return New maze()
                End If
                grid.Cells(x, y) = ToInt16(character.ToString())
            Next
        Next
        Return grid
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
    Private Function GenerateMaze(mazeSize As vec, noOfRoomTries As Integer, extraConnectorChance As Integer, roomExtraSize As Integer)
        Dim maze = New maze(mazeSize)
        Dim roomList = New List(Of room)()
        Dim regionAtPos(mazeSize.x, mazeSize.y) As Integer
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
            Dim newRoom = New room(newRoomPos, roomSize)
            'checks if it overlaps an existing room
            If roomList.Any(Function(r) r.Pos.x <= newRoomPos.x + roomSize.x AndAlso
                                        r.Pos.y <= newRoomPos.y + roomSize.y) Then Continue For
            roomList.Add(newRoom)
            'start region
            currentRegion += 1
            'carving
            For x = 0 To newRoom.Size.x
                For y = 0 to newRoom.Size.y
                    regionAtPos(x, y) = currentRegion
                Next
            Next
        Next

        Return maze
    End Function

    Private Sub DisplayGrid(maze As maze)
        For x = 0 To maze.Size.x + 1
            Write("██")
        Next
        WriteLine("")
        For y = 0 To maze.Size.y - 1
            Write("██")
            For x = 0 To maze.Size.x - 1
                Write(ToChar(maze.Cells(x, y)))
            Next
            WriteLine("██")
        Next
        For x = 0 To maze.Size.x + 1
            Write("██")
        Next
        WriteLine("")
    End Sub

    Private Function ToChar(num As Integer) As String
        Select Case num
            Case 0
                Return "  "
            Case 1
                Return "██"
        End Select
        Return ""
    End Function

    Private Structure maze
        Dim Size As vec
        Dim Cells(,) As Integer
        Sub New(mazeSize As vec)
            ReDim Cells(mazeSize.x, mazeSize.y)
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
        Randomize()
        Return CInt(Math.Floor((max - min + 1) * Rnd())) + min
    End Function
End Module
