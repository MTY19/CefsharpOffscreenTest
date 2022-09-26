using CefSharp.OffScreen.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.OffScreen.Helper
{
    public static class ChromiumWebBrowserHelper
    {
        //get vehicle list async
        public static async Task<List<Vehicle>> GetVehicleList(ChromiumWebBrowser browser, String script)
        {
            List<Vehicle> vehicleList = new List<Vehicle>();

            var JsonSerializeSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            JavascriptResponse response = await browser.EvaluateScriptAsync(script);

            dynamic arrayPage = response.Result;

            foreach (dynamic obj in arrayPage)
            {
                if (obj != null)
                {
                    vehicleList.Add(JsonConvert.DeserializeObject<Vehicle>(obj, JsonSerializeSettings));
                }
            }

            return vehicleList;
        }

        //login operation
        public static async Task<JavascriptResponse> Login(ChromiumWebBrowser browser, string username, string password)
        {
            //login operations
            var loginScript = $@"document.querySelector('[name=""user[email]""]').value = '{username}';
                                  document.querySelector('[name=""user[password]""]').value = '{password}';
                                  document.querySelector('button[type=submit]').click();";
            JavascriptResponse result = await browser.EvaluateScriptAsync(loginScript);

            return result;
        }

        //search vehicle
        public static async Task<JavascriptResponse> SearchVehicle(ChromiumWebBrowser browser,
            string stocktype, string makes, string models, int price, string distance, int zip)
        {
            //set values and search 
            var setModelSSearch = $@"document.querySelector('#make-model-search-stocktype').value = '{stocktype}';
                                    document.querySelector('#makes').value = '{makes}';
                                    document.querySelector('#models').value = '{models}';
                                    document.querySelector('#make-model-max-price').value = '{price}';
                                    document.querySelector('#make-model-maximum-distance').value = '{distance}';
                                    document.querySelector('#make-model-zip').value = {zip};
                                    document.querySelector('.sds-home-search__submit button[type=submit]').click();";

            JavascriptResponse result = await browser.EvaluateScriptAsync(setModelSSearch);

            return result;
        }
    }
}
