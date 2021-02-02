using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemon;

    //The wild pokemon gets setup here
    public Pokemon GetRandomWildPokemon()
    {
        var wildpokemon = wildPokemon[Random.Range(0, wildPokemon.Count)];
        wildpokemon.Init();
        return wildpokemon;
    }
}
