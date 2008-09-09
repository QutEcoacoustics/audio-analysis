using System;
using System.Xml;
using System.Data;
using System.Text;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Data.Common;

namespace QutSensors
{
	public class DB
    {
		#region Statics
		static string connectionString;

		public static string ConnectionString
		{
			get
			{
				if (connectionString == null)
					return System.Configuration.ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
				else
					return connectionString;
			}

			set {connectionString = value;}
		}
		
		public static DB Current
		{
			get
			{
				DB retVal = (DB)CallContext.GetData("DB");
				if (retVal == null)
				{
					retVal = new DB();
					CallContext.SetData("DB", retVal);
				}
				return retVal;
			}
		}

		private DB()
		{
		}

		public static string SqlEncode(string s)
		{
			if (s == null)
				return s;
			return s.Replace("'", "''");
		}
		#endregion

		private int? timeout;
		public int? Timeout
		{
			get { return timeout; }
			set { timeout = value; }
		}
		
		#region Basic Commands
		public SqlConnection transactedConnection;
		SqlTransaction transaction;

		public void CreateTransaction()
		{
			transactedConnection = DB.Current.CreateConnection();
			transaction = transactedConnection.BeginTransaction();
		}

		public void CommitTransaction()
		{
			transaction.Commit();
			transactedConnection.Dispose();
			transactedConnection = null;
		}

		public void RollbackTransaction()
		{
			transaction.Rollback();
			transactedConnection.Dispose();
			transactedConnection = null;
		}

		public SqlConnection CreateConnection()
		{
			if (transactedConnection != null)
				return transactedConnection;
			SqlConnection retVal = new SqlConnection(ConnectionString);
			retVal.Open();
			return retVal;
		}

		public void ExecuteNonQuery(SqlConnection conn, string sql, params object[] parameters)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue("@" + i.ToString(), parameters[i]);
				}

				cmd.ExecuteNonQuery();
			}
		}

		public void ExecuteNonQueryProcedure(SqlConnection conn, string sql)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				cmd.CommandType = CommandType.StoredProcedure;

				cmd.ExecuteNonQuery();
			}
		}

		public void ExecuteNonQueryProcedure(SqlConnection conn, string sql, string[] parNames, params object[] parameters)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				cmd.CommandType = CommandType.StoredProcedure;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue(parNames[i], parameters[i]);
				}

				cmd.ExecuteNonQuery();
			}
		}

		public object ExecuteScalar(SqlConnection conn, string sql, params object[] parameters)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue("@" + i.ToString(), parameters[i]);
				}

				return cmd.ExecuteScalar();
			}
		}

		public object ExecuteScalarProcedure(SqlConnection conn, string sql, string[] parNames, params object[] parameters)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				cmd.CommandType = CommandType.StoredProcedure;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue(parNames[i], parameters[i]);
				}

				return cmd.ExecuteScalar();
			}
		}

		public SqlDataReader ExecuteReader(string sql, params object[] parameters)
		{
			SqlConnection conn = CreateConnection();
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue("@" + i.ToString(), parameters[i]);
				}

				return cmd.ExecuteReader(transactedConnection == null
					? CommandBehavior.CloseConnection : CommandBehavior.Default);
			}
		}

		public SqlDataReader ExecuteReader(SqlConnection conn, string sql, params object[] parameters)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue("@" + i.ToString(), parameters[i]);
				}

				return cmd.ExecuteReader();
			}
		}

		public SqlDataReader ExecuteReaderProcedure(SqlConnection conn, string sql)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = 120; // Prevents "General Network Error" exception...

				return cmd.ExecuteReader();
			}
		}

		public SqlDataReader ExecuteReaderProcedure(SqlConnection conn, string sql, string[] parNames, params object[] parameters)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				cmd.CommandType = CommandType.StoredProcedure;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue(parNames[i], parameters[i]);
				}

				return cmd.ExecuteReader();
			}
		}

		public DataSet ExecuteDataSet(SqlConnection conn, string sql, params object[] parameters)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue("@" + i.ToString(), parameters[i]);
				}

				DataSet retVal = new DataSet();
				SqlDataAdapter adapter = new SqlDataAdapter(cmd);
				adapter.Fill(retVal);
				return retVal;
			}
		}

		public DataSet ExecuteDataSetProcedure(SqlConnection conn, string sql, string[] parNames, params object[] parameters)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				if (timeout != null)
					cmd.CommandTimeout = timeout.Value;
				if (transactedConnection != null)
					cmd.Transaction = transaction;

				cmd.CommandType = CommandType.StoredProcedure;

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
						parameters[i] = DBNull.Value;
					cmd.Parameters.AddWithValue(parNames[i], parameters[i]);
				}

				DataSet retVal = new DataSet();
				SqlDataAdapter adapter = new SqlDataAdapter(cmd);
				adapter.Fill(retVal);
				return retVal;
			}
		}
		#endregion
		
		#region Utilities
		public static string GetCommaSeparatedList(string[] items)
		{
			if (items == null || items.Length == 0)
				return "";
			else
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < items.Length - 1; i++)
					sb.AppendFormat("{0}, ", items[i]);
				sb.AppendFormat("{0}", items[items.Length - 1]);
				return sb.ToString();
			}
		}

		public static string GetCommaSeparatedList(List<string> items)
		{
			if (items == null || items.Count == 0)
				return "";
			else
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < items.Count - 1; i++)
					sb.AppendFormat("'{0}', ", items[i]);
				sb.AppendFormat("'{0}'", items[items.Count - 1]);
				return sb.ToString();
			}
		}
		#endregion
		
		public bool DoesTableExist(string tableName)
		{
			SqlConnection conn = CreateConnection();
			try
			{
				object o = ExecuteScalar(conn, @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME=@0", tableName);
				if (o != null && (int)o == 1)
					return true;
				return false;
			}
			finally
			{
				if (transactedConnection == null)
					conn.Dispose();
			}
		}

        /*public static void UpgradeMigrations()
        {
            RikMigrations.DbProvider.DefaultConnectionString = DB.ConnectionString;
            RikMigrations.MigrationManager.UpgradeMax(typeof(DB).Assembly);
        }*/
	}
}