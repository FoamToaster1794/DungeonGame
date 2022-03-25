Imports System.Console
Imports System.IO
Imports System.Convert

Module Module1
    Sub Main()
        Dim grid as grid = LoadGrid("blank.txt")
        If grid.Height > 0
            DisplayGrid(grid)
        End If
        ReadLine()
    End Sub
    
    Private Sub MainMenu()
        Clear()
        WriteLine("")
        
    End Sub

    Private Function LoadGrid(fileName As String) As grid
        Dim lines() As String = File.ReadAllLines(fileName)
        Dim input As String = lines(0).Remove(0, 8)
        Dim tempHeight, tempWidth As Integer
        If IsNumeric(input) AndAlso input > 0 AndAlso input Mod 1 = 0
            tempHeight = input
        Else
            WriteLine("Error has occured when loading grid")
            WriteLine("Height is not valid")
            Return New grid()
        End If
        input = lines(1).Remove(0, 7)
        If IsNumeric(input) AndAlso input > 0 AndAlso input Mod 1 = 0
            tempWidth = input
        Else
            WriteLine("Error has occured when loading grid")
            WriteLine("Width is not valid")
            Return New grid()
        End If
        Dim grid = New grid(tempWidth, tempHeight)
        For y = 0 To grid.Height - 1
            For x = 0 To lines(y + 2).Length - 1
                Dim character As Char = lines(y + 2)(x)
                If Not Char.IsNumber(character) OrElse ToInt16(character.ToString()) > 1
                    WriteLine("Error has occured when loading grid")
                    WriteLine("Grid cell at position (" & x & ", " & y & ") is not valid")
                    Return New grid()
                End If
                grid.Cells(x, y) = ToInt16(character.ToString())
            Next
        Next
        Return grid
    End Function
    
    Private Sub SaveGrid(grid As grid, fileName As String)
        FileOpen(0, fileName, OpenMode.Output)
        FileSystem.WriteLine(0, "Height: " & grid.Height)
        FileSystem.WriteLine(0, "Width: " & grid.Width)
        For y = 0 To grid.Height
            For x = 0 To grid.Width
                FileSystem.Write(0, grid.Cells(x, y))
            Next
            FileSystem.WriteLine(0, "")
        Next
        FileClose(0)
    End Sub
    
    Private Structure room
        Dim pos AS vec
        Dim size As vec
        Sub New(position As vec, roomSize As vec)
            pos = position
            size = roomSize
        End Sub
    End Structure
    Private Function GenerateMaze(mazeSize As vec, noOfRoomTries, extraConnectorChance, roomExtraSize As Integer)
        Dim grid = New grid(mazeSize.x, mazeSize.y)
        Dim roomList = New List(Of room)()
        Dim regionAtPos(mazeSize.x, mazeSize.y) As Integer
        Dim currentRegion As Integer = -1
        
        'Add rooms
        For z = 0 To noOfRoomTries - 1
            Dim size As Integer = GetRnd(1, 3 + roomExtraSize) * 2 + 1
            Dim rectangularity As Integer = GetRnd(0, 1 + size / 2) * 2
            Dim roomSize As vec = New vec(size, size)
            
            If(GetRnd(1, 2) = 2)
                roomSize.x += rectangularity
            Else
                roomSize.y += rectangularity
            End If
            
            Dim newRoomPos As vec = New vec(GetRnd(0, ((mazeSize.x - roomSize.x)/2)*2 + 1),
                                            GetRnd(0, ((mazeSize.y - roomSize.y)/2)*2 + 1))
            Dim newRoom As room = New room(newRoomPos, roomSize)
            'checks if it overlaps an existing room
            If roomList.Any(Function(r) r.pos.x <= newRoomPos.x + roomSize.x AndAlso
                                        r.pos.y <= newRoomPos.y + roomSize.y) Then Continue For
            roomList.Add(newRoom)
            'start region
            currentRegion += 1
            
        Next
        
        Return grid
    End Function
    
    Private Sub DisplayGrid(grid As grid)
        For x = 0 To grid.Width + 1
            Write("██")
        Next
        WriteLine("")
        For y = 0 To grid.Height - 1
            Write("██")
            For x = 0 To grid.Width - 1
                Write(ToChar(grid.Cells(x, y)))
            Next
            WriteLine("██")
        Next
        For x = 0 To grid.Width + 1
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
    
    Private Structure grid
        Dim Width As Integer
        Dim Height As Integer
        Dim Cells(,) As Integer
        Sub New(x As Integer, y As Integer)
            ReDim Cells(x, y)
            Width = x
            Height = y
        End Sub
    End Structure
    
    Private Structure vec
        Dim x, y As Integer
        Sub New(xPos, yPos As Integer)
            x = xPos
            y = yPos
        End Sub
    End Structure
    
    Private Function GetRnd(min, max) 'its inclusive on both ends
        Randomize()
        Return CInt(Math.Floor((max - min + 1)*Rnd())) + min
    End Function
End Module
