using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlight;
    
    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelecter;
    [SerializeField] GameObject moveSelecter;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;
   
    [SerializeField] Text ppText;
    [SerializeField] Text typeText; 
    
    [SerializeField] Text noText;
    [SerializeField] Text yesText;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    //Here you can type what needs to be in the dialogbox
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabeled)
    {
        dialogText.enabled = enabeled;
    } 
    public void EnableActionSelecter(bool enabeled)
    {
        actionSelecter.SetActive(enabeled);
    }
    public void EnableMoveSelecter(bool enabeled)
    {
        moveSelecter.SetActive(enabeled);
        moveDetails.SetActive(enabeled);
    }
    
    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    //if you hover over an action, it gets highlighted
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = highlight;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }
    } 
    
    //if you hover over a move, it gets highlighted
    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i == selectedMove)
            {
                moveTexts[i].color = highlight;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }

        ppText.text = $"PP {move.PP} / {move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;

        
    }

    //Here it shows the moves you can choose from
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }
    
    //Show if yes or no are highlighted
    public void UpdateChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = highlight;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = highlight;
        }
    }
}
