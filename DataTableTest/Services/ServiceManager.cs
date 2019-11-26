using DataTableTest.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Web;
using System.Data.Entity;

namespace DataTableTest.Services
{
    public class ServiceManager
    {
        public List<Customer> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, bool sortDir, out int filteredResultsCount, out int totalResultsCount)
        {
            var Db = new TravelTestDBEntities();
            // the example datatable used is not supporting multi column ordering
            // so we only need get the column order from the first column passed to us.        
            var whereClause = BuildDynamicWhereClause(Db, searchBy);

            if (String.IsNullOrEmpty(searchBy))
            {
                // if we have an empty search then just order the results by Id ascending
                sortBy = "CustomerId";
                sortDir = true;
            }

            var result = Db.Customers
                           .AsExpandable()
                           .Where(whereClause)
                           //.Select(m => new Customer
                           //{
                           //     CustomerId = m.CustomerId,
                           //    FirstName = m.FirstName,
                           //    Surname = m.Surname,
                           //    PassportNo = m.PassportNo,
                           //})
                           .OrderBy(sortBy, sortDir) // have to give a default order when skipping .. so use the PK
                           .Skip(skip)
                           .Take(take)
                           .ToList();
            
            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
            filteredResultsCount = Db.Customers.AsExpandable().Where(whereClause).Count();
            totalResultsCount = Db.Customers.Count();

            return result;
        }
        private Expression<Func<Customer, bool>> BuildDynamicWhereClause(DbContext entities, string searchValue)
        {
            // simple method to dynamically plugin a where clause
            var predicate = PredicateBuilder.New<Customer>(true); // true -where(true) return all
            if (String.IsNullOrWhiteSpace(searchValue) == false)
            {
                // as we only have 2 cols allow the user type in name 'firstname lastname' then use the list to search the first and last name of dbase
                var searchTerms = searchValue.Split(' ').ToList().ConvertAll(x => x.ToLower());

                predicate = predicate.Or(s => searchTerms.Any(srch => s.FirstName.ToLower().Contains(srch)));
                predicate = predicate.Or(s => searchTerms.Any(srch => s.Surname.ToLower().Contains(srch)));
                predicate = predicate.Or(s => searchTerms.Any(srch => s.PassportNo.ToLower().Contains(srch)));
            }
            return predicate;
        }
    }
}