using System.ComponentModel.DataAnnotations.Schema;

namespace tec_parts_replenishment_transpor_web.Models
{
    [Table("t_inventory_information")]
    public class InventoryInformationModel
    {
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Column("parts_id")]
        public int PartsId { get; set; }

        [Column("parts_num")]
        public string PartsNum { get; set; }

        [Column("inventory_num")]
        public int InventoryNum { get; set; }

        [Column("restock_last_date")]
        public DateTime? RestockLastDate { get; set; }
    }
}
