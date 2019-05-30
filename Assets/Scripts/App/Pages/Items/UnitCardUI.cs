﻿
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Loom.ZombieBattleground.Common;

public class UnitCardUI
{
    private GameObject _selfObj;

    private Image _frameImage;
    private Image _unitImage;
    private Image _rankImage;
    private Image _cardCountTrayImage;

    private TextMeshProUGUI _gooText;
    private TextMeshProUGUI _attackText;
    private TextMeshProUGUI _defenseText;
    private TextMeshProUGUI _bodyText;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _cardCountText;

    private ILoadObjectsManager _loadObjectsManager;

    public void Init(GameObject obj)
    {
        _selfObj = obj;
        _frameImage = _selfObj.transform.Find("Frame").GetComponent<Image>();
        _unitImage = _selfObj.transform.Find("Viewport/Picture").GetComponent<Image>();
        _rankImage = _selfObj.transform.Find("RankIcon").GetComponent<Image>();
        _cardCountTrayImage = _selfObj.transform.Find("AmountWithCounterTray/Tray").GetComponent<Image>();

        _gooText = _selfObj.transform.Find("GooText").GetComponent<TextMeshProUGUI>();
        _attackText = _selfObj.transform.Find("AttackText").GetComponent<TextMeshProUGUI>();
        _defenseText = _selfObj.transform.Find("DefenseText").GetComponent<TextMeshProUGUI>();
        _bodyText = _selfObj.transform.Find("BodyText").GetComponent<TextMeshProUGUI>();
        _titleText = _selfObj.transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        _cardCountText = _selfObj.transform.Find("AmountWithCounterTray/Text").GetComponent<TextMeshProUGUI>();

        _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
    }

    public void FillCardData(Card card, int cardCount = 0)
    {
        _titleText.text = card.Name;
        _bodyText.text = card.Description;
        _gooText.text = card.Cost.ToString();
        _attackText.text = card.Damage.ToString();
        _defenseText.text = card.Defense.ToString();
        _cardCountText.text = cardCount.ToString();

        string frameName = string.Format("Images/Cards/Frames/frame_{0}", card.Faction);
        _frameImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(frameName);

        string rarity = Enum.GetName(typeof(Enumerators.CardRank), card.Rank);
        string rankName = string.Format("Images/IconsRanks/rank_icon_{0}", rarity.ToLower());
        _rankImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(rankName);

        string imagePath = $"{Constants.PathToCardsIllustrations}{card.Picture.ToLowerInvariant()}";
        _unitImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(imagePath);
    }

    public RectTransform GetFrameRectTransform()
    {
        return _frameImage.GetComponent<RectTransform>();
    }

    public void EnableRenderer(bool enable)
    {
        _frameImage.enabled = enable;
        _unitImage.enabled = enable;
        _rankImage.enabled = enable;
        _cardCountTrayImage.enabled = enable;

        _gooText.enabled = enable;
        _attackText.enabled = enable;
        _defenseText.enabled = enable;
        _bodyText.enabled = enable;
        _titleText.enabled = enable;
        _cardCountText.enabled = enable;
    }
}
