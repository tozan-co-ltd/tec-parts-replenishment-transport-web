using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Net;
using tec_parts_replenishment_transpor_web.Commons;
using tec_parts_replenishment_transpor_web.Models;
using tec_parts_replenishment_transpor_web.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace tec_parts_replenishment_transpor_web.Controllers
{
    public class InventoryAdjustmentController : Controller
    {
        private readonly ILogger<InventoryAdjustmentController> _logger;

        public InventoryAdjustmentController(ILogger<InventoryAdjustmentController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 運搬画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 登録ボタンを押下した際の処理
        /// </summary>
        /// <param name="dataInventoryId"></param>
        /// <param name="dataInventoryNumInput"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register(string dataInventoryId, string dataInventoryNumInput)
        {
            try
            {
                bool resUpdate = UpdateInventoryAdjustment(Int32.Parse(dataInventoryId), Int32.Parse(dataInventoryNumInput ?? "0"));
                var result = new { res = resUpdate };

                return Json(result);
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;
                var result = new { res = exceptionMessage };

                return Json(result);
            }
        }


        /// <summary>
        /// 在庫調整更新
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public bool UpdateInventoryAdjustment(int inventoryId, int quantity)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();

            using var connection = new SqlConnection(connectionString);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // (1) 在庫データ取得
                // SQL Server: UPDLOCK + ROWLOCK
                var sqlGetInventory = InventoryAdjustmentsRepository.CreateSQLToGetInventoryForInventoryAdjustment();

                var inventory =
                    connection.QueryFirstOrDefault<InventoryInformationModel>(
                        sqlGetInventory,
                        new
                        {
                            InventoryId = inventoryId
                        },
                        transaction);

                if (inventory == null)
                {
                    var errorMessage = ErrorHandling.CreateErrorMessage("E4001");

                    throw new Exception(errorMessage);
                }

                // (2) 在庫数を加算
                var sqlUpdateInventory = InventoryAdjustmentsRepository.CreateSQLToUpdateInventoryAdjustment();

                var updateInventory =
                    connection.Execute(
                        sqlUpdateInventory,
                        new
                        {
                            InventoryId = inventoryId,
                            Quantity = quantity
                        },
                        transaction);

                if (updateInventory < 1)
                {
                    var errorMessage = ErrorHandling.CreateErrorMessage("E4001");

                    throw new Exception(errorMessage);
                }

                // (3) COMMIT
                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}