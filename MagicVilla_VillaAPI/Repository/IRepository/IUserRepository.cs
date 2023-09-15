using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
	public interface IUserRepository
	{
		//Kiểm tra xem có trùng name không?
		bool IsUniqueUser(string name);

		// lấy data từ LoginResquest lên để đăng nhập
		// trả về LoginReponse
		Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);


		//lấy data từ registerationRequest lên để đăng kí
		// trả về LocalUser
		Task<LocalUser> Register (RegisterationRequestDTO registerationRequestDTO);
	}
}
