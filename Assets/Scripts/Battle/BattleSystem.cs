using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, BattleOver}
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
    [SerializeField] GameObject pokeballSprite;



    public event Action<bool> OnBattleOver;

    int currentAction;
    int currentMove;
    int currentMember;
    BattleState state;
    BattleState? prevState;
    bool aboutToUseChoice;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;


    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

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

        escapeAttempts = 0;
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
        dialogBox.SetDialog($"Choose an action!");
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

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to send out {newPokemon.Base.Name}. Switch Pokemon?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
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
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
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
            if (PlayerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetTextMessage($"Choose Pokemon to continue");
                return;
            }


            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.AboutToUse)
            {
                prevState = null;
                StartCoroutine(SendNextTrainerPokemon());
            }

            else
                ActionSelection();
        }
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);

            if (aboutToUseChoice == true)
            {
                prevState = BattleState.AboutToUse;
                OpenPartyScreen();
            }
            else
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
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
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelecter(false);
                yield return ThrowPokeball();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
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
                yield return HandlePokemonFainted(targetUnit);
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
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
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
        while (pokemon.StatsChanges.Count > 0)
        {
            var message = pokemon.StatsChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} fainted!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            //exp gain
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            PlayerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{PlayerUnit.Pokemon.Base.Name} gained {expGain} exp!");
            yield return PlayerUnit.Hud.SetExpSmooth();


            //level up
            while (PlayerUnit.Pokemon.CheckForLevelUp())
            {
                PlayerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{PlayerUnit.Pokemon.Base.Name} grew to level {PlayerUnit.Pokemon.Level}!");

                yield return PlayerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
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
                    StartCoroutine(AboutToUse(nextPokemon));
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
            yield return dialogBox.TypeDialog($"A critical hit!");

        }

        if (damageDetails.TypeEffectiveness > 1)
        {
            yield return dialogBox.TypeDialog($"Its super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog($"Its not very effective");
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
              StartCoroutine(RunTurns(BattleAction.UseItem));
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
                StartCoroutine(RunTurns(BattleAction.Run));
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

        if (prevState == null)
        {

           state = BattleState.RunningTurn;
        }
        else if (prevState == BattleState.AboutToUse)
        {
            prevState = null;
            StartCoroutine(SendNextTrainerPokemon());
        }

    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealtyPokemon(); 
        EnemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} sends out {nextPokemon.Base.Name}");

        state = BattleState.RunningTurn;

    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal another trainer's pokemon!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"Go Pokeball!!");

        var pokeballObj = Instantiate(pokeballSprite, PlayerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

        yield return pokeball.transform.DOJump(EnemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return EnemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(EnemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(EnemyUnit.Pokemon);

        for (int i = 0; i <Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            yield return dialogBox.TypeDialog($"Gotcha! {EnemyUnit.Pokemon.Base.Name} was caught!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(EnemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{EnemyUnit.Pokemon.Base.Name} has been added to your party!");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return EnemyUnit.PlayBreakOutAnimation();

            if (shakeCount == 0)
                yield return dialogBox.TypeDialog($"Oh no! The Pokemon broke free!");
            else if (shakeCount == 1)
                yield return dialogBox.TypeDialog($"Aww! It appeared to be caught!");
            else if (shakeCount == 2)  
                yield return dialogBox.TypeDialog($"Aargh! Almost had it!");
            else if (shakeCount == 3)
                yield return dialogBox.TypeDialog($"Gah! It was so close too!");

            Destroy(pokeball);
            state = BattleState.RunningTurn;
            
        }
        

    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * StatusConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = PlayerUnit.Pokemon.Speed;
        int enemySpeed = EnemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
