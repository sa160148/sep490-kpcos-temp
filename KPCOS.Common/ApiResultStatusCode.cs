using System.ComponentModel.DataAnnotations;

namespace Common
{
    public enum ApiResultStatusCode
    {
        [Display(Name = "The operation was completed successfully")]
        Success = 0,

        [Display(Name = "An error occurred on the server")]
        ServerError = 1,

        [Display(Name = "The provided parameters are not valid")]
        BadRequest = 2,

        [Display(Name = "Not found")]
        NotFound = 3,

        [Display(Name = "The list is empty")]
        ListEmpty = 4,

        [Display(Name = "An error occurred during processing")]
        LogicError = 5,

        [Display(Name = "Authentication error")]
        UnAuthorized = 6
    }

}
