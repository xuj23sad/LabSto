namespace LabSto.Models.Entities
{
    public class StockRecord
    {
        public int Id { get; set; }
        public int ReagentId { get; set; }
        public string Type { get; set; } = "";         // 入库/出库
        public double Quantity { get; set; }           // 数量
        public string OperatorName { get; set; } = ""; // 操作人
        public string Remark { get; set; } = "";
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 导航属性
        public Reagent Reagent { get; set; } = null!;
    }
}
