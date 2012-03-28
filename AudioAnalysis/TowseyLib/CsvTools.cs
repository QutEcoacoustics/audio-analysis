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

        /*
         * 
        class Program
        {
            static void Main()
            {
            //
            // Get the DataTable.
            //
            DataTable table = GetTable();
            //
            // Use DataTable here with SQL, etc.
            //
            }

            /// <summary>
            /// This example method generates a DataTable.
            /// </summary>
            static DataTable GetTable()
            {
            //
            // Here we create a DataTable with four columns.
            //
            DataTable table = new DataTable();
            table.Columns.Add("Dosage", typeof(int));
            table.Columns.Add("Drug", typeof(string));
            table.Columns.Add("Patient", typeof(string));
            table.Columns.Add("Date", typeof(DateTime));

            //
            // Here we add five DataRows.
            //
            table.Rows.Add(25, "Indocin", "David", DateTime.Now);
            table.Rows.Add(50, "Enebrel", "Sam", DateTime.Now);
            table.Rows.Add(10, "Hydralazine", "Christoff", DateTime.Now);
            table.Rows.Add(21, "Combivent", "Janet", DateTime.Now);
            table.Rows.Add(100, "Dilantin", "Melanie", DateTime.Now);
            return table;
            }
        }


        #######################################################################################


        Program that uses DataTable with DataGridView [C#]

        using System.Collections.Generic;
        using System.Data;
        using System.Windows.Forms;

        namespace WindowsFormsApplication1
        {
            public partial class Form1 : Form
            {
            /// <summary>
            /// Contains column names.
            /// </summary>
            List<string> _names = new List<string>();

            /// <summary>
            /// Contains column data arrays.
            /// </summary>
            List<double[]> _dataArray = new List<double[]>();

            public Form1()
            {
                InitializeComponent();

                // Example column.
                _names.Add("Cat");
                // Three numbers of cat data
                _dataArray.Add(new double[]
                {
                1.0,
                2.2,
                3.4
                });

                // Another example column
                _names.Add("Dog");
                // Add three numbers of dog data
                _dataArray.Add(new double[]
                {
                3.3,
                5.0,
                7.0
                });
                // Render the DataGridView.
                dataGridView1.DataSource = GetResultsTable();
            }

            /// <summary>
            /// This method builds a DataTable of the data.
            /// </summary>
            public DataTable GetResultsTable()
            {
                // Create the output table.
                DataTable d = new DataTable();

                // Loop through all process names.
                for (int i = 0; i < this._dataArray.Count; i++)
                {
                // The current process name.
                string name = this._names[i];

                // Add the program name to our columns.
                d.Columns.Add(name);

                // Add all of the memory numbers to an object list.
                List<object> objectNumbers = new List<object>();

                // Put every column's numbers in this List.
                foreach (double number in this._dataArray[i])
                {
                    objectNumbers.Add((object)number);
                }

                // Keep adding rows until we have enough.
                while (d.Rows.Count < objectNumbers.Count)
                {
                    d.Rows.Add();
                }

                // Add each item to the cells in the column.
                for (int a = 0; a < objectNumbers.Count; a++)
                {
                    d.Rows[a][i] = objectNumbers[a];
                }
                }
                return d;
            }
            }
        }



        #######################################################################################
        SORTING A TABLE


        DataTable dt = new DataTable(); 
        //Define columns to DataTable 
        dt.Columns.Add("Id"); 
        dt.Columns.Add("Name"); 

        //Adding rows to DataTable 
        DataRow row = dt.NewRow(); 
        row["ID"] = 1; 
        row["Name"] = "Jack"; 
        dt.Rows.Add(row); 


        DataRow row1 = dt.NewRow(); 
        row1["ID"] = 2; 
        row1["Name"] = "Fruit"; 
        dt.Rows.Add(row1); 

         // Sorting data based on ID 
        dt.DefaultView.Sort = "ID ASC"; 

        GridView1.DataSource = dt; 
        GridView1.DataBind();


        #######################################################################################
        SORTING A TABLE

            // Get the DefaultViewManager of a DataTable.
            DataView view = DataTable1.DefaultView;

            // By default, the first column sorted ascending.
            view.Sort = "State, ZipCode DESC";

        #######################################################################################
        SORTING A TABLE

        In fact do not sort the table only a view.
        To sort the table use select.
        If you want to sort by a primary key, just use DataTable.Select() with no parameters. 
        No need for a DataView.

        using System;
        using System.Data;
 
        public class DataTableSortExample
        {
            public static void Main()
            {
                //adding up a new datatable
                DataTable dtEmployee = new DataTable("Employee"); 
 
                //adding up 3 columns to datatable
                dtEmployee.Columns.Add("ID", typeof(int));
                dtEmployee.Columns.Add("Name", typeof(string));
                dtEmployee.Columns.Add("Salary", typeof(double));
 
                //adding up rows to the datatable
                dtEmployee.Rows.Add(52, "Human1", 21000);
                dtEmployee.Rows.Add(63, "Human2", 22000);
                dtEmployee.Rows.Add(72, "Human3", 23000);
                dtEmployee.Rows.Add(110,"Human4", 24000);
 
                // sorting the datatable based on salary in descending order
                DataRow[] rows=  dtEmployee.Select(string.Empty,"Salary desc");
 
                //foreach datatable
                foreach (DataRow row in rows)
                {
                      Console.WriteLine(row["ID"].ToString() + ":" + row["Name"].ToString()
                         + ":" + row["Salary"].ToString());
                }
 
                Console.ReadLine();
            }
 
        }





        ######################################################################################

                //HERE IS ANOTHER EXAMPLE OF SELECT syntax

            DataRow[] result = table.Select("Size >= 230 AND Sex = 'm'");
            foreach (DataRow row in result)
            {
                Console.WriteLine("{0}, {1}", row[0], row[1]);
            }

        The syntax to the Select method is somewhat tricky. It is SQL-style syntax but because it is inside a string literal, you sometimes need to escape quotation marks. Some values, like characters, may need to be quoted. The AND and OR operators can be used as in SQL. There is an example of Select with DateTime filters on MSDN.


        ######################################################################################

                //HERE IS ANOTHER EXAMPLE OF SELECT syntax



        private void GetRowsByFilter()
        {
            DataTable table = DataSet1.Tables["Orders"];
            // Presuming the DataTable has a column named Date.
            string expression;
            expression = "Date > #1/1/00#";
            DataRow[] foundRows;

            // Use the Select method to find all rows matching the filter.
            foundRows = table.Select(expression);

            // Print column 0 of each returned row.
            for(int i = 0; i < foundRows.Length; i ++)
            {
                Console.WriteLine(foundRows[i][0]);
            }
        }



        #######################################################################################
        SORTING A TABLE

        using System.Data;  
  
        //create our datatable and setup with 2 columns  
        DataTable dt = new DataTable();  
        dt.Columns.Add("CustomerFirstname", typeof(string));  
        dt.Columns.Add("CustomerSurname", typeof(string));  
        DataRow dr;  
  
        //store some values into datatable  
        //just using 2 names as example  
        dr = dt.NewRow();  
        dr["CustomerFirstname"] = "John";  
        dr["CustomerSurname"] = "Murphy";  
        dt.Rows.Add(dr);  
        dr = dt.NewRow();  
        dr["CustomerFirstname"] = "John";  
        dr["CustomerSurname"] = "Doe";  
        dt.Rows.Add(dr);  
  
        //check to make sure our datatable has at  
        //least one value. For this example it's not  
        //really need but if we were taking the   
        //values from a database then this would be  
        //very important!  
        if (dt.Rows.Count > 0)  
        {  
           //convert DataTable to DataView  
           DataView dv = dt.DefaultView;  
           //apply the sort on CustomerSurname column  
           dv.Sort = "CustomerSurname";  

           //THIS IS THE IMPORTANT STEP!!!!!!!!!!!!!! 
           //save our newly ordered results back into our datatable  
           dt = dv.ToTable();  
        }  

*/

        //#######################################################################################
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
         public static void CreateCSVFile(DataTable dt, string strFilePath)

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
            }




        //#######################################################################################
        //WRITE A CSV FILE FROM A TABLE

        public static void DataTable2CSV(DataTable table, string filename, string seperateChar)
        {

            StreamWriter sr = null;

            try
            {

            sr = new StreamWriter(filename);
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
        }

    } //class
}//namespace
