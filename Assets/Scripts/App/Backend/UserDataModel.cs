﻿using System;

namespace Loom.ZombieBattleground.BackendCommunication
{
    [Serializable]
    public class UserDataModel
    {
        public string UserId;

        public byte[] PrivateKey;

        public bool IsValid;

        public bool IsRegistered;

        public string Email;

        public string Password;

        public string GUID;

        public string AccessToken;

        public UserDataModel(string userId, byte[] privateKey)
        {
            UserId = userId;
            PrivateKey = privateKey;
        }

        public override string ToString()
        {
            return $"(UserId: {UserId}, Email: {Email}, IsValid: {IsValid})";
        }
    }
}
