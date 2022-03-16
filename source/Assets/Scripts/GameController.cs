using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    static public GameController instance = null;

    public Deck playerDeck = new Deck();
    public Deck enemyDeck  = new Deck();

    public Hand playersHand = new Hand();
    public Hand enemysHand  = new Hand();

    public Player player = null;
    public Player enemy  = null;

    public List<CardData> cards = new List<CardData>();

    public Sprite[] healthNumbers = new Sprite[10];
    public Sprite[] damageNumbers = new Sprite[10];

    public GameObject cardPrefab = null;
    public Canvas     canvas     = null;

    public bool isPlayable = false;

    public GameObject effectFromLeftPrefab  = null;
    public GameObject effectFromRightPrefab = null;

    public Sprite fireBallImage       = null;
    public Sprite iceBallImage        = null;
    public Sprite multiFireBallImage  = null;
    public Sprite multiIceBallImage   = null;
    public Sprite fireAndIceBallImage = null;
    public Sprite DestructBallImage = null;

    public bool playersTurn = true;

    public Text turnText  = null;
    public Text scoreText = null;

    public int playerScore = 0;
    public int playerKills = 0;

    public Image enemySkipTurn = null;

    public Sprite fireDemon = null;
    public Sprite iceDemon = null;

    public AudioSource playerDieAudio  = null;
    public AudioSource enemyDieAudio   = null;

    private void Awake()
    {
        instance = this;

        SetUpEnemy();

        playerDeck.Create();
        enemyDeck.Create();

        StartCoroutine(DealHands());
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    public void SkipTurn()
    {
        if (playersTurn && isPlayable)
            NextPlayersTurn();
    }

    internal IEnumerator DealHands()
    {
        yield return new WaitForSeconds(1);
        for (int t=0;t<3;t++)
        {
            playerDeck.DealCard(playersHand);
            enemyDeck.DealCard(enemysHand);
            yield return new WaitForSeconds(1);
        }
        isPlayable = true;
    }

    internal bool UseCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        // RemoveCard
        // DealReplacementCard

        if (!CardValid(card,usingOnPlayer,fromHand))
            return false;

        isPlayable = false;

        CastCard(card,usingOnPlayer,fromHand);

        player.glowImage.gameObject.SetActive(false);
        enemy.glowImage.gameObject.SetActive(false);

        fromHand.RemoveCard(card);

        return false;
    }

    internal bool CardValid(Card cardBeingPlayed, Player usingOnPlayer, Hand fromHand)
    {
        bool valid = false;

        if (cardBeingPlayed==null)
            return false;

        if (fromHand.isPlayers)
        {
            if (cardBeingPlayed.cardData.cost<=player.mana)
            {
                if (usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
                if (!usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
            }
        }
        else // from enemys
        {
            if (cardBeingPlayed.cardData.cost<=enemy.mana)
            {
                if (!usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
                if (usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
            }
        }
        return valid;
    }

    internal void CastCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        if (card.cardData.isMirrorCard)
        {
            usingOnPlayer.SetMirror(true);
            usingOnPlayer.PlayMirrorSound();
            NextPlayersTurn();
            isPlayable = true;
        }
        else
        {
            if (card.cardData.isDefenseCard) // health cards
            {
                usingOnPlayer.health += card.cardData.damage;
                usingOnPlayer.PlayHealSound();

                if (usingOnPlayer.health>usingOnPlayer.maxHealth)
                    usingOnPlayer.health = usingOnPlayer.maxHealth;

                UpdateHealths();

                StartCoroutine(CastHealEffect(usingOnPlayer));
            }
            else // Attack card
            {
                CastAttackEffect(card,usingOnPlayer);
            }
            
            if (fromHand.isPlayers)
                playerScore += card.cardData.damage;
            
             UpdateScore();
        }
        
        if (fromHand.isPlayers)
        {
            GameController.instance.player.mana -= card.cardData.cost;
            GameController.instance.player.UpdateManaBalls();
        }
        else
        {
            GameController.instance.enemy.mana -= card.cardData.cost;
            GameController.instance.enemy.UpdateManaBalls();
        }
    }

    private IEnumerator CastHealEffect(Player usingOnPlayer)
    {
        yield return new WaitForSeconds(0.5f);
        NextPlayersTurn();
        isPlayable = true;
    }

    internal void CastAttackEffect(Card card, Player usingOnPlayer)
    {
        GameObject effectGO = null;
        if (usingOnPlayer.isPlayer)
            effectGO = Instantiate(effectFromRightPrefab, canvas.gameObject.transform);
        else
            effectGO = Instantiate(effectFromLeftPrefab, canvas.gameObject.transform);

        Effect effect = effectGO.GetComponent<Effect>();
        if (effect)
        {
            effect.targetPlayer = usingOnPlayer;
            effect.sourceCard   = card;

            switch(card.cardData.damageType)
            {
                case CardData.DamageType.Fire:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multiFireBallImage;
                    else
                        effect.effectImage.sprite = fireBallImage;
                    effect.PlayFireballSound();
                break;
                case CardData.DamageType.Ice:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multiIceBallImage;
                    else
                        effect.effectImage.sprite = iceBallImage;
                    effect.PlayIceSound();
                break;
                case CardData.DamageType.Both:
                    effect.effectImage.sprite = fireAndIceBallImage;
                    effect.PlayFireballSound();
                    effect.PlayIceSound();
                break;
                case CardData.DamageType.Destruct:
                    effect.effectImage.sprite = DestructBallImage;
                    effect.PlayDestructSound();
                    break;
            }
        }
    }

    internal void UpdateHealths()
    {
        player.UpdateHealth();
        enemy.UpdateHealth();

        if (player.health<=0)
        {
            StartCoroutine(GameOver());
        }
        if (enemy.health<=0)
        {
            playerKills++;
            playerScore+=100;
            UpdateScore();
            StartCoroutine(NewEnemy());
        }
    }

    private IEnumerator NewEnemy()
    {
        enemy.gameObject.SetActive(false);
        enemysHand.ClearHand();
        yield return new WaitForSeconds(0.75f);
        SetUpEnemy();
        enemy.gameObject.SetActive(true);
        StartCoroutine(DealHands());
    }

    private void SetUpEnemy()
    {
        enemy.mana = 0;
        enemy.health = 5;
        enemy.UpdateHealth();
        enemy.isFire = true;
        if (UnityEngine.Random.Range(0,2)==1)
            enemy.isFire = false;
        if (enemy.isFire)
            enemy.playerImage.sprite = fireDemon;
        else
            enemy.playerImage.sprite = iceDemon;
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    internal void NextPlayersTurn()
    {
        playersTurn = !playersTurn;
        bool enemyIsDead = false;
        if (playersTurn)
        {
            if (player.mana<5)
                player.mana++;
        }
        else // enemy
        {
            if (enemy.health>0)
            {
                if (enemy.mana<5)
                    enemy.mana++;
            }
            else
                enemyIsDead = true;
        }

        if (enemyIsDead)
        {
            playersTurn = !playersTurn;
            if (player.mana<5)
                player.mana++;
        }
        else
        {
            SetTurnText();
            if (!playersTurn)
                MonstersTurn();
        }

        player.UpdateManaBalls();
        enemy.UpdateManaBalls();
    }

    internal void SetTurnText()
    {
        if (playersTurn)
        {
            turnText.text = "Merlin's Turn";
        }
        else
        {
            turnText.text = "Enemys Turn";
        }
    }

    private void MonstersTurn()
    {
        Card card = AIChooseCard();
        StartCoroutine(MonsterCastCard(card));
    }

    private Card AIChooseCard()
    {
        List<Card> available = new List<Card>();
        for(int i=0;i<3;i++)
        {
            if (CardValid(enemysHand.cards[i], enemy, enemysHand))
                available.Add(enemysHand.cards[i]);
            else if (CardValid(enemysHand.cards[i], player, enemysHand))
                available.Add(enemysHand.cards[i]);
        }

        if (available.Count ==0) // none available
        {
            NextPlayersTurn();
            return null;
        }
        int choice = UnityEngine.Random.Range(0,available.Count);
        return available[choice];
    }

    private IEnumerator MonsterCastCard(Card card)
    {
        yield return new WaitForSeconds(0.5f);

        if (card)
        {
            TurnCard(card);

            yield return new WaitForSeconds(2);

            if (card.cardData.isDefenseCard)
                UseCard(card,enemy,enemysHand);
            else // attack card
                UseCard(card,player,enemysHand);

            yield return new WaitForSeconds(1);

            enemyDeck.DealCard(enemysHand);            

            yield return new WaitForSeconds(1);
        }
        else // no card to choose, so skip turn
        {
            enemySkipTurn.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            enemySkipTurn.gameObject.SetActive(false);
        }
    }

    internal void TurnCard(Card card)
    {
        Animator animator = card.GetComponentInChildren<Animator>();
        if (animator)
        {
            animator.SetTrigger("Flip");
        }
        else
            Debug.LogError("No Animator found.");
    }

    private void UpdateScore()
    {
        scoreText.text = "Demons killed: "+playerKills.ToString()+".  Score: "+playerScore.ToString();
    }

    internal void PlayPlayerDieSound()
    {
        playerDieAudio.Play();
    }

    internal void PlayEnemyDieSound()
    {
        enemyDieAudio.Play();
    }
}
