using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Data
{
    public class VillaStore
    {
        public static List<VillaDTO> villaList = new List<VillaDTO>
            {
                new VillaDTO
                {
                    Id = 1,
                    Name = "Pool View",
                    Rate = 10,
                    Occupancy = 10,
                },
                new VillaDTO
                {
                    Id = 2,
                    Name = "Beach View",
                    Rate = 20,
                    Occupancy = 20,
                },
            };
    }
}
