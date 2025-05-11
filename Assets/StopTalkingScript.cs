using UnityEngine;

public class StopTalkingScript : StateMachineBehaviour
{
    public string parameterNameToReset;
    public bool resetInt = true; // Keep only the bool for int reset
    public int intValueToSet = 0;   // The value to set the int parameter to.

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (resetInt)
        {
            animator.SetInteger(parameterNameToReset, intValueToSet);
        }
    }
}
