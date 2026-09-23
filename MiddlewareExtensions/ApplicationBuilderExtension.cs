using tec_parts_replenishment_transpor_web.SubscribeTableDependencies;

namespace tec_parts_replenishment_transpor_web.MiddlewareExtensions
{
    public static class ApplicationBuilderExtension
    {
        // ミドルウェアを作成とSubscribeTableDependencyのミドルウェア内のメソッド呼び出し
        public static void UseSqlTableDependency<T>(this IApplicationBuilder applicationBuilder, string connectionString)
            where T : ISubscribeTableDependency
        {
            var serviceProvider = applicationBuilder.ApplicationServices;
            var service = serviceProvider.GetService<T>();
            service.SubscribeTableDependency(connectionString);
        }
    }
}