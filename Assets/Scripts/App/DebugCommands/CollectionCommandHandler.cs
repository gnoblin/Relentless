﻿using System;
using System.Collections.Generic;
using System.Linq;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Iap;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public static class CollectionCommandHandler
    {
        private static BackendDataControlMediator _backendDataControlMediator;
        private static PlasmachainBackendFacade _plasmaChainBackendFacade;
        private static BackendFacade _backendFacade;
        private static IDataManager _dataManager;
        private static BackendDataSyncService _backendDataSyncService;

        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(CollectionCommandHandler));

            _backendFacade = GameClient.Get<BackendFacade>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmachainBackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendDataSyncService = GameClient.Get<BackendDataSyncService>();
        }

        [CommandHandler]
        public static async void RequestUserFullCardCollectionSync()
        {
            await _backendFacade.RequestUserFullCardCollectionSync(_backendDataControlMediator.UserDataModel.UserId);
            Debug.Log("Added request for full card sync");
        }

        [CommandHandler]
        public static void SetUserCollectionDirtyFlag()
        {
            _backendDataSyncService.SetCollectionDataDirtyFlag();
        }

        [CommandHandler]
        public static async void PlasmachainGetOwnedCards()
        {
            using (DAppChainClient client = await _plasmaChainBackendFacade.GetConnectedClient())
            {
                IReadOnlyList<CollectionCardData> cardsOwned = await _plasmaChainBackendFacade.GetCardsOwned(client);
                cardsOwned =
                    cardsOwned
                        .OrderBy(c => c.CardKey.MouldId.Id)
                        .ThenBy(c => c.CardKey.Variant)
                        .ToList();

                Debug.Log($"Result: {cardsOwned.Count} cards:\n" + String.Join("\n", cardsOwned));
            }
        }
    }
}
