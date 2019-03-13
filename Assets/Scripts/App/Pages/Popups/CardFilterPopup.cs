using System;
using System.Linq;
using System.Collections.Generic;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.PlayerLoop;

namespace Loom.ZombieBattleground
{
    public class CardFilterPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        public event Action<CardFilterData> ActionPopupHiding;
        
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private Button _buttonClose,
                       _buttonSave,
                       _buttonSelectNone,
                       _buttonSelectAll,
                       _buttonElement,
                       _buttonRank,
                       _buttonType,
                       _buttonGooCost;

        private Dictionary<Enumerators.Faction, Button> _buttonElementsDictionary;

        private Dictionary<Enumerators.CardRank, Button> _buttonRankDictionary;
        
        private Dictionary<Enumerators.CardType, Button> _buttonTypeDictionary;
        
        private List<Button> _buttonGooCostList;
        
        public readonly List<Enumerators.Faction> AllAvailableSetTypeList = new List<Enumerators.Faction>()
        {
            Enumerators.Faction.FIRE,
            Enumerators.Faction.WATER,
            Enumerators.Faction.EARTH,
            Enumerators.Faction.AIR,
            Enumerators.Faction.LIFE,
            Enumerators.Faction.TOXIC,
            Enumerators.Faction.ITEM
        };

        public readonly List<Enumerators.CardRank> AllAvailableRankList = new List<Enumerators.CardRank>()
        {
            Enumerators.CardRank.MINION,
            Enumerators.CardRank.OFFICER,
            Enumerators.CardRank.GENERAL,
            Enumerators.CardRank.COMMANDER
        };

        public readonly List<Enumerators.CardType> AllAvailableTypeList = new List<Enumerators.CardType>()
        {
            Enumerators.CardType.FERAL,
            Enumerators.CardType.WALKER,
            Enumerators.CardType.HEAVY,
            Enumerators.CardType.UNDEFINED
        };

        public CardFilterData FilterData;
        
        public enum TAB
        {
            NONE = -1,
            ELEMENT = 0,
            RANK = 1,
            TYPE = 2,
            GOO_COST = 3
        }
        
        private TAB _tab;
        
        private GameObject[] _tabObjects;
        
        public event Action<TAB> EventChangeTab;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _buttonElementsDictionary = new Dictionary<Enumerators.Faction, Button>();
            _buttonRankDictionary = new Dictionary<Enumerators.CardRank, Button>();
            _buttonTypeDictionary = new Dictionary<Enumerators.CardType, Button>();
            FilterData = new CardFilterData
            (
                AllAvailableSetTypeList,
                AllAvailableRankList,
                AllAvailableTypeList
            );
            _buttonGooCostList = new List<Button>();
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

            _buttonElementsDictionary.Clear();
            _buttonRankDictionary.Clear();
            _buttonTypeDictionary.Clear();
            _buttonGooCostList.Clear();
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardFilterPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false); 
            
            _buttonClose = Self.transform.Find("Scaler/Button_Close").GetComponent<Button>();                        
            _buttonClose.onClick.AddListener(ButtonCloseHandler);
            _buttonClose.onClick.AddListener(PlayClickSound);
            
            _buttonSave = Self.transform.Find("Scaler/Button_Save").GetComponent<Button>();                        
            _buttonSave.onClick.AddListener(ButtonSaveHandler);
            _buttonSave.onClick.AddListener(PlayClickSound);
            
            _buttonSelectNone = Self.transform.Find("Scaler/Button_SelectNone").GetComponent<Button>();                        
            _buttonSelectNone.onClick.AddListener(ButtonSelectNoneHandler);
            _buttonSelectNone.onClick.AddListener(PlayClickSound);
            
            _buttonSelectAll = Self.transform.Find("Scaler/Button_SelectAll").GetComponent<Button>();                        
            _buttonSelectAll.onClick.AddListener(ButtonSelectAllHandler);
            _buttonSelectAll.onClick.AddListener(PlayClickSound);
            
            _buttonElement = Self.transform.Find("Scaler/Button_Element").GetComponent<Button>();                        
            _buttonElement.onClick.AddListener(ButtonElementHandler);
            _buttonElement.onClick.AddListener(PlayClickSound);
            
            _buttonRank = Self.transform.Find("Scaler/Button_Rank").GetComponent<Button>();                        
            _buttonRank.onClick.AddListener(ButtonRankHandler);
            _buttonRank.onClick.AddListener(PlayClickSound);
            
            _buttonType = Self.transform.Find("Scaler/Button_Type").GetComponent<Button>();                        
            _buttonType.onClick.AddListener(ButtonTypeHandler);
            _buttonType.onClick.AddListener(PlayClickSound);
            
            _buttonGooCost = Self.transform.Find("Scaler/Button_GooCost").GetComponent<Button>();                        
            _buttonGooCost.onClick.AddListener(ButtonGooCostHandler);
            _buttonGooCost.onClick.AddListener(PlayClickSound);

            _buttonElementsDictionary.Clear();
            foreach(Enumerators.Faction faction in AllAvailableSetTypeList)
            {
                Button buttonElementIcon = Self.transform.Find("Scaler/Tab_Element/Group_ElementIcons/Button_element_"+faction.ToString().ToLower()).GetComponent<Button>();
                buttonElementIcon.onClick.AddListener
                (
                    ()=> ButtonElementIconHandler(faction)
                );
                buttonElementIcon.onClick.AddListener(PlayClickSound);

                _buttonElementsDictionary.Add(faction, buttonElementIcon);
            }

            _buttonRankDictionary.Clear();
            foreach (Enumerators.CardRank rank in AllAvailableRankList)
            {
                Button button = Self.transform.Find("Scaler/Tab_Rank/Group_RankIcons/Button_rank_"+rank.ToString().ToLower()).GetComponent<Button>();
                button.onClick.AddListener
                (
                    ()=> ButtonRankIconHandler(rank)
                );
                button.onClick.AddListener(PlayClickSound);

                _buttonRankDictionary.Add(rank, button);
            }
            
            _buttonTypeDictionary.Clear();
            foreach (Enumerators.CardType type in AllAvailableTypeList)
            {
                Button button = Self.transform.Find("Scaler/Tab_Type/Group_TypeIcons/Button_type_"+type.ToString().ToLower()).GetComponent<Button>();
                button.onClick.AddListener
                (
                    ()=> ButtonTypeIconHandler(type)
                );
                button.onClick.AddListener(PlayClickSound);

                _buttonTypeDictionary.Add(type, button);
            }

            _buttonGooCostList.Clear();
            for(int i=0;i<11;++i)
            {
                int gooIndex = i;
                Button button = Self.transform.Find("Scaler/Tab_GooCost/Group_GooIcons/Button_element_goo_" + i).GetComponent<Button>();
                button.onClick.AddListener(() =>
                    {
                        FilterData.GooCostList[gooIndex] = !FilterData.GooCostList[gooIndex];
                        UpdateGooCostButtonDisplay(gooIndex);
                    });
                button.onClick.AddListener(PlayClickSound);
                _buttonGooCostList.Add(button);
            }

            _tabObjects = new GameObject[]
            {
                Self.transform.Find("Scaler/Tab_Element").gameObject,
                Self.transform.Find("Scaler/Tab_Rank").gameObject,
                Self.transform.Find("Scaler/Tab_Type").gameObject,
                Self.transform.Find("Scaler/Tab_GooCost").gameObject
            };

            FilterData.Reset();
            LoadTabs();  
        }
        
        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #endregion

        #region Buttons Handlers

        private void ButtonCloseHandler()
        {
            _uiManager.HidePopup<CardFilterPopup>();
        }
        
        private void ButtonSaveHandler()
        {            
            if (!CheckIfAnyElementSelected())
            {
                OpenAlertDialog("No element selected!\nPlease select atleast one.");
                return;
            }
            
            if (!CheckIfAnyRankSelected())
            {
                OpenAlertDialog("No rank selected!\nPlease select atleast one.");
                return;
            }
            
            if (!CheckIfAnyTypeSelected())
            {
                OpenAlertDialog("No type selected!\nPlease select atleast one.");
                return;
            }
            
            if (!CheckIfAnyGooCostSelected())
            {
                OpenAlertDialog("No goo cost selected!\nPlease select atleast one.");
                return;
            }
            
            _uiManager.HidePopup<CardFilterPopup>();
            ActionPopupHiding?.Invoke(FilterData);            
        }
        
        private void ButtonElementIconHandler(Enumerators.Faction faction)
        {
            ToggleSelectedSetType(faction);            
        }

        private void ButtonRankIconHandler(Enumerators.CardRank rank)
        {
            FilterData.RankDictionary[rank] = !FilterData.RankDictionary[rank];
            UpdateRankButtonDisplay(rank);
        }
        
        private void ButtonTypeIconHandler(Enumerators.CardType type)
        {
            FilterData.TypeDictionary[type] = !FilterData.TypeDictionary[type];
            UpdateTypeButtonDisplay(type);
        }

        private void ButtonSelectNoneHandler()
        {
            switch (_tab)
            {
                case TAB.ELEMENT:
                    foreach (Enumerators.Faction faction in AllAvailableSetTypeList)
                        SetSelectedSetType(faction, false);
                    break;
                case TAB.RANK:
                    foreach (Enumerators.CardRank rank in AllAvailableRankList)
                    {
                        FilterData.RankDictionary[rank] = false;
                        UpdateRankButtonDisplay(rank);
                    }
                    break;
                case TAB.TYPE:
                    foreach (Enumerators.CardType type in AllAvailableTypeList)
                    {
                        FilterData.TypeDictionary[type] = false;
                        UpdateTypeButtonDisplay(type);
                    }
                    break;
                case TAB.GOO_COST:
                    for(int i=0; i<FilterData.GooCostList.Count;++i)
                    {
                        FilterData.GooCostList[i] = false;
                        UpdateGooCostButtonDisplay(i);
                    }
                    break;
                default:
                    return;
            }
        }
        
        private void ButtonSelectAllHandler()
        {
            switch (_tab)
            {
                case TAB.ELEMENT:
                    foreach (Enumerators.Faction faction in AllAvailableSetTypeList)
                        SetSelectedSetType(faction, true);
                    break;
                case TAB.RANK:
                    foreach (Enumerators.CardRank rank in AllAvailableRankList)
                    {
                        FilterData.RankDictionary[rank] = true;
                        UpdateRankButtonDisplay(rank);
                    }
                    break;
                case TAB.TYPE:
                    foreach (Enumerators.CardType type in AllAvailableTypeList)
                    {
                        FilterData.TypeDictionary[type] = true;
                        UpdateTypeButtonDisplay(type);
                    }
                    break;
                case TAB.GOO_COST:
                    for(int i=0; i<FilterData.GooCostList.Count;++i)
                    {
                        FilterData.GooCostList[i] = true;
                        UpdateGooCostButtonDisplay(i);
                    }
                    break;
                default:
                    return;
            }
        }
        
        private void ButtonElementHandler()
        {
            ChangeTab(TAB.ELEMENT);
        }
        
        private void ButtonRankHandler()
        {
            ChangeTab(TAB.RANK);
        }
        
        private void ButtonTypeHandler()
        {
            ChangeTab(TAB.TYPE);
        }
        
        private void ButtonGooCostHandler()
        {
            ChangeTab(TAB.GOO_COST);
        }

        #endregion
        
        private void LoadTabs()
        {
            _tab = TAB.NONE;
            ChangeTab(TAB.ELEMENT);
        }
        
        public void ChangeTab(TAB newTab)
        {
            if (newTab == _tab)
                return;
                
            _tab = newTab;            
            
            for(int i=0; i<_tabObjects.Length;++i)
            {
                GameObject tabObject = _tabObjects[i];
                tabObject.SetActive(i == (int)newTab);
            }
            
            switch (newTab)
            {
                case TAB.NONE:
                    break;
                case TAB.ELEMENT:
                    break;
                case TAB.RANK:
                    break;
                case TAB.TYPE:                                      
                    break;
                case TAB.GOO_COST:                    
                    break;
                default:
                    break;
            }            
            
            EventChangeTab?.Invoke(_tab);
        }

        #region Filter

        private void SetSelectedSetType(Enumerators.Faction faction, bool status)
        {
            FilterData.SetTypeDictionary[faction] = status;
            UpdateSetTypeButtonDisplay(faction);
        }
        
        private void ToggleSelectedSetType(Enumerators.Faction faction)
        {
            FilterData.SetTypeDictionary[faction] = !FilterData.SetTypeDictionary[faction];
            UpdateSetTypeButtonDisplay(faction);
        }
        
        private void UpdateSetTypeButtonDisplay(Enumerators.Faction faction)
        {
            _buttonElementsDictionary[faction].GetComponent<Image>().color =
                FilterData.SetTypeDictionary[faction] ? Color.white : Color.gray;
        }
        
        private void UpdateRankButtonDisplay(Enumerators.CardRank rank)
        {
            _buttonRankDictionary[rank].GetComponent<Image>().color =
                FilterData.RankDictionary[rank] ? Color.white : Color.gray;
        }
        
        private void UpdateTypeButtonDisplay(Enumerators.CardType type)
        {
            _buttonTypeDictionary[type].GetComponent<Image>().color =
                FilterData.TypeDictionary[type] ? Color.white : Color.gray;
        }
        
        private void UpdateGooCostButtonDisplay(int gooIndex)
        {
            _buttonGooCostList[gooIndex].GetComponent<Image>().color =
                FilterData.GooCostList[gooIndex] ? Color.white : Color.gray;
            _buttonGooCostList[gooIndex].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = _buttonGooCostList[gooIndex].GetComponent<Image>().color;
        }
        
        private bool CheckIfAnyElementSelected()
        {
            return FilterData.SetTypeDictionary.Any(kvp => kvp.Value);           
        }
        
        private bool CheckIfAnyGooCostSelected()
        {
            return FilterData.GooCostList.Any(selected => selected);
        }
        
        private bool CheckIfAnyRankSelected()
        {
            return FilterData.RankDictionary.Any(kvp => kvp.Value);           
        }
        
        private bool CheckIfAnyTypeSelected()
        {
            return FilterData.TypeDictionary.Any(kvp => kvp.Value);           
        }
        
        #endregion

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
        
        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
        
        public class CardFilterData
        {            
            public Dictionary<Enumerators.Faction, bool> SetTypeDictionary;
            public Dictionary<Enumerators.CardRank, bool> RankDictionary;
            public Dictionary<Enumerators.CardType, bool> TypeDictionary;
            public List<bool> GooCostList;
            
            public CardFilterData
            (
                List<Enumerators.Faction> availableSetTypeList,
                List<Enumerators.CardRank> availableRankList,
                List<Enumerators.CardType> availableTypeList
            )
            {
                SetTypeDictionary = new Dictionary<Enumerators.Faction, bool>();
                foreach(Enumerators.Faction faction in availableSetTypeList)
                {
                    SetTypeDictionary.Add(faction, true);
                }

                RankDictionary = new Dictionary<Enumerators.CardRank, bool>();
                foreach(Enumerators.CardRank rank in availableRankList)
                {
                    RankDictionary.Add(rank, true);
                }
                
                TypeDictionary = new Dictionary<Enumerators.CardType, bool>();
                foreach(Enumerators.CardType type in availableTypeList)
                {
                    TypeDictionary.Add(type, true);
                }
                
                GooCostList = new List<bool>();
                for(int i=0; i<11; ++i)
                {
                    GooCostList.Add(true);
                }
            }
            
            public List<Enumerators.Faction> GetFilterSetTypeList()
            {
                List<Enumerators.Faction> setTypeList = new List<Enumerators.Faction>();
                foreach (KeyValuePair<Enumerators.Faction, bool> kvp in SetTypeDictionary)
                {
                    if(kvp.Value)
                        setTypeList.Add(kvp.Key);
                }
                return setTypeList;
            }

            public void Reset()
            {
                for(int i=0; i<SetTypeDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.Faction, bool> kvp = SetTypeDictionary.ElementAt(i);
                    SetTypeDictionary[kvp.Key] = true;
                }
                for(int i=0; i<RankDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.CardRank, bool> kvp = RankDictionary.ElementAt(i);
                    RankDictionary[kvp.Key] = true;
                }
                for(int i=0; i<TypeDictionary.Count;++i)
                {
                    KeyValuePair<Enumerators.CardType, bool> kvp = TypeDictionary.ElementAt(i);
                    TypeDictionary[kvp.Key] = true;
                }
                for (int i = 0; i < GooCostList.Count; ++i)
                {
                    GooCostList[i] = true;
                }
            }
        }

    }
}
