using tec_parts_replenishment_transpor_web.Hubs;
using tec_parts_replenishment_transpor_web.Models;
using TableDependency.SqlClient;
using tec_parts_replenishment_transpor_web.Commons;
using Microsoft.AspNetCore.SignalR;

namespace tec_parts_replenishment_transpor_web.SubscribeTableDependencies
{
    public class SubscribeInventoryAdjustmentTableDependency : ISubscribeTableDependency
    {
        InventoryAdjustmentHub InventoryAdjustmentHub;
        SqlTableDependency<InventoryInformationModel> tableDependency;

        public SubscribeInventoryAdjustmentTableDependency(InventoryAdjustmentHub inventoryAdjustmentHub)
        {
            this.InventoryAdjustmentHub = inventoryAdjustmentHub;
        }

        // サブスクライブテーブルの依存関係
        public void SubscribeTableDependency(string connectionString)
        {
            try
            {
                tableDependency = new SqlTableDependency<InventoryInformationModel>(connectionString);
                tableDependency.OnChanged += TableDependency_OnChanged;
                tableDependency.OnError += TableDependency_OnError;
                tableDependency.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }
        // 変更されたテーブルの依存関係
        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<InventoryInformationModel> e)
        {
            // データを更新される時HUBのメソッドを呼びます
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                InventoryAdjustmentHub.SendInventoryInformations();
            }
        }


        // エラー時のテーブルの依存関係
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(InventoryInformationModel)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
