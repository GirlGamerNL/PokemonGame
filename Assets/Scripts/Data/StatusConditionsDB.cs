using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<StatusID, StatusConditions> conditions { get; set; } = new Dictionary<StatusID, StatusConditions>()
    {
        {
            StatusID.psn,
            new StatusConditions()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} was hurt by the poison");
                }
            }
        },
        {
            StatusID.brn,
            new StatusConditions()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp / 16);
                    pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} was hurt by the burn");
                }
            }
        },
        {
            StatusID.par,
            new StatusConditions()
            {
                Name = "Paralysis",
                StartMessage = "has been paralysised, they may not be able to attack",
                OnBeforeTurn = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                       pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} is paralyzed! It can't move!");
                       return false;
	                }
                    return true;
                }
            }
        },
        {
            StatusID.frz,
            new StatusConditions()
            {
                Name = "Freeze",
                StartMessage = "Became frozen",
                OnBeforeTurn = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                       pokemon.CureStatus();
                       pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} is not frozen anymore!");
                       return false;
	                }
                    pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} is frozen solid!");
                    return true;
                }
            }
        },
        {
            StatusID.slp,
            new StatusConditions()
            {
                Name = "Sleep",
                StartMessage = "fell asleep!",
                Onstart = (Pokemon pokemon) =>
                {
                    //Sleep for 1-3 turns
                    pokemon.StatusTime = Random.Range(1,4);
                },
                OnBeforeTurn = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true;
	                }

                    pokemon.StatusTime--;
                    pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} is fast asleep!");
                    return false;
                }
            }
        },
       
        //Volatile status conditions
        {
            StatusID.confusion,
            new StatusConditions()
            {
                Name = "Confusion",
                StartMessage = "has been confused!",
                Onstart = (Pokemon pokemon) =>
                {
                    //Confused for 1-4 turns
                    pokemon.VolatileStatusTime = Random.Range(1,5);
                },
                OnBeforeTurn = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} is no longer confused");
                        return true;
	                }

                    pokemon.VolatileStatusTime--;

                    if (Random.Range(1, 4) == 1)
                    return true;

	                
                    pokemon.StatsChanges.Enqueue($"{pokemon.Base.Name} is confused!");
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatsChanges.Enqueue("It hit itself in confusion");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(StatusConditions conditions)
    {
        if (conditions == null)
            return 1f;
        else if (conditions.Id == StatusID.slp || conditions.Id == StatusID.frz)
            return 2.5f;
        else if (conditions.Id == StatusID.par || conditions.Id == StatusID.psn || conditions.Id == StatusID.brn)
            return 1.5f;

        
        
        return 1f;
    }
}

public enum StatusID
{
    none, psn, brn, slp, par, frz, confusion
}
