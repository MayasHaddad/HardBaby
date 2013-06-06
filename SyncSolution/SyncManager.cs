using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data.SqlServer;
using System.Collections.ObjectModel;
using System.Data.SqlServerCe;
using Microsoft.Synchronization.Data.SqlServerCe;

namespace ClientDatabaseSchemaManager
{
    class SyncManager
    {
        public const string SyncDataBaseSchema = "dbo"; // name of the synchronization schema

        public string ServerConnectionString;
        public string ClientConnectionString;
        public string ScopeName;
        public SqlConnection ServerConn;
        public SqlConnection ClientSqlConn;
        public SqlCeConnection ClientCeSqlConn;
        public DbSyncScopeDescription ScopeDesc;
        public SyncOrchestratorOP SyncOrchestrator;
        public SyncOperationStatistics SyncStats;
        public SqlSyncScopeProvisioning ServerConfig;
        public SqlSyncScopeProvisioning ClientSqlConfig;
        public bool ClientIsCompact;

        public void ConnectToServerAndClient()
        // Connects to SQL Server and Client
        {
            this.ServerConn = new SqlConnection(this.ServerConnectionString);
            if (this.ClientIsCompact)
            {
                this.ClientCeSqlConn = new SqlCeConnection(this.ClientConnectionString);
                return;
            }
            this.ClientSqlConn = new SqlConnection(this.ClientConnectionString);
        }

        public void SetScopeDescription(string scopeName)
        {
            this.ScopeDesc = new DbSyncScopeDescription(scopeName);
            this.ScopeName = scopeName;
        }

        public void InitSyncOrchestrator()
        {
            if(this.ClientIsCompact)
            {
                this.SyncOrchestrator = new SyncOrchestratorOP(
                        new SqlCeSyncProvider(this.ScopeName, this.ClientCeSqlConn, SyncDataBaseSchema),
                        new SqlSyncProvider(this.ScopeName, this.ServerConn, null, SyncDataBaseSchema)
                        );
                return;
            }
            this.SyncOrchestrator = new SyncOrchestratorOP(
                        new SqlSyncProvider(this.ScopeName, this.ClientSqlConn, null, SyncDataBaseSchema),
                        new SqlSyncProvider(this.ScopeName, this.ServerConn, null, SyncDataBaseSchema)
                        );
        }

        public void DescribeTableSchema(string tableName)
        // Definition for the whole table
        {
            DbSyncTableDescription customerDescription =
                        SqlSyncDescriptionBuilder.GetDescriptionForTable(tableName, this.ServerConn);

            this.ScopeDesc.Tables.Add(customerDescription);
        }

        public void DescribePartialTableSchema(string tableName, Collection<string> columnsToInclude)
        // Definition for part of the table
        {
            DbSyncTableDescription customerContactDescription =
                        SqlSyncDescriptionBuilder.GetDescriptionForTable(tableName, columnsToInclude, this.ServerConn);

            this.ScopeDesc.Tables.Add(customerContactDescription);
        }

        public void SetSqlScopeProvisioning(SqlConnection connection)
        {
            if (connection.Equals(this.ServerConn))
            {
                // Create a provisioning object for ScopeDescription. 
                this.ServerConfig = new SqlSyncScopeProvisioning(this.ServerConn, this.ScopeDesc);
                this.ServerConfig.ObjectSchema = SyncDataBaseSchema;
            }
            else
            {
                // Provision the existing client database based on scope
                this.ClientSqlConfig = new SqlSyncScopeProvisioning(this.ClientSqlConn, this.ScopeDesc);

                this.ClientSqlConfig.ObjectSchema = SyncDataBaseSchema;
                this.ClientSqlConfig.Apply();
            }
        }

        public void SetSqlCeScopeProvisioning(SqlCeConnection connection)
        {
            DbSyncScopeDescription clientSqlCe1Desc = SqlSyncDescriptionBuilder.GetDescriptionForScope(ScopeName, null, SyncDataBaseSchema, this.ServerConn);
            SqlCeSyncScopeProvisioning clientSqlCe1Config = new SqlCeSyncScopeProvisioning(this.ClientCeSqlConn, clientSqlCe1Desc);
            clientSqlCe1Config.ObjectPrefix = SyncDataBaseSchema;
            clientSqlCe1Config.Apply();
        }

        public void AddFilterColumnsAndFilterClauses(string tableName, Dictionary<string, string> filterColumnsAndFilterClauses)
        {
            foreach (KeyValuePair<string, string> columnClausePair in filterColumnsAndFilterClauses)
            {
                // setting the filtering column
                this.ServerConfig.Tables[tableName].AddFilterColumn(columnClausePair.Key);
                // setting the filtering clause on that column
                this.ServerConfig.Tables[tableName].FilterClause = columnClausePair.Value;
            }
        }

        public void Synchronize()
        // Synchronizes the current Client DB with the SQL SERVER Data Base 
        {
            this.SyncStats = this.SyncOrchestrator.Synchronize();
        }

        public SyncOperationStatistics GetSyncStats()
        // Returns the most recent Synchronization Statistics
        {
            return this.SyncStats;
        }

        public void EndAllConnections()
        // Closes Server and Client connections
        {
            this.ServerConn.Close();
            this.ServerConn.Dispose();
            if (this.ClientIsCompact)
            {
                this.ClientCeSqlConn.Close();
                this.ClientCeSqlConn.Dispose();
                return;
            }
            this.ClientSqlConn.Close();
            this.ClientSqlConn.Dispose();
        }

        public SyncManager(string serverIPAdress, string serverDatabaseName, string clientIPAdress, string clientDatabaseName, string userName, string password, int isClientCe)
        {
            this.ServerConnectionString = @"Data Source=" + serverIPAdress + "; Initial Catalog=" + serverDatabaseName + "; Integrated Security=True";
            
            if (isClientCe == 1)
            {
                this.ClientIsCompact = true;
                this.ClientConnectionString = @"Data Source=C:\Users\user\Documents\Gestimum\" + clientDatabaseName + ".sdf; Encrypt Database=True;Password="+ password +";File Mode=shared read; Persist Security Info=False;";
                Console.WriteLine(this.ClientConnectionString);
                return;
            }

            this.ClientIsCompact = false;

            if (password == null)
            {
                this.ClientConnectionString = @"Data Source=" + clientIPAdress + "; Initial Catalog=" + clientDatabaseName + ";  Integrated Security=True";
                return;
            }
            
            this.ClientConnectionString = @"Data Source=" + clientIPAdress + "; Initial Catalog=" + clientDatabaseName + ";  User ID=" + userName + "; Pwd=" + password;
        }
    }
}