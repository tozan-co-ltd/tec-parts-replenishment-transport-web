using Dapper;
using Microsoft.AspNet.SignalR;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using tec_parts_replenishment_transpor_web.Commons;
using tec_parts_replenishment_transpor_web.Models;
using static tec_parts_replenishment_transpor_web.Commons.Const;

namespace tec_parts_replenishment_transpor_web.Repositories
{
    public class ReplenishmentsRepository
    {
        string connectionString;

        public ReplenishmentsRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public static List<ReplenishmentsModel> GetListReplenishments(string sql)
        {
            // 戻り値
            List<ReplenishmentsModel> supplys = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    supplys = connection.Query<ReplenishmentsModel>(sql).ToList();
                }
                return supplys;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 部品準備取得SQL作成
        /// </summary>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetReplenishments()
        {

            // "依頼中"のレコード
            var sql = $@"SELECT
                            t.restock_status_id AS RestockStatusId,
                            t.parts_num AS PartsNum,
                            t.quantity AS Quantity,
                            t.restock_status AS RestockStatus,
                            t.warehouse_information AS WarehouseInformation,
                            t.replenishment_request_date AS ReplenishmentRequestDate,
                            t.replenishment_complete_date AS ReplenishmentCompleteDate,
                            t.user_check AS UserCheck
                        FROM t_restocking_status AS t
                        WHERE  (t.restock_status =  {Status.C_PENDING_STATUS} OR t.restock_status = {Status.C_START_STATUS})
                          AND t.warehouse_information = 0
                          AND t.replenishment_complete_date IS NULL
                        ORDER BY replenishment_request_date ASC
                        ";

            return sql;
        }


        /// <summary>
        /// 部品補充に更新するSQL作成
        /// </summary>
        /// <param name="dataRestockStatusId"></param>
        /// <param name="statusBtn"></param>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToUpdateRestockingStatus(int dataRestockStatusId, string statusBtn)
        {
            int restockStatus;

            if (statusBtn == "開始")
            {
                restockStatus = Status.C_START_STATUS;
            }
            else if (statusBtn == "完了")
            {
                restockStatus = Status.C_COMPLETE_STATUS;
            }
            else
            {
                restockStatus = Status.C_CANCEL_STATUS;
            }

            var sql = $@"
                UPDATE t_restocking_status
                SET restock_status = {restockStatus}
                WHERE restock_status_id = {dataRestockStatusId};
            ";

            return sql;
        }


        /// <summary>
        /// 部品補充ステータスを取得するSQL作成
        /// </summary>
        /// <remarks>SELECT文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetRestockingStatus()
        {
            var sql = @"
                SELECT
                    t.restock_status_id AS RestockStatusId,
                    t.parts_num AS PartsNum,
                    t.quantity AS Quantity,
                    t.restock_status AS RestockStatus
                FROM t_restocking_status AS t
                WHERE t.restock_status_id = @RestockStatusId;
            ";

            return sql;
        }


        /// <summary>
        /// 在庫情報を取得するSQL作成
        /// </summary>
        /// <remarks>SELECT文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetInventoryForUpdate()
        {
            var sql = @"
                SELECT
                    t.inventory_id AS InventoryId,
                    t.parts_id AS PartsId,
                    t.parts_num AS PartsNum,
                    t.inventory_num AS InventoryNum,
                    t.restock_last_date AS RestockLastDate
                FROM t_inventory_information AS t WITH (UPDLOCK, ROWLOCK)
                WHERE t.parts_num = @PartsNum;
            ";

            return sql;
        }

        /// <summary>
        /// 在庫数を更新するSQL作成
        /// </summary>
        /// <remarks>UPDATE文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToUpdateInventory()
        {
            var sql = @"
                UPDATE t_inventory_information
                SET inventory_num = inventory_num - @Quantity
                WHERE parts_num = @PartsNum;
            ";

            return sql;
        }
    }
}
