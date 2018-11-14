#if UNITY_EDITOR
#define DISABLE_DATA_ENCRYPTION
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DataManager : IService, IDataManager
    {
        private ILocalizationManager _localizationManager;

        private ILoadObjectsManager _loadObjectsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Dictionary<Enumerators.CacheDataType, string> _cacheDataFileNames;

        private DirectoryInfo _dir;

        public DataManager(ConfigData configData)
        {
            FillCacheDataPaths();
            InitCachedData();
            ConfigData = configData;
        }

        private void InitCachedData()
        {
            CachedUserLocalData = new UserLocalData();
            CachedCardsLibraryData = new CardsLibraryData();
            CachedHeroesData = new HeroesData();
            CachedCollectionData = new CollectionData();
            CachedDecksData = new DecksData();
            CachedOpponentDecksData = new OpponentDecksData();
            CachedCreditsData = new CreditsData();
            CachedBuffsTooltipData = new TooltipContentData();
        }

        public TooltipContentData CachedBuffsTooltipData { get; set; }

        public UserLocalData CachedUserLocalData { get; set; }

        public CardsLibraryData CachedCardsLibraryData { get; set; }

        public HeroesData CachedHeroesData { get; set; }

        public CollectionData CachedCollectionData { get; set; }

        public DecksData CachedDecksData { get; set; }

        public OpponentDecksData CachedOpponentDecksData { get; set; }

        public CreditsData CachedCreditsData { get; set; }

        public ConfigData ConfigData { get; set; }

        public BetaConfig BetaConfig { get; set; }

        public async Task LoadRemoteConfig()
        {
            BetaConfig = await _backendFacade.GetBetaConfig(_backendDataControlMediator.UserDataModel.BetaKey);
            if (BetaConfig == null)
                throw new Exception("BetaConfig == null");
        }

        public async Task StartLoadCache()
        {
            Debug.Log("=== Start loading server ==== ");

            int count = Enum.GetNames(typeof(Enumerators.CacheDataType)).Length;
            for (int i = 0; i < count; i++)
            {
                await LoadCachedData((Enumerators.CacheDataType) i);
            }

            CachedCardsLibraryData.FillAllCards();

            // FIXME: remove next line after fetching collection from backend is implemented
            FillFullCollection();

            _localizationManager.ApplyLocalization();

#if DEV_MODE
            CachedUserLocalData.Tutorial = false;
#endif

            GameClient.Get<IApplicationSettingsManager>().ApplySettings();

            GameClient.Get<IGameplayManager>().IsTutorial = CachedUserLocalData.Tutorial;
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
            Debug.Log("== Saving cache type " + type);

            switch (type)
            {
                case Enumerators.CacheDataType.USER_LOCAL_DATA:
                    File.WriteAllText(GetPersistentDataItemPath(_cacheDataFileNames[type]), SerializeObject(CachedUserLocalData));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        public TooltipContentData.BuffInfo GetBuffInfoByType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return null;

            return CachedBuffsTooltipData.Buffs.Find(x => x.Type.ToLowerInvariant().Equals(type.ToLowerInvariant()));
        }

        public TooltipContentData.RankInfo GetRankInfoByType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return null;

            return CachedBuffsTooltipData.Ranks.Find(x => x.Type.ToLowerInvariant().Equals(type.ToLowerInvariant()));
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

            _dir = new DirectoryInfo(Application.persistentDataPath + "/");

            LoadLocalCachedData();

            GameClient.Get<ISoundManager>().ApplySoundData();

            CheckVersion();
        }

        public void Update()
        {
        }

        private uint GetMaxCopiesValue(Data.Card card, string setName)
        {
            Enumerators.CardRank rank = card.CardRank;
            uint maxCopies;

            if (setName.ToLowerInvariant().Equals("item"))
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

        private async Task LoadCachedData(Enumerators.CacheDataType type)
        {
            switch (type)
            {
                case Enumerators.CacheDataType.CARDS_LIBRARY_DATA:
                    string cardsLibraryFilePath = GetPersistentDataItemPath(_cacheDataFileNames[type]);
                    if (ConfigData.SkipBackendCardData && File.Exists(cardsLibraryFilePath))
                    {
                        Debug.LogWarning("===== Loading Card Library from cache ===== ");
                        CachedCardsLibraryData = DeserializeObjectFromPersistentData<CardsLibraryData>(cardsLibraryFilePath);
                    }
                    else
                    {
                        ListCardLibraryResponse listCardLibraryResponse = await _backendFacade.GetCardLibrary();
                        Debug.Log(listCardLibraryResponse.ToString());
                        CachedCardsLibraryData = listCardLibraryResponse.FromProtobuf();
                    }

                    break;
                case Enumerators.CacheDataType.HEROES_DATA:
                    ListHeroesResponse heroesList = await _backendFacade.GetHeroesList(_backendDataControlMediator.UserDataModel.UserId);
                    CachedHeroesData = JsonConvert.DeserializeObject<HeroesData>(heroesList.ToString());

                    break;
                case Enumerators.CacheDataType.COLLECTION_DATA:
                    GetCollectionResponse getCollectionResponse = await _backendFacade.GetCardCollection(_backendDataControlMediator.UserDataModel.UserId);
                    CachedCollectionData = getCollectionResponse.FromProtobuf();
                    break;
                case Enumerators.CacheDataType.DECKS_DATA:
                    ListDecksResponse listDecksResponse = await _backendFacade.GetDecks(_backendDataControlMediator.UserDataModel.UserId);
                    CachedDecksData = new DecksData();
                    CachedDecksData.Decks =
                        listDecksResponse.Decks
                            .Select(d => JsonConvert.DeserializeObject<Data.Deck>(d.ToString()))
                            .ToList();
                    break;
                case Enumerators.CacheDataType.DECKS_OPPONENT_DATA:
                    GetAIDecksResponse decksAIResponse = await _backendFacade.GetAIDecks();
                    CachedOpponentDecksData = new OpponentDecksData();
                    CachedOpponentDecksData.Decks =
                        decksAIResponse.Decks
                            .Select(d => JsonConvert.DeserializeObject<Data.Deck>(d.ToString()))
                            .ToList();
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
                    Enumerators.CacheDataType.HEROES_DATA, Constants.LocalHeroesDataFileName
                },
                {
                    Enumerators.CacheDataType.COLLECTION_DATA, Constants.LocalCollectionDataFileName
                },
                {
                    Enumerators.CacheDataType.DECKS_DATA, Constants.LocalDecksDataFileName
                },
                {
                    Enumerators.CacheDataType.DECKS_OPPONENT_DATA,  Constants.LocalOpponentDecksDataFileName
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
            if (!ConfigData.EncryptData)
                return data;

            return Utilites.Decrypt(data, Constants.PrivateEncryptionKeyForApp);
        }

        public string EncryptData(string data)
        {
            if (!ConfigData.EncryptData)
                return data;

            return Utilites.Encrypt(data, Constants.PrivateEncryptionKeyForApp);
        }

        private T DeserializeObjectFromAssets<T>(string fileName)
        {
            return JsonConvert.DeserializeObject<T>(_loadObjectsManager.GetObjectByPath<TextAsset>(fileName).text);
        }

        private T DeserializeObjectFromPersistentData<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(DecryptData(File.ReadAllText(path)));
        }

        private string SerializeObject(object obj)
        {
            string data = JsonConvert.SerializeObject(obj, Formatting.Indented);
            return EncryptData(data);
        }

        private void FillFullCollection()
        {
            CachedCollectionData = new CollectionData();
            CachedCollectionData.Cards = new List<CollectionCardData>();

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
