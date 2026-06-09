namespace LabSto.Models.Entities
{
    public class Reagent
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";        // 试剂名称
        public string CASNo { get; set; } = "";       // CAS号
        public string Category { get; set; } = "";    // 分类（有机/无机/危险品）
        public int DangerLevel { get; set; }          // 危险等级 0-3
        public string Unit { get; set; } = "";        // 单位（mL/g/瓶）
        public double Stock { get; set; }             // 当前库存
        public double MinStock { get; set; }          // 最低库存（预警用）
        public string Remark { get; set; } = "";
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 导航属性
        public ICollection<StockRecord> StockRecords { get; set; } = [];
        public ICollection<CabinetSlot> Slots { get; set; } = [];
    }
}