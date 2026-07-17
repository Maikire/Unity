using System.Collections;
using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// EnemyStatus
    /// </summary>
    public class EnemyStatus : CharacterStatus
    {
        [Tooltip("受伤声音")]
        public AudioClip HurtAudio;

        private SpriteRenderer sprite;
        private AudioSource audioSource;

        private void Awake()
        {
            sprite = this.GetComponentInChildren<SpriteRenderer>();
            audioSource = this.GetComponent<AudioSource>();
        }

        public override void Damage(float damage)
        {
            base.Damage(damage);
            this.StartCoroutine(Hurt());
        }

        /// <summary>
        /// Hurt
        /// </summary>
        /// <returns></returns>
        private IEnumerator Hurt()
        {
            sprite.color = new Color(0.5f, 0.5f, 0.5f);
            audioSource.PlayOneShot(HurtAudio);

            yield return new WaitForSeconds(0.06f);

            sprite.color = Color.white;
        }



    }
}
