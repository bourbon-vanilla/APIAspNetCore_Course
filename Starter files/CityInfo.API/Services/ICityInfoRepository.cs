using CityInfo.API.Contexts;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{
    public interface ICityInfoRepository
    {
        IEnumerable<City> GetCities();

        City GetCity(int cityId, bool includePointsOfInterest);

        IEnumerable<PointOfInterest> GetPointsOfInterestForCity(int cityId);

        PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterstId);

        bool CityExists(int cityId);

        void AddPointOfInterestForCity(int cityId, PointOfInterest newPointOfInterestEntity);

        void UpdatePointOfInterestForTheCity(int cityId, PointOfInterestForUpdateDto pointOfInterest);

        void Delete(PointOfInterest pointOfInterestEntity);

        bool Save();
    }


    internal class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _dbContext;

        public CityInfoRepository(CityInfoContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }


        public IEnumerable<City> GetCities()
        {
            return _dbContext.Cities
                .OrderBy(x => x.Name)
                .ToList();
        }

        public City GetCity(int cityId, bool includePointsOfInterest)
        {
            IQueryable<City> query = _dbContext.Cities;
            
            if (includePointsOfInterest)
                query = query.Include(city => city.PointsOfInterest);

            return query.FirstOrDefault(city => city.Id == cityId);
        }

        public PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterstId)
        {
            return _dbContext.PointsOfInterest
                .FirstOrDefault(poi => poi.CityId == cityId && poi.Id == pointOfInterstId);
        }

        public IEnumerable<PointOfInterest> GetPointsOfInterestForCity(int cityId)
        {
            return _dbContext.PointsOfInterest
                .Where(poi => poi.CityId == cityId)
                .ToList();
        }

        public bool CityExists(int cityId)
        {
            return _dbContext.Cities.Any(x => x.Id == cityId);
        }

        public void AddPointOfInterestForCity(int cityId, PointOfInterest newPointOfInterestEntity)
        {
            var city = GetCity(cityId, false);
            city.PointsOfInterest.Add(newPointOfInterestEntity);
        }

        public void UpdatePointOfInterestForTheCity(int cityId, PointOfInterestForUpdateDto pointOfInterest)
        {

        }

        public void Delete(PointOfInterest pointOfInterestEntity)
        {
            _dbContext.PointsOfInterest.Remove(pointOfInterestEntity);
        }

        public bool Save()
        {
            return _dbContext.SaveChanges() > 0;
        }
    }
}
