Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Media.Imaging
Imports Spire.Doc
Imports Spire.Pdf

Public Class DocumentToImageHelper
    Public Shared Async Function ConvertFirstPageToImageAsync(filePath As String) As Task(Of BitmapSource)
        Return Await Task.Run(Function()
                                  Dim extension = Path.GetExtension(filePath).ToLower()
                                  Select Case extension
                                      Case ".pdf"
                                          Return ConvertPdfToImage(filePath)
                                      Case ".doc", ".docx"
                                          Return ConvertWordToImage(filePath)
                                      Case ".txt", ".xml", ".json", ".csv", ".log", ".vb", ".cs"
                                          Dim text As String = File.ReadAllText(filePath)
                                          Return ConvertTextToImage(text)
                                      Case Else
                                          Return Nothing
                                  End Select
                              End Function).ConfigureAwait(True)
    End Function

    Private Shared Function ConvertPdfToImage(filePath As String) As BitmapSource
        Dim pdf As New PdfDocument()
        pdf.LoadFromFile(filePath)
        ' Convert only the first page (page index 0) to an image.
        Dim img As System.Drawing.Image = pdf.SaveAsImage(0)
        Return ConvertBitmapToBitmapSource(img)
    End Function

    Private Shared Function ConvertWordToImage(filePath As String) As BitmapSource
        Dim document As New Document()
        document.LoadFromFile(filePath)
        ' SaveToImages returns an array of System.Drawing.Image.
        Dim images() As System.Drawing.Image = document.SaveToImages(Spire.Doc.Documents.ImageType.Bitmap)
        Dim firstImage As System.Drawing.Image = images(0)
        Return ConvertBitmapToBitmapSource(New System.Drawing.Bitmap(firstImage))
    End Function

    ' New method to convert text content to an image.
    Public Shared Function ConvertTextToImage(text As String) As BitmapSource
        Dim font As New Font("Arial", 12)
        Dim padding As Integer = 10
        Dim bmpSize As SizeF

        ' Measure the text size using a temporary graphics context.
        Using g As Graphics = Graphics.FromHwnd(IntPtr.Zero)
            bmpSize = g.MeasureString(text, font)
        End Using

        Dim width As Integer = CInt(bmpSize.Width) + padding * 2
        Dim height As Integer = CInt(bmpSize.Height) + padding * 2
        Dim bmp As New Bitmap(width, height)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.White)
            g.DrawString(text, font, Brushes.Black, New RectangleF(padding, padding, bmp.Width - padding * 2, bmp.Height - padding * 2))
        End Using

        Return ConvertBitmapToBitmapSource(bmp)
    End Function

    Private Shared Function ConvertBitmapToBitmapSource(bitmap As System.Drawing.Bitmap) As BitmapSource
        Dim hBitmap = bitmap.GetHbitmap()
        Dim bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            hBitmap,
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions())
        bitmapSource.Freeze() ' Freeze for cross-thread use
        Return bitmapSource
    End Function
End Class
