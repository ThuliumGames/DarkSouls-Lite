//https://github.com/arkms/IKFoot_Floor-Unity

using UnityEngine;

public class FootIKSmooth : MonoBehaviour
{
    public bool IkActive= true;
    [Range(0f, 1f)]
    public float WeightPositionRight= 1f;
	[Range(0f, 1f)]
	public float WeightRotationRight= 0f;
    [Range(0f, 1f)]
    public float WeightPositionLeft = 1f;
	[Range(0f, 1f)]
	public float WeightRotationLeft = 0f;

    Animator anim;
    [Tooltip("Offset for Foot position")]
    public Vector3 offsetFoot;
    [Tooltip("Layer where foot can adjust to surface")]
    public LayerMask RayMask;

	Transform head;
    void Start()
    {
        anim = GetComponent<Animator>();
		head = GameObject.Find("spine.006").transform;
    }

    RaycastHit hit;

    void OnAnimatorIK(int _layerIndex)
    {
        if(IkActive)
        {
			
			//anim.SetLookAtWeight(((Camera.main.transform.right*Input.GetAxis("Horizontal")) + (new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized*Input.GetAxis("Vertical"))).magnitude*0.5f);
			//anim.SetLookAtPosition(head.position+(Vector3.down*2)+((Camera.main.transform.right*Input.GetAxis("Horizontal")) + (new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized*Input.GetAxis("Vertical"))).normalized*10);
			
			Vector3 FootPos = anim.GetIKPosition(AvatarIKGoal.RightFoot); //get current foot position (After animation apply)
            if (Physics.Raycast(FootPos + Vector3.up, Vector3.down, out hit, 1.1f, RayMask)) //Throw raycast to down
            {
				anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, WeightPositionRight);
				anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, WeightRotationRight);
				anim.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + offsetFoot); //Set foot where raycast hit

                if (WeightRotationRight > 0f) //adjust foot if is enable
                {
                    //Little formula to calculate foot rotation (This can be better)
                    Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
                    anim.SetIKRotation(AvatarIKGoal.RightFoot, footRotation);
                }
            }
            else //Raycast does not hit anything, so we keep original position and rotation
            {
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
            }

			FootPos = anim.GetIKPosition(AvatarIKGoal.LeftFoot); //get current foot position
            if (Physics.Raycast(FootPos + Vector3.up, Vector3.down, out hit, 1.1f, RayMask)) //Throw raycast to down
            {
				anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, WeightPositionLeft);
				anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, WeightRotationLeft);
				anim.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + offsetFoot);

                if (WeightRotationLeft > 0f) //adjust foot if is enable
                {
                    //Little formula to calculate foot rotation (This can be better)
                    Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
                    anim.SetIKRotation(AvatarIKGoal.LeftFoot, footRotation);
                }
            }
            else //Raycast does not hit anything, so we keep original position and rotation
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
            }
			
			
			/*FootPos = anim.GetIKPosition(AvatarIKGoal.LeftHand); //get current foot position
            if (Physics.Raycast(FootPos + Vector3.up, Vector3.down, out hit, 1.1f, RayMask)) //Throw raycast to down
            {
				anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, WeightPositionLeft);
				anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, WeightRotationLeft);
				anim.SetIKPosition(AvatarIKGoal.LeftHand, hit.point + offsetFoot);

                if (WeightRotationLeft > 0f) //adjust foot if is enable
                {
                    //Little formula to calculate foot rotation (This can be better)
                    Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.TransformDirection(new Vector3(-0.0988373384f,0.0804239288f,0.99184835f).normalized), hit.normal), hit.normal);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, footRotation);
                }
            }
            else //Raycast does not hit anything, so we keep original position and rotation
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
            }*/
        }
        else //IK is turn off, we not set anything
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
			anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
			anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        }

    } //End OnAnimatorIK()
}