using System.ComponentModel.DataAnnotations;

namespace LabSto.Models.ViewModels
{
    public class ReagentVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "请输入试剂名称")]
        public string Name { get; set; } = "";

        public string CASNo { get; set; } = "";

        [Required(ErrorMessage = "请选择分类")]
        public string Category { get; set; } = "";

        [Range(0, 3)]
        public int DangerLevel { get; set; }

        [Required]
        public string Unit { get; set; } = "";

        [Range(0, double.MaxValue, ErrorMessage = "库存不能为负")]
        public double Stock { get; set; }

        public double MinStock { get; set; }
        public string Remark { get; set; } = "";
    }
}
