using System.ComponentModel.DataAnnotations.Schema;

namespace tec_parts_replenishment_transpor_web.Models
{
    [Table("t_restocking_status")]
    public class ReplenishmentsModel
    {
        [Column("restock_status_id")]
        public int RestockStatusId { get; set; }

        [Column("parts_num")]
        public string PartsNum { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("restock_status")]
        public int RestockStatus { get; set; }

        [Column("warehouse_information")]
        public int WarehouseInformation { get; set; }

        [Column("replenishment_request_date")]
        public DateTime? ReplenishmentRequestDate { get; set; }

        [Column("replenishment_complete_date")]
        public DateTime? ReplenishmentCompleteDate { get; set; }

        [Column("user_check")]
        public int? UserCheck { get; set; }
    }
}
