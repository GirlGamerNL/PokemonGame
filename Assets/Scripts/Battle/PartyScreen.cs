﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

   PartyMemberUI[] memberSlots;
    List<Pokemon> pokemon;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Pokemon> pokemon)
    {
        this.pokemon = pokemon;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemon.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemon[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Choose a Pokemon";
    }

    public void UpdatePartySelection(int selectedMember)
    {
        for (int i = 0; i < pokemon.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void SetTextMessage(string message)
    {
        messageText.text = message;
    }
}
