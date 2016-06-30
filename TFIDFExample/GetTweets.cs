using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Microsoft.CSharp;




namespace TFIDFExample
{
    public class GetTweets
    {

        public string[] GetTweetsFromFile()
        {
            List<String> Tweet = new List<string>();
            StreamReader reader = new StreamReader(File.OpenRead(@"C:\Users\jasim\Documents\Visual Studio 2015\Projects\TweetClustering\TweetClustering\TweetClustering.csv"));

            while (!reader.EndOfStream)
            {
                String line = reader.ReadLine();
                String[] columns = line.Split(',');
                // process strings
                Tweet.Add(columns[6]);
            }

            return Tweet.ToArray();
        }

        public string[] GetTweetsFromExcelFile(string filename)
        {
            //string file = @"C:\Users\jasim\Documents\Visual Studio 2015\Projects\TFIDF_TwitterClustering\TweetClustering.xlsx";
            Excel.Application xlsApp = new Excel.Application();
            Excel.Workbook workbook;
            Excel.Worksheet worksheet;
            Excel.Range range;
            List<string> Tweets = new List<string>();
            


            xlsApp.Visible = false;

            //workbook = xlsApp.Workbooks.Open(@"C:\Users\jasim\Documents\Visual Studio 2015\Projects\TFIDF_TwitterClustering\TweetClustering.xlsx", 
            //    0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            //workbook = xlsApp.Workbooks.Add();
            
            //workbook = xlsApp.Workbooks.Open(Path.GetFullPath("TweetClustering.xlsx"));
            //workbook = xlsApp.Workbooks.Open(Path.GetFullPath("All_Tweets.xlsx"));
            workbook = xlsApp.Workbooks.Open(Path.GetFullPath(filename));

            worksheet = (Excel.Worksheet) workbook.Sheets[1];

            //range =  worksheet.Columns[6];
            range = (Excel.Range) worksheet.UsedRange;

            //web
            object[,] valueArray = (object[,]) range.get_Value(
                        Excel.XlRangeValueDataType.xlRangeValueDefault);

            // iterate through each cell and display the contents.
            for (int row = 2; row <= worksheet.UsedRange.Rows.Count; row++)
            {
                //for (int col = 1; col <= worksheet.UsedRange.Columns.Count; ++col)
                {
                    // Print value of the cell to Console.
                    
                    //Console.WriteLine((string) valueArray[row, 7].ToString());
                    Tweets.Add((string)valueArray[row, 1].ToString());
                }
            }

            //web

            workbook.Close();
            xlsApp.Quit();
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(xlsApp);
            Marshal.FinalReleaseComObject(xlsApp);
            xlsApp = null;

            return Tweets.ToArray();
        }

        public double[][] GetLabelsFromExcelFile(string filename)
        {
            //string file = @"C:\Users\jasim\Documents\Visual Studio 2015\Projects\TFIDF_TwitterClustering\TweetClustering.xlsx";
            Excel.Application xlsApp = new Excel.Application();
            Excel.Workbook workbook;
            Excel.Worksheet worksheet;
            Excel.Range range;
            List<string> Tweets = new List<string>();



            xlsApp.Visible = false;

            //workbook = xlsApp.Workbooks.Open(@"C:\Users\jasim\Documents\Visual Studio 2015\Projects\TFIDF_TwitterClustering\TweetClustering.xlsx", 
            //    0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            //workbook = xlsApp.Workbooks.Add();

            //workbook = xlsApp.Workbooks.Open(Path.GetFullPath("TweetClustering.xlsx"));
            //workbook = xlsApp.Workbooks.Open(Path.GetFullPath("All_Tweets.xlsx"));
            workbook = xlsApp.Workbooks.Open(Path.GetFullPath(filename));

            worksheet = (Excel.Worksheet)workbook.Sheets[1];

            //range =  worksheet.Columns[6];
            range = (Excel.Range)worksheet.UsedRange;

            //web
            object[,] valueArray = (object[,])range.get_Value(
                        Excel.XlRangeValueDataType.xlRangeValueDefault);

            double[][] data = new double[worksheet.UsedRange.Rows.Count][];

            // iterate through each cell and display the contents.
            for (int row = 1; row <= worksheet.UsedRange.Rows.Count; row++)
            {
                data[row-1] = new double[worksheet.UsedRange.Columns.Count];

                for (int col = 1; col <= worksheet.UsedRange.Columns.Count; col++)
                {
                    // Print value of the cell to Console.

                    data[row-1][col-1] = Convert.ToDouble( valueArray[row, col].ToString());
                }
            }

            //web

            workbook.Close();
            xlsApp.Quit();
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(xlsApp);
            Marshal.FinalReleaseComObject(xlsApp);
            xlsApp = null;

            return data;
        }
    }

    

    
}
