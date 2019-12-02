using DataTableTest.Models;
using LinqKit;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace DataTableTest.Services
{
	public class AdoService<T> where T : class
	{
		private string _conn;
		private string _table;
		public AdoService(string conn, string table)
		{
			_conn = conn;
			_table = table;
		}
		public List<Payment> GetPaymentsFromDB(string searchBy, int take, int skip, string sortBy, bool sortDir, out int filteredResultsCount, out long totalResultsCount)
		{
			totalResultsCount = 0;
			filteredResultsCount = 0;
			var payments = new List<Payment>();
			string sql = "SELECT * FROM PRI_LOC_PAYMENT";
			string sql2 = string.Format("SELECT COUNT(*) FROM ({0})",sql);
			var sql3 = string.Empty;
			var whereClause = BuildDynamicWhereClause(searchBy);
			if(!string.IsNullOrEmpty(whereClause))
			{
				sql = sql + string.Format(" where {0}", whereClause);
			}
			if (string.IsNullOrEmpty(searchBy))
			{
				// if we have an empty search then just order the results by Id ascending
				sortBy = "ACCOUNT_ID";
				sortDir = true;
			}
			sql3 = string.Format("select count(*) from ({0})", sql);
			sql = string.Format("SELECT * FROM (SELECT a.*, rownum rnum FROM ({0}) a WHERE rownum <= {1}) b  WHERE rnum > {2}", sql,take+skip,skip);
			using (var con = new OracleConnection { ConnectionString = _conn })
			{
				con.Open();
				
				var cmd = con.CreateCommand();
				cmd.CommandText = sql;
				var reader = cmd.ExecuteReader();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						var bankDetail = new Payment
						{
							ACCOUNT_ID = long.Parse(reader[0].ToString()),
							BENEFICIARY = reader[4].ToString(),
						};
						payments.Add(bankDetail);
					}
				}
				cmd.CommandText = sql2;
				reader = cmd.ExecuteReader();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						totalResultsCount = long.Parse(reader[0].ToString());
					}
				}
				cmd.CommandText = sql3;
				reader = cmd.ExecuteReader();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						filteredResultsCount = int.Parse(reader[0].ToString());
					}
				}
				con.Close();
				
			}
			return payments;
		}
		public List<object> GetPaymentLogsFromDB(string searchBy, int take, int skip, string sortBy, bool sortDir, out int filteredResultsCount, out long totalResultsCount)
		{
			totalResultsCount = 0;
			filteredResultsCount = 0;
			var payments = new List<object>();
			string sql = string.Format("SELECT * FROM {0}", _table);
			string sql2 = string.Format("SELECT COUNT(*) FROM ({0})", sql);
			var sql3 = string.Empty;
			var whereClause = BuildDynamicWhereClause(searchBy);
			if (!string.IsNullOrEmpty(whereClause))
			{
				sql = sql + string.Format(" where {0}", whereClause);
			}
			if (string.IsNullOrEmpty(searchBy))
			{
				// if we have an empty search then just order the results by Id ascending
				sortBy = "ACCOUNT_ID";
				sortDir = true;
			}
			sql3 = string.Format("select count(*) from ({0})", sql);
			sql = string.Format("SELECT * FROM (SELECT a.*, rownum rnum FROM ({0}) a WHERE rownum <= {1}) b  WHERE rnum > {2}", sql, take + skip, skip);
			using (var con = new OracleConnection { ConnectionString = _conn })
			{
				con.Open();

				var cmd = con.CreateCommand();
				cmd.CommandText = sql;
				var reader = cmd.ExecuteReader();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						var item = MapObject(reader);
						payments.Add(item);
					}
				}
				cmd.CommandText = sql2;
				reader = cmd.ExecuteReader();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						totalResultsCount = long.Parse(reader[0].ToString());
					}
				}
				cmd.CommandText = sql3;
				reader = cmd.ExecuteReader();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						filteredResultsCount = int.Parse(reader[0].ToString());
					}
				}
				con.Close();

			}
			return payments;
		}

		private string BuildDynamicWhereClause(string searchValue)
		{
			var whereClause = string.Empty;
			if(!string.IsNullOrEmpty(searchValue))
			{
				PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
				foreach (PropertyDescriptor prop in props)
				{
					whereClause = whereClause+string.Format(" {0} like '%{1}%' or", prop.DisplayName, searchValue);
				}
				if(whereClause.EndsWith("or"))
				{
					whereClause = whereClause.Substring(0, whereClause.Length - 2);
				}
			}
			return whereClause;
		}

		private PaymentLog PaymentLogMapper(OracleDataReader reader)
		{
			return  new PaymentLog
			{
				ID = long.Parse(reader[0].ToString()),
				PAYMENT_ID = reader[1].ToString(),
				PAYMENT_SOURCE = reader[2].ToString(),
				ACTION_DATE = reader[3].ToString(),
				STATUS = reader[4].ToString(),
				SERVICE_CODE = reader[5].ToString(),
				SERVICE_DESCRIPRION = reader[6].ToString(),
				PAYMENT_BATCH = reader[7].ToString(),
				MODULE_NAME = reader[8].ToString(),
				SERVICE_NAME = reader[9].ToString(),
				TRANSACTION_TYPE = reader[10].ToString(),
			};
		}
		private Payment PaymentMapper(OracleDataReader reader)
		{
			return new Payment
			{
				ACCOUNT_ID = long.Parse(reader[0].ToString()),
				BENEFICIARY = reader[4].ToString(),
			};
		}

		private object MapObject(OracleDataReader reader)
		{
			Type a = typeof(T);
			Type b = typeof(PaymentLog);
			Type c = typeof(Payment);
			if (a.Equals(b))
			{
				return PaymentLogMapper(reader);
			}
			else if(a.Equals(c))
			{
				return PaymentMapper(reader);
			}
			else
			{
				return null;
			}
		}
	}
}