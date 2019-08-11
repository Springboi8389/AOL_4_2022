﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WeatherNet;
using WeatherNet.Clients;

namespace aol.Classes
{
    public class wData
    {
        public string ip;
        public string type;
        public string continent_code;
        public string continent_name;
        public string country_code;
        public string country_name;
        public string region_code;
        public string region_name;
        public string city;
        public string zip;
        public string latitude;
        public string longitude;
        public List<loc> location;
        public string country_flag;
        public string country_flag_emoji;
        public string country_flag_emoji_unicode;
        public string calling_code;
        public string is_eu;
    }

    public class loc
    {
        public string geoname_id;
        public string capital;
        public List<langs> languages;
    }

    public class langs
    {
        public string code;
        public string name;
        public string native;
    }

    class location
    {
        public static string getIP()
        {
            string ip = "";
            using (WebClient wc = new WebClient())
            {
                ip = wc.DownloadString("https://ipv4.icanhazip.com/");
            }
            return ip;
        }

        public static List<string> getCityState()
        {
            string json = "";
            string apiURL = "http://api.ipstack.com/" + getIP() + "?access_key=7d7a9198de3b50a37caf5115c63fb4ec&format=1";

            List<string> tmpList = new List<string>();

            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString(apiURL);
            }

            JToken token = JObject.Parse(json);

            tmpList.Add((string)token.SelectToken("city"));
            tmpList.Add((string)token.SelectToken("region_name")); // region_code = FL
            tmpList.Add((string)token.SelectToken("zip"));

            return tmpList;
        }

        public static string getWeather()
        {
            ClientSettings.ApiUrl = "http://api.openweathermap.org/data/2.5";
            ClientSettings.ApiKey = "d8f6deea88bb177513cc8a14cf629020";

            List<string> cityDat = getCityState();

            var result = CurrentWeather.GetByCityName(cityDat[0], cityDat[1], "en", "imperial");

            var t = result.Item.Title;
            var t2 = Math.Round(result.Item.Temp);

            return t + " " + t2.ToString();
        }
    }
}