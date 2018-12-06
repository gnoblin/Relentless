#if UNITY_EDITOR
#define DISABLE_DATA_ENCRYPTION
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Card = Loom.ZombieBattleground.Data.Card;
using CardList = Loom.ZombieBattleground.Data.CardList;
using Deck = Loom.ZombieBattleground.Data.Deck;
using Hero = Loom.ZombieBattleground.Data.Hero;

namespace Loom.ZombieBattleground
{
    public class DataManager : IService, IDataManager
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                Converters = {
                    new StringEnumConverter()
                },
                CheckAdditionalContent = true,
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (sender, args) =>
                {
                    Debug.LogException(args.ErrorContext.Error);
                }
            };

        private ILocalizationManager _localizationManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Dictionary<Enumerators.CacheDataType, string> _cacheDataFileNames;

        private DirectoryInfo _dir;

        private List<string> _names;

        public DataManager(ConfigData configData)
        {
            FillCacheDataPaths();
            InitCachedData();
            ConfigData = configData;
        }

        private void InitCachedData()
        {
            CachedUserLocalData = new UserLocalData();
            CachedCardsLibraryData = new CardsLibraryData(new List<Card>());
            CachedHeroesData = new HeroesData(new List<Hero>());
            CachedCollectionData = new CollectionData();
            CachedDecksData = new DecksData(new List<Deck>());
            CachedAiDecksData = new AIDecksData();
            CachedCreditsData = new CreditsData();
            CachedBuffsTooltipData = new TooltipContentData();
        }

        public TooltipContentData CachedBuffsTooltipData { get; set; }

        public UserLocalData CachedUserLocalData { get; set; }

        public CardsLibraryData CachedCardsLibraryData { get; set; }

        public HeroesData CachedHeroesData { get; set; }

        public CollectionData CachedCollectionData { get; set; }

        public DecksData CachedDecksData { get; set; }

        public AIDecksData CachedAiDecksData { get; set; }

        public CreditsData CachedCreditsData { get; set; }

        public ConfigData ConfigData { get; set; }

        public UserInfo UserInfo { get; set; }

        public async Task StartLoadCache()
        {
            Debug.Log("=== Start loading server ==== ");

            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
            {
                await LoadCachedData((Enumerators.CacheDataType) i);
            }

            // FIXME: remove next line after fetching collection from backend is implemented
            FillFullCollection();

            _localizationManager.ApplyLocalization();

#if DEV_MODE
            CachedUserLocalData.Tutorial = false;
#endif

            GameClient.Get<IApplicationSettingsManager>().ApplySettings();

            GameClient.Get<IGameplayManager>().IsTutorial = CachedUserLocalData.Tutorial;

#if DEVELOPMENT
            foreach (Enumerators.CacheDataType dataType in _cacheDataFileNames.Keys)
            {
                await SaveCache(dataType);
            }
#endif
        }

        public void DeleteData()
        {
            InitCachedData();
            FileInfo[] files = _dir.GetFiles();

            foreach (FileInfo file in files)
            {
                if (_cacheDataFileNames.Values.Any(path => path.EndsWith(file.Name)) ||
                    file.Extension.Equals("dat", StringComparison.InvariantCultureIgnoreCase) ||
                    file.Name.Contains(Constants.VersionFileResolution))
                {
                    file.Delete();
                }
            }

            using (File.Create(_dir + BuildMetaInfo.Instance.ShortVersionName + Constants.VersionFileResolution))
            {
            }

            PlayerPrefs.DeleteAll();
        }

        public Task SaveCache(Enumerators.CacheDataType type)
        {
            string dataPath = GetPersistentDataItemPath(_cacheDataFileNames[type]);
             string data = "";
             switch (type)
             {
                 case Enumerators.CacheDataType.USER_LOCAL_DATA:
                     data = SerializePersistentObject(CachedUserLocalData);
                     break;
 #if DEVELOPMENT
                 case Enumerators.CacheDataType.CARDS_LIBRARY_DATA:
                     data = SerializePersistentObject(CachedCardsLibraryData);
                     break;
                 case Enumerators.CacheDataType.CREDITS_DATA:
                     data = SerializePersistentObject(CachedCreditsData);
                     break;
                 case Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA:
                     data = SerializePersistentObject(CachedBuffsTooltipData);
                     break;
#endif
                  default:
                      throw new ArgumentOutOfRangeException();
              }
              if (data.Length > 0)
              {
                  if (!File.Exists(dataPath)) File.Create(dataPath).Close();

                  File.WriteAllText(dataPath, data);
              }
            return Task.CompletedTask;
        }

        public TooltipContentData.CardTypeInfo GetCardTypeInfo(Enumerators.CardType cardType)
        {
            return CachedBuffsTooltipData.CardTypes.Find(x => x.Type == cardType);
        }

        public TooltipContentData.GameMechanicInfo GetGameMechanicInfo(Enumerators.GameMechanicDescriptionType gameMechanic)
        {
            return CachedBuffsTooltipData.Mechanics.Find(x => x.Type == gameMechanic);
        }

        public TooltipContentData.RankInfo GetCardRankInfo(Enumerators.CardRank rank)
        {
            return CachedBuffsTooltipData.Ranks.Find(x => x.Type == rank);
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            Debug.Log("Encryption: " + ConfigData.EncryptData);
            Debug.Log("Skip Card Data Backend: " + ConfigData.SkipBackendCardData);

            _localizationManager = GameClient.Get<ILocalizationManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _uiManager = GameClient.Get<IUIManager>();

            _dir = new DirectoryInfo(Application.persistentDataPath + "/");

            LoadLocalCachedData();

            GameClient.Get<ISoundManager>().ApplySoundData();

            CheckVersion();
        }

        public void Update()
        {
        }

        private uint GetMaxCopiesValue(Data.Card card, Enumerators.SetType setName)
        {
            Enumerators.CardRank rank = card.CardRank;
            uint maxCopies;

            if (setName == Enumerators.SetType.ITEM)
            {
                maxCopies = Constants.CardItemMaxCopies;
                return maxCopies;
            }

            switch (rank)
            {
                case Enumerators.CardRank.MINION:
                    maxCopies = Constants.CardMinionMaxCopies;
                    break;
                case Enumerators.CardRank.OFFICER:
                    maxCopies = Constants.CardOfficerMaxCopies;
                    break;
                case Enumerators.CardRank.COMMANDER:
                    maxCopies = Constants.CardCommanderMaxCopies;
                    break;
                case Enumerators.CardRank.GENERAL:
                    maxCopies = Constants.CardGeneralMaxCopies;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return maxCopies;
        }

        private void CheckVersion()
        {
            FileInfo[] files = _dir.GetFiles();
            bool versionMatch = false;
            foreach (FileInfo file in files)
            {
                if (file.Name == BuildMetaInfo.Instance.ShortVersionName + Constants.VersionFileResolution)
                {
                    versionMatch = true;
                    break;
                }
            }

            if (!versionMatch)
            {
                DeleteVersionFile();
            }
        }

        private void DeleteVersionFile()
        {
            FileInfo[] files = _dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Name.Contains(Constants.VersionFileResolution))
                {
                    file.Delete();
                    break;
                }
            }

            using (File.Create(_dir + BuildMetaInfo.Instance.ShortVersionName + Constants.VersionFileResolution))
            {
            }
        }

        private void ConfirmDeleteDeckReceivedHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmDeleteDeckReceivedHandler;
        }

        private void ShowLoadDataFailMessage(string msg)
        {
            _uiManager.HidePopup<LoginPopup>();
            _uiManager.DrawPopup<LoadDataMessagePopup>(msg);
        }

        private async Task LoadCachedData(Enumerators.CacheDataType type)
        {
            switch (type)
            {
                case Enumerators.CacheDataType.CARDS_LIBRARY_DATA:
                    string cardsLibraryFilePath = GetPersistentDataItemPath(_cacheDataFileNames[type]);

                    List<Card> cardList;
                    if (ConfigData.SkipBackendCardData && File.Exists(cardsLibraryFilePath))
                    {
                        Debug.LogWarning("===== Loading Card Library from persistent data ===== ");
                        cardList = DeserializeObjectFromPersistentData<CardList>(cardsLibraryFilePath).Cards;
                    }
                    else
                    {
                        try
                        {
                            ListCardLibraryResponse listCardLibraryResponse = await _backendFacade.GetCardLibrary();
                            Debug.Log(listCardLibraryResponse.ToString());
                            cardList = listCardLibraryResponse.Cards.Select(card => card.FromProtobuf()).ToList();
                        }
                        catch(Exception)
                        {
                            ShowLoadDataFailMessage("Issue with Loading Card Library Data");
                            throw;
                        }
                    }
                    CachedCardsLibraryData = new CardsLibraryData(cardList);

                    break;
                case Enumerators.CacheDataType.HEROES_DATA:
                    try
                    {
                        ListHeroesResponse heroesList = await _backendFacade.GetHeroesList(_backendDataControlMediator.UserDataModel.UserId);
                        CachedHeroesData = new HeroesData(heroesList.Heroes.Select(hero => hero.FromProtobuf()).ToList());
                    }
                    catch (Exception)
                    {
                        ShowLoadDataFailMessage("Issue with Loading Heroes Data");
                        throw;
                    }
                    break;
                case Enumerators.CacheDataType.COLLECTION_DATA:
                    try
                    {
                        GetCollectionResponse getCollectionResponse = await _backendFacade.GetCardCollection(_backendDataControlMediator.UserDataModel.UserId);
                        CachedCollectionData = getCollectionResponse.FromProtobuf();
                    }
                    catch (Exception)
                    {
                        ShowLoadDataFailMessage("Issue with Loading Card Collection Data");
                        throw;
                    }

                    break;
                case Enumerators.CacheDataType.DECKS_DATA:
                    try
                    {
                        ListDecksResponse listDecksResponse = await _backendFacade.GetDecks(_backendDataControlMediator.UserDataModel.UserId);
                        CachedDecksData = new DecksData(listDecksResponse.Decks.Select(deck => deck.FromProtobuf()).ToList());
                    }
                    catch (Exception)
                    {
                        ShowLoadDataFailMessage("Issue with Loading Decks Data");
                        throw;
                    }

                    break;
                case Enumerators.CacheDataType.DECKS_OPPONENT_DATA:
                    try
                    {
                        GetAIDecksResponse decksAiResponse = await _backendFacade.GetAiDecks();
                        CachedAiDecksData = new AIDecksData();
                        CachedAiDecksData.Decks =
                            decksAiResponse.Decks
                                .Select(d => d.FromProtobuf())
                                .ToList();
                    }
                    catch (Exception)
                    {
                        ShowLoadDataFailMessage("Issue with Loading Opponent AI Decks");
                        throw;
                    }
                    break;
                case Enumerators.CacheDataType.CREDITS_DATA:
                    CachedCreditsData = DeserializeObjectFromAssets<CreditsData>(_cacheDataFileNames[type]);
                    break;
                case Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA:
                    CachedBuffsTooltipData = DeserializeObjectFromAssets<TooltipContentData>(_cacheDataFileNames[type]);
                    break;
                default:
                    break;
            }
        }

        private void LoadLocalCachedData()
        {
            string userLocalDataFilePath = GetPersistentDataItemPath(_cacheDataFileNames[Enumerators.CacheDataType.USER_LOCAL_DATA]);
            if (File.Exists(userLocalDataFilePath))
            {
                CachedUserLocalData = DeserializeObjectFromPersistentData<UserLocalData>(userLocalDataFilePath);
            }
        }

        private void FillCacheDataPaths()
        {
            _cacheDataFileNames = new Dictionary<Enumerators.CacheDataType, string>
            {
                {
                    Enumerators.CacheDataType.USER_LOCAL_DATA, Constants.LocalUserDataFileName
                },
                {
                    Enumerators.CacheDataType.CARDS_LIBRARY_DATA, Constants.LocalCardsLibraryDataFileName
                },
                {
                    Enumerators.CacheDataType.CREDITS_DATA, Constants.LocalCreditsDataFileName
                },
                {
                    Enumerators.CacheDataType.BUFFS_TOOLTIP_DATA, Constants.LocalBuffsTooltipDataFileName
                }
            };
        }

        public string DecryptData(string data)
        {
#if !DISABLE_DATA_ENCRYPTION
            if (!ConfigData.EncryptData)
                return data;

            return Utilites.Decrypt(data, Constants.PrivateEncryptionKeyForApp);
#else
            return data;
#endif
        }

        public string EncryptData(string data)
        {
#if !DISABLE_DATA_ENCRYPTION
            if (!ConfigData.EncryptData)
                return data;

            return Utilites.Encrypt(data, Constants.PrivateEncryptionKeyForApp);
#else
            return data;
#endif
        }

        public string SerializeToJson(object obj, bool indented = false)
        {
            return JsonConvert.SerializeObject(
                obj,
                indented ? Formatting.Indented : Formatting.None,
                JsonSerializerSettings
            );
        }

        public T DeserializeFromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);
        }

        private T DeserializeObjectFromAssets<T>(string fileName)
        {
            return DeserializeFromJson<T>(_loadObjectsManager.GetObjectByPath<TextAsset>(fileName).text);
        }

        private T DeserializeObjectFromPersistentData<T>(string path)
        {
            return DeserializeFromJson<T>(DecryptData(File.ReadAllText(path)));
        }

        private string SerializePersistentObject(object obj)
        {
            string data = SerializeToJson(obj, true);
            return EncryptData(data);
        }

        private void FillFullCollection()
        {
            CachedCollectionData = new CollectionData
            {
                Cards = new List<CollectionCardData>()
            };

            foreach (Data.CardSet set in CachedCardsLibraryData.Sets)
            {
                foreach (Data.Card card in set.Cards)
                {
                    CachedCollectionData.Cards.Add(
                        new CollectionCardData
                        {
                            Amount = (int) GetMaxCopiesValue(card, set.Name),
                            CardName = card.Name
                        });
                }
            }
        }

        private static string GetPersistentDataItemPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}
