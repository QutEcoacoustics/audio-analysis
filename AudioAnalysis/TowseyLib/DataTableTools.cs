﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace TowseyLib
{
    public static class DataTableTools
    {


        public static DataTable CreateTable(string[] headers, Type[] types)
        {
            if (headers.Length != types.Length) return null;
            DataTable table = new DataTable();
            for (int i = 0; i < headers.Length; i++) table.Columns.Add(headers[i], types[i]);
            //table.Columns.Add("Drug", typeof(string));
            //table.Columns.Add("Patient", typeof(string));
            //table.Columns.Add("Date", typeof(DateTime));

            // Here we add five DataRows.
            //table.Rows.Add(25, "Indocin", "David", DateTime.Now);
            //table.Rows.Add(50, "Enebrel", "Sam", DateTime.Now);
            //table.Rows.Add(10, "Hydralazine", "Christoff", DateTime.Now);
            //table.Rows.Add(21, "Combivent", "Janet", DateTime.Now);
            //table.Rows.Add(100, "Dilantin", "Melanie", DateTime.Now);

            //Another way to add rows to DataTable 
            //DataRow row = dt.NewRow();
            //row["ID"] = 1;
            //row["Name"] = "Jack";
            //dt.Rows.Add(row); 
            return table;
        }


        public static DataTable CreateTable(string[] headers, Type[] types, List<double[]> data)
        {
            if (headers.Length != types.Length) return null;
            DataTable table = new DataTable();
            for (int i = 0; i < headers.Length; i++) table.Columns.Add(headers[i], types[i]);

            //check all rows are of same length
            int rowCount = data[0].Length;
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i].Length != rowCount) data[i] = new double[rowCount];
            }

            for (int r = 0; r < rowCount; r++)
            {
                DataRow row = table.NewRow();
                for (int c = 0; c < data.Count; c++)
                {
                    row[c] = data[c][r];
                }
                table.Rows.Add(row); 
            }
            return table;
        }



        public static DataTable CreateTable(string[] headers, string[] types)
        {
            Type[] typeOfs = new Type[types.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                if (types[i].Equals("string")) typeOfs[i] = typeof(string);
                else
                    if (types[i].Equals("int")) typeOfs[i] = typeof(int);
                    else
                        if (types[i].Equals("double")) typeOfs[i] = typeof(double);
                        else
                            if (types[i].Equals("bool")) typeOfs[i] = typeof(bool);
                            else
                                if (types[i].Equals("DateTime")) typeOfs[i] = typeof(DateTime);
            }
            return CreateTable(headers, typeOfs);
        }


        /// <summary>
        /// setup skeleton of new table with same headers and column types as passed tabloel
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable CreateTable(DataTable dt)
        {
            var headers = new List<string>();
            var typeOfs = new List<Type>();


            //DataColumn[] cols = dt.Columns;
            foreach (DataColumn col in dt.Columns)
            {
                headers.Add(col.ColumnName);
                typeOfs.Add(col.DataType);
            }
            return CreateTable(headers.ToArray(), typeOfs.ToArray());
        }


        /// <summary>
        /// create a new table using columns selected from the passed table
        /// WARNING THIS METHOD APPEARS NOT TO WORK - ADD TO THE TODO LIST
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable CreateTable(DataTable dt, string[] newHeaders)
        {
            DataTable newTable = new DataTable();
            foreach (string header in newHeaders)
            {
                if (dt.Columns.Contains(header))
                {
                    DataColumn col = dt.Columns[header];
                    DataTableTools.AddColumn2Table(newTable, col);
                }
            }
            return newTable;
        }






        //#######################################################################################
        /*    Program that uses DataTable with DataGridView [C#]

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

        */


        // #######################################################################################
        // SORTING A TABLE

        /// <summary>
        /// sorts all the rows in a table without filtering
        /// The empty string is the filtering term.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sortString"></param>
        /// <returns></returns>
        public static DataRow[] SortRows(DataTable dt, string sortString)
        {
            DataRow[] rows = dt.Select(string.Empty, sortString);
            return rows;
        }

        /// <summary>
        /// sorts all the rows in a table without filtering
        /// The empty string is the filtering term.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sortString"></param>
        /// <returns></returns>
        public static DataTable SortTable(DataTable dt, string sortString)
        {
            DataRow[] rows = SortRows(dt, sortString);
            DataTable opDataTable = CreateTable(dt);
            foreach (DataRow row in rows)
            {
                opDataTable.ImportRow(row);
            }
            return opDataTable;
        }


        /*          #######################################################################################
                    SORTING A TABLE
                     // Sorting data based on ID 
                    dt.DefaultView.Sort = "ID ASC"; 

                    GridView1.DataSource = dt; 
                    GridView1.DataBind();
                    #######################################################################################
                    SORTING A TABLE USING DEFAULTVIEW

                        // Get the DefaultViewManager of a DataTable.
                        DataView view = DataTable1.DefaultView;

                        // By default, the first column sorted ascending.
                        view.Sort = "State, ZipCode DESC";
                    #######################################################################################
                    SORTING A TABLE USING SELECT

                    In fact do not sort the table only a view.
                    To sort the table use select.
                    If you want to sort by a primary key, just use DataTable.Select() with no parameters. 
                    No need for a DataView.

                    // sorting the datatable based on salary column in descending order
                    DataRow[] rows=  dtEmployee.Select(string.Empty,"Salary desc");

                    //HERE IS ANOTHER EXAMPLE OF SELECT syntax
                        DataRow[] result = table.Select("Size >= 230 AND Sex = 'm'");
                        foreach (DataRow row in result)
                        {
                            Console.WriteLine("{0}, {1}", row[0], row[1]);
                        }

                    The syntax to the Select method is somewhat tricky. 
                    It is SQL-style syntax but because it is inside a string literal, you sometimes need to escape quotation marks. 
                    Some values, like characters, may need to be quoted. The AND and OR operators can be used as in SQL. 
                    There is an example of Select with DateTime filters on MSDN.

                   // Use the Select method to find all rows matching the filter.    
                    private void GetRowsByFilter()
                    {
                        // Presuming the DataTable has a column named Date.
                        string expression = "Date > #1/1/00#";
                        DataRow[] foundRows = dt.Select(expression);
                    }
         

           #######################################################################################
           SORTING A TABLE

              //convert DataTable to DataView  
              DataView dv = dt.DefaultView;  
              //apply the sort on CustomerSurname column  
              dv.Sort = "CustomerSurname";  

              //THIS IS THE IMPORTANT STEP!!!!!!!!!!!!!! 
              //save our newly ordered results back into our datatable  
              dt = dv.ToTable();  

        */

        // #######################################################################################################################
        /// <summary>
        /// NOT DEBUGGED!!!!!
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="colName"></param>
        /// <param name="value"></param>
        public static void DeleteRows(DataTable dt, string colName, string value)
        {
            var rows = dt.Select(colName + " != " + value);
            foreach (var row in rows)
                row.Delete();
        }


        public static void AddColumn2Table(DataTable dt, string columnName, double[] array)
        {
            if (array == null) return;
            if (array.Length == 0) return;

            int index = 0;
            if (!dt.Columns.Contains(columnName)) dt.Columns.Add(columnName, typeof(double));
            foreach (DataRow row in dt.Rows)
            {
                row[columnName] = (double)array[index++];
            }
        }

        /// <summary>
        /// WARNING THIS METHOD APPEARS NOT TO WORK - ADD TO THE TODO LIST
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="col"></param>
        public static void AddColumn2Table(DataTable dt, DataColumn col)
        {
            if (dt == null) return;
            if ((col == null) || (col.Container == null) || (col.Container.Components == null)) return;
            int rowCount = col.Container.Components.Count;
            if (rowCount == 0) return;
            string name = col.ColumnName;
            Type type = col.GetType();
            if (!dt.Columns.Contains(name)) dt.Columns.Add(name, type);

            for (int r = 0; r < rowCount; r++)
            {
                dt.Rows[r][name] = col.Container.Components[r];
            } //for all rows
        } //AddColumn2Table()



        public static List<int> Column2ListOfInt(DataTable dt, string colName)
        {
            if (dt == null) return null;
            var list = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add((int)row[colName]);
            }
            return list;
        }

        public static double GetDoubleFromDataRow(DataRow row, int location)
        {
            var value = row[location];
            var isDouble = value is double;

            if (isDouble) return (double)value;
            else
            {
                double result;
                if (double.TryParse(value.ToString(), out result)) return result;
            }
            return Double.NaN;
        }

        public static List<double> Column2ListOfDouble(DataTable dt, string colName)
        {
            if (dt == null) return null;
            var list = new List<double>();
            var colType = dt.Columns[colName].DataType;

            foreach (DataRow row in dt.Rows)
            {
                var value = row[colName];
                var isDouble = value is double;

                if (isDouble)
                {
                    list.Add((double)value);
                }
                else
                {
                    double result;
                    if (double.TryParse(value.ToString(), out result))
                    {
                        list.Add(result);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return list;
        }

        public static List<double[]> ListOfColumnValues(DataTable dt)
        {
            if (dt == null) return null;
            var list = new List<double[]>();
            foreach (DataColumn col in dt.Columns)
            {
                string name = col.ColumnName;
                List<double> values = Column2ListOfDouble(dt, name);
                list.Add(values.ToArray());
            }
            return list;
        }

        public static string[] GetColumnNames(DataTable dt)
        {
            if(dt == null) return null;
            var names = new List<string>();
            foreach (DataColumn col in dt.Columns) names.Add(col.ColumnName);
            return names.ToArray();
        }


        public static Type[] GetColumnTypes(DataTable dt)
        {
            if (dt == null) return null;
            var types = new List<Type>();
            foreach (DataColumn col in dt.Columns) types.Add(col.DataType);
            return types.ToArray();
        }



        /// <summary>
        /// normalises the column values in a data table to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable NormaliseColumnValues(DataTable dt)
        {
            if (dt == null) return null;
            List<double[]> columns = DataTableTools.ListOfColumnValues(dt);
            List<double[]> newList = new List<double[]>(); 
            for (int i = 0; i < columns.Count; i++)
            {
                double[] processedColumn = DataTools.normalise(columns[i]); //normalise all values in [0,1]
                newList.Add(processedColumn);
            }
            string[] headers = DataTableTools.GetColumnNames(dt);
            Type[] types = DataTableTools.GetColumnTypes(dt);
            for (int i = 0; i < columns.Count; i++)
            {
                if (types[i] == typeof(int))   types[i] = typeof(double);
                else
                if (types[i] == typeof(Int32)) types[i] = typeof(double);
            }
            var processedtable = DataTableTools.CreateTable(headers, types, newList);
            return processedtable;
        }




        public static void RemoveTableColumns(DataTable dt, bool[] retainColumn)
        {
            if (dt == null) return;
            int colCount = dt.Columns.Count;
            string[] names = GetColumnNames(dt);
            for (int i = 0; i < colCount; i++)
            {
                if ((i >= retainColumn.Length) || (!retainColumn[i]))
                {
                    dt.Columns.Remove(names[i]);
                }
            }
        }


       public static void WriteTable2Console(DataTable dt)
       {
           if (dt == null) return;
           string[] headers = DataTableTools.GetColumnNames(dt);
           foreach(string name in headers) Console.Write(" {0,-10}", name);
           Console.WriteLine();
            var rows = dt.Rows;
            foreach(DataRow row in rows)
            {
                for (int i = 0; i < headers.Length; i++) Console.Write(" {0:f2}{1,-7}", row[headers[i]], " ");
                Console.WriteLine();
            }
       }


    } //class
}
