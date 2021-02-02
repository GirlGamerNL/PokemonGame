using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemon;

    //This is going to be where you can switch pokemon in the middle of the battle

    public List<Pokemon> Pokemon { get { return pokemon; } }
    
    private void Start()
    {
        foreach (var pokemon in pokemon)
        {
            pokemon.Init();
        }
    }

    public Pokemon GetHealtyPokemon()
    {
        return pokemon.Where(x => x.HP > 0).FirstOrDefault();
    }
}
