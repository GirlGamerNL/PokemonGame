using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move 
{
  public MoveBase Base { get; set; }

    public int PP { get; set; }

    //This is very basic for the moves
    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;  
    }
}
