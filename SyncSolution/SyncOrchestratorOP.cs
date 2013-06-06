using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using System;
public class SyncOrchestratorOP : SyncOrchestrator
{
    public SyncOrchestratorOP(RelationalSyncProvider localProvider, RelationalSyncProvider remoteProvider)
    {
        this.LocalProvider = localProvider;
        this.RemoteProvider = remoteProvider;
        this.Direction = SyncDirectionOrder.UploadAndDownload;
    }
}