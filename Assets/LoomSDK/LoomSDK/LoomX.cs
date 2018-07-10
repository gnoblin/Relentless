﻿
using System;
using Loom.Unity3d;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using GrandDevs.Internal;

public static class LoomX
{
    private static Contract _contract;
    private static readonly string FileName = Application.dataPath+"/PrivateKey.txt";
    private static string Key_PrivateKey = "PrivateKey";
    
    private static byte[] GetPrivateKeyFromFile()
    {
        byte[] privateKey;
        if (File.Exists(FileName))
            privateKey = File.ReadAllBytes(FileName);
        else
        {
            privateKey = CryptoUtils.GeneratePrivateKey();
            File.WriteAllBytes(FileName, privateKey);
        }

        return privateKey;
    }

    private static byte[] GetPrivateKeyFromPlayerPrefs()
    {
        byte[] privateKey;
        if (PlayerPrefs.HasKey(Key_PrivateKey))
        {
            var privateKeyStr = PlayerPrefs.GetString(Key_PrivateKey);
            privateKey = Convert.FromBase64String(privateKeyStr);
        }
        else
        {
            privateKey = CryptoUtils.GeneratePrivateKey();
            var privateKeyStr = Convert.ToBase64String(privateKey);
            PlayerPrefs.SetString(Key_PrivateKey, privateKeyStr);
        }

        return privateKey;
    }

    private static async Task Init()
    {
        var privateKey = GetPrivateKeyFromPlayerPrefs();
        var publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
        var callerAddr = Address.FromPublicKey(publicKey);

        var writer = RPCClientFactory.Configure()
            .WithLogger(Debug.unityLogger)
            .WithWebSocket("ws://127.0.0.1:46657/websocket")
            .Create();

        var reader = RPCClientFactory.Configure()
            .WithLogger(Debug.unityLogger)
            .WithWebSocket("ws://127.0.0.1:9999/queryws")
            .Create();

        var client = new DAppChainClient(writer, reader)
        {
            Logger = Debug.unityLogger
        };
        
        client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]{
            new NonceTxMiddleware{
                PublicKey = publicKey,
                Client = client
            },
            new SignedTxMiddleware(privateKey)
        });

        var contractAddr = await client.ResolveContractAddressAsync("ZombieBattleground");
        _contract = new Contract(client, contractAddr, callerAddr);
    }

    
    public static async void SignUp(CreateAccountRequest accountTx, Action<BroadcastTxResult> result)
    {
        if (_contract == null)
        {
            await Init();
        }
        
        await _contract.CallAsync<CreateAccountRequest>("CreateAccount", accountTx, result);
    }
    
    
    public static async void SetMessage(MapEntry entry)
    {
        if (_contract == null)
        {
            await Init();
        }

        await _contract.CallAsync("SetMsg", entry);
    }
    

    public static async void SetMessageEcho(MapEntry entry, Action<BroadcastTxResult> result)
    {
        if (_contract == null)
        {
            await Init();
        }

        await _contract.CallAsync<MapEntry>("SetMsgEcho", entry, result);
    }

    
    public static async void GetMessage(MapEntry entry, Action<MapEntry> result)
    {
        if (_contract == null)
        {
            await Init();
        }

        var mapEntry = await _contract.StaticCallAsync<MapEntry>("GetMsg", entry);
        result?.Invoke(mapEntry);
    }

}
