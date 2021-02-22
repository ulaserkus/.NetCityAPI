using AutoMapper;
using CityAPI.Data;
using CityAPI.Dtos;
using CityAPI.Helpers;
using CityAPI.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CityAPI.Controllers
{
    [Route("api/cities/{cityId}/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private IAppRepository _appRepository;
        private IMapper _mapper;
        private IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotoController(IAppRepository appRepository, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _appRepository = appRepository;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;
            Account account = new Account(cloudinaryConfig.Value.CloudName, cloudinaryConfig.Value.ApiKey, cloudinaryConfig.Value.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        [HttpPost]
        public ActionResult AddPhotoForCity(int cityId, [FromBody] PhotoForCreationDto photoForCreation)
        {
            var city = _appRepository.GetCityById(cityId);
            if (city == null)
            {
                return BadRequest("Could not find the city");
            }
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(currentUserId != city.UserId)
            {
                return Unauthorized();
            }

            var file = photoForCreation.File;
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream)
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                   
                }
            }
            photoForCreation.Url = uploadResult.Uri.ToString();
            photoForCreation.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreation);
            photo.City = city;
            if (!city.Photos.Any(p => p.IsMain))
            {
                photo.IsMain = true;
            }

            city.Photos.Add(photo);
            if (_appRepository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }
            return BadRequest("Could not add th photo");
        }
        [HttpGet("{id}",Name ="GetPhoto")]
        public ActionResult GetPhoto(int id)
        {
            var photoFromDb = _appRepository.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromDb);

            return Ok(photo);
        }
    }
}
