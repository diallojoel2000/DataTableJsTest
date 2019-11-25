using DataTableTest.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace DataTableTest.Services
{
    public class ServiceManager
    {
        public List<Person> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, bool sortDir, out int filteredResultsCount, out int totalResultsCount)
        {
            var Db = new TestContext();
            // the example datatable used is not supporting multi column ordering
            // so we only need get the column order from the first column passed to us.        
            var whereClause = BuildDynamicWhereClause(Db, searchBy);

            if (String.IsNullOrEmpty(searchBy))
            {
                // if we have an empty search then just order the results by Id ascending
                sortBy = "PersonId";
                sortDir = true;
            }

            var result = Db.Persons
                           .AsExpandable()
                           .Where(whereClause)
                           .Select(m => new Person
                           {
                                PersonId = m.PersonId,
                               Firstname = m.Firstname,
                               Lastname = m.Lastname,
                               Address1 = m.Address1,
                           })
                           //.OrderBy(sortBy, sortDir) // have to give a default order when skipping .. so use the PK
                           .Skip(skip)
                           .Take(take)
                           .ToList();
            
            // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
            filteredResultsCount = Db.Persons.AsExpandable().Where(whereClause).Count();
            totalResultsCount = Db.Persons.Count();

            return result;
        }
        private Expression<Func<Person, bool>> BuildDynamicWhereClause(TestContext entities, string searchValue)
        {
            // simple method to dynamically plugin a where clause
            var predicate = PredicateBuilder.New<Person>(true); // true -where(true) return all
            if (String.IsNullOrWhiteSpace(searchValue) == false)
            {
                // as we only have 2 cols allow the user type in name 'firstname lastname' then use the list to search the first and last name of dbase
                var searchTerms = searchValue.Split(' ').ToList().ConvertAll(x => x.ToLower());

                predicate = predicate.Or(s => searchTerms.Any(srch => s.Firstname.ToLower().Contains(srch)));
                predicate = predicate.Or(s => searchTerms.Any(srch => s.Lastname.ToLower().Contains(srch)));
            }
            return predicate;
        }
    }
}