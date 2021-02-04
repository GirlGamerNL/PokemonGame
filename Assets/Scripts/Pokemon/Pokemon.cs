using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
   [SerializeField] PokemonBase _base;
   [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }


    public PokemonBase Base { get { return _base; }}

    public int Level { get { return level; } }
    public int HP { get; set; }

    public int Exp { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatsBoosts { get; private set; }
    public StatusConditions Status { get; private set; }
    public StatusConditions VolatileStatus { get; private set; }
    public int StatusTime { get; set; }
    public int VolatileStatusTime { get; set; }

    public Queue<string> StatsChanges { get; private set; } 
    public bool HpChanged { get; set; }

    

    public event System.Action OnStatusChanged;

    //This is the setup for the beginning of the battle, and the leanable moves
    public void Init()
    {
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)           
                Moves.Add(new Move(move.Base));
            
            if (Moves.Count >= PokemonBase.MaxNumOfMoves)           
                break;        
        }

        Exp = Base.GetExpForLevel(level);
        
        CalculateStats();
        HP = MaxHp;

        StatsChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    int GetStat(Stat stat)
    {
        int statval = Stats[stat];

        int boost = StatsBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        return statval;
    }

    private void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defence, Mathf.FloorToInt((Base.Defence * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SPAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefence, Mathf.FloorToInt((Base.SPDefence * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHP * level) /100f) + 10 + Level;
    }

    void ResetStatBoost()
    {
        StatsBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            { Stat.Defence, 0},
            { Stat.SpAttack, 0},
            { Stat.SpDefence, 0},
            { Stat.Speed, 0},
            { Stat.Accuracy, 0},
            { Stat.Evasion, 0}
        };
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statsBoost in statBoosts)
        {
            var stat = statsBoost.stat;
            var boost = statsBoost.boost;

            StatsBoosts[stat] = Mathf.Clamp(StatsBoosts[stat] + boost, - 6, 6);

            if (boost > 0)
                StatsChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else
                StatsChanges.Enqueue($"{Base.Name}'s {stat} fell!");

            Debug.Log($"{stat} has been boosted to {StatsBoosts[stat]}");

        }
    } 

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
       return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;

        Moves.Add(new Move(moveToLearn.Base));

        
    }
    
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }   
     public int Defence
    {
        get { return GetStat(Stat.Defence); }
    }   
     public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }   
     public int SpDefence
    {
        get { return GetStat(Stat.SpDefence); }
    }   
     public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }   
    
    public int MaxHp { get; private set; }

    //Here you can (or the opponent) takes damage (real calculations)
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;

        if (Random.value * 100f <= 6.25)
        {
            critical = 2f;
        }

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false

        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defence = (move.Base.Category == MoveCategory.Special) ? attacker.SpDefence : attacker.Defence;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (5 * attacker.Level + 10) / 250;
        float d = a * move.Base.Power * ((float)attack / defence) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);


        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }


    public void SetStatus(StatusID statusID)
    {
        if (Status != null) return;

        Status = StatusConditionsDB.conditions[statusID];
        Status?.Onstart?.Invoke(this);
        StatsChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }
    public void SetVolatileStatus(StatusID statusID)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = StatusConditionsDB.conditions[statusID];
        VolatileStatus?.Onstart?.Invoke(this);
        StatsChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
        
    }
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    //This is for the enemy to choose a move
    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeTurn()
    {
        bool canPerfromMove = true;
        if (Status?.OnBeforeTurn != null)
        {
            if (!Status.OnBeforeTurn(this))
                canPerfromMove = false;
        } 
        
        if (VolatileStatus?.OnBeforeTurn != null)
        {
            if (!VolatileStatus.OnBeforeTurn(this))
                canPerfromMove = false;
        }
        return canPerfromMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();

    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}