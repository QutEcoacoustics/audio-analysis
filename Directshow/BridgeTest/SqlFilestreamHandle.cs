using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.SqlClient;
using Microsoft.Win32.SafeHandles;

public class SqlFilestreamHandle : IDisposable
{
	#region External Definitions
	const UInt32 DESIRED_ACCESS_READ = 0x00000000;
	const UInt32 DESIRED_ACCESS_WRITE = 0x00000001;
	const UInt32 DESIRED_ACCESS_READWRITE = 0x00000002;

	const UInt32 SQL_FILESTREAM_OPEN_NO_FLAGS = 0x00000000;
	const UInt32 SQL_FILESTREAM_OPEN_FLAG_ASYNC = 0x00000001;
	const UInt32 SQL_FILESTREAM_OPEN_FLAG_NO_BUFFERING = 0x00000002;
	const UInt32 SQL_FILESTREAM_OPEN_FLAG_NO_WRITE_THROUGH = 0x00000004;
	const UInt32 SQL_FILESTREAM_OPEN_FLAG_SEQUENTIAL_SCAN = 0x00000008;
	const UInt32 SQL_FILESTREAM_OPEN_FLAG_RANDOM_ACCESS = 0x00000010;

	[DllImport("sqlncli10.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	static extern SafeFileHandle OpenSqlFilestream(
			string FilestreamPath,
			UInt32 DesiredAccess,
			UInt32 OpenOptions,
			byte[] FilestreamTransactionContext,
			UInt32 FilestreamTransactionContextLength,
			Int64 AllocationSize);
	#endregion

	SqlTransaction tran;
	List<IDisposable> disposableItems = new List<IDisposable>();

	public SqlFilestreamHandle(string table, string column, string whereClause, params object[] parameters)
	{
		try
		{
			var conn = new SqlConnection(QutSensors.DB.ConnectionString);
			disposableItems.Add(conn);

			conn.Open();
			string sql = string.Format("SELECT {0}.PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT() FROM {1}", column, table);
			if (whereClause != null)
				sql += " WHERE " + whereClause;
			var cmd = new SqlCommand(sql, conn);
			disposableItems.Add(cmd);

			tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
			cmd.Transaction = tran;

			for (int i = 0; i < parameters.Length; i++)
				cmd.Parameters.Add(new SqlParameter("@" + i.ToString(), parameters[i]));

			var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				FileName = reader.GetString(0);
				Context = reader.GetSqlBytes(1).Buffer;
				disposableItems.Add(reader);
			}
			else
				throw new Exception("No data found for sql file wrapper");
		}
		catch
		{
			for (int i = disposableItems.Count - 1; i >= 0; i--)
				disposableItems[i].Dispose();
			throw;
		}
	}

	public void Open()
	{
		Handle = OpenSqlFilestream(FileName, DESIRED_ACCESS_READ, SQL_FILESTREAM_OPEN_NO_FLAGS,
						 Context, (UInt32)Context.Length, 0);
	}

	public void Close()
	{
		if (Handle != null)
		{
			Handle.Close();
			Handle = null;
		}
	}

	#region Properties
	public SafeFileHandle Handle { get; protected set; }

	public string FileName { get; protected set; }
	public byte[] Context { get; protected set; }
	#endregion

	#region IDisposable Members
	~SqlFilestreamHandle()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);
	}

	public void Dispose(bool disposing)
	{
		if (Handle != null)
			Close();

		// Dispose reader before transaction
		int lastIndex = disposableItems.Count - 1;
		disposableItems[lastIndex].Dispose();
		disposableItems.RemoveAt(lastIndex);

		tran.Commit();

		for (int i = disposableItems.Count - 1; i >= 0; i--)
			disposableItems[i].Dispose();

		if (disposing)
			GC.SuppressFinalize(this);
	}
	#endregion
}