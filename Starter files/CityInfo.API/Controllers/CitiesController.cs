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
        [HttpGet]
        public IActionResult GetCities()
        {
            return Ok(CitiesDataStore.Current.Cities);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity(int id)
        {
            var foundCity = CitiesDataStore.Current.Cities
                .FirstOrDefault(x => x.Id == id);

            if (foundCity == null)
                return NotFound();

            return Ok(foundCity);
        }
    }
}
