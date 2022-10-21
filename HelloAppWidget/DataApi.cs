using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace HelloAppWidget
{

    public class DataFromApi
    {
        public string Price { get; set; }
        public DateTime Date { get; set; }
    }

    public class DataApi
    {
		private readonly System.Net.Http.HttpClient _client;
         
        public DataApi()
        {
            _client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());

        }

        public async Task<DataFromApi> GetDataFromServerAsync()
        {

           // _client.GetStringAsync()

            await Task.Delay(500);

            return new DataFromApi()
            {
                Date = DateTime.Now,
                Price = new Random().Next().ToString()
            };

        }
    }
		

}
