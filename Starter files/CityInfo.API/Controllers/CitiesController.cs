using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Produces("application/json", "application/xml")]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? 
                throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? 
                throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet]
        public IActionResult GetCities()
        {
            var cityEntities = _cityInfoRepository.GetCities();

            var cityDtos = _mapper
                .Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities);

            return Ok(cityDtos);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity(int id, bool includePointsOfInterest = false)
        {
            var cityEntity = _cityInfoRepository
                .GetCity(id, includePointsOfInterest);

            if (cityEntity == null)
                return NotFound();

            return includePointsOfInterest ?
                Ok(_mapper.Map<CityDto>(cityEntity)) :
                Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(cityEntity));
        }
    }
}
