using System;
using System.Data.SqlClient;
using System.IO;
using System.Data.SqlServerCe;
using System.Data;
public class Utility
{

    // ---------  BEGIN CONNECTION STRINGS ALL FOR SAMPLES ----------- //

    // Set the connection strings for samples with servers or clients that 
    // use DbSyncProvider. 
    public static string ConnStr_DbSync1
    {

        get { return "Data Source=localhost; Initial Catalog=SyncSamplesDb_Peer1; Integrated Security=True"; }

    }

    public static string ConnStr_DbSync2
    {

        get { return "Data Source=local; Initial Catalog=SyncSamplesDb_Peer2; Integrated Security=True"; }

    }

    public static string ConnStr_DbSync3
    {

        get { return "Data Source=localhost; Initial Catalog=SyncSamplesDb_Peer3; Integrated Security=True"; }

    }

    // Set the connection strings for samples with clients that 
    // use SqlCeSyncProvider. 
    public static string ConnStr_SqlCeSync1
    {

        get { return @"Data Source='SyncSampleClient1.sdf'"; }

    }


    public static string ConnStr_SqlCeSync2
    {

        get { return @"Data Source='SyncSampleClient2.sdf'"; }

    }

    // Set the connection strings for samples with servers or clients that 
    // use SqlSyncProvider. 
    public static string ConnStr_SqlSync_Server
    {

        get { return @"Data Source=localhost; Initial Catalog=SyncSamplesDb_SqlPeer1; Integrated Security=True"; }

    }

    public static string ConnStr_SqlSync_Client
    {

        get { return "Data Source=localhost; Initial Catalog=SyncSamplesDb_SqlPeer2; Integrated Security=True"; } // For remote tests server add : "Data Source : srv-ds\\sqlserver2012 User ID=mayas-had; Pwd=Ma11asH"

    }

    public static string ConnStr_SqlAzure_Server
    {

        get { return @"Server=<server_name>;Database=SyncSamplesDbSqlAzure1;User ID=<user_id>;Password=<password>;Trusted_Connection=False;Encrypt=False;"; }

    }


    // Set the password and connection string for samples with clients 
    // that use SqlCeClientSyncProvider.    
    private static string _clientPassword;

    public static string Password_SqlCeClientSync
    {
        get { return _clientPassword; }
        set { _clientPassword = value; }
    }

    public static void SetPassword_SqlCeClientSync()
    {
        Console.WriteLine("Type a strong password for the client");
        Console.WriteLine("database, and then press Enter.");
        Utility.Password_SqlCeClientSync = Console.ReadLine();
    }

    public static string ConnStr_SqlCeClientSync
    {
        get { return @"Data Source='SyncSampleClient.sdf'; Password=" + Utility.Password_SqlCeClientSync; }
    }

    // Set the connection string for samples with servers that 
    // use DbServerSyncProvider. 
    private static string _serverName = "localhost";
    private static string _serverDbName = "SyncSamplesDb";

    public static void SetServerAndDb_DbServerSync(string serverName, string serverDbName)
    {
        _serverName = serverName;
        _serverDbName = serverDbName;
    }

    public static string ConnStr_DbServerSync
    {

        get { return "Data Source=" + _serverName + "; Initial Catalog=" + _serverDbName + "; Integrated Security=True"; }

    }

    // -----------  END CONNECTION STRINGS ALL FOR SAMPLES ----------- //




    /* ----  BEGIN CODE FOR SQLSYNCPROVIDER, SQLCESYNCPROVIDER, ---- //
       ---------------  AND DBSYNCPROVIDER SAMPLES   --------------- */


    public static void MakeDataChangesOnNode(string nodeConnString, string tableName)
    {
        int rowCount = 0;

        using (SqlConnection nodeConn = new SqlConnection(nodeConnString))
        {
            SqlCommand sqlCommand = nodeConn.CreateCommand();

            if (tableName == "Customer")
            {

                if (nodeConnString == Utility.ConnStr_SqlSync_Server)
                {
                    sqlCommand.CommandText =
                    "INSERT INTO Sales.Customer (CustomerName, SalesPerson, CustomerType) " +
                    "VALUES ('Cycle Mart', 'James Bailey', 'Retail') " +

                    "UPDATE Sales.Customer " +
                    "SET  SalesPerson = 'James Bailey' " +
                    "WHERE CustomerName = 'Tandem Bicycle Store' " +

                    "DELETE FROM Sales.Customer WHERE CustomerName = 'Sharp Bikes'";
                }

                else if (nodeConnString == Utility.ConnStr_DbSync1)
                {
                    sqlCommand.CommandText =
                    "INSERT INTO Sales.Customer (CustomerName, SalesPerson, CustomerType) " +
                    "VALUES ('Cycle Mart', 'James Bailey', 'Retail')";
                }

                else if (nodeConnString == Utility.ConnStr_DbSync2)
                {
                    sqlCommand.CommandText =
                    "UPDATE Sales.Customer " +
                    "SET  SalesPerson = 'James Bailey' " +
                    "WHERE CustomerName = 'Tandem Bicycle Store' ";
                }

                else if (nodeConnString == Utility.ConnStr_DbSync3)

                    sqlCommand.CommandText =
                    "DELETE FROM Sales.Customer WHERE CustomerName = 'Sharp Bikes'";
            }

            else if (tableName == "CustomerContact")
            {

                if (nodeConnString == Utility.ConnStr_SqlSync_Server)
                {
                    sqlCommand.CommandText =
                    "DELETE FROM Sales.CustomerContact WHERE PhoneType = 'Mobile'";
                }

            }

            nodeConn.Open();
            rowCount = sqlCommand.ExecuteNonQuery();
            nodeConn.Close();
        }

        // rowCount/2 is used because a row is inserted or updated in the 
        // metadata table for every insert, update, or delete in the base table.
        Console.WriteLine("Total rows inserted, updated, or deleted at all nodes: " + rowCount / 2);
    }

    public static void MakeConflictingChangeOnNode(string nodeConnString, string tableName)
    {
        int rowCount = 0;

        using (SqlConnection nodeConn = new SqlConnection(nodeConnString))
        {
            SqlCommand sqlCommand = nodeConn.CreateCommand();

            if (tableName == "Customer")
            {

                if (nodeConnString == Utility.ConnStr_SqlSync_Server)
                {
                    sqlCommand.CommandText =
                    "UPDATE Sales.Customer " +
                    "SET  SalesPerson = 'ChangeFromNodeOne' " +
                    "WHERE CustomerName = 'Tandem Bicycle Store' ";
                }

                else if (nodeConnString == Utility.ConnStr_SqlSync_Client)
                {
                    sqlCommand.CommandText =
                    "UPDATE Sales.Customer " +
                    "SET  SalesPerson = 'ChangeFromNodeTwo' " +
                    "WHERE CustomerName = 'Tandem Bicycle Store' ";
                }

            }

            nodeConn.Open();
            rowCount = sqlCommand.ExecuteNonQuery();
            nodeConn.Close();
        }

        // rowCount/2 is used because a row is inserted or updated in the 
        // metadata table for every insert, update, or delete in the base table.
        Console.WriteLine("Total rows inserted, updated, or deleted at all nodes: " + rowCount / 2);
    }

    public static void MakeFailingChangeOnNode(string nodeConnString)
    {
        int rowCount = 0;

        using (SqlConnection nodeConn = new SqlConnection(nodeConnString))
        {
            SqlCommand sqlCommand = nodeConn.CreateCommand();

            if (nodeConnString == Utility.ConnStr_SqlSync_Client)
            {
                sqlCommand.CommandText =
                "DELETE FROM Sales.Customer " +
                "WHERE CustomerName = 'Rural Cycle Emporium'";
            }

            nodeConn.Open();
            rowCount = sqlCommand.ExecuteNonQuery();
            nodeConn.Close();
        }

        Console.WriteLine("Total rows inserted, updated, or deleted at all nodes: " + rowCount / 2);
    }

    public static void CleanUpNode(string nodeConnString)
    {
        using (SqlConnection nodeConn = new SqlConnection(nodeConnString))
        {
            SqlCommand sqlCommand = nodeConn.CreateCommand();
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.CommandText = "usp_ResetPeerData";

            nodeConn.Open();
            sqlCommand.ExecuteNonQuery();
            nodeConn.Close();
        }
    }

    public static void CleanUpSqlNode(string nodeConnString)
    {
        using (SqlConnection nodeConn = new SqlConnection(nodeConnString))
        {
            SqlCommand sqlCommand = nodeConn.CreateCommand();
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.CommandText = "usp_CleanupAfterAppRun";

            nodeConn.Open();
            sqlCommand.ExecuteNonQuery();
            nodeConn.Close();
        }
    }

    // ---- The rest of the code in this section is related to backup 
    // ---- and restore of a SQL Server database.

    // Return the path to the SQL Server backup file (.bak). Change this path if your
    // backup directory is set to something other than the default.
    public static string DatabaseBackupFilePath
    {
        get { return @"C:\Program Files\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQL\Backup\SyncSamplesDb_SqlPeer1.bak"; }
    }

    public static void CreateDatabaseBackup()
    {

        Console.Write("BACKING UP SERVER DATABASE...");

        // Connect to SyncSamplesDb_SqlNode2 (Utility.ConnStr_SqlSync_Client) and back up
        // SyncSamplesDb_SqlNode1 by calling usp_SampleDbBackupRestore.

        using (SqlConnection conn = new SqlConnection(Utility.ConnStr_SqlSync_Client))
        {
            conn.Open();

            SqlCommand backupCommand = new SqlCommand();
            backupCommand.CommandType = CommandType.StoredProcedure;
            backupCommand.CommandText = "usp_SampleDbBackupRestore";
            backupCommand.Parameters.Add("@Operation", SqlDbType.VarChar).Value = "BACKUP";
            backupCommand.Connection = conn;
            backupCommand.ExecuteNonQuery();

            conn.Close();
            conn.Dispose();
        }

        Console.WriteLine("COMPLETED");
        Console.WriteLine(String.Empty);

    }

    public static void RestoreDatabaseFromBackup()
    {

        Console.Write("RESTORING SERVER DATABASE...");

        // Connect to SyncSamplesDb_SqlNode2 (Utility.ConnStr_SqlSync_Client) and restore
        // SyncSamplesDb_SqlNode1 by calling usp_SampleDbBackupRestore. This procedure
        // was created in SyncSamplesDb_SqlNode2 because holding a connection to SyncSamplesDb_SqlNode1
        // does not allow the database to be restored.
        using (SqlConnection conn = new SqlConnection(Utility.ConnStr_SqlSync_Client))
        {
            conn.Open();

            SqlCommand backupCommand = new SqlCommand();
            backupCommand.CommandType = CommandType.StoredProcedure;
            backupCommand.CommandText = "usp_SampleDbBackupRestore";
            backupCommand.Parameters.Add("@Operation", SqlDbType.VarChar).Value = "RESTORE";
            backupCommand.Connection = conn;
            backupCommand.ExecuteNonQuery();

            conn.Close();
            conn.Dispose();
        }

        Console.WriteLine("COMPLETED");

    }

    public static void DeleteDatabaseBackup()
    {
        if (File.Exists(Utility.DatabaseBackupFilePath))
        {
            File.Delete(Utility.DatabaseBackupFilePath);
        }
    }

    /* ----   END CODE FOR SQLSYNCPROVIDER, SQLCESYNCPROVIDER,  ---- //
       ---------------  AND DBSYNCPROVIDER SAMPLES   --------------- */




    // ----------  BEGIN CODE RELATED TO SQL SERVER COMPACT --------- //

    public static void DeleteAndRecreateCompactDatabase(string sqlCeConnString, bool recreateDatabase)
    {

        using (SqlCeConnection clientConn = new SqlCeConnection(sqlCeConnString))
        {
            if (File.Exists(clientConn.Database))
            {
                File.Delete(clientConn.Database);
            }
        }

        if (recreateDatabase == true)
        {
            SqlCeEngine sqlCeEngine = new SqlCeEngine(sqlCeConnString);
            sqlCeEngine.CreateDatabase();
        }

    }

    // ----------  END CODE RELATED TO SQL SERVER COMPACT --------- //




    /* ----------  BEGIN CODE FOR DBSERVERSYNCPROVIDER AND --------- //
       ----------      SQLCECLIENTSYNCPROVIDER SAMPLES     --------- */

    public static void MakeDataChangesOnServer(string tableName)
    {
        int rowCount = 0;

        using (SqlConnection serverConn = new SqlConnection(Utility.ConnStr_DbServerSync))
        {
            SqlCommand sqlCommand = serverConn.CreateCommand();

            if (tableName == "Customer")
            {
                sqlCommand.CommandText =
                    "INSERT INTO Sales.Customer (CustomerName, SalesPerson, CustomerType) " +
                    "VALUES ('Cycle Mart', 'James Bailey', 'Retail') " +

                    "UPDATE Sales.Customer " +
                    "SET  SalesPerson = 'James Bailey' " +
                    "WHERE CustomerName = 'Tandem Bicycle Store' " +

                    "DELETE FROM Sales.Customer WHERE CustomerName = 'Sharp Bikes'";
            }

            else if (tableName == "CustomerContact")
            {
                sqlCommand.CommandText =
                    "DECLARE @CustomerId uniqueidentifier " +
                    "DECLARE @InsertString nvarchar(1024) " +
                    "SELECT @CustomerId = CustomerId FROM Sales.Customer " +
                    "WHERE CustomerName = 'Tandem Bicycle Store' " +
                    "SET @InsertString = " +
                    "'INSERT INTO Sales.CustomerContact (CustomerId, PhoneNumber, PhoneType) " +
                    "VALUES (''' + CAST(@CustomerId AS nvarchar(38)) + ''', ''959-555-2021'', ''Fax'')' " +
                    "EXECUTE sp_executesql @InsertString " +

                    "SELECT @CustomerId = CustomerId FROM Sales.Customer " +
                    "WHERE CustomerName = 'Rural Cycle Emporium' " +
                    "SET @InsertString = " +
                    "'UPDATE Sales.CustomerContact SET PhoneNumber = ''158-555-0142'' " +
                    "WHERE (CustomerId = ''' + CAST(@CustomerId AS nvarchar(38)) + ''' AND PhoneType = ''Business'')' " +
                    "EXECUTE sp_executesql @InsertString " +

                    "DELETE FROM Sales.CustomerContact WHERE PhoneType = 'Mobile'";
            }

            else if (tableName == "CustomerAndOrderHeader")
            {
                //Specify the number of rows to insert into the Customer
                //and OrderHeader tables.
                sqlCommand.CommandText = "usp_InsertCustomerAndOrderHeader ";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add("@customer_inserts", SqlDbType.Int);
                sqlCommand.Parameters["@customer_inserts"].Value = 13;
                sqlCommand.Parameters.Add("@orderheader_inserts", SqlDbType.Int);
                sqlCommand.Parameters["@orderheader_inserts"].Value = 33;
                sqlCommand.Parameters.Add("@sets_of_inserts", SqlDbType.Int);
                sqlCommand.Parameters["@sets_of_inserts"].Value = 2;
            }

            else if (tableName == "Vendor")
            {
                sqlCommand.CommandText =
                    "INSERT INTO Sales.Vendor (VendorName, CreditRating, PreferredVendor) " +
                    "VALUES ('Victory Bikes', 4, 0) " +

                    "UPDATE Sales.Vendor " +
                    "SET CreditRating = 2 " +
                    "WHERE VendorName = 'Metro Sport Equipment'" +

                    "DELETE FROM Sales.Vendor " +
                    "WHERE VendorName = 'Premier Sport, Inc.'";
            }

            serverConn.Open();
            rowCount = sqlCommand.ExecuteNonQuery();
            serverConn.Close();
        }

        Console.WriteLine("Rows inserted, updated, or deleted at the server: " + rowCount);
    }

    //Get a dataset to use for schema creation.
    public static DataSet CreateDataSetFromServer()
    {
        DataSet dataSet = new DataSet();
        SqlDataAdapter dataAdapter = new SqlDataAdapter();

        using (SqlConnection serverConn = new SqlConnection(Utility.ConnStr_DbServerSync))
        {
            SqlCommand createDataSet = serverConn.CreateCommand();
            createDataSet.CommandText =
                "SELECT OrderId, CustomerId, OrderDate " +
                "FROM Sales.OrderHeader";

            serverConn.Open();
            dataAdapter.SelectCommand = createDataSet;
            dataAdapter.FillSchema(dataSet, SchemaType.Source);
            serverConn.Close();
        }

        return dataSet;
    }

    //Revert changes that were made during synchronization.
    public static void CleanUpServer()
    {
        using (SqlConnection serverConn = new SqlConnection(Utility.ConnStr_DbServerSync))
        {
            SqlCommand sqlCommand = serverConn.CreateCommand();
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.CommandText = "usp_InsertSampleData";

            serverConn.Open();
            sqlCommand.ExecuteNonQuery();
            serverConn.Close();
        }
    }


    //Create the Customer table on the client.
    public static void CreateTableOnClient()
    {
        using (SqlCeConnection clientConn = new SqlCeConnection(Utility.ConnStr_SqlCeClientSync))
        {
            SqlCeCommand createTable = clientConn.CreateCommand();
            createTable.CommandText =
                "CREATE TABLE Customer( " +
                "CustomerId uniqueidentifier NOT NULL " +
                    "PRIMARY KEY DEFAULT NEWID(), " +
                "CustomerName nvarchar(100) NOT NULL, " +
                "SalesPerson nvarchar(100) NOT NULL, " +
                "CustomerType nvarchar(100) NOT NULL, " +
                "SalesNotes nvarchar(1000) NULL)";

            clientConn.Open();
            createTable.ExecuteNonQuery();
            clientConn.Close();
        }
    }

    //Add DEFAULT constraints on the client.
    public static void MakeSchemaChangesOnClient(IDbConnection clientConn, IDbTransaction clientTran, string tableName)
    {

        //Execute the command over the same connection and within
        //the same transaction that Sync Framework uses
        //to create the schema on the client.
        SqlCeCommand alterTable = new SqlCeCommand();
        alterTable.Connection = (SqlCeConnection)clientConn;
        alterTable.Transaction = (SqlCeTransaction)clientTran;
        alterTable.CommandText = String.Empty;

        //Execute the command, then leave the transaction and 
        //connection open. The client provider will commit and close.
        switch (tableName)
        {
            case "Customer":
                alterTable.CommandText =
                    "ALTER TABLE Customer " +
                    "ADD CONSTRAINT DF_CustomerId " +
                    "DEFAULT NEWID() FOR CustomerId";
                alterTable.ExecuteNonQuery();
                break;

            case "OrderHeader":
                alterTable.CommandText =
                    "ALTER TABLE OrderHeader " +
                    "ADD CONSTRAINT DF_OrderId " +
                    "DEFAULT NEWID() FOR OrderId";
                alterTable.ExecuteNonQuery();

                alterTable.CommandText =
                    "ALTER TABLE OrderHeader " +
                    "ADD CONSTRAINT DF_OrderDate " +
                    "DEFAULT GETDATE() FOR OrderDate";
                alterTable.ExecuteNonQuery();
                break;

            case "OrderDetail":
                alterTable.CommandText =
                    "ALTER TABLE OrderDetail " +
                    "ADD CONSTRAINT DF_Quantity " +
                    "DEFAULT 1 FOR Quantity";
                alterTable.ExecuteNonQuery();
                break;

            case "Vendor":
                alterTable.CommandText =
                    "ALTER TABLE Vendor " +
                    "ADD CONSTRAINT DF_VendorId " +
                    "DEFAULT NEWID() FOR VendorId";
                alterTable.ExecuteNonQuery();
                break;
        }

    }

    public static void MakeDataChangesOnClient(string tableName)
    {
        int rowCount = 0;

        using (SqlCeConnection clientConn = new SqlCeConnection(Utility.ConnStr_SqlCeClientSync))
        {

            SqlCeCommand sqlCeCommand = clientConn.CreateCommand();

            clientConn.Open();

            if (tableName == "Customer")
            {
                sqlCeCommand.CommandText =
                    "INSERT INTO Customer (CustomerName, SalesPerson, CustomerType) " +
                    "VALUES ('Cycle Merchants', 'Brenda Diaz', 'Wholesale') ";
                rowCount = sqlCeCommand.ExecuteNonQuery();

                sqlCeCommand.CommandText =
                    "UPDATE Customer " +
                    "SET SalesPerson = 'Brenda Diaz' " +
                    "WHERE CustomerName = 'Exemplary Cycles'";
                rowCount += sqlCeCommand.ExecuteNonQuery();

                sqlCeCommand.CommandText =
                    "DELETE FROM Customer " +
                    "WHERE CustomerName = 'Aerobic Exercise Company'";
                rowCount += sqlCeCommand.ExecuteNonQuery();
            }

            else if (tableName == "Vendor")
            {
                sqlCeCommand.CommandText =
                    "INSERT INTO Vendor (VendorName, CreditRating, PreferredVendor) " +
                    "VALUES ('Cycling Master', 2, 1) ";
                rowCount = sqlCeCommand.ExecuteNonQuery();

                sqlCeCommand.CommandText =
                    "UPDATE Vendor " +
                    "SET CreditRating = 2 " +
                    "WHERE VendorName = 'Mountain Works'";
                rowCount += sqlCeCommand.ExecuteNonQuery();

                sqlCeCommand.CommandText =
                    "DELETE FROM Vendor " +
                    "WHERE VendorName = 'Compete, Inc.'";
                rowCount += sqlCeCommand.ExecuteNonQuery();
            }

            clientConn.Close();

        }

        Console.WriteLine("Rows inserted, updated, or deleted at the client: " + rowCount);
    }

    //Make a change at the client that fails when it is
    //applied at the server.
    public static void MakeFailingChangeOnClient()
    {
        int rowCount = 0;

        using (SqlCeConnection clientConn = new SqlCeConnection(Utility.ConnStr_SqlCeClientSync))
        {

            SqlCeCommand sqlCeCommand = clientConn.CreateCommand();

            clientConn.Open();

            sqlCeCommand.CommandText =
                "DELETE FROM Customer " +
                "WHERE CustomerName = 'Rural Cycle Emporium'";
            rowCount += sqlCeCommand.ExecuteNonQuery();

            clientConn.Close();

        }

        Console.WriteLine("Rows deleted at the client that will fail at the server: " + rowCount);
    }

    //Make changes at the client and server that conflict
    //when they are synchronized.
    public static void MakeConflictingChangesOnClientAndServer()
    {
        int rowCount = 0;

        using (SqlConnection serverConn = new SqlConnection(Utility.ConnStr_DbServerSync))
        {
            SqlCommand sqlCommand = serverConn.CreateCommand();
            sqlCommand.CommandText =
                "INSERT INTO Sales.Customer (CustomerId, CustomerName, SalesPerson, CustomerType) " +
                "VALUES ('009aa4b6-3433-4136-ad9a-a7e1bb2528f7', 'Cycle Merchants', 'Brenda Diaz', 'Wholesale') " +

                "DELETE FROM Sales.Customer WHERE CustomerName = 'Aerobic Exercise Company' " +

                "UPDATE Sales.Customer " +
                "SET SalesPerson = 'James Bailey' " +
                "WHERE CustomerName = 'Sharp Bikes' " +

                "UPDATE Sales.Customer " +
                "SET CustomerType = 'Distributor' " +
                "WHERE CustomerName = 'Exemplary Cycles'";

            serverConn.Open();
            rowCount = sqlCommand.ExecuteNonQuery();
            serverConn.Close();
        }

        using (SqlCeConnection clientConn = new SqlCeConnection(Utility.ConnStr_SqlCeClientSync))
        {

            SqlCeCommand sqlCeCommand = clientConn.CreateCommand();

            clientConn.Open();

            sqlCeCommand.CommandText =
                "INSERT INTO Customer (CustomerId, CustomerName, SalesPerson, CustomerType) " +
                "VALUES ('009aa4b6-3433-4136-ad9a-a7e1bb2528f7', 'Cycle Merchants', 'James Bailey', 'Wholesale')";
            rowCount += sqlCeCommand.ExecuteNonQuery();

            sqlCeCommand.CommandText =
                "UPDATE Customer " +
                "SET CustomerType = 'Retail' " +
                "WHERE CustomerName = 'Aerobic Exercise Company'";
            rowCount += sqlCeCommand.ExecuteNonQuery();

            sqlCeCommand.CommandText =
               "DELETE FROM Customer WHERE CustomerName = 'Sharp Bikes'";
            rowCount += sqlCeCommand.ExecuteNonQuery();

            sqlCeCommand.CommandText =
                "UPDATE Customer " +
                "SET CustomerType = 'Wholesale' " +
                "WHERE CustomerName = 'Exemplary Cycles'";
            rowCount += sqlCeCommand.ExecuteNonQuery();

            clientConn.Close();
        }

        Console.WriteLine("Conflicting rows inserted, updated, or deleted at the client or server: " + rowCount);
    }

    /* ----------  END CODE FOR DBSERVERSYNCPROVIDER AND   --------- //
       ----------      SQLCECLIENTSYNCPROVIDER SAMPLES     --------- */
}