namespace tec_parts_replenishment_transpor_web.Commons
{
    /// <summary>
    /// 空箱供給状態の列挙型
    /// </summary>
    public enum EnumEmptyBoxSupplyStatus : byte
    {
        /// <summary>
        /// 依頼中
        /// </summary>
        Requesting = 1,
        /// <summary>
        /// 準備完了
        /// </summary>
        Ready = 2,
        /// <summary>
        /// 運搬開始
        /// </summary>
        TransportationStart = 3,
        /// <summary>
        /// 運搬終了
        /// </summary>
        TransportationEnd = 4
    };
}    
