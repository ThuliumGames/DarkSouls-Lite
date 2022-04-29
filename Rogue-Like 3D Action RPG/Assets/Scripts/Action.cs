using UnityEngine;

[System.Serializable]
public class followup {
    public Action move;

    [Header("0=auto 1=Y 2=B 3=RT  :  negative=hold")]
    public int button;

    [Header("time in frames")]
    public int startTime;
    public int endTime;

    [Header("% : 0-1")]
    public float startPoint;
}

[CreateAssetMenu(fileName = "Move", menuName = "ScrptblObj/NewMove", order = 1)]
public class Action : ScriptableObject {
    public AnimationClip clip;
    public float motionValue;
    [Header ("Fixed Stamina")]
    public float staminaUse;
    [Header ("Continuous Stamina (Only for holdable moves)")]
    public float staminaDrain;
    [Header ("")]
    public int thisButton;
    public followup[] followups;

    [Header("time in frames : '-' = Right Hand : '+1000' = both")]
    public int[] activeFrames;
    [Header("")]
    public int blockStart;
    public int blockEnd;
    [Header("")]
    public int parryStart;
    public int parryEnd;
}