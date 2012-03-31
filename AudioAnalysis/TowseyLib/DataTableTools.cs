using System;
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
        for(int i=0; i<headers.Length; i++) table.Columns.Add(headers[i], types[i]);
        //table.Columns.Add("Drug", typeof(string));
        //table.Columns.Add("Patient", typeof(string));
        //table.Columns.Add("Date", typeof(DateTime));

        //
        // Here we add five DataRows.
        //
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


    public static DataTable CreateTable(string[] headers, string[] types)
    {
        Type[] typeOfs = new Type[types.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            if(types[i].Equals("string")) typeOfs[i] = typeof(string);
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



    #######################################################################################
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
 */
        /// <summary>
        /// NOT DEBUGGED!!!!!
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="colName"></param>
        /// <param name="value"></param>
    public static void DeleteRows(DataTable dt, string colName, string value)
    {
        var rows = dt.Select(colName+ " != "+value);
        foreach (var row in rows)
            row.Delete();
    }



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
        //DataTable opDataTable = new DataTable();
        DataTable opDataTable = CreateTable(dt);
        foreach (DataRow row in rows)
        {
            opDataTable.ImportRow(row);
        }
        return opDataTable;
    }

 /*

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


    }
}
