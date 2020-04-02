using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, 
            IMailService mailService, ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            return Execute(() =>
            {
                if (!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation("City with the cityId={cityId} was not found", cityId);
                    return NotFound();
                }

                var pointsOfInterestEntities = _cityInfoRepository
                    .GetPointsOfInterestForCity(cityId); ;

                var pointsOfInterestDtos = _mapper
                    .Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestEntities);

                return Ok(pointsOfInterestDtos);
            });
        }

        [HttpGet("{id}", Name = "GetPointOfInterest")]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            if (!_cityInfoRepository.CityExists(cityId))
                return NotFound();

            var pointOfInterestEntity = _cityInfoRepository
                .GetPointOfInterestForCity(cityId, id);

            if (pointOfInterestEntity == null)
                return NotFound();

            var pointOfInterestDto = _mapper
                .Map<PointOfInterestDto>(pointOfInterestEntity);

            return Ok(pointOfInterestDto);
        }

        [HttpPost]
        public IActionResult CreatePointOfInterests(int cityId, 
            [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            if (pointOfInterest.Name == pointOfInterest.Description)
                ModelState.AddModelError(nameof(pointOfInterest.Description), "The description should be different from the name");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cityExists = _cityInfoRepository.CityExists(cityId);
            if (!cityExists)
                return NotFound();

            var newPointOfInterestEntity = _mapper
                .Map<Entities.PointOfInterest>(pointOfInterest);

            _cityInfoRepository
                .AddPointOfInterestForCity(cityId, newPointOfInterestEntity);

            _cityInfoRepository.Save();

            var newPointOfInterestDto = _mapper
                .Map<PointOfInterestDto>(newPointOfInterestEntity);

            return CreatedAtRoute("GetPointOfInterest", 
                new { cityId, id = newPointOfInterestDto.Id }, newPointOfInterestDto); // 201
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest.Name == pointOfInterest.Description)
                ModelState.AddModelError(nameof(pointOfInterest.Description), "The description should be different from the name");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cityExists = _cityInfoRepository.CityExists(cityId);
            if (!cityExists)
                return NotFound();

            var pointOfInterestEntity = _cityInfoRepository
                .GetPointOfInterestForCity(cityId, id);

            if (pointOfInterestEntity == null)
                return NotFound();

            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            _cityInfoRepository.UpdatePointOfInterestForTheCity(cityId, pointOfInterest);

            _cityInfoRepository.Save();

            return NoContent(); // 204
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id, [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var cityExists = _cityInfoRepository.CityExists(cityId);
            if (!cityExists)
                return NotFound();

            var pointOfInterestEntity = _cityInfoRepository
                .GetPointOfInterestForCity(cityId, id);

            if (pointOfInterestEntity == null)
                return NotFound();

            var pointOfInterestToPatch = _mapper
                .Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (pointOfInterestToPatch.Name == pointOfInterestToPatch.Description)
                ModelState.AddModelError(nameof(pointOfInterestToPatch.Description), "The description should be different from the name");

            if (!TryValidateModel(pointOfInterestToPatch))
                return BadRequest(ModelState);

            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            _cityInfoRepository.UpdatePointOfInterestForTheCity(cityId, pointOfInterestToPatch);

            _cityInfoRepository.Save();

            return NoContent(); // 204
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            var cityExists = _cityInfoRepository.CityExists(cityId);
            if (!cityExists)
                return NotFound();

            var pointOfInterestEntity = _cityInfoRepository
                .GetPointOfInterestForCity(cityId, id);

            if (pointOfInterestEntity == null)
                return NotFound();

            _cityInfoRepository.Delete(pointOfInterestEntity);

            _cityInfoRepository.Save();

            _mailService.Send("Point of interests deleted",
                $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

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
