using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;


namespace TowseyLib
{
    public static class CsvTools
    {


        //READING A TABLE FROM A CSV FILE

         public static void TransferCSVToTable(DataTable dt, string  filePath)
           {
               string[] csvRows = System.IO.File.ReadAllLines(filePath);
               string[] fields = null; 
               foreach(string csvRow in csvRows)
               {
                  fields = csvRow.Split(',');
                  DataRow row = dt.NewRow();
                  row.ItemArray = fields;
                  dt.Rows.Add(row);
               }
           }



        //#######################################################################################
        //READING A TABLE FROM A CSV FILE

        // using System.Data;
        // using System.Data.OleDb;
        // using System.Globalization;
        // using System.IO;
/*
        static DataTable GetDataTableFromCsv(string path, bool isFirstRowHeader)
        {
            string header = isFirstRowHeader ? "Yes" : "No";

            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            string sql = @"SELECT * FROM [" + fileName + "]";

            using(OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly + 
                      ";Extended Properties=\"Text;HDR=" + header + "\""))
            using(OleDbCommand command = new OleDbCommand(sql, connection))
            using(OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

 * 
 */ 
 
        //#######################################################################################
        //WRITE A CSV FILE FROM A TABLE
         public static void DataTable2CSV(DataTable dt, string strFilePath)
        {
            // Create the CSV file to which grid data will be exported.
            StreamWriter sw = new StreamWriter(strFilePath, false);

            // First we will write the headers.
            //DataTable dt = m_dsProducts.Tables[0];

            int iColCount = dt.Columns.Count;
            for (int i = 0; i < iColCount; i++)
            {
                sw.Write(dt.Columns[i]);
                if (i < iColCount - 1) sw.Write(","); 
            }

            sw.Write(sw.NewLine);

            // Now write all the rows.
            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < iColCount; i++)
                {

                    if (!Convert.IsDBNull(dr[i]))
                    {
                        sw.Write(dr[i].ToString());
                    }
                    if (i < iColCount - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        } // DataTable2CSV()




        //#######################################################################################
        //WRITE A CSV FILE FROM A TABLE

        public static void DataTable2CSV(DataTable table, string strFilePath, string seperateChar)
        {

            StreamWriter sr = null;

            try
            {

            sr = new StreamWriter(strFilePath);
            string seperator = "";
            StringBuilder builder = new StringBuilder();
            foreach (DataColumn col in table.Columns)
            {
                builder.Append(seperator).Append(col.ColumnName);
                seperator = seperateChar;
            }
            sr.WriteLine(builder.ToString());

            foreach (DataRow row in table.Rows)
            {
                seperator = "";
                builder = new StringBuilder();
                foreach (DataColumn col in table.Columns)
                {
                    builder.Append(seperator).Append(row[col.ColumnName]);
                    seperator = seperateChar;
                }

                sr.WriteLine(builder.ToString());

            }
            }
            finally
            {
                if (sr != null)	{ sr.Close();}
            }
        } // DataTable2CSV()


        public static string WriteDataTableRow(DataRow row, string seperateChar)
        {
            string seperator = "";
            StringBuilder builder = new StringBuilder();
            foreach (object item in row.ItemArray)
            {
                builder.Append(seperator).Append(item);
                seperator = seperateChar;
            }
            return builder.ToString();
        } // WriteDataTableRow()




    } //class
}//namespace
