using System.Linq.Expressions;

namespace tec_parts_replenishment_transpor_web.Commons
{
    /// <summary>
    /// Cドライブ接続に関する関数
    /// </summary>
    public static class ConnectionCDriver
    {
        /// <summary>
        /// Cドライブ接続文字列取得
        /// </summary>
        /// <param name="section">デバッグまたは本番環境</param>
        /// <param name="in_out">入力または出力</param>
        /// <param name="folder_name">フォルダ名</param>
        /// <returns>フォルダパス</returns>
        public static string GetCDriverConnectionString(string section, string in_out, string folder_name)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false);
                var configuration = builder.Build();
                var folderName = configuration.GetSection(section).GetValue<string>(in_out + ":" + folder_name);

                if (folderName != null)
                    return folderName;
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}