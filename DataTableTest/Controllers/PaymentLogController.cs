using DataTableTest.Models;
using DataTableTest.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataTableTest.Controllers
{
    public class PaymentLogController : Controller
    {
        // GET: PaymentLog
        public ActionResult Index()
        {
            return View();
        }
		public JsonResult GetPayment(DataTableAjaxPostModel model)
		{
			// action inside a standard controller
			int filteredResultsCount;
			long totalResultsCount;
			var res = PaymentSearchFunc(model, out filteredResultsCount, out totalResultsCount);

			//var result = new List<Payment>(res.Count);
			//foreach (var s in res)
			//{
			//	// simple remapping adding extra info to found dataset
			//	result.Add(new PaymentLog
			//	{
			//		ACCOUNT_ID = s.ACCOUNT_ID,
			//		BENEFICIARY = s.BENEFICIARY,
			//	});
			//};

			return Json(new
			{
				// this is what datatables wants sending back
				draw = model.draw,
				recordsTotal = totalResultsCount,
				recordsFiltered = filteredResultsCount,
				data = res
			});
		}

		public IList<object> PaymentSearchFunc(DataTableAjaxPostModel model, out int filteredResultsCount, out long totalResultsCount)
		{
			var searchBy = (model.search != null) ? model.search.value : null;
			var take = model.length;
			var skip = model.start;

			string sortBy = "";
			bool sortDir = true;

			if (model.order != null)
			{
				// in this example we just default sort on the 1st column
				sortBy = model.columns[model.order[0].column].data;
				sortDir = model.order[0].dir.ToLower() == "asc";
			}

			// search the dbase taking into consideration table sorting and paging
			var conn = ConfigurationManager.ConnectionStrings["PrimusContext"].ToString();
			var result = new AdoService<PaymentLog>(conn, "PRI_PAYMENT_LOG").GetPaymentLogsFromDB(searchBy, take, skip, sortBy, sortDir, out filteredResultsCount, out totalResultsCount);
			if (result == null)
			{
				// empty collection...
				return new List<object>();
			}
			return result;
		}

	}
}