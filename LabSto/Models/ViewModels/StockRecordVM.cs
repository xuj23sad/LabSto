using System.ComponentModel.DataAnnotations;

namespace LabSto.Models.ViewModels
{
    public class StockRecordVM
    {
        [Required]
        public int ReagentId { get; set; }

        [Required(ErrorMessage = "请选择类型")]
        public string Type { get; set; } = "";         // 入库/出库

        [Range(0.01, double.MaxValue, ErrorMessage = "数量必须大于0")]
        public double Quantity { get; set; }

        public string Remark { get; set; } = "";
    }
}
