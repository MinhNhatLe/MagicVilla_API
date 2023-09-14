using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
	public class UserRepository : IUserRepository
	{
		private ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager) 
		{
			_db = db;
			secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }
		public bool IsUniqueUser(string name)
		{
			var user = _db.LocalUsers.FirstOrDefault(x => x.UserName == name);
			if (user == null)
			{
				return true;
			}
			return false;
		}

		public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
		{			
			//var user = _db.LocalUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower() && u.Password == loginRequestDTO.Password);
   //         if (user == null)
			//{
			//	return new LoginResponseDTO()
			//	{
			//		Token = "",
			//		User = null,
			//	};
			//}
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());
			bool IsValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            if (user == null || IsValid == false)
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null,
                };
            }
			//role identity
			var roles = await _userManager.GetRolesAsync(user);
            // if user was found Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
			// mã hóa key
			var key = Encoding.ASCII.GetBytes(secretKey);
			//mô tả token
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				// mô tả subject cho nó
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name, user.Id.ToString()),
					new Claim(ClaimTypes.Role, roles.FirstOrDefault())
				}),
				// đặt thời hạn
				Expires = DateTime.UtcNow.AddDays(7),
				// Ký thông tin xác thực
				SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			// tạo token
			var token = tokenHandler.CreateToken(tokenDescriptor);
			LoginResponseDTO loginResponseDTO = new LoginResponseDTO
			{
				User = _mapper.Map<UserDTO>(user),
				Token = tokenHandler.WriteToken(token),
				Role = roles.FirstOrDefault(),
			};
			return loginResponseDTO;
		}

        //public async Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO)
        //{
        //	LocalUser user = new()
        //	{
        //		UserName = registerationRequestDTO.UserName,
        //		Name = registerationRequestDTO.Name,
        //		Password = registerationRequestDTO.Password,
        //		Role = registerationRequestDTO.Role,
        //	};
        //	_db.LocalUsers.Add(user);
        //	await _db.SaveChangesAsync();
        //	user.Password = "";
        //	return user;
        //}
        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Email = registerationRequestDTO.UserName,
                NormalizedEmail = registerationRequestDTO.UserName.ToUpper(),
                Name = registerationRequestDTO.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }
                    await _userManager.AddToRoleAsync(user, "admin");
                    var userToReturn = _db.ApplicationUsers
                        .FirstOrDefault(u => u.UserName == registerationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);

                }
            }
            catch (Exception e)
            {

            }

            return new UserDTO();
        }
    }
}
