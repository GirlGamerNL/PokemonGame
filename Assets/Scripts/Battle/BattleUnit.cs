using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit { get { return isPlayerUnit; } }

    public BattleHud Hud { get { return hud; } }

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public Pokemon Pokemon { get; set; }

    //This is the setup for the individual pokemon
    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        
        if (isPlayerUnit)        
           image.sprite = Pokemon.Base.BackSprite;
        
        else       
          image.sprite = Pokemon.Base.FrontSprite;


        hud.gameObject.SetActive(true);

        hud.SetData(pokemon);
        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    //This plays when the battle starts
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-500f, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(500, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    //This is when a pokemon attacks
    public void PlayAttackAnimation()
    {
        var sequene = DOTween.Sequence();

        if (isPlayerUnit)
            sequene.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequene.Append(image.transform.DOLocalMoveX(originalPos.x - 50, 0.25f));

        sequene.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    //This is when a pokemon get hit
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));

    }

    //This is when a pokemon faints
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0, 0.5f));


    }
}
