namespace CharacterSystem
{
    /// <summary>
    /// Player
    /// </summary>
    public class PlayerStatus : CharacterStatus
    {
        public override void Die()
        {
            base.Die();
            this.gameObject.SetActive(false);
        }

    }
}
