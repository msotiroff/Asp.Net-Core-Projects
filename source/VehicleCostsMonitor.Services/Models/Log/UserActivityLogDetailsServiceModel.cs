﻿namespace VehicleCostsMonitor.Services.Models.Log
{
    using Common.AutoMapping.Interfaces;
    using System;
    using System.ComponentModel.DataAnnotations;
    using VehicleCostsMonitor.Models;

    public class UserActivityLogDetailsServiceModel : IAutoMapWith<UserActivityLog>
    {
        public int Id { get; set; }
        
        [Display(Name = "Date")]
        public DateTime DateTime { get; set; }
        
        [Display(Name = "User")]
        public string UserEmail { get; set; }

        [Display(Name = "Http Method")]
        public string HttpMethod { get; set; }

        [Display(Name = "Controller")]
        public string ControllerName { get; set; }

        [Display(Name = "Action")]
        public string ActionName { get; set; }

        [Display(Name = "Area")]
        public string AreaName { get; set; }
        
        public string Url { get; set; }

        [Display(Name = "Query string")]
        public string QueryString { get; set; }

        [Display(Name = "Action arguments")]
        public string ActionArguments { get; set; }
    }
}
