﻿namespace VehicleCostsMonitor.Web.Areas.Vehicle.Controllers
{
    using Areas.Vehicle.Models;
    using AutoMapper;
    using Infrastructure.Collections;
    using Infrastructure.Filters;
    using Infrastructure.Providers.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Services.Interfaces;
    using Services.Models.Entries.Interfaces;
    using Services.Models.Vehicle;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using VehicleCostsMonitor.Common.Notifications;
    using VehicleCostsMonitor.Models;
    using static VehicleCostsMonitor.Models.Common.ModelConstants;
    using static WebConstants;

    [Authorize]
    [Route("[area]/[action]/{id?}")]
    public class VehicleController : BaseVehicleController
    {
        private readonly UserManager<User> userManager;
        private readonly IVehicleService vehicles;
        private readonly IManufacturerService manufacturers;
        private readonly IVehicleModelService models;
        private readonly IVehicleElementService vehicleElements;
        private readonly IDateTimeProvider dateTimeProvider;

        public VehicleController(
            UserManager<User> userManager,
            IVehicleService vehicles,
            IManufacturerService manufacturers,
            IVehicleModelService models,
            IVehicleElementService vehicleElements,
            IDateTimeProvider dateTimeProvider)
        {
            this.userManager = userManager;
            this.vehicles = vehicles;
            this.manufacturers = manufacturers;
            this.models = models;
            this.vehicleElements = vehicleElements;
            this.dateTimeProvider = dateTimeProvider;
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetModelsByManufacturerId(int manufacturerId)
        {
            var models = await this.models.GetByManufacturerIdAsync(manufacturerId);

            return Json(new SelectList(models));
        }
        
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await this.InitializeCreationModel();

            return View(model);
        }

        [HttpPost]
        [ValidateModelState]
        public async Task<IActionResult> Create(VehicleCreateViewModel model)
        {
            model.UserId = this.userManager.GetUserId(User);

            var serviceModel = Mapper.Map<VehicleCreateServiceModel>(model);

            var newVehicleId = await this.vehicles.CreateAsync(serviceModel);

            return RedirectToAction(nameof(Details), new { id = newVehicleId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id, int pageIndex = 1)
        {
            var vehicle = await this.vehicles.GetAsync(id);
            if (vehicle == null)
            {
                this.ShowNotification(NotificationMessages.VehicleDoesNotExist);
                return RedirectToHome();
            }
            
            var model = this.InitializeDetailedModel(vehicle, pageIndex);
            
            return View(model);
        }

        [HttpGet]
        [EnsureOwnership]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await this.vehicles.GetForUpdateAsync(id);
            if (vehicle == null)
            {
                this.ShowNotification(NotificationMessages.VehicleDoesNotExist);
                this.RedirectToHome();
            }

            var model = await this.InitializeEditionModel(Mapper.Map<VehicleUpdateViewModel>(vehicle));

            return View(model);
        }

        [HttpPost]
        [ValidateModelState]
        [EnsureOwnership]
        public async Task<IActionResult> Edit(VehicleUpdateViewModel model)
        {
            var serviceModel = Mapper.Map<VehicleUpdateServiceModel>(model);

            var success = await this.vehicles.UpdateAsync(serviceModel);
            if (!success)
            {
                this.ShowNotification(NotificationMessages.InvalidOperation);
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }

            this.ShowNotification(NotificationMessages.VehicleUpdatedSuccessfull, NotificationType.Success);
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpGet]
        [EnsureOwnership]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await this.vehicles.GetAsync(id);
            if (model == null)
            {
                this.ShowNotification(NotificationMessages.VehicleDoesNotExist);
                this.RedirectToHome();
            }

            return View(model);
        }

        [HttpPost]
        [ActionName(nameof(Delete))]
        [EnsureOwnership]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await this.vehicles.DeleteAsync(id);
            if (!success)
            {
                this.ShowNotification(NotificationMessages.InvalidOperation);
                return RedirectToAction(nameof(Delete), new { id });
            }

            this.ShowNotification(NotificationMessages.VehicleDeletedSuccessfull, NotificationType.Success);
            return RedirectToAction("index", "profile", new { area = "user", id = this.userManager.GetUserId(User) });
        }

        [ResponseCache(Duration = 3600)]
        private async Task<VehicleCreateViewModel> InitializeCreationModel()
        {
            var allManufacturers = await this.manufacturers.AllAsync();
            var allVehicleTypes = await this.vehicleElements.GetVehicleTypes();
            var allFuelTypes = await this.vehicleElements.GetFuelTypes();
            var allGearingTypes = await this.vehicleElements.GetGearingTypes();
            var allAvailableYears = Enumerable
                .Range(YearOfManufactureMinValue, this.dateTimeProvider.GetCurrentDateTime().Year - YearOfManufactureMinValue + 1)
                .Select(y => y.ToString());

            var model = new VehicleCreateViewModel
            {
                AllManufacturers = allManufacturers.Select(m => new SelectListItem(m.Name, m.Id.ToString())),
                AllVehicleTypes = allVehicleTypes.Select(m => new SelectListItem(m.Name, m.Id.ToString())),
                AllFuelTypes = allFuelTypes.Select(m => new SelectListItem(m.Name, m.Id.ToString())),
                AllGearingTypes = allGearingTypes.Select(m => new SelectListItem(m.Name, m.Id.ToString())),
                AvailableYears = allAvailableYears.Select(y => new SelectListItem(y, y))
            };

            return model;
        }

        [ResponseCache(Duration = 3600)]
        private async Task<VehicleUpdateViewModel> InitializeEditionModel(VehicleUpdateViewModel model)
        {
            var allManufacturers = await this.manufacturers.AllAsync();
            var allVehicleTypes = await this.vehicleElements.GetVehicleTypes();
            var allFuelTypes = await this.vehicleElements.GetFuelTypes();
            var allGearingTypes = await this.vehicleElements.GetGearingTypes();
            var allAvailableYears = Enumerable
                .Range(YearOfManufactureMinValue, this.dateTimeProvider.GetCurrentDateTime().Year - YearOfManufactureMinValue + 1)
                .Select(y => y.ToString());
            
            model.AllManufacturers = allManufacturers.Select(m => new SelectListItem(m.Name, m.Id.ToString()));
            model.AllVehicleTypes = allVehicleTypes.Select(m => new SelectListItem(m.Name, m.Id.ToString()));
            model.AllFuelTypes = allFuelTypes.Select(m => new SelectListItem(m.Name, m.Id.ToString()));
            model.AllGearingTypes = allGearingTypes.Select(m => new SelectListItem(m.Name, m.Id.ToString()));
            model.AvailableYears = allAvailableYears.Select(y => new SelectListItem(y, y));

            return model;
        }
        
        private VehicleDetailsViewModel InitializeDetailedModel(VehicleDetailsServiceModel vehicle, int pageIndex)
        {
            var allEntries = vehicle
                .FuelEntries
                .Cast<IEntryModel>()
                .Concat(vehicle.CostEntries
                    .Cast<IEntryModel>())
                .OrderByDescending(e => e.DateCreated)
                .ToList();

            pageIndex = Math.Max(1, pageIndex);
            var totalPages = (int)(Math.Ceiling(allEntries.Count() / (double)EntriesListPageSize));
            pageIndex = Math.Min(pageIndex, Math.Max(1, totalPages));

            var costs = vehicle.CostEntries.GroupBy(e => e.ToString()).ToDictionary(x => x.Key, y => y.Sum(e => e.Price));
            costs.Add("Fuel", vehicle.FuelEntries.Sum(fe => fe.Price));
            var routes = vehicle.FuelEntries.SelectMany(fe => fe.Routes).GroupBy(r => r).ToDictionary(x => x.Key, x => x.Count());

            var minConsumption = vehicle.FuelEntries.Any(fe => fe.Average.Value > 0) 
                ? vehicle.FuelEntries.Where(fe => fe.Average > 0).Min(fe => fe.Average.Value) 
                : 0;

            var maxConsumption = vehicle.FuelEntries.Any(fe => fe.Average.Value > 0) 
                ? vehicle.FuelEntries.Where(fe => fe.Average > 0).Max(fe => fe.Average.Value) 
                : 0;

            var step = (maxConsumption - minConsumption) / ConsumptionHistogramRangesCount;
            
            var model = Mapper.Map<VehicleDetailsViewModel>(vehicle);
            model.Stats = new Statistics
            {
                Costs = costs,
                Routes = routes,
                MinConsumption = minConsumption,
                MaxConsumption = maxConsumption,
                ConsumptionRanges = new List<ConsumptionInRange>()
            };

            for (int i = 0; i < ConsumptionHistogramRangesCount; i++)
            {
                model.Stats.ConsumptionRanges.Add(new ConsumptionInRange
                {
                    From = minConsumption,
                    To = minConsumption + step,
                    Count = vehicle.FuelEntries.Count(fe => fe.Average > 0 && fe.Average >= minConsumption && fe.Average <= minConsumption + step)
                });

                minConsumption += step;
            }

            model.Stats.MileageByDate = vehicle.FuelEntries
                .OrderByDescending(fe => fe.DateCreated)
                .Take(MileageByDateItemsCount)
                .Select(fe => new MileageByDate
                {
                    Date = fe.DateCreated,
                    Consumption = fe.Average.HasValue ? fe.Average.Value : default(double)
                })
                .OrderBy(m => m.Date);


            var entriesToShow = allEntries
                .Skip((pageIndex - 1) * EntriesListPageSize)
                .Take(EntriesListPageSize)
                .ToList();
            
            model.Entries = new PaginatedList<IEntryModel>(entriesToShow, pageIndex, totalPages);
            return model;
        }

    }
}