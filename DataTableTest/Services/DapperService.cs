using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using DataTableTest.Models;
using LinqKit;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using Oracle.ManagedDataAccess.Client;

namespace DataTableTest.Services
{
	public class DapperService
	{
		private string _conn;
		public DapperService(string conn)
		{
			_conn = conn;
		}
		public List<Payment> GetPaymentsFromDB(string searchBy, int take, int skip, string sortBy, bool sortDir, out int filteredResultsCount, out int totalResultsCount)
		{
			string sql = "SELECT * FROM PRI_LOC_PAYMENT where rownum<= 10";
			using (var connection = new OracleConnection { ConnectionString = _conn })
			{
				var Db = connection.Query<Payment>(sql).AsQueryable();
				// the example datatable used is not supporting multi column ordering
				// so we only need get the column order from the first column passed to us.        
				var whereClause = BuildDynamicWhereClause(searchBy);

				if (String.IsNullOrEmpty(searchBy))
				{
					// if we have an empty search then just order the results by Id ascending
					sortBy = "ACCOUNT_ID";
					sortDir = true;
				}

				var result = Db.AsExpandable().Where(whereClause)
							   .OrderBy(sortBy, sortDir) // have to give a default order when skipping .. so use the PK
							   .Skip(skip)
							   .Take(take)
							   .ToList();

				// now just get the count of items (without the skip and take) - eg how many could be returned with filtering
				filteredResultsCount = Db.AsExpandable().Where(whereClause).Count();
				totalResultsCount = Db.Count();
				return result;
			}
		}
		private Expression<Func<Payment, bool>> BuildDynamicWhereClause( string searchValue)
		{
			// simple method to dynamically plugin a where clause
			var predicate = PredicateBuilder.New<Payment>(true); // true -where(true) return all
			if (String.IsNullOrWhiteSpace(searchValue) == false)
			{
				var searchTerms = searchValue.Split(' ').ToList().ConvertAll(x => x.ToLower());

				//predicate = predicate.Or(s => searchTerms.Any(srch => s.PaymentId.ToLower().Contains(srch)));
				//predicate = predicate.Or(s => searchTerms.Any(srch => s.Surname.ToLower().Contains(srch)));
				//predicate = predicate.Or(s => searchTerms.Any(srch => s.PassportNo.ToLower().Contains(srch)));
			}
			return predicate;
		}
	}
}