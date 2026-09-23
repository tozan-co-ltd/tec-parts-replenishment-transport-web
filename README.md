# tec-parts-replenishment-transport-web
テクノエイト部品補充管理アプリ(WEB)

SQL Server ブローカーを有効にする方法
1. 新しいクエリし、「SELECT is_broker_enabled FROM sys.databases WHERE name = '[DB NAME]'」を入力し、実行する 
2. 「ALTER DATABASE [DATABASE NAME] SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE」を入力し、実行する


