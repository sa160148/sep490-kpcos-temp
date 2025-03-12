using System.ComponentModel.DataAnnotations;

namespace KPCOS.Common
{
    public enum ApiResultStatusCode
    {
        [Display(Name = "Thao tác đã hoàn thành thành công")]
        Success = 0,

        [Display(Name = "Đã xảy ra lỗi trên máy chủ")]
        ServerError = 1,

        [Display(Name = "Các tham số được cung cấp không hợp lệ")]
        BadRequest = 2,

        [Display(Name = "Không tìm thấy")]
        NotFound = 3,

        [Display(Name = "Danh sách trống")]
        ListEmpty = 4,

        [Display(Name = "Đã xảy ra lỗi trong quá trình xử lý")]
        LogicError = 5,

        [Display(Name = "Lỗi xác thực")]
        UnAuthorized = 6
    }

}
