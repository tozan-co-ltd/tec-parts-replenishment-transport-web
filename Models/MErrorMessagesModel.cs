using System.ComponentModel.DataAnnotations.Schema;

namespace tec_parts_replenishment_transpor_web.Models
{
    /// <summary>
    /// エラーメッセージテーブルのModel
    /// </summary>
    public class MErrorMessagesModel
    {
        /// <summary>
        /// エラーコード
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
