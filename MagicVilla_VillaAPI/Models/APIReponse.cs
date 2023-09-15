using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace MagicVilla_VillaAPI.Models
{
    public class APIReponse
    {
        public APIReponse() 
        {
            // thông báo message lỗi
            ErrorMessages = new List<string>();
        }
        // trạng thái status
        public HttpStatusCode StatusCode { get; set; }
        // thành công hay thất bại
        public bool IsSuccess { get; set; } = true;
        // thông báo message lỗi
        public List<string> ErrorMessages { get; set; }
        // kết quả của object đó
        public object Result { get; set; }
    }
}