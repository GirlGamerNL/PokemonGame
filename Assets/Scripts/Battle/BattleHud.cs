using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPbar hpbar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Pokemon _pokemon;
    Dictionary<StatusID, Color> statusColors;


    //Here it sets the data for whatever pokemon are in battle
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = " Lvl " + pokemon.Level;
        hpbar.SetHP((float)pokemon.HP / pokemon.MaxHp);

        statusColors = new Dictionary<StatusID, Color>()
        {
            {StatusID.psn, psnColor },
            {StatusID.brn, brnColor },
            {StatusID.par, parColor },
            {StatusID.frz, frzColor },
            {StatusID.slp, slpColor }
        };

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

     void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.Id];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (_pokemon.HpChanged)
        {
            yield return hpbar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
       
    }
}
