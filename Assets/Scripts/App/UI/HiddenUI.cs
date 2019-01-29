﻿using System;
using System.Linq;
using CodeStage.AdvancedFPSCounter;
using Opencoding.Console;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class HiddenUI : MonoBehaviour
    {
        public GameObject UIRoot;
        public BaseRaycaster[] AlwaysActiveRaycasters = new BaseRaycaster[0];

        private AFPSCounter _afpsCounter;
        private bool _isVisible;

        private bool ShouldBeVisible => _afpsCounter.OperationMode == OperationMode.Normal;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            _isVisible = UIRoot.gameObject.activeInHierarchy;
            _afpsCounter = FindObjectOfType<AFPSCounter>();
            if (_afpsCounter == null)
                throw new Exception("AFPSCounter instance not found in scene");
        }

        void Update()
        {
            if (_isVisible != ShouldBeVisible)
            {
                BaseRaycaster[] raycasters = FindObjectsOfType<BaseRaycaster>();
                bool raycastersEnabled = !ShouldBeVisible;
                foreach (BaseRaycaster raycaster in raycasters)
                {
                    if (AlwaysActiveRaycasters.Contains(raycaster))
                        continue;

                    raycaster.enabled = raycastersEnabled;
                }

                // Update UI
                if (this.UIRoot != null)
                {
                    UIRoot.gameObject.SetActive(ShouldBeVisible);
                }

                _isVisible = ShouldBeVisible;
            }
        }

        #region UI Handlers

        public void SubmitBugReport()
        {
            UserReportingScript.Instance.CreateUserReport();
        }

        public void OpenDebugConsole()
        {
            _afpsCounter.OperationMode = OperationMode.Disabled;
            DebugConsole.IsVisible = true;
        }

        public void SkipTutorial()
        {
            GeneralCommandsHandler.SkipTutorialFlow();
        }

        #endregion
    }
}
