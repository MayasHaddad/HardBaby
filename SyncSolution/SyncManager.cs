using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data.SqlServer;
using System.Collections.ObjectModel;

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
        public DbSyncScopeDescription ScopeDesc;
        public SyncOrchestratorOP SyncOrchestrator;
        public SyncOperationStatistics SyncStats;
        public SqlSyncScopeProvisioning ServerConfig;
        public SqlSyncScopeProvisioning ClientSqlConfig;

        public void ConnectToServerAndClient()
        // Connects to SQL Server and Client
        {
            this.ServerConn = new SqlConnection(this.ServerConnectionString);
            this.ClientSqlConn = new SqlConnection(this.ClientConnectionString);
        }

        public void SetScopeDescription(string scopeName)
        {
            this.ScopeDesc = new DbSyncScopeDescription(scopeName);
            this.ScopeName = scopeName;
        }

        public void InitSyncOrchestrator()
        {
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
            this.ClientSqlConn.Close();
            this.ClientSqlConn.Dispose();
        }

        public SyncManager(string serverIPAdress, string serverDatabaseName, string clientIPAdress, string clientDatabaseName, string userName, string password)
        {
            this.ServerConnectionString = @"Data Source=" + serverIPAdress + "; Initial Catalog=" + serverDatabaseName + "; Integrated Security=True";
            if (password == null)
            {
                this.ClientConnectionString = @"Data Source=" + clientIPAdress + "; Initial Catalog=" + clientDatabaseName + ";  Integrated Security=True";
            }
            else
            {
                this.ClientConnectionString = @"Data Source=" + clientIPAdress + "; Initial Catalog=" + clientDatabaseName + ";  User ID=" + userName + "; Pwd=" + password;
            }
        }
    }
}