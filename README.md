# IkeaHebrewScraper
Scraper for the Ikea Hebrew Catalog 2017

 - c# console application
 - <=.net 4.5
 - Supports Hebrew encoding


Steps
-----

 1. Use web scraping tool to get all the Ikea categories URL (e.g., http://webscraper.io/) - see the `ikeaurls.xlsx`	 in `ik` folder.
 2. Use *wget* tool to download all the webpages - see the zip file `ikea_categories_heb_htmls.zip`	in the `ik` folder.
 3. Use this code to build the catalog

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
