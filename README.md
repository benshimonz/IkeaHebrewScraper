# IkeaHebrewScraper
Scraper for the Ikea Hebrew Catalog 2017

 - c# console application
 - <=.net 4.5
 - Supports Hebrew encoding


How to get images to appear in Excel given image URL
----------------------------------------------------

    Dim url_column As Range
    Dim image_column As Range
    
    Set url_column = Worksheets(1).UsedRange.Columns("A")
    Set image_column = Worksheets(1).UsedRange.Columns("B")
    
    Dim i As Long
    For i = 1 To url_column.Cells.Count
    
      With image_column.Worksheet.Pictures.Insert(url_column.Cells(i).Value)
        .Left = image_column.Cells(i).Left
        .Top = image_column.Cells(i).Top
        image_column.Cells(i).EntireRow.RowHeight = .Height
      End With
    
    Next
