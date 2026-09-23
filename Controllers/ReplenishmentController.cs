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
    public class ReplenishmentController : Controller
    {
        private readonly ILogger<ReplenishmentController> _logger;

        public ReplenishmentController(ILogger<ReplenishmentController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///部品補充画面表示
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 登録ボタンを押下した際の処理
        /// </summary>
        /// <param name="dataRestockStatusId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateForReplenishmentStart(string dataRestockStatusId, string statusBtn)
        {
            try
            {
                bool resUpdate = UpdateRestockingStatusForTransportationStart(Int32.Parse(dataRestockStatusId), statusBtn);
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
        /// 部品補充ステータスを更新
        /// </summary>
        /// <param name="dataRestockStatusId"></param>
        /// <returns></returns>
        public bool UpdateRestockingStatusForTransportationStart(int dataRestockStatusId, string statusBtn)
        {
            string? errorMessage;

            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var sqlUpdate = ReplenishmentsRepository.CreateSQLToUpdateRestockingStatus(dataRestockStatusId, statusBtn);

                var updateDetail = connection.Execute(sqlUpdate,new { dataRestockStatusId });

                if (updateDetail >= 1)
                {
                    return true;
                }

                errorMessage = ErrorHandling.CreateErrorMessage("E4001");
                throw new Exception(errorMessage);
            }
        }


        /// <summary>
        /// 部品補充キャンセルを登録
        /// </summary>
        /// <param name="dataRestockStatusId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RegisterForReplenishmentCancel(string dataRestockStatusId, string statusBtn)
        {
            try
            {
                statusBtn = string.Empty;
                bool resUpdate = UpdateRestockingStatusForTransportationStart(Int32.Parse(dataRestockStatusId), statusBtn);
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
        /// 部品補充完了を登録
        /// </summary>
        /// <param name="dataRestockStatusId"></param>
        /// <param name="statusBtn"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RegisterForReplenishmentComplete(string dataRestockStatusId, string statusBtn)
        {
            try
            {
                bool resUpdate = UpdateForReplenishmentComplete(Int32.Parse(dataRestockStatusId), statusBtn);
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
        /// 部品補充完了ステータスを更新
        /// </summary>
        /// <param name="dataRestockStatusId"></param>
        /// <param name="statusBtn"></param>
        /// <returns></returns>
        public bool UpdateForReplenishmentComplete( int dataRestockStatusId, string statusBtn)
        {
            string? errorMessage;

            var connectionString =
                ConnectToSQLServer.GetSQLServerConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // (1) 完了ボタン押下
                        // (2) restock_status_id / parts_num / quantity 取得
                        var sqlGetRestockingStatus = ReplenishmentsRepository.CreateSQLToGetRestockingStatus();

                        var restockingStatus =
                            connection.QueryFirstOrDefault<ReplenishmentsModel>(
                                sqlGetRestockingStatus,
                                new
                                {
                                    RestockStatusId = dataRestockStatusId
                                },
                                transaction);

                        if (restockingStatus == null)
                        {
                            errorMessage = ErrorHandling.CreateErrorMessage("E4001");
                            throw new Exception(errorMessage);
                        }

                        // (3) restock_status = 3
                        var sqlUpdateRestockStatus =ReplenishmentsRepository.CreateSQLToUpdateRestockingStatus(dataRestockStatusId, statusBtn);

                        var updateRestockStatus = connection.Execute(sqlUpdateRestockStatus, new{ dataRestockStatusId }, transaction: transaction);

                        if (updateRestockStatus < 1)
                        {
                            errorMessage = ErrorHandling.CreateErrorMessage("E4001");

                            throw new Exception(errorMessage);
                        }

                        //  (4) 在庫データ取得   SQL Server: UPDLOCK + ROWLOCK
                        var sqlGetInventory = ReplenishmentsRepository.CreateSQLToGetInventoryForUpdate();

                        var inventory = connection.QueryFirstOrDefault<InventoryInformationModel>(sqlGetInventory,new{PartsNum = restockingStatus.PartsNum},transaction);

                        if (inventory == null)
                        {
                            errorMessage = ErrorHandling.CreateErrorMessage("E4001");

                            throw new Exception(errorMessage);
                        }

                        // 在庫数確認
                        //if (inventory.InventoryNum < restockingStatus.Quantity)
                        //{
                        //    errorMessage = ErrorHandling.CreateErrorMessage("E4001");

                        //    throw new Exception(errorMessage);
                        //}

                        // (5) 在庫数をマイナスして更新
                        var sqlUpdateInventory = ReplenishmentsRepository.CreateSQLToUpdateInventory();

                        var updateInventory =
                            connection.Execute(
                                sqlUpdateInventory,
                                new
                                {
                                    PartsNum = restockingStatus.PartsNum,
                                    Quantity = restockingStatus.Quantity
                                },
                                transaction: transaction);

                        if (updateInventory < 1)
                        {
                            errorMessage = ErrorHandling.CreateErrorMessage("E4001");

                            throw new Exception(errorMessage);
                        }

                        // COMMIT
                        transaction.Commit();

                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}