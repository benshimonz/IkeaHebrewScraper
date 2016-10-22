using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IkeaIL
{
    class Program
    {
        private static Encoding encode = System.Text.Encoding.GetEncoding(1255); // Hebrew (Windows) 

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Directory path of the category files is required as argument 1");
                Environment.Exit(-1);
            }
            string htmlFolderPath = args[0];
            List<IkeaItem> ikeaItemsList = new List<IkeaItem>();

            try
            {
                string[] filePaths = Directory.GetFiles(htmlFolderPath, "*.html");
                int catFilesCount = filePaths.Length;

                for (int i = 0; i < catFilesCount; i++)
                {
                    string filePath = filePaths[i];
                    Console.WriteLine(string.Format("Working on category file ({1} of {2} category files):\n{0}\n###############################", filePath, i+1, catFilesCount));

                    if (!File.Exists(filePath))
                        continue;
                    HtmlDocument doc = new HtmlDocument();
                    
                    doc.Load(filePath);

                    HtmlNodeCollection linkNodes  = doc.DocumentNode.SelectNodes("//*[@class=\"ButtonsText\"]");

                    if (linkNodes == null || linkNodes.Count == 0)
                    {
                        Console.WriteLine(string.Format("*Zero Links on category file :\n{0}\n", filePath));

                        continue;
                    }

                    Parallel.ForEach(linkNodes, link =>
                    {
                    //    foreach (HtmlNode link in linkNodes)
                    //{
                        HtmlAttribute att = link.Attributes["href"];
                        if (att != null)
                        {
                            string newUrlProd = WebUtility.HtmlDecode("http://www.ikea.co.il" + att.Value);
                            Console.WriteLine(string.Format("Working on product page:\n{0}\n", newUrlProd));
                            string webPageString = ReadWebPageAsString(newUrlProd);

                            HtmlDocument docProd = new HtmlDocument();
                            docProd.LoadHtml(webPageString);
                            IkeaItem ikeaItem = CreateIkeaItem(newUrlProd, docProd);
                            ikeaItemsList.Add(ikeaItem);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            WriteResultsToCsv(ikeaItemsList);
        }

        private static IkeaItem CreateIkeaItem(string newUrlProd, HtmlDocument docProd)
        {
            IkeaItem ikeaItem = new IkeaItem();
            ikeaItem.ProdName = FindProductName(docProd);
            ikeaItem.ProductSKU = FindProductSKU(docProd);
            ikeaItem.ProductTopCat = FindProductTopCategory(docProd);
            ikeaItem.ProductSubCat = FindProductSubCategory(docProd);
            ikeaItem.ProductPrice = FindProductPrice(docProd);
            ikeaItem.ProductDesc = FindProductDesc(docProd);
            ikeaItem.ProductGeneralDesc = FindProductGeneralDesc(docProd);
            ikeaItem.ProductExtraDetails = FindProductExtraDetails(docProd);
            ikeaItem.ProductDesign = FindProductDesignDesc(docProd);
            ikeaItem.ProductCare = FindProductCare(docProd);
            ikeaItem.ProductMaterials = FindProductMaterials(docProd);
            ikeaItem.ProductManufacturingCountry = FindProductManufacturingCountry(docProd);
            ikeaItem.ProductUrl = newUrlProd;
            ikeaItem.ProductImageUrl = FindProductImageUrl(docProd);
            return ikeaItem;
        }

        private static void WriteResultsToCsv(List<IkeaItem> ikeaItemsList)
        {
            using (TextWriter textWriter = new StreamWriter("ikeaItems.csv", false, encode))
            {
                var csv = new CsvWriter(textWriter);
                csv.Configuration.Encoding = encode;
                csv.WriteRecords(ikeaItemsList);
            }
        }

        private static string FindProductExtraDetails(HtmlDocument docProd)
        {
            string extraDetails = "";
            try
            {
                HtmlNode prodExtraDetailsNode = docProd.DocumentNode.SelectNodes("//div[@class=\"ProductTextNormal\"]")[2];
                extraDetails = prodExtraDetailsNode.InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(extraDetails);
        }

        private static string FindProductCare(HtmlDocument docProd)
        {
            string prodCare = "";
            try
            {
                HtmlNode prodCareNode = docProd.DocumentNode.SelectNodes("//div[@class=\"ProductTextNormal\"]")[3];
                prodCare = prodCareNode.InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(prodCare);
        }

        private static string FindProductMaterials(HtmlDocument docProd)
        {
            string materials = "";
            try
            {
                //HtmlNode prodMaterialsNode = docProd.DocumentNode.SelectSingleNode("//h6[text()='חומרים']");

                HtmlNode prodMaterialsNode = docProd.DocumentNode.SelectNodes("//div[@class=\"ProductTextNormal\"]")[4];
                materials = prodMaterialsNode.InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(materials);
        }

        private static string FindProductManufacturingCountry(HtmlDocument docProd)
        {
            string ManufacturingCountry = "";
            try
            {
                HtmlNode prodManufacturingCountryNode = docProd.DocumentNode.SelectNodes("//div[@class=\"ProductTextNormal\"]")[5];
                ManufacturingCountry = prodManufacturingCountryNode.InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(ManufacturingCountry);
        }        

        private static string FindProductSubCategory(HtmlDocument docProd)
        {
            string subCat = "";
            try
            {
                HtmlNode prodSubCatNode = docProd.DocumentNode.SelectNodes("//a[@class=\"NavigationBarStyle\"]").Last<HtmlNode>();
                subCat = prodSubCatNode.InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(subCat);
        }

        private static string FindProductImageUrl(HtmlDocument docProd)
        {
            string prodImg = "";
            try
            {
                HtmlNode prodImgNode = docProd.DocumentNode.SelectSingleNode("//*[@class=\"main_image\"]");
                prodImg = WebUtility.HtmlDecode("http://www.ikea.co.il" + prodImgNode.Attributes["src"].Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(prodImg);
        }

        private static string FindProductTopCategory(HtmlDocument docProd)
        {
            string topCat = "";
            try
            {
                HtmlNode prodTopCatNode = docProd.DocumentNode.SelectSingleNode("//*[@class=\"sideMenuTitlelink\"]");
                topCat = prodTopCatNode.InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(topCat);
            
        }

        private static string FindProductGeneralDesc(HtmlDocument docProd)
        {
            string prodGeneralDesc = "";
            try
            {
                HtmlNode prodGeneralDescNode = docProd.DocumentNode.SelectSingleNode("//*[@class=\"ProductTextNormal\"]");
                prodGeneralDesc = prodGeneralDescNode.ChildNodes[1].InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(prodGeneralDesc);
        }

        private static string FindProductDesignDesc(HtmlDocument docProd)
        {
            string prodDesignDesc = "";
            try
            {
                HtmlNode prodDesignDescNode = docProd.DocumentNode.SelectSingleNode("//*[@class=\"ProductTextNormal\"]");
                prodDesignDesc = prodDesignDescNode.ChildNodes[3].InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return prodDesignDesc;
        }

        private static string FindProductPrice(HtmlDocument docProd)
        {
            string prodPrice = "";
            try
            {
                HtmlNode prodPriceNode = docProd.DocumentNode.SelectSingleNode("//*[@class=\"ProductSalePrice2\"]");
                prodPrice = RemoveNonNumericChars(prodPriceNode.InnerText);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(prodPrice);
        }

        private static string ReadWebPageAsString(string newUrlProd)
        {
            try
            {
                // WebClient is still convenient
                // Assume UTF8, but detect BOM - could also honor response charset I suppose
                using (var client = new WebClient())
                using (var stream = client.OpenRead(newUrlProd))
                using (var textReader = new StreamReader(stream, encode, true))
                {
                    return textReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return "";
        }

        private static string FindProductDesc(HtmlDocument docProd)
        {
            string prodDesc = "";
            try
            {
                HtmlNode prodDescNode = docProd.DocumentNode.SelectSingleNode("//*[@class=\"ProductTextBig\"]");
                prodDesc = prodDescNode.InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(prodDesc);
        }

        private static string FindProductSKU(HtmlDocument docProd)
        {
            string SKU = "";
            try
            {
                HtmlNode prodSKUNode = docProd.DocumentNode.SelectSingleNode("//*[@class=\"ProductTextNormal1\"]");
                SKU = CleanValue(prodSKUNode.InnerText).Split(':')[1];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return CleanValue(SKU);
        }

        private static string FindProductName(HtmlDocument docProd)
        {
            string prodName = "";
            try
            {
                HtmlNode prodNameNode = docProd.DocumentNode.SelectSingleNode("//*[@class=\"ProductTitle2\"]");
                prodName = CleanValue(prodNameNode.InnerText);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return prodName;
        }

        private static string CleanValue(string innerText)
        {
            string newCleanedValue = WebUtility.HtmlDecode(innerText).Trim();
            newCleanedValue = Regex.Replace(newCleanedValue, @"\r\n?|\n", " ");
            return newCleanedValue;
        }

        private static string RemoveNonNumericChars(string price)
        {
              Regex digitsOnly = new Regex(@"[^\d]");
              return digitsOnly.Replace(price, "");
        }
    }
}
