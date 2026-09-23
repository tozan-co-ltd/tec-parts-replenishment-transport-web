using tec_parts_replenishment_transpor_web.Hubs;
using tec_parts_replenishment_transpor_web.Models;
using TableDependency.SqlClient;
using tec_parts_replenishment_transpor_web.Commons;
using Microsoft.AspNetCore.SignalR;

namespace tec_parts_replenishment_transpor_web.SubscribeTableDependencies
{
    public class SubscribeReplenishmentTableDependency : ISubscribeTableDependency
    {
        ReplenishmentHub replenishmentHub;
        SqlTableDependency<ReplenishmentsModel> tableDependency;

        public SubscribeReplenishmentTableDependency(ReplenishmentHub replenishmentHub)
        {
            this.replenishmentHub = replenishmentHub;
        }

        // サブスクライブテーブルの依存関係
        public void SubscribeTableDependency(string connectionString)
        {
            try
            {
                tableDependency = new SqlTableDependency<ReplenishmentsModel>(connectionString);
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
        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<ReplenishmentsModel> e)
        {
            // データを更新される時HUBのメソッドを呼びます
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                replenishmentHub.SendReplenishments();
            }
        }


        // エラー時のテーブルの依存関係
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(ReplenishmentsModel)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
