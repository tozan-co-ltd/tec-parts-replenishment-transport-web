using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using tec_parts_replenishment_transpor_web.Models;
using tec_parts_replenishment_transpor_web.Repositories;

namespace tec_parts_replenishment_transpor_web.Commons
{
    /// <summary>
    /// 検証エラー処理に関する関数
    /// </summary>
    public static class ValidationPassword
    {
        /// <summary>
        /// パスワードをチェック
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<(bool isValid, string error)> ValidatePasswordForRegisterOutOfStock(string password)
        {
            string? errorMessage = null;

            try
            {
                // 取込フォルダパス
                var in_path = "password";

                // フォルダ名
                var folder_name = "parts_supply_request";

#if DEBUG
                // デバッグ
                // ...\tec_parts_replenishment_transpor_web\wwwroot\epss\
                var section = "developmentFolderPath";
                var rootPath = Directory.GetCurrentDirectory();
                var _folderPath = ConnectionCDriver.GetCDriverConnectionString(section, in_path, folder_name);
                var folderPath = Path.Combine(rootPath, _folderPath, "pw_parts.txt");
#else
                // 本番環境
                // C:\\Program Files\\epss\\pw_parts.txt
                var section = "productionFolderPath";
                var _folderPath = ConnectionCDriver.GetCDriverConnectionString(section, in_path, folder_name);
                var folderPath = Path.Combine(_folderPath, "pw_parts.txt");
#endif

                string fileContent = await System.IO.File.ReadAllTextAsync(folderPath);
                // 文字列を行に分割する
                string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                // 最初の行を取得する
                string firstLine = lines.Length > 0 ? lines[0] : "";
                // 半角数字4桁で設定
                bool isValidFormat = Regex.IsMatch(firstLine, @"^\d{4}$");

                if (!isValidFormat)
                {
                    // 「設定パスワードが正しくありません。設定ファイルを確認してください。」
                    errorMessage = ErrorHandling.CreateErrorMessage("E6001");
                    return (isValidFormat, errorMessage);
                }

                bool isValid = password == firstLine.Trim();
                if (!isValid)
                {
                    //「パスワードが正しくありません。正しい値を入力してください。」
                    errorMessage = ErrorHandling.CreateErrorMessage("E2005");
                }

                return (isValid, errorMessage);
            }
            catch (FileNotFoundException)
            {
                // 「設定ファイルが見つかりませんでした。」
                errorMessage = ErrorHandling.CreateErrorMessage("E6003");
                return (false, errorMessage);
            }
            catch (UnauthorizedAccessException)
            {
                // 「設定ファイルへのアクセスが拒否されました。」
                errorMessage = ErrorHandling.CreateErrorMessage("E6002");
                return (false, errorMessage);
            }
            catch (Exception)
            {
                // 他の例外を処理する
                errorMessage = ErrorHandling.CreateErrorMessage("E9999");
                return (false, errorMessage);
            }
        }
    }
}
