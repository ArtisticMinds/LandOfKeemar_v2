using UnityEngine;

public class EffectActivation : StateMachineBehaviour
{
    public string _trigger = "PagliaioExit";
    public string checkColliderName= "Bip001 Pelvis";
    [SerializeField]  GameObject instantiateObject;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Instantiate(instantiateObject, animator.rootPosition, Quaternion.identity);
//        Debug.Log("caduta nano");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime <= 1 || animator.IsInTransition(0)) return;
        var cam = Camera.allCameras[0];

        //impossible
        if (cam == null) return;

        Physics.Linecast(cam.transform.position, animator.transform.GetChild(1).position, out var hitInfo);
        if (!hitInfo.transform.name.Equals(animator.transform.name)) return;
        animator.SetTrigger(_trigger);
    }

 
}