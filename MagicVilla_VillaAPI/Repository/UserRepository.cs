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
		private string secretKey;

		public UserRepository(ApplicationDbContext db, IConfiguration configuration) 
		{
			_db = db;
			secretKey = configuration.GetValue<string>("ApiSettings:Secret");
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
			var user = _db.LocalUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower() && u.Password == loginRequestDTO.Password);
			if (user == null)
			{
				return new LoginResponseDTO()
				{
					Token = "",
					User = null,
				};
			}
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
					new Claim(ClaimTypes.Role, user.Role)
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
				User = user,
				Token = tokenHandler.WriteToken(token),
			};
			return loginResponseDTO;
		}

		public async Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO)
		{
			LocalUser user = new()
			{
				UserName = registerationRequestDTO.UserName,
				Name = registerationRequestDTO.Name,
				Password = registerationRequestDTO.Password,
				Role = registerationRequestDTO.Role,
			};
			_db.LocalUsers.Add(user);
			await _db.SaveChangesAsync();
			user.Password = "";
			return user;
		}
	}
}
