﻿using System;
using System.Collections;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Sharing;

#if UNITY_WSA && !UNITY_EDITOR
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Sharing;
#endif

public class ImportAnchorManager : AnchorManager<ImportAnchorManager>
{
    enum ImportState
    {
        Start,
        Failed,
        ReadyToImport,
        DataRequested,
        DataDownloadedReadyForImport,
        Importing,
        AnchorImportedAndLocked
    }

    ImportState currentState = ImportState.Start;

    byte[] rawAnchorData;

#if UNITY_WSA && !UNITY_EDITOR
 
  WorldAnchor worldAnchor;
 
#endif

    void Update()
    {
        if (SharingStage.Instance.IsConnected)
        {
            switch (currentState)
            {
#if UNITY_WSA && !UNITY_EDITOR
        case ImportState.Start:
          ConnectToRoom();
          this.currentState = ImportState.ReadyToImport;
          break;
        case ImportState.ReadyToImport:
          MakeAnchorDataRequest();
          break;
        case ImportState.DataDownloadedReadyForImport:
          // DataReady is set when the anchor download completes.
          currentState = ImportState.Importing;
          StatusTextDisplay.Instance.SetStatusText("importing room lock data");
 
          WorldAnchorTransferBatch.ImportAsync(rawAnchorData, ImportComplete);
          break;
#endif
            }
        }
    }
    protected override void AddRoomManagerHandlers()
    {
        base.AddRoomManagerHandlers();
        this.roomManagerListener.AnchorsDownloadedEvent += this.OnAnchorDonwloadCompleted;
    }

    protected override void OnDestroy()
    {
        if (roomManagerListener != null)
        {
            roomManagerListener.AnchorsDownloadedEvent -= OnAnchorDonwloadCompleted;
        }
        base.OnDestroy();
    }
    void OnAnchorDonwloadCompleted(
      bool successful,
      AnchorDownloadRequest request,
      XString failureReason)
    {
        // If we downloaded anchor data successfully we should import the data.
        if (successful)
        {
            StatusTextDisplay.Instance.SetStatusText(
              "room sync data downloaded");

            int datasize = request.GetDataSize();

            if (SharingStage.Instance.ShowDetailedLogs)
            {
                Debug.LogFormat("Anchor Manager: Anchor size: {0} bytes.", datasize.ToString());
            }

            rawAnchorData = new byte[datasize];

            request.GetData(rawAnchorData, datasize);

            currentState = ImportState.DataDownloadedReadyForImport;
        }
        else
        {
            StatusTextDisplay.Instance.SetStatusText(
              "retrying room lock request");

            // If we failed, we can ask for the data again.
            Debug.LogWarning("Anchor Manager: Anchor DL failed " + failureReason);

#if UNITY_WSA && !UNITY_EDITOR
      MakeAnchorDataRequest();
#endif
        }
    }

#if UNITY_WSA && !UNITY_EDITOR
 
  void MakeAnchorDataRequest()
  {
    StatusTextDisplay.Instance.SetStatusText("requesting sync data");
 
    if (roomManager.DownloadAnchor(currentRoom, currentRoom.GetAnchorName(0)))
    {
      currentState = ImportState.DataRequested;
    }
    else
    {
      Debug.LogError("Anchor Manager: Couldn't make the download request.");
 
      currentState = ImportState.Failed;
    }
  }
  void ImportComplete(SerializationCompletionReason status, WorldAnchorTransferBatch anchorBatch)
  {
    if (status == SerializationCompletionReason.Succeeded)
    {
      if (anchorBatch.GetAllIds().Length > 0)
      {
        string first = anchorBatch.GetAllIds()[0];
 
        if (SharingStage.Instance.ShowDetailedLogs)
        {
          Debug.Log("Anchor Manager: Sucessfully imported anchor " + first);
        }
        this.worldAnchor = anchorBatch.LockObject(first, gameObject);
 
        StatusTextDisplay.Instance.SetStatusText("room lock imported");
      }
 
      base.FireCompleted(true);
    }
    else
    {
      StatusTextDisplay.Instance.SetStatusText("retrying room lock import");
 
      Debug.LogError("Anchor Manager: Import failed");
 
      currentState = ImportState.DataDownloadedReadyForImport;
    }
  }
#endif // UNITY_WSA
}