using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Android.Net;
using Javax.Net.Ssl;
using Xamarin.Android.Net;
using System.Collections.Generic;
using System.Linq;

namespace HelloAppWidget
{

    public class VattenfallSpotPrice
    {
        
        public string TimeStampHour { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
    }

    public interface IDataFromApi
    {
        Task<IEnumerable<VattenfallSpotPrice>> GetDataFromVattenFallAsync();
    }

    public class DataApi : IDataFromApi
    {
		private readonly System.Net.Http.HttpClient _client;
         
        public DataApi()
        {
            _client = new HttpClient(new IgnoreSSLClientHandler());

        }

        public async Task<IEnumerable<VattenfallSpotPrice>> GetDataFromVattenFallAsync()
        {
            try
            {
                string spotPricesUrl = "https://www.vattenfall.se/api/price/spot/pricearea/2019-01-07/2019-01-07/SN4";

                var json = await _client.GetStringAsync(spotPricesUrl);

                var model = JsonConvert.DeserializeObject<List<VattenfallSpotPrice>>(json);

                return model.ToList();
            }
            catch(Exception exception)
            {
                return null;
            }


        }
     
        internal class IgnoreSSLClientHandler : AndroidClientHandler
        {
            [Obsolete]
            protected override SSLSocketFactory ConfigureCustomSSLSocketFactory(HttpsURLConnection connection)
            {
                return SSLCertificateSocketFactory.GetInsecure(1000, null);
            }

            protected override IHostnameVerifier GetSSLHostnameVerifier(HttpsURLConnection connection)
            {
                return new IgnoreSSLHostnameVerifier();
            }
        }

        internal class IgnoreSSLHostnameVerifier : Java.Lang.Object, IHostnameVerifier
        {
            public bool Verify(string hostname, ISSLSession session)
            {
                return true;
            }
        }
        
    }
		

}
