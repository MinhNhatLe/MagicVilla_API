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

namespace MagicVilla_VillaAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/VillaNumberAPI")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VillaAPINumberController : ControllerBase
    {
        protected APIReponse _reponse;
        private readonly IVillaNumberRepository _dbVillaNumber;
        private readonly IMapper _mapper;
        private readonly IVillaRepository _dbVilla;

        public VillaAPINumberController(IVillaNumberRepository dbVillaNumber, IMapper mapper, IVillaRepository dbVilla)
        {
            _dbVillaNumber = dbVillaNumber;
            _mapper = mapper;
            _dbVilla = dbVilla;
            _reponse = new();
        }
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIReponse>> GetVillaNumbers()
        {
            try
            {
                IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync(includeProperties: "Villa");
                _reponse.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
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
        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIReponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                var villa = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villa == null)
                {
                    _reponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_reponse);
                }
                _reponse.Result = _mapper.Map<VillaNumberDTO>(villa);
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
        public async Task<ActionResult<APIReponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createNumberDTO)
        {
            try
            {
                if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createNumberDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "VillaNumber already exists");
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                if (await _dbVilla.GetAsync(u => u.Id == createNumberDTO.VillaID) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is invalid");
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                if (createNumberDTO == null)
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createNumberDTO);
                await _dbVillaNumber.CreateAsync(villaNumber);
                await _dbVillaNumber.SaveAsync();
                _reponse.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _reponse.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVillaNumber", new { id = villaNumber.VillaNo }, _reponse);
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

        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIReponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                var villa = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villa == null)
                {
                    _reponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_reponse);
                }
                //VillaStore.villaList.Remove(villa);
                await _dbVillaNumber.RemoveAsync(villa);
                await _dbVillaNumber.SaveAsync();
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
        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<APIReponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateNumberDTO)
        {
            try
            {
                if (updateNumberDTO == null || id == 0 || id != updateNumberDTO.VillaNo)
                {
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                if (await _dbVilla.GetAsync(u => u.Id == updateNumberDTO.VillaID) == null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa ID is invalid");
                    _reponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_reponse);
                }
                var model = _mapper.Map<VillaNumber>(updateNumberDTO);
                await _dbVillaNumber.UpdateAsync(model);
                await _dbVillaNumber.SaveAsync();
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

    }
}
