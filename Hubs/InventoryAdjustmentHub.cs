using Microsoft.AspNetCore.SignalR;
using tec_parts_replenishment_transpor_web.Commons;
using tec_parts_replenishment_transpor_web.Models;
using tec_parts_replenishment_transpor_web.Repositories;

namespace tec_parts_replenishment_transpor_web.Hubs
{
    public class InventoryAdjustmentHub : Hub
    {
        public InventoryAdjustmentHub(IConfiguration configuration)
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task SendInventoryInformations()
        {
            try
            {
                // SQL作成
                var sql = InventoryAdjustmentsRepository.CreateSQLToGetInventoryInformations();
                List<InventoryInformationModel> listInventoryInformations = InventoryAdjustmentsRepository.GetListInventoryInformations(sql);
                if (Clients != null)
                    await Clients.All.SendAsync("ReceivedInventoryInformations", listInventoryInformations);
            }
            catch (Exception)
            {
                // エラーメッセージ作成
                // 「SQLServerでエラーが発生しました。」
                var errorMessage = ErrorHandling.CreateErrorMessage("E4001");
                await Clients.Caller.SendAsync("Error", errorMessage);
            }
        }
    }
}
