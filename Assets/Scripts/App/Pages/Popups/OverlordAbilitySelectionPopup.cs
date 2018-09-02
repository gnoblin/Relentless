// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class OverlordAbilitySelectionPopup : IUIPopup
    {
        private const int ABILITY_LIST_SIZE = 5;

        private const int MAX_SELECTED_ABILITIES = 2;

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IDataManager _dataManager;

        private Button _continueButton;

        private Button _buyButton;

        private Button _cancelButton;

        private GameObject _abilitiesGroup;

        private TextMeshProUGUI _title;

        private TextMeshProUGUI _skillName;

        private TextMeshProUGUI _skillDescription;

        private Image _heroImage;

        private List<AbilityInstance> _abilities;

        private Hero _heroData;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _abilities = new List<AbilityInstance>();
        }

        public void Dispose()
        {
            foreach (AbilityInstance abilityInstance in _abilities)
            {
                abilityInstance.Dispose();
            }

            _abilities.Clear();
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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/OverlordAbilityPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _continueButton = Self.transform.Find("Button_Continue").GetComponent<Button>();
            _cancelButton = Self.transform.Find("Button_Cancel").GetComponent<Button>();

            _continueButton.onClick.AddListener(ContinueButtonOnClickHandler);
            _cancelButton.onClick.AddListener(CancelButtonOnClickHandler);

            _title = Self.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            _skillName = Self.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
            _skillDescription = Self.transform.Find("SkillDescription").GetComponent<TextMeshProUGUI>();

            _abilitiesGroup = Self.transform.Find("Abilities").gameObject;

            _heroImage = Self.transform.Find("HeroImage").GetComponent<Image>();

            _abilities.Clear();

            for (int i = 0; i < ABILITY_LIST_SIZE; i++)
            {
                AbilityInstance abilityInstance = new AbilityInstance(_abilitiesGroup.transform);
                abilityInstance.SelectionChanged += AbilityInstanceOnSelectionChanged;
                _abilities.Add(abilityInstance);
            }
        }

        public void Show(object data)
        {
            Show();

            FillInfo((Hero)data);
            _abilities[0].IsSelected = true;
            AbilityInstanceOnSelectionChanged(_abilities[0]);
        }

        public void Update()
        {
        }

        public void ContinueButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            OnHidePopupEvent?.Invoke();

            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();
        }

        public void CancelButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _uiManager.HidePopup<OverlordAbilitySelectionPopup>();
        }

        private void FillInfo(Hero heroData)
        {
            _heroData = heroData;
            _heroImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroData.element.ToLower());
            _heroImage.SetNativeSize();

            for (int i = 0; i < ABILITY_LIST_SIZE; i++)
            {
                HeroSkill skill = null;
                if (i < heroData.skills.Count)
                {
                    skill = heroData.skills[i];
                }

                _abilities[i].Skill = skill;
                _abilities[i].AllowMultiSelect = false;
            }
        }

        private void AbilityInstanceOnSelectionChanged(AbilityInstance ability)
        {
            _skillName.text = ability.Skill.title;
            _skillDescription.text = ability.Skill.description;
        }

        private class AbilityInstance : IDisposable
        {
            public event Action<AbilityInstance> SelectionChanged;

            public readonly GameObject SelfObject;

            private readonly ILoadObjectsManager _loadObjectsManager;

            private readonly Toggle _abilityToggle;

            private readonly Image _glowImage;

            private readonly Image _abilityIconImage;

            private readonly Transform _parentGameObject;

            private HeroSkill _skill;

            private bool _allowMultiSelect;

            private bool _isSelected;

            public AbilityInstance(Transform root)
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                _parentGameObject = root;
                SelfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/OverlordAbilityPopupAbilityItem"), root, false);

                _abilityToggle = SelfObject.GetComponent<Toggle>();
                _abilityToggle.group = root.GetComponent<ToggleGroup>();

                _glowImage = SelfObject.transform.Find("Glow").GetComponent<Image>();
                _abilityIconImage = SelfObject.transform.Find("AbilityIcon").GetComponent<Image>();

                _abilityToggle.onValueChanged.AddListener(OnToggleValueChanged);

                UpdateUIState();
            }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    _abilityToggle.isOn = value;
                }
            }

            public bool AllowMultiSelect
            {
                get => _allowMultiSelect;
                set => _abilityToggle.group = value?null:_parentGameObject.GetComponent<ToggleGroup>();
            }

            public HeroSkill Skill
            {
                get => _skill;
                set
                {
                    if (_skill == value)
                    
return;

                    _skill = value;
                    UpdateUIState();
                }
            }

            public void Dispose()
            {
                Object.Destroy(SelfObject);
            }

            private void OnToggleValueChanged(bool selected)
            {
                _isSelected = selected;
                UpdateUIState();

                SelectionChanged?.Invoke(this);
            }

            private void UpdateUIState()
            {
                _glowImage.gameObject.SetActive(_isSelected);

                _abilityToggle.interactable = Skill != null;
                if (Skill != null)
                {
                    _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/" + Skill.iconPath);
                } else
                {
                    _abilityIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/Icons/overlordability_locked");
                }
            }
        }
    }
}
