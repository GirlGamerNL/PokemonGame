using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;


    public event Action OnEncounterd;
    public event Action<Collider2D> OnEnterTrainersView;

    private Vector2 input;
    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    //Here the player moves when you press the buttons
    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;
            
           
            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.X))
        {
            Interact();
        }
    }

    private void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.interactableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
        
    }

    private void OnMoveOver()
    {
        CheckForEncounters();
        CheckIfInTrainersView();
    }

    //When you walk through grass, you can get an encounter
    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.grassLayer) != null)
        {
            if (Random.Range(1, 101) <= 10)
            {
                character.Animator.IsMoving = false;
                OnEncounterd();
            }
        }
    }

    private void CheckIfInTrainersView()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.fovLayer) != null)
        {
            var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.fovLayer);
            if (collider != null)
            {
                character.Animator.IsMoving = false;
                OnEnterTrainersView?.Invoke(collider);
            }
        }
    }


    public string Name { get => name; }
    public Sprite Sprite { get => sprite; }
}
