﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemAbilityManager : SystemResourceManager {

    #region Singleton
    private static SystemAbilityManager instance;

    public static SystemAbilityManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<SystemAbilityManager>();
            }

            return instance;
        }
    }
    #endregion

    const string resourceClassName = "BaseAbility";

    protected override void Awake() {
        //Debug.Log(this.GetType().Name + ".Awake()");
        base.Awake();
    }

    public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
        rawResourceList = Resources.LoadAll<BaseAbility>(resourceClassName);
        base.LoadResourceList();
    }

    public BaseAbility GetResource(string resourceName) {
        //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
        if (!RequestIsEmpty(resourceName)) {
            string keyName = prepareStringForMatch(resourceName);
            if (resourceList.ContainsKey(keyName)) {
                return (resourceList[keyName] as BaseAbility);
            }
        }
        return null;
    }


    public List<BaseAbility> GetResourceList() {
        List<BaseAbility> returnList = new List<BaseAbility>();

        foreach (UnityEngine.Object listItem in resourceList.Values) {
            returnList.Add(listItem as BaseAbility);
        }
        return returnList;

    }
}