using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;
    
    [TextArea]
    [SerializeField] string description;
    
    
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;


    //Base stats
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int spAttack;
    [SerializeField] int spDefence;
    [SerializeField] int speed;
    
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;
   
    
    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;

    public static int MaxNumOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }

        return -1;
    }

    //This is all the info for the pokemon
    public string Name
    {
        get { return name; }
    } 
    
    public string Description
    {
        get { return description; }
    }   
    
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }   
    public Sprite BackSprite
    {
        get { return backSprite; }
    }   
    
    public PokemonType Type1
    {
        get { return type1; }
    } 
    public PokemonType Type2
    {
        get { return type2; }
    }  
    public int MaxHP
    {
        get { return maxHP; }
    }  
    public int Attack
    {
        get { return attack; }
    } 
    public int Defence
    {
        get { return defence; }
    }  
    public int SPAttack
    {
        get { return spAttack; }
    }  
    public int SPDefence
    {
        get { return spDefence; }
    }  
    public int Speed
    {
        get { return speed; }
    }

    public int CatchRate => catchRate;

    public int ExpYield => expYield;

    public GrowthRate GrowthRate => growthRate;

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}



public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Grass,
    Electric,
    Poison,
    Steel,
    Dark,
    Fairy,
    Dragon,
    Fighting,
    Flying,
    Psychic,
    Ice,
    Bug,
    Rock,
    Ground,
    Ghost
}

public enum GrowthRate
{
    Fast, MediumFast
}

public enum Stat
{
    Attack,
    Defence,
    SpAttack,
    SpDefence,
    Speed,


    Accuracy,
    Evasion
}

public class TypeChart
{
    //This is the whole type chart for effectiveness

   static float[][] chart =
    {
        //                           nor   fir     wat     gra     ele     poi     ste     dark    fai     dra     figh    fly     psy     ice     bug        roc      gro       gho
        /*Normal*/      new float[] {1.0f , 1.0f  , 1.0f  , 1.0f , 1.0f,  1.0f,    1.0f,   1.0f,   1.0f,   1.0f,   1.0f,  1.0f,    1.0f,   1.0f,   1.0f,     0.5f,    1.0f,     0.0f},
       
        /*Fire*/        new float[] {1.0f , 0.5f  , 0.5f  , 2.0f , 1.0f,  1.0f,    2.0f,   1.0f,   1.0f,   0.5f,   1.0f,  1.0f,    1.0f,   2.0f,   2.0f,     0.5f,    1.0f,     1.0f},
       
        /*Water*/       new float[] {1.0f , 2.0f  , 0.5f  , 0.5f , 1.0f,  1.0f,    1.0f,   1.0f,   1.0f,   0.5f,   1.0f,  1.0f,    1.0f,   1.0f,   1.0f,     2.0f,    2.0f,     1.0f},
       
        /*Grass*/       new float[] {1.0f , 0.5f  , 2.0f  , 0.5f , 1.0f,  0.5f,    0.5f,   1.0f,   1.0f,   0.5f,   1.0f,  0.5f,    1.0f,   1.0f,   0.5f,     2.0f,    2.0f,     1.0f},
        
        /*Electric*/    new float[] {1.0f , 1.0f  , 2.0f  , 0.5f , 0.5f,  1.0f,    1.0f,   1.0f,   1.0f,   0.5f,   1.0f,  2.0f,    1.0f,   1.0f,   1.0f,     1.0f,    0.0f,     1.0f},
        
        /*Poison*/      new float[] {1.0f , 1.0f  , 1.0f  , 2.0f , 1.0f,  0.5f,    0.0f,   1.0f,   2.0f,   1.0f,   1.0f,  1.0f,    1.0f,   1.0f,   1.0f,     0.5f,    0.5f,     0.5f},
        
        /*Steel*/       new float[] {1.0f , 0.5f  , 0.5f  , 1.0f , 0.5f,  1.0f,    0.5f,   1.0f,   2.0f,   1.0f,   1.0f,  1.0f,    1.0f,   2.0f,   1.0f,     2.0f,    1.0f,     1.0f},
        
        /*Dark*/        new float[] {1.0f , 1.0f  , 1.0f  , 1.0f , 1.0f,  1.0f,    1.0f,   0.5f,   0.5f,   0.5f,   0.5f,  1.0f,    2.0f,   1.0f,   1.0f,     1.0f,    1.0f,     2.0f},
        
        /*Fairy*/       new float[] {1.0f , 0.5f  , 1,0f  , 1.0f , 1.0f,  0.5f,    0.5f,   2.0f,   1.0f,   2.0f,   2.0f,  1.0f,    1.0f,   1.0f,   1.0f,     1.0f,    1.0f,     1.0f},
        
        /*Dragon*/      new float[] {1.0f , 1.0f  , 1.0f  , 1.0f , 1.0f,  1.0f,    0.5f,   1.0f,   0.0f,   2.0f,   1.0f,  1.0f,    1.0f,   1.0f,   1.0f,     1.0f,    1.0f,     1.0f},
        
        /*Fighting*/    new float[] {2.0f , 1.0f  , 1.0f  , 1.0f , 1.0f,  0.5f,    2.0f,   2.0f,   0.5f,   1.0f,   1.0f,  0.5f,    0.5f,   2.0f,   0.5f,     2.0f,    1.0f,     0.0f},
        
        /*Flying*/      new float[] {1.0f , 1.0f  , 1.0f  , 2.0f , 0.5f,  1.0f,    0.5f,   1.0f,   1.0f,   1.0f,   2.0f,  1.0f,    1.0f,   1.0f,   2.0f,     0.5f,    1.0f,     1.0f},
       
        /*Psychic*/     new float[] {1.0f , 1.0f  , 1.0f  , 1.0f , 1.0f,  2.0f,    0.5f,   0.0f,   1.0f,   1.0f,   2.0f,  1.0f,    0.5f,   1.0f,   1.0f,     1.0f,    1.0f,     1.0f},
        
        /*Ice*/         new float[] {1.0f , 0.5f  , 0.5f  , 2.0f , 1.0f,  1.0f,    1.0f,   1.0f,   1.0f,   2.0f,   1.0f,  2.0f,    1.0f,   0.5f,   1.0f,     1.0f,    2.0f,     1.0f},
        
        /*Bug*/         new float[] {1.0f , 0.5f  , 1.0f  , 2.0f , 1.0f,  0.5f,    0.5f,   2.0f,   0.5f,   1.0f,   0.5f,  0.5f,    1.0f,   1.0f,   1.0f,     1.0f,    1.0f,     0.5f},
        
        /*Rock*/        new float[] {1.0f , 2.0f  , 1.0f  , 1.0f , 1.0f,  1.0f,    0.5f,   1.0f,   1.0f,   1.0f,   0.5f,  2.0f,    1.0f,   2.0f,   2.0f,     1.0f,    0.5f,     1.0f},
       
        /*Ground*/      new float[] {1.0f , 2.0f  , 1.0f  , 0.5f , 2.0f,  2.0f,    2.0f,   1.0f,   1.0f,   1.0f,   1.0f,  0.0f,    1.0f,   1.0f,   0.5f,     2.0f,    1.0f,     1.0f},
        
        /*Ghost*/       new float[] {0.0f , 1.0f  , 1.0f  , 1.0f , 1.0f,  1.0f,    1.0f,   0.5f,   1.0f,   1.0f,   1.0f,  1.0f,    2.0f,   1.0f,   1.0f,     1.0f,    1.0f,     2.0f},
     

    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
        {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}