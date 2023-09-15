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
			// lấy giá trị của secretKey
			secretKey = configuration.GetValue<string>("ApiSettings:Secret");
		}

        //Kiểm tra xem có trùng name không?
        public bool IsUniqueUser(string name)
		{
			var user = _db.LocalUsers.FirstOrDefault(x => x.UserName == name);
			// nếu uer rỗng thì ok cứ đăng kí
			if (user == null)
			{
				return true;
			}
			return false;
		}

		public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
		{
			var user = _db.LocalUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower() && u.Password == loginRequestDTO.Password);
			
			// nếu user nhập sai thìn trả về rỗng
			if (user == null)
			{
				return new LoginResponseDTO()
				{
					Token = "",
					User = null,
				};
			}
			// Nếu user tìm thấy thì sẽ sinh ra (Generate) JWT token

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

			// sau khi có token rồi thì return về cho nó
			LoginResponseDTO loginResponseDTO = new LoginResponseDTO
			{
				User = user,
				Token = tokenHandler.WriteToken(token),
			};
			return loginResponseDTO;
		}

		public async Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO)
		{
			//gán data nhập vào LocalUser
			LocalUser user = new()
			{
				UserName = registerationRequestDTO.UserName,
				Name = registerationRequestDTO.Name,
				Password = registerationRequestDTO.Password,
				Role = registerationRequestDTO.Role,
			};
			_db.LocalUsers.Add(user);
			await _db.SaveChangesAsync();
			// trả password về chuỗi rỗng để bảo mật thông tin
			user.Password = "";
			return user; // user này là LocalUser
		}
	}
}
