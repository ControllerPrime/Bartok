﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bartok : MonoBehaviour
{
    static public Bartok S;

    [Header("set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;

    [Header("Set Dynamically")]
    public Deck deck;
    public List<CardBartok> drawPile;
    public List<CardBartok> discardPile;

    void Awake()
    {
        S = this;
    }

    void Start()
    {

        deck = GetComponent<Deck>();   // Get the deck
        deck.InitDeck(deckXML.text);   // Pass DeckXML to it
        Deck.Shuffle(ref deck.cards);   // This shuffles the deck
    }
}
