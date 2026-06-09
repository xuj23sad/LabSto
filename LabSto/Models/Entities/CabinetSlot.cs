using LabSto.Models.Entities;

namespace LabSto.Models.Entities
{
    public class CabinetSlot
    {
        public int Id { get; set; }
        public int CabinetId { get; set; }
        public string SlotNo { get; set; } = "";       // 格位编号 如 A1 B2
        public string Status { get; set; } = "空置";   // 空置/占用
        public int? ReagentId { get; set; }            // 可空，空置时为null

        // 导航属性
        public Cabinet Cabinet { get; set; } = null!;
        public Reagent Reagent { get; set; }
    }
}
