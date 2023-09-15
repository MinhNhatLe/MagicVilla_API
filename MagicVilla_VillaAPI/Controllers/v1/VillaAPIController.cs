using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace MagicVilla_VillaAPI.Controllers.v1
{
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v{version:apiVersion}/VillaAPI")]
    //[Route("api/VillaHolo")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VillaAPIController : ControllerBase
    {
        protected APIReponse _reponse;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
            _reponse = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ResponseCache(Duration = 30)] // này là lưu cache trong vòng 30
        //[ResponseCache(CacheProfileName = "Default30")]
        //[Authorize]
        public async Task<ActionResult<APIReponse>> GetVillas([FromQuery(Name = "filterOccupancy")]int? occupancy, [FromQuery(Name ="SearchName")]string? search, int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                IEnumerable<Villa> villaList;// lấy danh sách Villa ra
                if (occupancy > 0)
                {
                    villaList = await _dbVilla.GetAllAsync(u => u.Occupancy == occupancy, pageSize: pageSize,
                        pageNumber: pageNumber);
                }
                else
                {
                    villaList = await _dbVilla.GetAllAsync(pageSize: pageSize,
                       pageNumber: pageNumber);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    villaList = villaList.Where(u => u.Name.ToLower().Contains(search.ToLower()));
                }

                // nó sẽ reponse header ra số bao nhiêu item và bao nhiêu số trang
                Pagination pagination = new() { PageNumber = pageNumber, PageSize = pageSize };
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

                // này là cốt lỗi của func này
                _reponse.Result = _mapper.Map<List<VillaDTO>>(villaList); // map dữ liệu từ Villa ra VillaDTO để hiển thị
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(200, Type = typeof(VillaDto))]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        //[Authorize]
        public async Task<ActionResult<APIReponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    //_logger.LogError("Get Villa Error with id " + id);
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
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIReponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)// nếu name villa trùng nhau báo lỗi
                {
                    ModelState.AddModelError("ErrorMessages", "Villa already exists");
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                if (createDTO == null) // nếu để null báo lỗi
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                Villa villa = _mapper.Map<Villa>(createDTO); // map dữ liệu data nhập vào villa để tạo & lưu
                await _dbVilla.CreateAsync(villa);
                await _dbVilla.SaveAsync();

                _reponse.Result = _mapper.Map<VillaDTO>(villa);  // map dữ liệu villa mới tạo xong ra VillaDTO để hiển thị
                _reponse.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _reponse); // tạo sang nó sẽ route sang GetVilla để hiển thị
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
        [Authorize(Roles = "CUSTOM")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIReponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0) // id =0 báo lỗi
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
                var villa = await _dbVilla.GetAsync(u => u.Id == id); // kiểm tra id có = nhau hem?
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
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<APIReponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
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
                var model = _mapper.Map<Villa>(updateDTO); // map dữ liệu vào Villa
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

        [HttpPatch("{id:int}", Name = "PatchVilla")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
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
