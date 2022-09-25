using CefSharp.OffScreen;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CefSharp.OffScreen
{
    /// <summary>
    /// CefSharp.OffScreen task https://billowy-heron-cf0.notion.site/c-coding-test-04c1ca6d187942adaa04a155bf0a0633
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            #if ANYCPU
                //Only required for PlatformTarget of AnyCPU
                CefRuntime.SubscribeAnyCpuAssemblyResolver();
            #endif

            const string mainUrl = "https://www.cars.com/signin/";
            const string carDetailUrl = "https://www.cars.com/vehicledetail/";
            //Model S definitions
            List<Vehicle> firstVehicleList = new List<Vehicle>();
            List<Vehicle> firstVehicleHomeDeliveryList = new List<Vehicle>();
            VehicleDetail firstVehicleDetail = new VehicleDetail();
            //Model X definitions
            List<Vehicle> secondVehicleList = new List<Vehicle>();
            List<Vehicle> secondVehicleHomeDeliveryList = new List<Vehicle>();
            VehicleDetail secondVehicleDetail = new VehicleDetail();
            var JsonSerializeSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            AsyncContext.Run(async delegate
            {
                var settings = new CefSettings()
                {
                    //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                    CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
                };

                //Perform dependency check to make sure all relevant resources are in our output directory.
                var success = await Cef.InitializeAsync(settings, performDependencyCheck: true, browserProcessHandler: null);

                if (!success)
                {
                    throw new Exception("Unable to initialize CEF, check the log file.");
                }

                // Create the CefSharp.OffScreen.ChromiumWebBrowser instance
                using (var browser = new ChromiumWebBrowser(mainUrl))
                {
                    var initialLoadResponse = await browser.WaitForInitialLoadAsync();

                    if (!initialLoadResponse.Success)
                    {
                        throw new Exception(string.Format("Page load failed with ErrorCode:{0}, HttpStatusCode:{1}", initialLoadResponse.ErrorCode, initialLoadResponse.HttpStatusCode));
                    }

                    //login operations
                    var loginScript = @"document.querySelector('[name=""user[email]""]').value = 'johngerson808@gmail.com';
                                  document.querySelector('[name=""user[password]""]').value = 'test8008';
                                  document.querySelector('button[type=submit]').click();";
                    _ = await browser.EvaluateScriptAsync(loginScript);

                    #region Model S

                    //For browser render
                    await Task.Delay(3500);

                    //seach vehicle 
                    //Select "used cars" then select " tesla" then "model s" then max price " 100K"
                    //then set distance to "all miles from" then set the zipcode to be 94596
                    var setModelSSearch = @"document.querySelector('#make-model-search-stocktype').value = 'used';
                                    document.querySelector('#makes').value = 'tesla';
                                    document.querySelector('#models').value = 'tesla-model_s';
                                    document.querySelector('#make-model-max-price').value = '100000';
                                    document.querySelector('#make-model-maximum-distance').value = 'all';
                                    document.querySelector('#make-model-zip').value = 94596;
                                    document.querySelector('.sds-home-search__submit button[type=submit]').click();";

                    _ = await browser.EvaluateScriptAsync(setModelSSearch);

                    //wait browser render
                    await Task.Delay(3500);

                    //first page collect vehicle list data
                    var scriptFirstPage = @"(function (){
                      
                        var arr = Array.from(document.getElementsByClassName('vehicle-badging')).map(x => (                             
                                 x.getAttribute('data-override-payload')
                        ));
        
                        return arr;
                     })();";

                    //get first page vehicle data list
                    List<Vehicle> firstPageList = await GetVehicleList(browser, scriptFirstPage);
                    firstVehicleList.AddRange(firstPageList);

                    await Task.Delay(3500);

                    //go to next page
                    var goNextPage = @"document.querySelector('#next_paginate').click();";
                    _ = await browser.EvaluateScriptAsync(goNextPage);

                    await Task.Delay(3500);

                    //second page collect data
                    var scriptSecondPage = @"(function (){
                      
                        var arr = Array.from(document.getElementsByClassName('vehicle-badging')).map(x => (                             
                                 x.getAttribute('data-override-payload')
                        ));
        
                        return arr;
                     })();";

                    //get second page vehicle data list
                    List<Vehicle> secondPageList = await GetVehicleList(browser, scriptSecondPage);
                    firstVehicleList.AddRange(secondPageList);

                    await Task.Delay(3500);

                    //get random car from list 
                    Random rnd = new Random();
                    int randomCar = rnd.Next(0, firstVehicleList.Count - 1);
                    //Choose a specific car
                    browser.LoadUrl(carDetailUrl + firstVehicleList[randomCar].listing_id);

                    await Task.Delay(3500);

                    var scriptFirstCarDetail = @"(function (){

                        let vehicle = CARS['initialActivity']; 

                        return vehicle;
                     })();";

                    //collect specific car data
                    JavascriptResponse responseFirstCarDetail = await browser.EvaluateScriptAsync(scriptFirstCarDetail);
                    string firstCarDetailJson = JsonConvert.SerializeObject(responseFirstCarDetail.Result, JsonSerializeSettings);
                    firstVehicleDetail = JsonConvert.DeserializeObject<VehicleDetail>(firstCarDetailJson, JsonSerializeSettings);

                    await Task.Delay(3500);
                    //for the home delivery
                    //go main page
                    browser.LoadUrl("https://www.cars.com");
                    await Task.Delay(3500);

                    //seach vehicle 
                    //Select "used cars" then select " tesla" then "model s" then max price " 100K"
                    //then set distance to "all miles from" then set the zipcode to be 94596
                    _ = await browser.EvaluateScriptAsync(setModelSSearch);

                    await Task.Delay(3500);

                    //filter home delivery
                    var homeDeliveryCheck = @"document.querySelector('#mobile_home_delivery_true').checked = true;";
                    _ = await browser.EvaluateScriptAsync(homeDeliveryCheck);

                    await Task.Delay(3500);

                    //collect data home delivery data
                    //get second page vehicle data list
                    firstVehicleHomeDeliveryList = await GetVehicleList(browser, scriptSecondPage);

                    #endregion

                    #region Model X

                    //go to main page    
                    browser.LoadUrl("https://www.cars.com");
                    //For browser render
                    await Task.Delay(3500);

                    //seach vehicle 
                    //Select "used cars" then select " tesla" then "model s" then max price " 100K"
                    //then set distance to "all miles from" then set the zipcode to be 94596
                    var setModelXSearch = @"document.querySelector('#make-model-search-stocktype').value = 'used';
                                    document.querySelector('#makes').value = 'tesla';
                                    document.querySelector('#models').value = 'tesla-model_x';
                                    document.querySelector('#make-model-max-price').value = '100000';
                                    document.querySelector('#make-model-maximum-distance').value = 'all';
                                    document.querySelector('#make-model-zip').value = 94596;
                                    document.querySelector('.sds-home-search__submit button[type=submit]').click();";

                    _ = await browser.EvaluateScriptAsync(setModelXSearch);

                    //wait browser render
                    await Task.Delay(3500);

                    //first page collect vehicle list data
                    //get first page vehicle data list
                    List<Vehicle> firstXPageList = await GetVehicleList(browser, scriptFirstPage);
                    secondVehicleList.AddRange(firstXPageList);

                    await Task.Delay(3500);

                    //go to next page
                    _ = await browser.EvaluateScriptAsync(goNextPage);

                    await Task.Delay(3500);

                    //get second page vehicle data list
                    List<Vehicle> secondXPageList = await GetVehicleList(browser, scriptSecondPage);
                    secondVehicleList.AddRange(secondXPageList);

                    await Task.Delay(3500);

                    //get random car from list 
                    Random rndom = new Random();
                    int randomSecondCar = rndom.Next(0, secondVehicleList.Count - 1);
                    //Choose a specific car
                    browser.LoadUrl(carDetailUrl + secondVehicleList[randomSecondCar].listing_id);

                    await Task.Delay(3500);

                    var scriptSecondCarDetail = @"(function (){

                        let vehicle = CARS['initialActivity']; 

                        return vehicle;
                     })();";

                    //collect specific car data
                    JavascriptResponse responseSecondCarDetail = await browser.EvaluateScriptAsync(scriptSecondCarDetail);
                    string secondCarDetailJson = JsonConvert.SerializeObject(responseSecondCarDetail.Result, JsonSerializeSettings);
                    secondVehicleDetail = JsonConvert.DeserializeObject<VehicleDetail>(secondCarDetailJson, JsonSerializeSettings);

                    await Task.Delay(3500);
                    //for the home delivery
                    //go main page
                    browser.LoadUrl("https://www.cars.com");
                    await Task.Delay(3500);

                    //seach vehicle 
                    //Select "used cars" then select " tesla" then "model s" then max price " 100K"
                    //then set distance to "all miles from" then set the zipcode to be 94596
                    _ = await browser.EvaluateScriptAsync(setModelXSearch);

                    await Task.Delay(3500);

                    //filter home delivery
                    _ = await browser.EvaluateScriptAsync(homeDeliveryCheck);

                    await Task.Delay(3500);

                    //collect data home delivery data
                    //get second page vehicle data list
                    secondVehicleHomeDeliveryList = await GetVehicleList(browser, scriptSecondPage);

                    #endregion

                    //complete result
                    Result result = new Result
                    {
                        firstVehicleList = firstVehicleList,
                        firstVehicleHomeDeliveryList = firstVehicleHomeDeliveryList,
                        firstVehicleDetail = firstVehicleDetail,
                        secondVehicleList = secondVehicleList,
                        secondVehicleHomeDeliveryList = secondVehicleHomeDeliveryList,
                        secondVehicleDetail = secondVehicleDetail
                    };

                    //complete json 
                    var resultJson = JsonConvert.SerializeObject(result, JsonSerializeSettings);

                    // Wait for the screenshot to be taken.
                    //var bitmapAsByteArray = await browser.CaptureScreenshotAsync();
                    // File path to save our screenshot e.g. C:\Users\{username}\Desktop\CefSharp screenshot.png
                    var resultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "result.json");

                    Console.WriteLine();
                    Console.WriteLine("Json ready. Saving to {0}", resultPath);

                    File.WriteAllText(resultPath, resultJson);

                    Console.WriteLine("Json file saved. Press key for exit !!!");
                }
                Console.ReadLine();
                // Clean up Chromium objects. You need to call this in your application otherwise
                // you will get a crash when closing.
                Cef.Shutdown();
            });

            return 0;
        }

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

        public class Result
        {
            public List<Vehicle> firstVehicleList { get; set; }
            public List<Vehicle> firstVehicleHomeDeliveryList { get; set; }
            public VehicleDetail firstVehicleDetail { get; set; }
            public List<Vehicle> secondVehicleList { get; set; }
            public List<Vehicle> secondVehicleHomeDeliveryList { get; set; }
            public VehicleDetail secondVehicleDetail { get; set; }
        }

        public class Vehicle
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string bodystyle { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool cpo_indicator { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string customer_id { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string horizontal_position { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string listing_id { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string make { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string model { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int model_year { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string msrp { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string price { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool sponsored { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string stock_type { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string trim { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int vertica_position { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string web_page_type_from { get; set; }
        }

        public class VehicleDetail
        {
            public string customer_id { get; set; }
            public string dealer_name { get; set; }
            public Vhr vhr { get; set; }
            public string trip_id { get; set; }
            public object marketing_click_data { get; set; }
            public string vertical_position { get; set; }
            public string page_type { get; set; }
            public int photo_count { get; set; }
            public string mileage { get; set; }
            public string profileAccountType { get; set; }
            public string trim { get; set; }
            public bool loginStatus { get; set; }
            public string make { get; set; }
            public string local_zone { get; set; }
            public object utm_source { get; set; }
            public string msrp { get; set; }
            public string page_name { get; set; }
            public object page_detail { get; set; }
            public string market_name { get; set; }
            public object utm_medium { get; set; }
            public string drivetrain { get; set; }
            public bool cpo_indicator { get; set; }
            public string useragent { get; set; }
            public string model { get; set; }
            public string private_seller { get; set; }
            public string url_referer { get; set; }
            public string vin { get; set; }
            public object utm_term { get; set; }
            public string designated_market_area_key { get; set; }
            public string canonical_mmt { get; set; }
            public string seller_type { get; set; }
            public string fuel_type { get; set; }
            public string stock_sub { get; set; }
            public string stock_type { get; set; }
            public string listing_id { get; set; }
            public object utm_campaign { get; set; }
            public string canonical_mmty { get; set; }
            public List<string> page_features { get; set; }
            public string lat_long { get; set; }
            public string page_key { get; set; }
            public string interior_color { get; set; }
            public string search_instance_id { get; set; }
            public object price_badge { get; set; }
            public string platform_id { get; set; }
            public bool sponsored { get; set; }
            public string aff_code { get; set; }
            public List<string> badges { get; set; }
            public string zip_ip2geo { get; set; }
            public string dealer_zip { get; set; }
            public string page_channel { get; set; }
            public object utm_content { get; set; }
            public string url_base { get; set; }
            public string partner_name { get; set; }
            public string price { get; set; }
            public string exterior_color { get; set; }
            public string dealer_dma { get; set; }
            public string bodystyle { get; set; }
            public string marketing_click_id { get; set; }
            public string marketing_referral_source { get; set; }
            public object seller_id { get; set; }
            public string cat { get; set; }
            public string year { get; set; }
        }

        public class Vhr
        {
            public bool accidents_or_damage { get; set; }
            public bool one_owner_vehicle { get; set; }
            public bool open_recall { get; set; }
            public bool personal_use_only { get; set; }
            public string vhr_type { get; set; }
        }
    }
}
