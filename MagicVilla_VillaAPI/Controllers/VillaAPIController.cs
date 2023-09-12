using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    //[Route("api/VillaHolo")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        //private readonly ILogger<VillaAPIController> _logger;

        //public VillaAPIController(ILogger<VillaAPIController> logger)
        //{
        //    _logger = logger;
        //}

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            //_logger.LogInformation("Getting all villas");
            return Ok(VillaStore.villaList);
        }
        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200, Type = typeof(VillaDto))]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if(id == 0)
            {
                //_logger.LogError("Get Villa Error with id " + id);
                return BadRequest();
            }

            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO VillaDTO)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            // custom Validation
            if(VillaStore.villaList.FirstOrDefault(u => u.Name.ToLower() == VillaDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa already exists");
                return BadRequest(ModelState);
            }
            if (VillaDTO == null)
            {
                return BadRequest();
            }
            if (VillaDTO.Id > 0) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            VillaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            VillaStore.villaList.Add(VillaDTO);

            return CreatedAtRoute("GetVilla", new { id = VillaDTO.Id }, VillaDTO);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if(id == 0)
            {
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            VillaStore.villaList.Remove(villa);
            return NoContent();
        }
        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult UpdateVilla (int id, [FromBody] VillaDTO villaDTO)
        {
            if (id == null || id == 0 || id != villaDTO.Id)
            {
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            villa.Id = villaDTO.Id;
            villa.Name = villaDTO.Name;
            villa.Rate = villaDTO.Rate;
            villa.Occupancy = villaDTO.Occupancy;
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "PatchVilla")]
        [ProducesResponseType (StatusCodes.Status400BadRequest)]
        [ProducesResponseType (StatusCodes.Status204NoContent)]
        public IActionResult UpdatePartialVilla (int id, JsonPatchDocument<VillaDTO> patchDTO)
        {
            if(patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null)
            {
                return BadRequest();
            }
            patchDTO.ApplyTo(villa, ModelState);
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }

    }
}
