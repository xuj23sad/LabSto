namespace LabSto.Models.Entities
{
    public class Cabinet
    {
        public int Id { get; set; }
        public string CabinetNo { get; set; } = "";    // 柜编号
        public string Location { get; set; } = "";     // 位置描述
        public int Capacity { get; set; }              // 总格位数
        public string StorageType { get; set; } = "";  // 常温/低温/防火
        public string Status { get; set; } = "正常";

        // 导航属性
        public ICollection<CabinetSlot> Slots { get; set; } = [];
    }
}