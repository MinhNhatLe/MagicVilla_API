using MagicVilla_Web.Models;

namespace MagicVilla_Web.Services.IServices
{
    public interface IBaseService
    {
        APIResponse responseModel { get; set; } // API trả về
        Task<T> SendAsync<T>(APIRequest apiRequest); //API request lên
    }
}