﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// the player can either be human or an ai
public enum PlayerType
{
    human,
    ai
}
[System.Serializable]
public class Player
{
    public PlayerType type = PlayerType.ai;
    public int playerNum;
    public SlotDef handSlotDef;
    public List<CardBartok> hand;       // the cards in this player's hand

    // Add a card to the hand
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null) hand = new List<CardBartok>();

        // add the card to the hand
        hand.Add(eCB);

        // sort the cards by rank using LINQ if this is a human
        if (type == PlayerType.human)
        {
            CardBartok[] cards = hand.ToArray();

            // this is the LINQ call
            cards = cards.OrderBy(cd => cd.rank).ToArray();

            hand = new List<CardBartok>(cards);

            // the LINQ operations can be a slow, but since we're only doing it once every round, it's not an issue
        }

        eCB.SetSortingLayerName("10");
        eCB.eventualSortLayer = handSlotDef.layerName;

        FanHand();
        return (eCB);
    }

    // remove a card from the hand
    public CardBartok RemoveCard(CardBartok cb)
    {
        // if hand is null or doesnt contain cb, return null
        if(hand == null || !hand.Contains(cb)) return null;
        hand.Remove(cb);
        FanHand();
        return (cb);
    }

    public void FanHand()
    {
        // start Rot is the rotation about z of the first card
        float startRot = 0;
        startRot = handSlotDef.rot;
        if(hand.Count > 1)
        {
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }

        // move all the cards to their new positions
        Vector3 pos;
        float rot;
        Quaternion rotQ;

        for (int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);

            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;

            pos = rotQ * pos;

            // add the base pos of the player's hand (bottom center of the fan of the cards)
            pos += handSlotDef.pos;
            pos.z = -0.5f * i;

            // if not the initial deal, start moving the card immediately
            if(Bartok.S.phase != TurnPhase.idle)
            {
                hand[i].timeStart = 0;
            }

            // set the localPos and rotation of the ith card in the hand
            hand[i].MoveTo(pos, rotQ);      // told to interpolate
            hand[i].state = CBState.toHand;

            /**
            hand[i].transform.localPosition = pos;
            hand[i].transform.rotation = rotQ;
            hand[i].state = CBState.hand;
            */

            hand[i].faceUp = (type == PlayerType.human);

            // set the SetOrder of the cards so that they overlap properly
            //hand[i].SetSortOrder(i * 4);
            hand[i].eventualSortOrder = i * 4;

        }
    }

    public void TakeTurn()
    {
        Utils.tr("Player.TakeTurn");

        // dont need to do anything if this is the human player
        if(type == PlayerType.human)
        {
            return;
        }

        Bartok.S.phase = TurnPhase.waiting;

        CardBartok cb;

        // if this is an ai player, need to make a choice what to play
        List <CardBartok> validCards = new List<CardBartok>();
        foreach(CardBartok tCB in hand)
        {
            if(Bartok.S.ValidPlay(tCB))
            {
                validCards.Add(tCB);
            }
        }

        // if there are no valid cards
        if(validCards.Count == 0)
        {
            cb = AddCard(Bartok.S.Draw());
            cb.callbackPlayer = this;
            return;
        }

        // pick one if there is a card or more to play
        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }

    public void CBCallback(CardBartok tCB)
    {
        Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        Bartok.S.PassTurn();
    }
}