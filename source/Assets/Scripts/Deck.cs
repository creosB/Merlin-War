using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    public List<CardData> cardDatas = new List<CardData>();

    public void Create()
    {
        // first create a list of carddata for the pack
        List<CardData> cardDataInOrder = new List<CardData>();
        foreach(CardData cardData in GameController.instance.cards)
        {
            for(int i=0;i<cardData.numberInDeck;i++)
                cardDataInOrder.Add(cardData);
        }

        // then randomise the order
        while(cardDataInOrder.Count>0)
        {
            int randomIndex = Random.Range(0,cardDataInOrder.Count);
            cardDatas.Add(cardDataInOrder[randomIndex]);
            cardDataInOrder.RemoveAt(randomIndex);
        }
    }

    private CardData RandomCard()
    {
        CardData result = null;

        if (cardDatas.Count == 0)
            Create();

        result = cardDatas[0];
        cardDatas.RemoveAt(0);

        return result;
    }

    private Card CreateNewCard(Vector3 position, string animName)
    {
        GameObject newCard = GameObject.Instantiate(GameController.instance.cardPrefab,
                                                    GameController.instance.canvas.gameObject.transform);
        newCard.transform.position = position;
        Card card = newCard.GetComponent<Card>();
        if (card)
        {
            card.cardData = RandomCard();
            card.Initialise();

            Animator animator = newCard.GetComponentInChildren<Animator>();
            if (animator)
            {
                animator.CrossFade(animName,0);
            }
            else
            {
                Debug.LogError("No Animator found!");
            }

            return card;
        }
        else
        {
            Debug.LogError("No card component found!");
            return null;
        }
    }

    internal void DealCard(Hand hand)
    {
        for (int h = 0; h<3; h++)
        {
            if (hand.cards[h]==null)
            {
                if (hand.isPlayers)
                {
                    GameController.instance.player.PlayDealSound();
                }
                else
                {
                    GameController.instance.enemy.PlayDealSound();
                }
                hand.cards[h] = CreateNewCard(hand.positions[h].position, hand.animNames[h]);
                return;
            }
        }
    }


}
