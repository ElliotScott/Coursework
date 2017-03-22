﻿namespace CourseworkClient
{
    /// <summary>
    /// The protocols that are attached to all data that is sent between the server and the client. The Protocols enumeration on the server should match this exactly.
    /// </summary>
    public enum Protocol
    {
        Connect,
        Disconnect,
        LogIn,
        CreateAccount,
        BadCredentials,
        GoodCredentials,
        UsernameTaken,
        AddToQueue,
        FriendStatus,
        LoggedIn,
        EnterMatch,
        CardData,
        EffectData,
        DeckData,
        DeckCardsData,
        CardEffect,
        DataTransmissionTest,
        UsernameNotTaken,
        PlayUnit,
        PlayTech,
        DiscardTech,
        PlayUpgrade,
        AttackWithUnit,
        DefendWithUnit,
        NoCounter,  
        EndTurn,
        EquipUpgrade,
        KillUnit,
        ControlUnit,
        ReplaceUnit,
        ReturnUnit,
        PlayUnitFromDeck,
        NoCardsInDeck,
        NoCardsInUpgradeDeck,
        DiscardFromDeck,
        DiscardFromUpgradeDeck,
        AddCardToEnemyHand,
        BeginSelection,
        EndSelection,
        AddToEnemyFromDiscard,
        Artillery,
        DeathInHonour,
        RemoveCardFromEnemyHand,
        HealHalf,
        HealFull,
        PowerExtraction,
        AddCardFromDiscard,
        ReturnUnitToHand,
        WonGame,
        EloAndCoins,
        BasicPack,
        PremiumPack,
        PackCards,
        UpdatedDecks,
        NewDeckCards,
        NewDBDeckID,
    }
}
