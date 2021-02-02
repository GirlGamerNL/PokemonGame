using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver}
public enum BattleAction { Move, SwitchPokemon, UseItem, Run}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit PlayerUnit;
    [SerializeField] BattleUnit EnemyUnit;
    [SerializeField] BattleHud PlayerHud;
    [SerializeField] BattleHud EnemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;



    public event Action<bool> OnBattleOver;

    int currentAction;
    int currentMove;
    int currentMember;
    BattleState state;
    BattleState? prevState;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;


    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

       StartCoroutine(SetupBattle());
    } 
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();


       StartCoroutine(SetupBattle());
    }

    //This is the start of the battle, everything gets set up here
    private IEnumerator SetupBattle()
    {
        PlayerUnit.Clear();
        EnemyUnit.Clear();

        if (!isTrainerBattle)
        {
            //wild battle

            PlayerUnit.Setup(playerParty.GetHealtyPokemon());
            EnemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(PlayerUnit.Pokemon.Moves);

            yield return StartCoroutine(dialogBox.TypeDialog($"A wild {EnemyUnit.Pokemon.Base.Name} appeared!"));
        }
        else
        {
            //trainer battle

            //show trainer sprites
            PlayerUnit.gameObject.SetActive(false);
            EnemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");

            trainerImage.gameObject.SetActive(false);
            EnemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealtyPokemon();
            EnemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sends out {enemyPokemon.Base.Name}!");

            playerImage.gameObject.SetActive(false);          
            PlayerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealtyPokemon();
            PlayerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
            dialogBox.SetMoveNames(PlayerUnit.Pokemon.Moves);
        }

       
        partyScreen.Init();
        ActionSelection();
    }
    
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemon.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    //This is the begin of the players turn
    private void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action!");
        dialogBox.EnableActionSelecter(true);
    }

    //Here the player chooses there move
    private void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelecter(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelecter(true);
    }

    //This handles everything that happens
    public void HandleUpdate() 
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    private void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.D))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.A))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.S))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.W))
            currentMember -= 2;

        currentMove = Mathf.Clamp(currentMember, 0, playerParty.Pokemon.Count - 1);

        partyScreen.UpdatePartySelection(currentMember);

        if (Input.GetKeyDown(KeyCode.X))
        {
            var selectedMember = playerParty.Pokemon[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetTextMessage($"{selectedMember} has no energy left to battle!");
                return;
            }
            if (selectedMember == PlayerUnit.Pokemon)
            {
                partyScreen.SetTextMessage($"{selectedMember} is already in battle");
                return;
            }

            partyScreen.gameObject.SetActive(false);
           
            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }

           
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    //Here you can select what move you want to do
    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.D))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.A))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.S))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.W))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, PlayerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, PlayerUnit.Pokemon.Moves[currentMove]);

        //Here it starts performing the move
        if (Input.GetKeyDown(KeyCode.X))
        {
            var move = PlayerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelecter(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelecter(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {

        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            PlayerUnit.Pokemon.CurrentMove = PlayerUnit.Pokemon.Moves[currentMove];
            EnemyUnit.Pokemon.CurrentMove = EnemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = PlayerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = EnemyUnit.Pokemon.CurrentMove.Base.Priority;

            //Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                   playerGoesFirst = PlayerUnit.Pokemon.Speed >= EnemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? PlayerUnit : EnemyUnit;
            var secondUnit = (playerGoesFirst) ? EnemyUnit : PlayerUnit;

            var SecondPokemon = secondUnit.Pokemon;

            //First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (SecondPokemon.HP > 0)
            {
                //Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }           
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemon[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            //Enemy turn
            var EnemyMove = EnemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(EnemyUnit, PlayerUnit, EnemyMove);
            yield return RunAfterTurn(EnemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeTurn();
        if (!canRunMove)
        {
            yield return ShowStatChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {

            sourceUnit.PlayAttackAnimation();

            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted!");
                targetUnit.PlayFaintAnimation();

                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                }
            }

          
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} missed!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects Effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        var effects = Effects;
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }
        
        if (effects.Status != StatusID.none)
        {
            target.SetStatus(effects.Status);
        }
        
        if (effects.VolatileStatus != StatusID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }
        yield return ShowStatChanges(source);
        yield return ShowStatChanges(target);
    }
   IEnumerator RunAfterTurn(BattleUnit sourceUnit)
   {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        
        
        //Statuses like burn or poison will hurt the pokemon after the turns
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} Fainted!");
            sourceUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        

        float moveAccuracy = move.Base.Accuracy;
        
        int accuracy = source.StatsBoosts[Stat.Accuracy];
        int evasion = target.StatsBoosts[Stat.Evasion];
       
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];
        
        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];
        
        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatChanges(Pokemon pokemon)
    {
        while (pokemon.statsChanges.Count > 0)
        {
            var message = pokemon.statsChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealtyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
                BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
            {
               BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealtyPokemon();
                if (nextPokemon != null)
                    StartCoroutine(SendNextTrainerPokemon(nextPokemon));
                else
                    BattleOver(true);

                        
            }
        }
    }

    //Here it shows when you get a crit, when something is supper effective or not very effective
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1)
        {
            yield return dialogBox.TypeDialog("A critical hit!");

        }

        if (damageDetails.TypeEffectiveness > 1)
        {
            yield return dialogBox.TypeDialog("Its super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("Its not very effective");
        }
      
    }
    
    //Here you get to select the action. Fight or run
    private void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.D))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.A))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.S))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.W))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

            dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                // Bag
            }
            else if (currentAction == 2)
            {
                // Pokemon
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Run
            }
        }
    }

    private void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;

        partyScreen.SetPartyData(playerParty.Pokemon);
        partyScreen.gameObject.SetActive(true);
    }


    IEnumerator SwitchPokemon(Pokemon newpokemon)
    {       
        if (PlayerUnit.Pokemon.HP <= 0)
        {          
            yield return dialogBox.TypeDialog($"{PlayerUnit.Pokemon.Base.Name} come back!!");
            PlayerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }
 
        PlayerUnit.Setup(newpokemon);

        dialogBox.SetMoveNames(newpokemon.Moves);

        yield return StartCoroutine(dialogBox.TypeDialog($"Go {newpokemon.Base.Name}!"));

        state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerPokemon(Pokemon nextPokemon)
    {
        state = BattleState.Busy;

        EnemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} sends out {nextPokemon.Base.Name}");

    }
}
