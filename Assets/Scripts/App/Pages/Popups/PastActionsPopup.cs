using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PastActionsPopup : IUIPopup
    {
        public GameObject Self { get; private set; }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IGameplayManager _gameplayManager;

        private Transform _parentOfRightBlockElements;

        private Image _effectTypeImage;

        private ActionElement _leftBlockCardUnitElement,
                              _rightBlockCardUnitElement,
                              _leftBlockCardSpellElement,
                              _rightBlockCardSpellElement,
                              _leftBlockOverlordElement,
                              _rightBlockOverlordElement,
                              _leftBlockOverlordSkillElement;

        private List<ActionElement> _rightBlockElements;

        private Sprite _attackActionSprite,
                       _effectActionSprite;

        private PastActionParam _currentPastActionParam;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
        }

        public void Hide()
        {
            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;

            _currentPastActionParam = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
              _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PastActionPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _parentOfRightBlockElements = Self.transform.Find("Block_OnWho/Group_MultipleItems");

            _effectTypeImage = Self.transform.Find("Block_Effect/Image_Effect").GetComponent<Image>();

            _attackActionSprite = _loadObjectsManager.GetObjectByPath<Sprite>("battleground_past_action_bar_icon_attack");
            _effectActionSprite = _loadObjectsManager.GetObjectByPath<Sprite>("battleground_past_action_bar_icon_effect");

            Setup(_currentPastActionParam);
        }

        public void Show(object data)
        {
            if (data != null && data is PastActionParam param)
            {
                _currentPastActionParam = param;
            }

            Show();
        }

        public void Update()
        {
        }


        private void Setup(PastActionParam pastActionParam)
        {
            if (pastActionParam == null)
                return;

            _leftBlockCardUnitElement = new UnitCardElement(Self.transform.Find("Block_Who/Card_Unit").gameObject);
            _leftBlockCardSpellElement = new SpellCardElement(Self.transform.Find("Block_Who/Card_Spell").gameObject);
            _leftBlockOverlordElement = new OverlordElement(Self.transform.Find("Block_Who/Item_Overlord").gameObject);
            _leftBlockOverlordSkillElement = new OverlordSkillElement(Self.transform.Find("Block_Who/Item_OverlordSkill").gameObject);

            _rightBlockCardUnitElement = new UnitCardElement(Self.transform.Find("Block_OnWho/Card_Unit").gameObject, true);
            _rightBlockCardSpellElement = new SpellCardElement(Self.transform.Find("Block_OnWho/Card_Spell").gameObject, false);
            _rightBlockOverlordElement = new OverlordElement(Self.transform.Find("Block_OnWho/Item_Overlord").gameObject, true);

            if (_rightBlockElements != null)
            {
                foreach (ActionElement actionElement in _rightBlockElements)
                {
                    actionElement.Dispose();
                }
                _rightBlockElements.Clear();
                _rightBlockElements = null;
            }

            // setup center block

            if (pastActionParam.TargetEffects.Count > 0)
            {
                if (pastActionParam.ActionType.ToString().Contains("ATTACK"))
                {
                    _effectTypeImage.sprite = _attackActionSprite;
                }
                else
                {
                    _effectTypeImage.sprite = _effectActionSprite;
                }

                _effectTypeImage.enabled = true;
            }
            else
            {
                _effectTypeImage.enabled = false;
            }

            // setup left block

            switch (pastActionParam.Caller)
            {
                case Player player:
                    _leftBlockOverlordElement.Init(player);
                    break;
                case BoardSkill skill:
                    _leftBlockOverlordSkillElement.Init(skill);
                    break;
                case BoardUnitModel unit:
                    _leftBlockCardUnitElement.Init(unit.Card);
                    break;
                case SpellBoardCard spellBoardCard:
                    _leftBlockCardSpellElement.Init(spellBoardCard.WorkingCard);
                    break;
                case BoardSpell spell:
                    _leftBlockCardSpellElement.Init(spell.Card);
                    break;
                case UnitBoardCard unitBoardCard:
                    _leftBlockCardUnitElement.Init(unitBoardCard.WorkingCard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pastActionParam.Caller), pastActionParam.Caller, null);
            }

            // setup right block

            if (pastActionParam.TargetEffects.Count <= 0)
                return;

                if (pastActionParam.TargetEffects.Count > 1)
                {
                    _rightBlockElements = new List<ActionElement>();

                    TargetEffectParam actionWithPlayer = pastActionParam.TargetEffects.Find(targ => targ.Target is Player);

                    if (actionWithPlayer != null)
                    {
                        _rightBlockOverlordElement.Init((Player) actionWithPlayer.Target, actionWithPlayer.ActionEffectType);

                        foreach (TargetEffectParam targetEffect in pastActionParam.TargetEffects)
                        {
                            if (targetEffect == actionWithPlayer)
                                continue;

                            ActionElement actionElement;
                            switch (targetEffect.Target)
                            {
                                case BoardCard crd when crd is SpellBoardCard:
                                    actionElement = new SmallSpellCardElement(_parentOfRightBlockElements, true);
                                    actionElement.Init(crd.WorkingCard, targetEffect.ActionEffectType);
                                    break;
                                case BoardCard crd when crd is UnitBoardCard:
                                {
                                    actionElement = new SmallUnitCardElement(_parentOfRightBlockElements, true);
                                    actionElement.Init(crd.WorkingCard, targetEffect.ActionEffectType);
                                    break;
                                }
                                case BoardUnitModel unt:
                                    actionElement = new SmallUnitCardElement(_parentOfRightBlockElements, true);
                                    actionElement.Init(unt.Card, targetEffect.ActionEffectType);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(targetEffect.Target), targetEffect.Target, null);
                            }

                            _rightBlockElements.Add(actionElement);
                        }

                        _parentOfRightBlockElements.GetComponent<RectTransform>().anchoredPosition = new Vector2(520f, 0f);
                    }
                    else
                    {
                        foreach (TargetEffectParam targetEffect in pastActionParam.TargetEffects)
                        {
                            ActionElement actionElement;
                            switch (targetEffect.Target)
                            {
                                case BoardCard crd when crd is SpellBoardCard:
                                    actionElement = new SmallSpellCardElement(_parentOfRightBlockElements, true);
                                    actionElement.Init(crd.WorkingCard, targetEffect.ActionEffectType);
                                    break;
                                case BoardCard crd when crd is UnitBoardCard:
                                {
                                    actionElement = new SmallUnitCardElement(_parentOfRightBlockElements, true);
                                    actionElement.Init(crd.WorkingCard, targetEffect.ActionEffectType);
                                    break;
                                }
                                case BoardUnitModel unitModel:
                                    actionElement = new SmallUnitCardElement(_parentOfRightBlockElements, true);
                                    actionElement.Init(unitModel.Card, targetEffect.ActionEffectType);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(targetEffect.Target), targetEffect.Target, null);
                            }

                            _rightBlockElements.Add(actionElement);
                        }

                        _parentOfRightBlockElements.GetComponent<RectTransform>().anchoredPosition = new Vector2(20f, 0f);
                    }
                }
                else
                {
                    TargetEffectParam targetEffect = pastActionParam.TargetEffects[0];

                    switch (targetEffect.Target)
                    {
                        case Player pl:
                            _rightBlockOverlordElement.Init(pl, targetEffect.ActionEffectType);
                            break;
                        case BoardCard crd when crd is SpellBoardCard:
                            _rightBlockCardSpellElement.Init(crd.WorkingCard, targetEffect.ActionEffectType);
                            break;
                        case BoardCard crd when crd is UnitBoardCard:
                            _rightBlockCardUnitElement.Init(crd.WorkingCard, targetEffect.ActionEffectType);
                            break;
                        case BoardUnitModel unt:
                            _rightBlockCardUnitElement.Init(unt.Card, targetEffect.ActionEffectType);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(targetEffect.Target), targetEffect.Target, null);
                    }
                }
        }

        public class PastActionParam
        {
            public Enumerators.ActionType ActionType;
            public object Caller;
            public List<TargetEffectParam> TargetEffects;
        }

        public class TargetEffectParam
        {
            public object Target;
            public Enumerators.ActionEffectType ActionEffectType;
        }

        public class ActionElement
        {
            protected ILoadObjectsManager _loadObjectsManager;

            public ActionElement()
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            }

            public virtual void Init(WorkingCard workingCard, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None) { }

            public virtual void Init(BoardSkill skill, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None) { }

            public virtual void Init(Player player, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None) { }

            public virtual void Dispose() { }
        }

        public class UnitCardElement : ActionElement
        {
            private GameObject _selfObject;

            private TextMeshProUGUI _gooText,
                                    _titleText,
                                    _bodyText,
                                    _attackText,
                                    _defenseText;

            private Image _frameImage,
                          _unitTypeIconImage,
                          _pictureImage,
                          _effectImage;

            private bool _withEffect;

            public UnitCardElement(GameObject selfObject, bool withEffect = false)
            {
                _selfObject = selfObject;
                _withEffect = withEffect;

                _gooText = _selfObject.transform.Find("Text_Goo").GetComponent<TextMeshProUGUI>();
                _titleText = _selfObject.transform.Find("Text_Title").GetComponent<TextMeshProUGUI>();
                _bodyText = _selfObject.transform.Find("Text_Body").GetComponent<TextMeshProUGUI>();
                _attackText = _selfObject.transform.Find("Text_Attack").GetComponent<TextMeshProUGUI>();
                _defenseText = _selfObject.transform.Find("Text_Defense").GetComponent<TextMeshProUGUI>();

                _frameImage = _selfObject.transform.Find("Frame").GetComponent<Image>();
                _unitTypeIconImage = _selfObject.transform.Find("Image_UnitType").GetComponent<Image>();
                _pictureImage = _selfObject.transform.Find("Image_Mask/Image_Picture").GetComponent<Image>();

                if (_withEffect)
                {
                    _effectImage = _selfObject.transform.Find("Image_Effect").GetComponent<Image>();
                }

                _selfObject.SetActive(false);
            }

            public override void Init(WorkingCard workingCard, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None)
            {
                Card LibraryCard = workingCard.LibraryCard;

                _titleText.text = LibraryCard.Name;
                _bodyText.text = LibraryCard.Description;
                _gooText.text = LibraryCard.Cost.ToString();
                _attackText.text = LibraryCard.Damage.ToString();
                _defenseText.text = LibraryCard.Health.ToString();

                string rarity = Enum.GetName(typeof(Enumerators.CardRank), LibraryCard.CardRank);

                string setName = LibraryCard.CardSetType.ToString();

                string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

                if (!string.IsNullOrEmpty(LibraryCard.Frame))
                {
                    frameName = "Images/Cards/Frames/" + LibraryCard.Frame;
                }

                _frameImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);
                _pictureImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format(
                    "Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), LibraryCard.Picture.ToLower()));
                _unitTypeIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", LibraryCard.Type + "_icon"));

                if (_withEffect)
                {
                    if (actionEffectType != Enumerators.ActionEffectType.None)
                    {
                        _effectImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsBuffTypes/battleground_past_action_bar_icon_" + actionEffectType.ToString().ToLower());
                    }
                    else
                    {
                        _effectImage.enabled = false;
                    }
                }

                _selfObject.SetActive(true);
            }
        }

        public class SpellCardElement : ActionElement
        {
            private GameObject _selfObject;

            private TextMeshProUGUI _gooText,
                        _titleText,
                        _bodyText;

            private Image _frameImage,
                          _pictureImage,
                          _effectImage;

            private bool _withEffect;

            public SpellCardElement(GameObject selfObject, bool withEffect = false)
            {
                _selfObject = selfObject;
                _withEffect = withEffect;

                _gooText = _selfObject.transform.Find("Text_Goo").GetComponent<TextMeshProUGUI>();
                _titleText = _selfObject.transform.Find("Text_Title").GetComponent<TextMeshProUGUI>();
                _bodyText = _selfObject.transform.Find("Text_Body").GetComponent<TextMeshProUGUI>();

                _frameImage = _selfObject.transform.Find("Frame").GetComponent<Image>();
                _pictureImage = _selfObject.transform.Find("Image_Mask/Image_Picture").GetComponent<Image>();

                if (_withEffect)
                {
                    _effectImage = _selfObject.transform.Find("Image_Effect").GetComponent<Image>();
                }

                _selfObject.SetActive(false);
            }

            public override void Init(WorkingCard workingCard, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None)
            {
                Card LibraryCard = workingCard.LibraryCard;

                _titleText.text = LibraryCard.Name;
                _bodyText.text = LibraryCard.Description;
                _gooText.text = LibraryCard.Cost.ToString();

                string rarity = Enum.GetName(typeof(Enumerators.CardRank), LibraryCard.CardRank);

                string setName = LibraryCard.CardSetType.ToString();

                string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

                if (!string.IsNullOrEmpty(LibraryCard.Frame))
                {
                    frameName = "Images/Cards/Frames/" + LibraryCard.Frame;
                }

                _frameImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);
                _pictureImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format(
                    "Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), LibraryCard.Picture.ToLower()));

                if (_withEffect)
                {
                    if (actionEffectType != Enumerators.ActionEffectType.None)
                    {
                        _effectImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsBuffTypes/battleground_past_action_bar_icon_" + actionEffectType.ToString().ToLower());
                    }
                    else
                    {
                        _effectImage.enabled = false;
                    }
                }

                _selfObject.SetActive(true);
            }
        }

        public class OverlordElement : ActionElement
        {
            private GameObject _selfObject;

            private Image _overlordImage,
                          _effectImage;

            private bool _withEffect;

            public OverlordElement(GameObject selfObject, bool withEffect = false)
            {
                _selfObject = selfObject;
                _withEffect = withEffect;

                _overlordImage = _selfObject.transform.Find("Image_Overlord").GetComponent<Image>();

                if (_withEffect)
                {
                    _effectImage = _selfObject.transform.Find("Image_Effect").GetComponent<Image>();
                }

                _selfObject.SetActive(false);
            }

            public override void Init(Player player, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None)
            {
                _overlordImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("CZB_2D_Hero_Portrait_" + player.SelfHero.HeroElement.ToString() + "_EXP");

                if (_withEffect)
                {
                    if (actionEffectType != Enumerators.ActionEffectType.None)
                    {
                        _effectImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsBuffTypes/battleground_past_action_bar_icon_" + actionEffectType.ToString().ToLower());
                    }
                    else
                    {
                        _effectImage.enabled = false;
                    }
                }

                _selfObject.SetActive(true);
            }
        }

        public class OverlordSkillElement : ActionElement
        {
            private GameObject _selfObject;

            private Image _skillImage,
                          _effectImage;

            private bool _withEffect;

            public OverlordSkillElement(GameObject selfObject, bool withEffect = false)
            {
                _selfObject = selfObject;
                _withEffect = withEffect;

                _skillImage = _selfObject.transform.Find("Image_SkillPicture").GetComponent<Image>();

                if (_withEffect)
                {
                    _effectImage = _selfObject.transform.Find("Image_Effect").GetComponent<Image>();
                }

                _selfObject.SetActive(false);
            }

            public override void Init(BoardSkill skill, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None)
            {
                _skillImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" +
                                                                                skill.OwnerPlayer.SelfHero.Element.ToLower() + "_" +
                                                                                skill.Skill.OverlordSkill.ToString().ToLower());

                if (_withEffect)
                {
                    if (actionEffectType != Enumerators.ActionEffectType.None)
                    {
                        _effectImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsBuffTypes/battleground_past_action_bar_icon_" + actionEffectType.ToString().ToLower());
                    }
                    else
                    {
                        _effectImage.enabled = false;
                    }
                }

                _selfObject.SetActive(true);
            }
        }

        public class SmallUnitCardElement : ActionElement
        {
            private GameObject _selfObject;

            private TextMeshProUGUI _gooText,
                                    _titleText,
                                    _bodyText,
                                    _attackText,
                                    _defenseText;

            private Image _frameImage,
                          _unitTypeIconImage,
                          _pictureImage,
                          _effectImage;

            private bool _withEffect;

            public SmallUnitCardElement(Transform parent, bool withEffect = false)
            {
                _selfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/PastActionBar/Item_CardUnitSmall"), parent, false);
                _withEffect = withEffect;

                _gooText = _selfObject.transform.Find("Root/Text_Goo").GetComponent<TextMeshProUGUI>();
                _titleText = _selfObject.transform.Find("Root/Text_Title").GetComponent<TextMeshProUGUI>();
                _bodyText = _selfObject.transform.Find("Root/Text_Body").GetComponent<TextMeshProUGUI>();
                _attackText = _selfObject.transform.Find("Root/Text_Attack").GetComponent<TextMeshProUGUI>();
                _defenseText = _selfObject.transform.Find("Root/Text_Defense").GetComponent<TextMeshProUGUI>();

                _frameImage = _selfObject.transform.Find("Root/Frame").GetComponent<Image>();
                _unitTypeIconImage = _selfObject.transform.Find("Root/Image_UnitType").GetComponent<Image>();
                _pictureImage = _selfObject.transform.Find("Root/Image_Mask/Image_Picture").GetComponent<Image>();

                if (_withEffect)
                {
                    _effectImage = _selfObject.transform.Find("Root/Image_Effect").GetComponent<Image>();
                }

                _selfObject.SetActive(false);
            }

            public override void Init(WorkingCard workingCard, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None)
            {
                Card LibraryCard = workingCard.LibraryCard;

                _titleText.text = LibraryCard.Name;
                _bodyText.text = LibraryCard.Description;
                _gooText.text = LibraryCard.Cost.ToString();
                _attackText.text = LibraryCard.Damage.ToString();
                _defenseText.text = LibraryCard.Health.ToString();

                string rarity = Enum.GetName(typeof(Enumerators.CardRank), LibraryCard.CardRank);

                string setName = LibraryCard.CardSetType.ToString();

                string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

                if (!string.IsNullOrEmpty(LibraryCard.Frame))
                {
                    frameName = "Images/Cards/Frames/" + LibraryCard.Frame;
                }

                _frameImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);
                _pictureImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format(
                    "Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), LibraryCard.Picture.ToLower()));
                _unitTypeIconImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", LibraryCard.Type + "_icon"));

                if (_withEffect)
                {
                    if (actionEffectType != Enumerators.ActionEffectType.None)
                    {
                        _effectImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsBuffTypes/battleground_past_action_bar_icon_" + actionEffectType.ToString().ToLower());
                    }
                    else
                    {
                        _effectImage.enabled = false;
                    }
                }

                _selfObject.SetActive(true);
            }


            public override void Dispose()
            {
                Object.Destroy(_selfObject);
            }
        }

        public class SmallSpellCardElement : ActionElement
        {
            private GameObject _selfObject;

            private TextMeshProUGUI _gooText,
                        _titleText,
                        _bodyText;

            private Image _frameImage,
                          _pictureImage,
                          _effectImage;

            private bool _withEffect;

            public SmallSpellCardElement(Transform parent, bool withEffect = false)
            {
                _selfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/PastActionBar/Item_CardSpellSmall"), parent, false);
                _withEffect = withEffect;

                _gooText = _selfObject.transform.Find("Root/Text_Goo").GetComponent<TextMeshProUGUI>();
                _titleText = _selfObject.transform.Find("Root/Text_Title").GetComponent<TextMeshProUGUI>();
                _bodyText = _selfObject.transform.Find("Root/Text_Body").GetComponent<TextMeshProUGUI>();

                _frameImage = _selfObject.transform.Find("Root/Frame").GetComponent<Image>();
                _pictureImage = _selfObject.transform.Find("Root/Image_Mask/Image_Picture").GetComponent<Image>();

                if (_withEffect)
                {
                    _effectImage = _selfObject.transform.Find("Root/Image_Effect").GetComponent<Image>();
                }

                _selfObject.SetActive(false);
            }

            public override void Init(WorkingCard workingCard, Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None)
            {
                Card LibraryCard = workingCard.LibraryCard;

                _titleText.text = LibraryCard.Name;
                _bodyText.text = LibraryCard.Description;
                _gooText.text = LibraryCard.Cost.ToString();

                string rarity = Enum.GetName(typeof(Enumerators.CardRank), LibraryCard.CardRank);

                string setName = LibraryCard.CardSetType.ToString();

                string frameName = string.Format("Images/Cards/Frames/frame_{0}_{1}", setName, rarity);

                if (!string.IsNullOrEmpty(LibraryCard.Frame))
                {
                    frameName = "Images/Cards/Frames/" + LibraryCard.Frame;
                }

                _frameImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);
                _pictureImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format(
                    "Images/Cards/Illustrations/{0}_{1}_{2}", setName.ToLower(), rarity.ToLower(), LibraryCard.Picture.ToLower()));

                if (_withEffect)
                {
                    if (actionEffectType != Enumerators.ActionEffectType.None)
                    {
                        _effectImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(
                            "Images/IconsBuffTypes/battleground_past_action_bar_icon_" + actionEffectType.ToString().ToLower());
                    }
                    else
                    {
                        _effectImage.enabled = false;
                    }
                }

                _selfObject.SetActive(true);
            }

            public override void Dispose()
            {
                Object.Destroy(_selfObject);
            }
        }
    }
}
