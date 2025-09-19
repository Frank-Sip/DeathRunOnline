
using UnityEngine;

[CreateAssetMenu(fileName = "Player Ani Config", menuName = "Player/Player Animation Set")]
public class IndividualAnimationsSO : ScriptableObject
{
    public AnimationClip[] idleAnimations;
    public AnimationClip[] mainAnimations;
}
