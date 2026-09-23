using tec_parts_replenishment_transpor_web.Models;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Dapper;
using tec_parts_replenishment_transpor_web.Commons;
using Microsoft.AspNet.SignalR;
using System.Net;

namespace tec_parts_replenishment_transpor_web.Repositories
{
    public class InventoryAdjustmentsRepository
    {
        string connectionString;

        public InventoryAdjustmentsRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public static List<InventoryInformationModel> GetListInventoryInformations(string sql)
        {
            // 戻り値
            List<InventoryInformationModel> supplys = new();

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

                    supplys = connection.Query<InventoryInformationModel>(sql).ToList();
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
        public static string CreateSQLToGetInventoryInformations()
        {
            // "依頼中"のレコード
            var sql = $@"SELECT
                            inventory_id AS InventoryId,
                            parts_id AS PartsId,
                            parts_num AS PartsNum,
                            inventory_num AS InventoryNum,
                            restock_last_date AS RestockLastDate
                        FROM t_inventory_information;
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
                restockStatus = 2;
            }
            else if (statusBtn == "完了")
            {
                restockStatus = 3;
            }
            else
            {
                restockStatus = 4;
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

        /// <summary>
        /// 在庫情報を取得するSQL作成
        /// </summary>
        /// <remarks>SELECT文</remarks>
        /// <returns>SQL</returns>
        public static string CreateSQLToGetInventoryForInventoryAdjustment()
        {
            var sql = @"
                SELECT
                    inventory_id,
                    parts_num,
                    inventory_num
                FROM t_inventory_information WITH (UPDLOCK, ROWLOCK)
                WHERE inventory_id = @InventoryId
            ";

            return sql;
        }

        public static string CreateSQLToUpdateInventoryAdjustment()
        {
            var sql = @"
                    UPDATE t_inventory_information
                    SET inventory_num = inventory_num + @Quantity
                    WHERE inventory_id = @InventoryId
                    ";

            return sql;
        }
    }
}
