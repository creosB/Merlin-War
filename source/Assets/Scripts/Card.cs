using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public CardData cardData = null;

    public Text titleText =null;
    public Text descriptionText = null;
    public Image damageImage = null;
    public Image costImage = null;
    public Image cardImage = null;
    public Image frameImage = null;
    public Image burnImage = null;

    public void Initialise()
    {
        if (cardData==null)
        {
            Debug.LogError("Card has no CardData!");
            return;
        }

        titleText.text = cardData.cardTitle;
        descriptionText.text = cardData.description;
        cardImage.sprite = cardData.cardImage;
        frameImage.sprite = cardData.frameImage;
        costImage.sprite = GameController.instance.healthNumbers[cardData.cost];
        damageImage.sprite = GameController.instance.damageNumbers[cardData.damage];
    }
}
