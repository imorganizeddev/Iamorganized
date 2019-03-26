using System;
using System.Net;
using System.IO;
using System.Text;
using Android.Widget;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Stripe;
using Newtonsoft.Json.Linq;

namespace IMInventory
{
	public class Payment
	{
		public Payment ()
		{
		}
        GeneralValues objGeneralValues = new GeneralValues();
		
        public bool CancelSubscription(string subscriptionid, string customerid) {
            bool success = false;
            var webrequest = (HttpWebRequest)WebRequest.Create("https://api.stripe.com/v1/customers/" + customerid + "/subscriptions/"+subscriptionid);
            webrequest.Method = "DELETE";
            webrequest.Headers.Add("Authorization", "Bearer sk_test_SuM74xuNwHHVfbN8kBuZlmmO");
            StreamReader reader = null;
            string stripeResponse;
            try
            {
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                StreamReader responseStream = new StreamReader(webresponse.GetResponseStream());
                string obj = responseStream.ReadToEnd();
                var parsedjson = JObject.Parse(obj);
                subscriptionid = parsedjson["id"].ToString();
                success = true;
            }
            catch { }
            return success;
        
        }

        public bool GetSubscriptionStatus(string customerid, string subscriptionid)
        {

            bool success = false;
            var webrequest = (HttpWebRequest)WebRequest.Create("https://api.stripe.com/v1/customers/" + customerid + "/subscriptions/" + subscriptionid);
            webrequest.Method = "GET";
            webrequest.Headers.Add("Authorization", "Bearer "+objGeneralValues.SecretKey);
           
            try
            {
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                StreamReader responseStream = new StreamReader(webresponse.GetResponseStream());
                string obj = responseStream.ReadToEnd();
                var parsedjson = JObject.Parse(obj);
                string status = parsedjson["status"].ToString();
                if (status.Trim() == "active")
                {
                    success = true;
                }
                
            }
            catch { }
            return success;

        }
	}
}

