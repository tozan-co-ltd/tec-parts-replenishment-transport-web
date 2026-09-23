using tec_parts_replenishment_transpor_web.Models;
using tec_parts_replenishment_transpor_web.Repositories;

namespace tec_parts_replenishment_transpor_web.Commons
{
    /// <summary>
    /// エラー処理に関する関数
    /// </summary>
    public static class ErrorHandling
    {
        /// <summary>
        /// エラーメッセージ作成
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <remarks>エラーコードから表示用エラーメッセージを作成する</remarks>
        /// <returns>エラーメッセージ</returns>
        public static string CreateErrorMessage(string errorCode)
        {
            try
            {
                // 戻り値
                string errorMessage;

                MErrorMessagesModel mErrorMessagesModel = new()
                {
                    ErrorCode = errorCode,
                };

                // エラーメッセージ取得
                var getErrorMessages = GetErrorMessage(mErrorMessagesModel);

                // 表示用エラーメッセージ作成
                if (getErrorMessages.Count == 0)
                {
                    errorMessage = "E4001 SQLServerでエラーが発生しました。";
                }
                else
                {
                    var getErrorMessage = getErrorMessages[0];
                    errorMessage = getErrorMessage.ErrorCode + " " + getErrorMessage.ErrorMessage;
                }
                return errorMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// エラーメッセージ取得
        /// </summary>
        /// <param name="model">MErrorMessagesModel</param>
        /// <returns>エラーメッセージリスト</returns>
        public static List<MErrorMessagesModel> GetErrorMessage(MErrorMessagesModel model)
        {
            try
            {
                // SQL作成
                var sql = MErrorMessagesRepository.CreateSQLToGetErrorMessage(model.ErrorCode);
                // DB接続
                List<MErrorMessagesModel> strList = MErrorMessagesRepository.ConnectMErrorMessages(sql);
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
