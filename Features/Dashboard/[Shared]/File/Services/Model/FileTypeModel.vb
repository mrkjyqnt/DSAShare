Public Class FileTypeModel
    Public Property Extension As String
    Public Property Category As String

    Public Shared ReadOnly Property FileTypes As List(Of FileTypeModel)
        Get
            Return New List(Of FileTypeModel) From {
                New FileTypeModel With {.Extension = ".pdf", .Category = "DOCS"},
                New FileTypeModel With {.Extension = ".docx", .Category = "DOCS"},
                New FileTypeModel With {.Extension = ".xlsx", .Category = "DOCS"},
                New FileTypeModel With {.Extension = ".pptx", .Category = "DOCS"},
                New FileTypeModel With {.Extension = ".txt", .Category = "TEXT"},
                New FileTypeModel With {.Extension = ".html", .Category = "TEXT"},
                New FileTypeModel With {.Extension = ".png", .Category = "IMAGE"},
                New FileTypeModel With {.Extension = ".jpeg", .Category = "IMAGE"},
                New FileTypeModel With {.Extension = ".jpg", .Category = "IMAGE"},
                New FileTypeModel With {.Extension = ".gif", .Category = "IMAGE"},
                New FileTypeModel With {.Extension = ".bmp", .Category = "IMAGE"},
                New FileTypeModel With {.Extension = ".mp3", .Category = "MEDIA"},
                New FileTypeModel With {.Extension = ".mp4", .Category = "MEDIA"},
                New FileTypeModel With {.Extension = ".avi", .Category = "MEDIA"},
                New FileTypeModel With {.Extension = ".mov", .Category = "MEDIA"},
                New FileTypeModel With {.Extension = ".zip", .Category = "COMPRESSED"},
                New FileTypeModel With {.Extension = ".rar", .Category = "COMPRESSED"},
                New FileTypeModel With {.Extension = ".exe", .Category = "EXE"}
            }
        End Get
    End Property
    Public Shared Function GetCategoryByExtension(extension As String) As String
        If String.IsNullOrWhiteSpace(extension) Then Return "UNKNOWN"

        ' Trim and clean the extension
        Dim cleanExtension = extension.Trim().ToLower()

        ' Handle cases with multiple dots
        cleanExtension = If(cleanExtension.StartsWith("."), cleanExtension, "." & cleanExtension)

        Dim fileType = FileTypes.FirstOrDefault(Function(f)
                                                    Return f.Extension.Trim().ToLower() = cleanExtension
                                                End Function)

        Return If(fileType?.Category, "UNKNOWN")
    End Function
End Class
