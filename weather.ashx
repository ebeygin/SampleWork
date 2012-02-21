<%@ WebHandler Language="C#" Class="weather" %>

using System;
using System.Configuration;
using System.IO;
using System.Web;
using WeatherService.Web;
using WeatherService.Web.DTO;
using System.Text.RegularExpressions;

public class weather : IHttpHandler {

    private static bool? disableWeather = Convert.ToBoolean(ConfigurationManager.AppSettings["disableWeather"]);
    
    public void ProcessRequest (HttpContext context) {
        var request = context.Request;
        
        if (disableWeather.HasValue && disableWeather.Value == true)
        {
            var fiNoService = new FileInfo(HttpRuntime.AppDomainAppPath + "/images/bgs/clear.jpg");
            context.Response.ContentType = "image/jpeg";
            context.Response.WriteFile(fiNoService.FullName);
            
            return;
        }

        var fi = new FileInfo(HttpRuntime.AppDomainAppPath + "/images/bgs/clear.jpg");
        if (!string.IsNullOrEmpty(request["zip"]) && Regex.IsMatch(request["zip"], "[0-9]{5}"))
        {
            string zipcode = request["zip"];
            string current = GetCurrentConditions(zipcode);

            fi = new FileInfo(HttpRuntime.AppDomainAppPath + "/images/bgs/" + current + ".jpg");
            if (!fi.Exists)
                fi = new FileInfo(HttpRuntime.AppDomainAppPath + "/images/bgs/clear.jpg");
        }
        
        context.Response.ContentType = "image/jpeg";
        context.Response.WriteFile(fi.FullName);
               
        HttpCachePolicy cache = context.Response.Cache;
        cache.SetCacheability(HttpCacheability.Server);
        cache.VaryByParams["zip"] = true;
        cache.SetLastModifiedFromFileDependencies();
        cache.SetMaxAge(TimeSpan.FromMinutes(20));
        cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

    private string GetCurrentConditions(string zip)
    {
        var currentConditions = new CurrentConditionsDTO();
        WeatherWebService weatherService = new WeatherWebService();

        try
        {
           currentConditions = weatherService.GetCurrentConditions(zip);
        }
        catch (Exception)
        {
            //should log this
        }

        if (currentConditions == null || currentConditions.WeatherCondition == null)
            return "clear";

        return currentConditions.WeatherCondition;
    } 

}