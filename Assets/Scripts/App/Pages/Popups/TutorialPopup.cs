// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class TutorialPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ITutorialManager _tutorialManager;

        private ISoundManager _soundManager;

        private TextMeshProUGUI _text;

        private GameObject _yesnoObject;

        private ButtonShiftingContent _nextButton, _playButton, _skipButton;

        private GameObject _focusedObject;

        private GameObject _bubbleObject;

        private Image _janeImage;

        private List<GameObject> _focusObjects;

        private Sprite[] _janePoses;

        private Button _buttonBack;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _soundManager = GameClient.Get<ISoundManager>();

            _janePoses = Resources.LoadAll<Sprite>("Images/Tutorial");

            _focusObjects = new List<GameObject>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
            
return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
            {
                Hide();
            }

            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TutorialPopup"));

            // _selfPage.transform.SetParent(GameObject.Find("CanvasTutorial").transform, false);
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _bubbleObject = Self.transform.Find("Description").gameObject;

            _text = Self.transform.Find("Description/Text").GetComponent<TextMeshProUGUI>();
            _focusedObject = Self.transform.Find("TutorialFocusObject").gameObject;

            _nextButton = Self.transform.Find("Button_Next").GetComponent<ButtonShiftingContent>();
            _playButton = Self.transform.Find("Button_Play").GetComponent<ButtonShiftingContent>();
            _skipButton = Self.transform.Find("Button_Skip").GetComponent<ButtonShiftingContent>();
            _buttonBack = Self.transform.Find("Button_Back").GetComponent<Button>();

            _janeImage = Self.transform.Find("NPC").GetComponent<Image>();

            _nextButton.onClick.AddListener(_tutorialManager.NextButtonClickHandler);
            _playButton.onClick.AddListener(_tutorialManager.NextButtonClickHandler);
            _skipButton.onClick.AddListener(SkipButtonOnClickHandler);
            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);

            _focusObjects.Clear();

            foreach (Transform obj in Self.transform.Find("FocusObjects").transform)
            {
                _focusObjects.Add(obj.gameObject);
            }

            _nextButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(false);
            _skipButton.gameObject.SetActive(true);
        }

        public void Show(object data)
        {
            Show();

            if (_tutorialManager.CurrentStep == 22)
            {
                _bubbleObject.SetActive(false);
                _tutorialManager.IsBubbleShow = false;
                GameClient.Get<ITimerManager>().AddTimer(ShowBubble, null, 6f, false);
            }

            _text.text = (string)data;
        }

        public void Update()
        {
        }

        public void UpdatePose(Enumerators.TutorialJanePoses pose)
        {
            _janeImage.sprite = _janePoses[(int)pose];
        }

        public void ShowBubble(object[] param)
        {
            _bubbleObject.SetActive(true);
            _tutorialManager.IsBubbleShow = true;
        }

        public void SetPosition(Vector2 position)
        {
            _focusedObject.transform.position = new Vector3(position.x, position.y, 0);
        }

        public void ShowTutorialFocus(int step)
        {
            HideTutorialFocus();

            // _focusObjects[step].SetActive(true);
            Self.transform.Find("FocusObjects/Step_" + step).gameObject.SetActive(true);
        }

        public void HideTutorialFocus()
        {
            foreach (GameObject obj in _focusObjects)
            {
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                }
            }

            _nextButton.gameObject.SetActive(false);
            _playButton.gameObject.SetActive(false);

            // _skipButton.gameObject.SetActive(false);
        }

        public void ShowNextButton()
        {
            _nextButton.gameObject.SetActive(true);
        }

        public void ShowQuestion()
        {
            _playButton.gameObject.SetActive(true);

            // _skipButton.gameObject.SetActive(true);
        }

        private void SkipButtonOnClickHandler()
        {
            _tutorialManager.SkipTutorial(Enumerators.AppState.DECK_SELECTION);
        }

        private void BackButtonOnClickHandler()
        {
            _tutorialManager.SkipTutorial(Enumerators.AppState.MAIN_MENU);
        }
    }
}
