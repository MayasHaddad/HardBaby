using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Microsoft.Synchronization.Data.SqlServerCe;
using ClientDatabaseSchemaManager;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Gestimum.Synchronization
{
    class Synchronizer
    {
        static void Main(string[] args)
        {
            if (args.Length != 7 && args.Length != 8)
            {
                Console.Error.Write(
                    "Usage: " +
                    MethodBase.GetCurrentMethod().DeclaringType.Name + 
                    " ServerIPAdress ServerDatabaseName ClientIPAdress ClientDatabaseName IsClientSqlCompact IsNewReplication [UserName] [Password] "
                    );
                return;
            }

            string password = null;
            string serverIPAdress = args[0];
            string serverDatabaseName = args[1];
            string clientIPAdress = args[2];
            string clientDatabaseName = args[3];
            string userName = args[6];
            int newReplication = int.Parse(args[5]);
            int isClientCe = int.Parse(args[4]);

            if (args.Length == 8)
            {
                password = args[7];
            }
            SyncManager mySyncManager = new SyncManager(serverIPAdress,
                                                        serverDatabaseName, 
                                                        clientIPAdress, 
                                                        clientDatabaseName, 
                                                        userName,
                                                        password,
                                                        isClientCe);
                
            try
            {
                mySyncManager.ConnectToServerAndClient();

                mySyncManager.SetScopeDescription(userName+"vb");
                mySyncManager.InitSyncOrchestrator();

                if (newReplication != 0)
                // we perform a from-zero synchronization
                {
                    mySyncManager.DescribeTableSchema("dbo.ADRESSES");

                    mySyncManager.SetSqlScopeProvisioning(mySyncManager.ServerConn);

                    // Configure the scope and change-tracking infrastructure.
                    mySyncManager.ServerConfig.Apply();

                    // Provision the client database.
                    if (mySyncManager.ClientIsCompact)
                    {
                        mySyncManager.SetSqlCeScopeProvisioning(mySyncManager.ClientCeSqlConn);
                    }
                    else
                    {
                        mySyncManager.SetSqlScopeProvisioning(mySyncManager.ClientSqlConn);
                    }

                    mySyncManager.Synchronize();
                }
                else
                // Synchronize Only
                {
                    mySyncManager.Synchronize();
                }
            }
            finally
            {
                mySyncManager.EndAllConnections();
            }
        }
    }
}