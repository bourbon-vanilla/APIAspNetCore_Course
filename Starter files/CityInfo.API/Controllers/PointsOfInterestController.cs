using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Produces("application/json", "application/xml")]
    [Route("api/cities/{cityId}/pointsofinterest")]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        }


        [HttpGet]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            return Execute(() =>
            {
                var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);

                if (city == null)
                {
                    _logger.LogInformation("City with the cityId={cityId} was not found", cityId);
                    return NotFound();
                }
                return Ok(city.PointsOfInterest);
            });
        }

        [HttpGet("{id}", Name = "GetPointOfInterest")]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);

            if (city == null)
                return NotFound();

            var pointOfInterest = city.PointsOfInterest.FirstOrDefault(x => x.Id == id);

            if (pointOfInterest == null)
                return NotFound();

            return Ok(pointOfInterest);
        }

        [HttpPost]
        public IActionResult CreatePointOfInterests(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            if (pointOfInterest.Name == pointOfInterest.Description)
                ModelState.AddModelError(nameof(pointOfInterest.Description), "The description should be different from the name");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
                return NotFound();

            var maxIdPointsOfInterests = CitiesDataStore.Current.Cities.SelectMany(x => x.PointsOfInterest).Max(x => x.Id);

            var newPointOfInterests = new PointOfInterestDto
            {
                Id = ++maxIdPointsOfInterests,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterest.Add(newPointOfInterests);

            return CreatedAtRoute("GetPointOfInterest", 
                new { cityId, id = newPointOfInterests.Id }, newPointOfInterests); // 201
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest.Name == pointOfInterest.Description)
                ModelState.AddModelError(nameof(pointOfInterest.Description), "The description should be different from the name");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
                return NotFound();

            var foundPointOfInterest = city.PointsOfInterest.FirstOrDefault(x => x.Id == id);

            if (foundPointOfInterest == null)
                return NotFound();

            foundPointOfInterest.Name = pointOfInterest.Name;
            foundPointOfInterest.Description = pointOfInterest.Description;

            return NoContent(); // 204
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id, [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
                return NotFound();

            var foundPointOfInterest = city.PointsOfInterest.FirstOrDefault(x => x.Id == id);
            if (foundPointOfInterest == null)
                return NotFound();

            var pointOfInterestToPatch = new PointOfInterestForUpdateDto
            {
                Name = foundPointOfInterest.Name,
                Description = foundPointOfInterest.Description
            };

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (pointOfInterestToPatch.Name == pointOfInterestToPatch.Description)
                ModelState.AddModelError(nameof(pointOfInterestToPatch.Description), "The description should be different from the name");

            if (!TryValidateModel(pointOfInterestToPatch))
                return BadRequest(ModelState);

            foundPointOfInterest.Name = pointOfInterestToPatch.Name;
            foundPointOfInterest.Description = pointOfInterestToPatch.Description;

            return NoContent(); // 204
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
                return NotFound();

            var foundPointOfInterest = city.PointsOfInterest.FirstOrDefault(x => x.Id == id);
            if (foundPointOfInterest == null)
                return NotFound();

            city.PointsOfInterest.Remove(foundPointOfInterest);

            _mailService.Send("Point of interests deleted",
                $"Point of interest {foundPointOfInterest.Name} with id {foundPointOfInterest.Id} was deleted.");

            return NoContent(); // 204
        }


        private IActionResult Execute(Func<IActionResult> func, [CallerMemberName] string methodName = "")
        {
            _logger.LogInformation("Start of controller method: {ControllerMethodName}; Route: {Route}", methodName, Request.Path);
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                var message = "Critical error happended during controller method '{0}' call";
                _logger.LogCritical(string.Format(message, "ControllerMethodName"), methodName, ex);
                return StatusCode(500, string.Format(message, methodName));
            }
            finally
            {
                _logger.LogInformation("End of controller method: {ControllerMethodName}", methodName);
            }
        }
    }
}
