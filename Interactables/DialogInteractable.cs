﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogInteractable : InteractableOption {

    private BoxCollider boxCollider;

    [SerializeField]
    private List<Dialog> dialogList;

    // put this in interactableoption?
    [SerializeField]
    private INamePlateUnit namePlateUnit;

    private int dialogIndex = -1;

    public int MyDialogIndex { get => dialogIndex; set => dialogIndex = value; }
    public List<Dialog> MyDialogList { get => dialogList; set => dialogList = value; }

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };
    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyDialogNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyDialogNamePlateImage : base.MyNamePlateImage); }

    protected override void Awake() {
        //Debug.Log("NameChangeInteractable.Awake()");
        base.Awake();
        namePlateUnit = GetComponent<INamePlateUnit>();

    }

    protected override void Start() {
        //Debug.Log("DialogInteractable.Start()");
        base.Start();
        boxCollider = GetComponent<BoxCollider>();
        CreateEventReferences();
        Spawn();
    }

    private void CreateEventReferences() {
        //Debug.Log("PlayerManager.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPrerequisiteUpdated += UpdateDialogStatus;
        if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
            Debug.Log(gameObject.name + ".DialogInteractable.CreateEventSubscriptions(): player unit is already spawned.");
            HandlePlayerUnitSpawn();
        } else {
            //Debug.Log(gameObject.name + ".DialogInteractable.CreateEventSubscriptions(): player unit is not yet spawned");
        }
        eventReferencesInitialized = true;
    }

    public override void CleanupEventReferences() {
        //Debug.Log("PlayerManager.CleanupEventReferences()");
        base.CleanupEventReferences();
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnPrerequisiteUpdated -= UpdateDialogStatus;
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
        }
        CleanupConfirm();
        eventReferencesInitialized = false;
    }

    public override void OnDisable() {
        //Debug.Log("PlayerManager.OnDisable()");
        base.OnDisable();
        CleanupEventReferences();
    }

    public void UpdateDialogStatus() {

        //Debug.Log(gameObject.name + ".DialogInteractable.UpdateDialogStatus()");
        if (PlayerManager.MyInstance.MyCharacter == null) {
            //Debug.Log(gameObject.name + ".DialogInteractable.UpdateDialogStatus(): player has no character");
            return;
        }
        if (namePlateUnit.MyNamePlate == null) {
            //Debug.Log(gameObject.name + ".DialogInteractable.UpdateDialogStatus(): nameplate is null!");
            return;
        }
        int validOptionCount = GetCurrentOptionList().Count;
        //Debug.Log(gameObject.name + ".DialogInteractable.UpdateDialogStatus(): validoptionCount: " + validOptionCount);
        if (validOptionCount == 0) {
            namePlateUnit.MyNamePlate.MyDialogIndicatorImage.gameObject.SetActive(false);
        } else {
            namePlateUnit.MyNamePlate.MyDialogIndicatorImage.gameObject.SetActive(true);
        }
        MiniMapStatusUpdateHandler(this);
    }

    public void HandlePlayerUnitSpawn() {
        //Debug.Log(gameObject.name + ".DialogInteractable.HandlePlayerUnitSpawn()");

        UpdateDialogStatus();
    }

    public void CleanupEventReferences(ICloseableWindowContents windowContents) {
        Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventReferences(ICloseableWindowContents)");
        CleanupEventReferences();
    }

    public override void HandleConfirmAction() {
        Debug.Log(gameObject.name + ".NameChangeInteractable.HandleConfirmAction()");
        base.HandleConfirmAction();

        // just to be safe
        CleanupConfirm();
    }

    public void CleanupConfirm() {
        if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.dialogWindow != null && PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents != null) {
            (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).OnConfirmAction -= HandleConfirmAction;
            (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).OnCloseWindowHandler -= CleanupConfirm;
        }
    }

    public void CleanupConfirm(ICloseableWindowContents contents) {
        CleanupConfirm();
    }

    private void Spawn() {
        //Debug.Log(gameObject.name + ".DialogInteractable.Spawn()");
        if (boxCollider != null) {
            boxCollider.enabled = true;
        }
        //interactable.InitializeMaterials();
        MiniMapStatusUpdateHandler(this);
    }

    private void DestroySpawn() {
        //Debug.Log(gameObject.name + ".NameChangeInteractable.DestroySpawn()");
        boxCollider.enabled = false;
        MiniMapStatusUpdateHandler(this);
    }

    public List<Dialog> GetCurrentOptionList() {
        //Debug.Log("DialogInteractable.GetValidOptionList()");
        List<Dialog> currentList = new List<Dialog>();
        foreach (Dialog dialog in dialogList) {
            if (SystemDialogManager.MyInstance.GetResource(dialog.MyName).MyPrerequisitesMet == true && SystemDialogManager.MyInstance.GetResource(dialog.MyName).TurnedIn == false) {
                currentList.Add(SystemDialogManager.MyInstance.GetResource(dialog.MyName));
            }
        }
        //Debug.Log("DialogInteractable.GetValidOptionList(): List Size: " + validList.Count);
        return currentList;
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".DialogInteractable.Interact()");
        List<Dialog> currentList = GetCurrentOptionList();
        if (currentList.Count == 0) {
            return false;
        } else if (currentList.Count == 1) {
            (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).Setup(currentList[0].MyName, this.interactable);
            (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).OnConfirmAction += HandleConfirmAction;
            (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).OnCloseWindowHandler += CleanupConfirm;
        } else {
            interactable.OpenInteractionWindow();
        }
        return true;
    }

    public override bool CanInteract(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".DialogInteractable.CanInteract()");
        if (!base.CanInteract(source)) {
            return false;
        }
        if (GetCurrentOptionList().Count == 0) {
            return false;
        }
        return true;

    }

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>

    public override void StopInteract() {
        base.StopInteract();
        PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
    }

    public override bool HasMiniMapText() {
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        if (!base.SetMiniMapText(text)) {
            text.text = "";
            text.color = new Color32(0, 0, 0, 0);
            return false;
        }
        text.text = "o";
        text.fontSize = 50;
        text.color = Color.white;
        return true;
    }

    public override int GetCurrentOptionCount() {
        return GetCurrentOptionList().Count;
    }

}
