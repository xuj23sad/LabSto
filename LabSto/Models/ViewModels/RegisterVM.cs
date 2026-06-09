using System.ComponentModel.DataAnnotations;

namespace LabSto.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "请输入用户名")]
        public string UserName { get; set; } = "";

        [Required, EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password), MinLength(6, ErrorMessage = "密码至少6位")]
        public string Password { get; set; } = "";

        [Compare("Password", ErrorMessage = "两次密码不一致")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }
}