
using System;
using System.Collections;

namespace DBSCAN
{
	/// <summary>
	/// Cluster data using DBSCAN (Density-Based Spatical Clustering of Application with Noise) methed
	/// See "Data Mining" for further information
	/// </summary>
	public sealed class DBSCAN
	{
		public ArrayList DataPoints = new ArrayList(128);
		private ArrayList DP2DP;
		private int m_Core_Num;
		private int m_MinPts;
		private double m_eps;

		/// <summary>
		/// Add DataPoint to DBSCAN module to cluster
		/// </summary>
		public void AddDataPoint(DataPoint dp)
		{
			DataPoints.Add(dp);
			m_Core_Num = 0;
			m_MinPts = 0;
			m_eps = 0;
		}

		public void RemoveAllDataPoints()
		{
			DataPoints.Clear();
			DP2DP.Clear();
			m_Core_Num = 0;
			m_MinPts = 0;
			m_eps = 0;
		}

		public void ResetAllDataPointsState()
		{
			foreach(DataPoint dp in DataPoints)
			{
				dp.class_id = 0;
				dp.core_tag = false;
				dp.used_tag = false;
			}
		}
		public void PrepareDBSCAN_Table()
		{
			int dp_count = DataPoints.Count;
			DP2DP = new ArrayList(dp_count);
			for(int i=0;i<dp_count;i++)
			{
				// SortedList use DBSCANSort so that can support duplicate key
				// dp_count also include the point itself
				DP2DP.Add(new SortedList(new DBSCANSort(), dp_count));
			}
			SortedList sl;
			DataPoint dp;
			for(int i=0;i<dp_count;i++)
			{
				sl=(SortedList)DP2DP[i];
				dp=(DataPoint)DataPoints[i];
				for(int j=0;j<dp_count;j++)
				{
					double distance = dp.Distance((DataPoint)DataPoints[j]);
					sl.Add(distance, DataPoints[j]);
				}
			}
		}

		public int BuildCorePoint(double eps, int MinPts)
		{
			ResetAllDataPointsState();
			int core_num = 0;
			SortedList sl;
			DataPoint src_dp, des_dp;
			for(int i=0;i<DataPoints.Count;i++)
			{
				sl=(SortedList)DP2DP[i];
				des_dp=(DataPoint)sl.GetByIndex(MinPts);
				src_dp=(DataPoint)DataPoints[i];
				if(src_dp.Distance(des_dp)<eps)
				{
					src_dp.core_tag=true;
					core_num++;
				}
			}
			if(core_num>0)
			{
				m_Core_Num = core_num;
				m_MinPts = MinPts;
				m_eps = eps;
			}
			return core_num;
		}

		public void DBSCAN_Cluster()
		{
			DataPoint dp;
			int current_class_id = 1;
			for(int i=0;i<DataPoints.Count;i++)
			{
				dp=(DataPoint)DataPoints[i];
				if(dp.used_tag==false && dp.core_tag==true)
				{
					dp.class_id = current_class_id;
					dp.used_tag = true;
					CorePointCluster(i, current_class_id);
					current_class_id++;
				}
			}		
		}

		private void CorePointCluster(int dp_pos, int core_class_id)
		{
			DataPoint src_dp, des_dp;
			SortedList sl=(SortedList)DP2DP[dp_pos];
			src_dp=(DataPoint)sl.GetByIndex(0);
			int i=1;
			des_dp=(DataPoint)sl.GetByIndex(i);
			while(src_dp.Distance(des_dp)<m_eps)
			{
				if(des_dp.used_tag == false)
				{
					des_dp.class_id = core_class_id;
					des_dp.used_tag = true;
					if(des_dp.core_tag == true)
						CorePointCluster(DataPoints.IndexOf(des_dp),core_class_id);
				}
				i++;
				try 
				{
					des_dp=(DataPoint)sl.GetByIndex(i);
				}
				catch( ArgumentOutOfRangeException )
				{
					// To avoid eps is too large that out of index
					return;
				}
			}
		}
	}

	/// <summary>
	/// DBSCAN DataPoint
	/// </summary>
	public class DataPoint
	{
		public bool	core_tag	= false;
		public int	class_id	= 0;	// 0 indicate NOISE
		public bool	used_tag	= false;

		public double d1;	// dimension x-axis
		public double d2;	// dimension y-axis
		// dimension n (n>=3) can be extend by inherient this class 
		// and reimplement following two method.

		public DataPoint(double x, double y)
		{
			d1=x;
			d2=y;
		}

		public double Distance(DataPoint dp)
		{
			if(this != dp)
			{
				double d1sq=(d1-dp.d1)*(d1-dp.d1);
				double d2sq=(d2-dp.d2)*(d2-dp.d2);
				return Math.Sqrt( d1sq + d2sq );
			}
			else
				return 0;
		}
	}

	public class DBSCANSort:IComparer
	{
		public int Compare(object x, object y)
		{
			int iResult;
			if((double)x > (double)y)
				iResult = 1;
			else
				iResult = -1;
			return iResult;
		}
	}
}
