using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataTableTest.Models
{
	public class PaymentLog
	{
		public long ID { get; set; }
		public string PAYMENT_ID { get; set; }
		public string PAYMENT_SOURCE { get; set; }
		public string ACTION_DATE { get; set; }
		public string STATUS { get; set; }
		public string SERVICE_CODE { get; set; }
		public string SERVICE_DESCRIPRION { get; set; }
		public string PAYMENT_BATCH { get; set; }
		public string MODULE_NAME { get; set; }
		public string SERVICE_NAME { get; set; }
		public string TRANSACTION_TYPE { get; set; }

	}
}