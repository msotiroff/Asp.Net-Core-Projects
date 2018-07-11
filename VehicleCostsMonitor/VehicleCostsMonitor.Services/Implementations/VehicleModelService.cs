﻿namespace VehicleCostsMonitor.Services.Implementations
{
    using AutoMapper.QueryableExtensions;
    using Data;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Models.VehicleModel;
    using System.Linq;
    using System.Threading.Tasks;
    using VehicleCostsMonitor.Models;

    public class VehicleModelService : DataAccessService, IVehicleModelService
    {
        public VehicleModelService(JustMonitorDbContext db) 
            : base(db) { }

        public async Task<bool> CreateAsync(string modelName, int manufacturerId)
        {
            var model = new Model
            {
                Name = modelName,
                ManufactureId = manufacturerId
            };

            if (!this.ValidateModelState(model))
            {
                return false;
            }

            try
            {
                await this.db.Models.AddAsync(model);
                await this.db.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var model = await this.db.Models.FindAsync(id);
            if (model == null)
            {
                return false;
            }

            this.db.Remove(model);
            await this.db.SaveChangesAsync();

            return true;
        }

        public async Task<ModelConciseServiceModel> GetAsync(int id)
            => await this.db.Models
                .Where(m => m.Id == id)
                .ProjectTo<ModelConciseServiceModel>()
                .FirstOrDefaultAsync();
        
    }
}
