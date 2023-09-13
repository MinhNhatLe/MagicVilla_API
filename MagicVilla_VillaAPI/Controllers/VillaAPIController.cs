using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    //[Route("api/VillaHolo")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        protected APIReponse _reponse;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
            this._reponse = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIReponse>> GetVillas()
        {
            try
            {
                IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
                _reponse.Result = _mapper.Map<List<VillaDTO>>(villaList);
                _reponse.StatusCode = HttpStatusCode.OK;
                return Ok(_reponse);
            }
            catch (Exception ex)
            {
                _reponse.IsSuccess = false;
                _reponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }
            return _reponse;
        }
        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200, Type = typeof(VillaDto))]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        public async Task<ActionResult<APIReponse>> GetVilla(int id)
        {
            try
            {
                if( id == 0)
                {
                    //_logger.LogError("Get Villa Error with id " + id);
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }

                //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
                var villa =await _dbVilla.GetAsync(u=> u.Id == id);
                if (villa == null)
                {
                    _reponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_reponse);
                }
                _reponse.Result = _mapper.Map<VillaDTO>(villa);
                _reponse.StatusCode = HttpStatusCode.OK;
                return Ok(_reponse);
            }
            catch (Exception ex)
            {
                _reponse.IsSuccess = false;
                _reponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }
            return _reponse;

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIReponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}
                // custom Validation
                //if(VillaStore.villaList.FirstOrDefault(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa already exists");
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                if (createDTO == null)
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                //if (villaDTO.Id > 0) 
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}
                //createDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
                //createDTO.Id = _db.Villas.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
                //VillaStore.villaList.Add(villaDTO);
                Villa villa = _mapper.Map<Villa>(createDTO);
                //Villa model = new()
                //{
                //    Amenity = createDTO.Amenity,
                //    Details = createDTO.Details,
                //    //Id = createDTO.Id,
                //    ImageUrl = createDTO.ImageUrl,
                //    Name = createDTO.Name,
                //    Occupancy = createDTO.Occupancy,
                //    Rate = createDTO.Rate,
                //    Sqft = createDTO.Sqft,
                //    CreatedDate = DateTime.Now,
                //};
                await _dbVilla.CreateAsync(villa);
                await _dbVilla.SaveAsync();
                _reponse.Result = _mapper.Map<VillaDTO>(villa);
                _reponse.StatusCode= HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _reponse);
            }
            catch (Exception ex)
            {
                _reponse.IsSuccess = false;
                _reponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }
            return _reponse;
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIReponse>> DeleteVilla(int id)
        {
            try
            {
                if(id == 0)
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    _reponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_reponse);
                }
                //VillaStore.villaList.Remove(villa);
                await _dbVilla.RemoveAsync(villa);
                await _dbVilla.SaveAsync();
                _reponse.StatusCode = HttpStatusCode.NoContent;
                _reponse.IsSuccess = true;
                return Ok(_reponse);
            }
            catch (Exception ex)
            {
                _reponse.IsSuccess = false;
                _reponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }
            return _reponse;
        }
        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task <ActionResult<APIReponse>> UpdateVilla (int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id == 0 || id != updateDTO.Id)
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
                //villa.Id = villaDTO.Id;
                //villa.Name = villaDTO.Name;
                //villa.Rate = villaDTO.Rate;
                //villa.Occupancy = villaDTO.Occupancy;
                var model = _mapper.Map<Villa>(updateDTO);
                //Villa model = new()
                //{
                //    Amenity = updateDTO.Amenity,
                //    Details = updateDTO.Details,
                //    Id = updateDTO.Id,
                //    ImageUrl = updateDTO.ImageUrl,
                //    Name = updateDTO.Name,
                //    Occupancy = updateDTO.Occupancy,
                //    Rate = updateDTO.Rate,
                //    Sqft = updateDTO.Sqft,
                //    UpdatedDate = DateTime.Now,
                //};
                await _dbVilla.UpdateAsync(model);
                await _dbVilla.SaveAsync();
                _reponse.Result = HttpStatusCode.NoContent;
                _reponse.IsSuccess= true;
                return Ok(_reponse);
            }
            catch (Exception ex)
            {
                _reponse.IsSuccess = false;
                _reponse.ErrorMessages = new List<string>()
                {
                    ex.ToString()
                };
            }
            return _reponse;
        }

        [HttpPatch("{id:int}", Name = "PatchVilla")]
        [ProducesResponseType (StatusCodes.Status400BadRequest)]
        [ProducesResponseType (StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePartialVilla (int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if(patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked:false);
            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
            //VillaUpdateDTO villaDTO = new()
            //{
            //    Amenity = villa.Amenity,
            //    Details = villa.Details,
            //    Id = villa.Id,
            //    ImageUrl = villa.ImageUrl,
            //    Name = villa.Name,
            //    Occupancy = villa.Occupancy,
            //    Rate = villa.Rate,
            //    Sqft = villa.Sqft,                
            //};
            if (villa == null)
            {
                return BadRequest();
            }
            patchDTO.ApplyTo(villaDTO, ModelState);
            //villa.Amenity = villaDTO.Amenity;
            //villa.Details = villaDTO.Details;
            //villa.ImageUrl = villaDTO.ImageUrl;
            //villa.Name = villaDTO.Name;
            //villa.Occupancy = villaDTO.Occupancy;
            //villa.Rate = villaDTO.Rate;
            //villa.Sqft = villaDTO.Sqft;

            Villa model = _mapper.Map<Villa>(villaDTO);
            //Villa model = new Villa()
            //{
            //    Amenity = villaDTO.Amenity,
            //    Details = villaDTO.Details,
            //    Id = villaDTO.Id,
            //    ImageUrl = villaDTO.ImageUrl,
            //    Name = villaDTO.Name,
            //    Occupancy = villaDTO.Occupancy,
            //    Rate = villaDTO.Rate,
            //    Sqft = villaDTO.Sqft,
            //};
            await _dbVilla.UpdateAsync(model);
            await _dbVilla.SaveAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }

    }
}
