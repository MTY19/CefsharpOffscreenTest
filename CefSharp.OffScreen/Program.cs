using CefSharp.OffScreen;
using CefSharp.OffScreen.Helper;
using CefSharp.OffScreen.Models;
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
                    _ = await ChromiumWebBrowserHelper.Login(browser, "johngerson808@gmail.com", "test8008");

                    #region Model S

                    //For browser render
                    await Task.Delay(5000);

                    //seach vehicle 
                    //Select "used cars" then select " tesla" then "model s" then max price " 100K"
                    //then set distance to "all miles from" then set the zipcode to be 94596
                    _ = await ChromiumWebBrowserHelper.SearchVehicle(browser, "used", "tesla",
                        "tesla-model_s", 100000, "all", 94596);

                    //wait browser render
                    await Task.Delay(5000);

                    //first page collect vehicle list data
                    var scriptFirstPage = @"(function (){
                      
                        var arr = Array.from(document.getElementsByClassName('vehicle-badging')).map(x => (                             
                                 x.getAttribute('data-override-payload')
                        ));
        
                        return arr;
                     })();";

                    //get first page vehicle data list
                    List<Vehicle> firstPageList = await ChromiumWebBrowserHelper.GetVehicleList(browser, scriptFirstPage);
                    firstVehicleList.AddRange(firstPageList);

                    await Task.Delay(5000);

                    //go to next page
                    var goNextPage = @"document.querySelector('#next_paginate').click();";
                    _ = await browser.EvaluateScriptAsync(goNextPage);

                    await Task.Delay(5000);

                    //second page collect data
                    var scriptSecondPage = @"(function (){
                      
                        var arr = Array.from(document.getElementsByClassName('vehicle-badging')).map(x => (                             
                                 x.getAttribute('data-override-payload')
                        ));
        
                        return arr;
                     })();";

                    //get second page vehicle data list
                    List<Vehicle> secondPageList = await ChromiumWebBrowserHelper.GetVehicleList(browser, scriptSecondPage);
                    firstVehicleList.AddRange(secondPageList);

                    await Task.Delay(5000);

                    //get random car from list 
                    Random rnd = new Random();
                    int randomCar = rnd.Next(0, firstVehicleList.Count - 1);
                    //Choose a specific car
                    browser.LoadUrl(carDetailUrl + firstVehicleList[randomCar].listing_id);

                    await Task.Delay(6000);

                    var scriptFirstCarDetail = @"(function (){

                        let vehicle = CARS['initialActivity']; 

                        return vehicle;
                     })();";

                    //collect specific car data
                    JavascriptResponse responseFirstCarDetail = await browser.EvaluateScriptAsync(scriptFirstCarDetail);
                    string firstCarDetailJson = JsonConvert.SerializeObject(responseFirstCarDetail.Result, JsonSerializeSettings);
                    firstVehicleDetail = JsonConvert.DeserializeObject<VehicleDetail>(firstCarDetailJson, JsonSerializeSettings);

                    await Task.Delay(5000);
                    //for the home delivery
                    //go main page
                    browser.LoadUrl("https://www.cars.com");
                    await Task.Delay(5000);

                    //seach vehicle 
                    //Select "used cars" then select " tesla" then "model s" then max price " 100K"
                    //then set distance to "all miles from" then set the zipcode to be 94596
                    _ = await ChromiumWebBrowserHelper.SearchVehicle(browser, "used", "tesla",
                     "tesla-model_s", 100000, "all", 94596);

                    await Task.Delay(5000);

                    //filter home delivery
                    var homeDeliveryCheck = @"document.querySelector('#mobile_home_delivery_true').checked = true;";
                    _ = await browser.EvaluateScriptAsync(homeDeliveryCheck);

                    await Task.Delay(5000);

                    //collect data home delivery data
                    //get second page vehicle data list
                    firstVehicleHomeDeliveryList = await ChromiumWebBrowserHelper.GetVehicleList(browser, scriptSecondPage);

                    #endregion

                    #region Model X

                    //go to main page    
                    browser.LoadUrl("https://www.cars.com");
                    //For browser render
                    await Task.Delay(5000);

                    //seach vehicle 
                    //Select "used cars" then select " tesla" then "model s" then max price " 100K"
                    //then set distance to "all miles from" then set the zipcode to be 94596
                    _ = await ChromiumWebBrowserHelper.SearchVehicle(browser, "used", "tesla",
                     "tesla-model_x", 100000, "all", 94596);

                    //wait browser render
                    await Task.Delay(5000);

                    //first page collect vehicle list data
                    //get first page vehicle data list
                    List<Vehicle> firstXPageList = await ChromiumWebBrowserHelper.GetVehicleList(browser, scriptFirstPage);
                    secondVehicleList.AddRange(firstXPageList);

                    await Task.Delay(5000);

                    //go to next page
                    _ = await browser.EvaluateScriptAsync(goNextPage);

                    await Task.Delay(5000);

                    //get second page vehicle data list
                    List<Vehicle> secondXPageList = await ChromiumWebBrowserHelper.GetVehicleList(browser, scriptSecondPage);
                    secondVehicleList.AddRange(secondXPageList);

                    await Task.Delay(5000);

                    //get random car from list 
                    Random rndom = new Random();
                    int randomSecondCar = rndom.Next(0, secondVehicleList.Count - 1);
                    //Choose a specific car
                    browser.LoadUrl(carDetailUrl + secondVehicleList[randomSecondCar].listing_id);

                    await Task.Delay(6000);

                    var scriptSecondCarDetail = @"(function (){

                        let vehicle = CARS['initialActivity']; 

                        return vehicle;
                     })();";

                    //collect specific car data
                    JavascriptResponse responseSecondCarDetail = await browser.EvaluateScriptAsync(scriptSecondCarDetail);
                    string secondCarDetailJson = JsonConvert.SerializeObject(responseSecondCarDetail.Result, JsonSerializeSettings);
                    secondVehicleDetail = JsonConvert.DeserializeObject<VehicleDetail>(secondCarDetailJson, JsonSerializeSettings);

                    await Task.Delay(5000);
                    //for the home delivery
                    //go main page
                    browser.LoadUrl("https://www.cars.com");
                    await Task.Delay(5000);

                    //seach vehicle 
                    //Select "used cars" then select " tesla" then "model s" then max price " 100K"
                    //then set distance to "all miles from" then set the zipcode to be 94596
                    _ = await ChromiumWebBrowserHelper.SearchVehicle(browser, "used", "tesla",
                     "tesla-model_x", 100000, "all", 94596);
                    //_ = await browser.EvaluateScriptAsync(setModelXSearch);

                    await Task.Delay(5000);

                    //filter home delivery
                    _ = await browser.EvaluateScriptAsync(homeDeliveryCheck);

                    await Task.Delay(5000);

                    //collect data home delivery data
                    //get second page vehicle data list
                    secondVehicleHomeDeliveryList = await ChromiumWebBrowserHelper.GetVehicleList(browser, scriptSecondPage);

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

  

    }
}
