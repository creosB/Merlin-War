using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Effect : MonoBehaviour
{
    public Player targetPlayer = null;
    public Card   sourceCard   = null;
    public Image  effectImage  = null;

    public AudioSource iceAudio         = null;
    public AudioSource fireballAudio    = null;
    public AudioSource DestructBallAudio = null;

    public void EndTrigger()
    {
        bool bounce = false;
        if (targetPlayer.hasMirror())
        {
            bounce = true;
            targetPlayer.SetMirror(false);
            targetPlayer.PlaySmashSound();

            if (targetPlayer.isPlayer)
            {
                GameController.instance.CastAttackEffect(sourceCard, GameController.instance.enemy);
            }
            else
            {
                GameController.instance.CastAttackEffect(sourceCard, GameController.instance.player);
            }
        }
        else
        {
            int damage = sourceCard.cardData.damage;
            if (!targetPlayer.isPlayer) // monster / enemy
            {
                if (sourceCard.cardData.damageType == CardData.DamageType.Fire && targetPlayer.isFire)
                    damage = damage / 2;
                if (sourceCard.cardData.damageType == CardData.DamageType.Ice && !targetPlayer.isFire)
                    damage = damage / 2;
            }
            targetPlayer.health -= damage;
            targetPlayer.PlayHitAnim();

            GameController.instance.UpdateHealths();

            if (targetPlayer.health<=0)
            {
                targetPlayer.health =0;
                if (targetPlayer.isPlayer)
                {
                    GameController.instance.PlayPlayerDieSound();
                }
                else
                {
                    GameController.instance.PlayEnemyDieSound();
                }
            }

            if (!bounce)
                GameController.instance.NextPlayersTurn();

            GameController.instance.isPlayable = true;
        }
        Destroy(gameObject);
    }

    internal void PlayIceSound()
    {
        iceAudio.Play();
    }

    internal void PlayFireballSound()
    {
        fireballAudio.Play();
    }
    internal void PlayDestructSound()
    {
        DestructBallAudio.Play();
    }
}
