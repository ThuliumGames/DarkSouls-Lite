using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
	
	//Defaults
	[Header("Defaults")]
	public float def_Speed;
	public float g_Accel, a_Accel, jump_Height, g_Rot, a_Rot, roll_Speed, damage, gravity, stun;
	public float[] charge_Damages;
	
	//Stats
	[Header("Stats")]
	[Header("Multipliers")]
	public float g_Multi=1;
	public float a_Multi=1, g_Accel_Multi=1, a_Accel_Multi=1, jump_multi=1, roll_Multi=1, weapon_Scale_Multi=1, damage_Multi=1, charge_Speed_Multi=1, charge_Damage_Multi=1, overcharge_Multi=1, gravity_Multi=1;
	[Header("Dividers")]
	public float g_Div=1;
	public float a_Div=1, g_Accel_Div=1, a_Accel_Div=1, jump_Div=1, roll_Div=1, weapon_Scale_Div=1, damage_Div=1, charge_Speed_Div=1, charge_Damage_Div=1, overcharge_Div=1, gravity_Div=1;
	[Header("Adders")]
	public float g_Add=0;
	public float a_Add=0, g_Accel_Add=0, a_Accel_Add=0, jump_Add=0, roll_Add=0, extra_Jumps=0, weapon_Scale_Add=0, damage_Add=0, charge_Speed_Add=0, charge_Damage_Add=0, overcharge_Add=0, gravity_Add=0;
	
	
	[Header("Specials")]
	public string element_Type;
	public float chance_Orb;
	public float chance_Lazer;
	
	//Final Values
	[HideInInspector]
	public float g_Speed, a_Speed, tot_g_Accel, tot_a_Accel, j_Height, g_r_Speed, a_r_Speed, total_Damage, weapon_Scale_Total, tot_Gravity;
	
	public LayerMask lm;
	
	Rigidbody rb;
	[HideInInspector]
	public Animator anim;
	
	Transform cam, looker, vis;
	
	float maxSlope, actionSpeed, actionAccel, maxUp, rotationSpeed, airJump, yVel, preY, landing, stunTime;
	Vector3 preVel;
	
	[HideInInspector]
	public float jump, dodge, a1, a2, a3, a4, chargeLevel;
	
	bool action;
	
	TrailRenderer weaponTrail;
	
	public Material[] trailMats;
	MeshRenderer charge;
	
	[HideInInspector]
	public bool canMove;
	
	Vector3 gravity_Vector;
	
	public GameObject HitStun;
	
	void Start () {
		rb = GetComponent<Rigidbody>();
		anim = GetComponentInChildren<Animator>();
		vis = anim.transform;
		cam = Camera.main.transform;
		looker = cam.GetChild(0).transform;
		weaponTrail = GetComponentInChildren<TrailRenderer>();
		charge = weaponTrail.transform.parent.GetChild(1).GetComponent<MeshRenderer>();
	}
	
	void FixedUpdate () {
		
		//preVel = rb.velocity;
		
//CalculateStats
		CalculateStats ();
//Inputs
		if (Input.GetButton("Jump") && jump == -1) {
			jump = 0.5f;
		}
		if (Input.GetButton("Dodge") && dodge == -1) {
			dodge = 0.5f;
		}
		if (Input.GetButton("Fire1") && a1 == -1) {
			a1 = 0.5f;
		}
		if (Input.GetButton("Fire2") && a2 == -1) {
			a2 = 0.5f;
		}
		/*anim.SetBool("Dodge", dodge>0);
		anim.SetBool("Jump", jump>0);*/
		
		jump = Mathf.Clamp01(jump-(0.02f*Globals.animationSpeed))-System.Convert.ToInt32(jump<=0&&!Input.GetButton("Jump"));
		dodge = Mathf.Clamp01(dodge-(0.02f*Globals.animationSpeed))-System.Convert.ToInt32(dodge<=0&&!Input.GetButton("Dodge"));
		a1 = Mathf.Clamp01(a1-(0.02f*Globals.animationSpeed))-System.Convert.ToInt32(a1<=0&&!Input.GetButton("Fire1"));
		a2 = Mathf.Clamp01(a2-(0.02f*Globals.animationSpeed))-System.Convert.ToInt32(a2<=0&&!Input.GetButton("Fire2"));
		
//---------------------------------------------------------------------------------------------------------------------------------------//

//Setup
		Vector2 moVec = new Vector2 (Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		float joyMag = moVec.magnitude;
		looker.eulerAngles = new Vector3 (0, cam.eulerAngles.y, 0);
		Vector3 wantedVel = /*vis.forward * joyMag * Mathf.Clamp01(Vector3.Dot (vis.forward, */looker.TransformDirection(new Vector3 (moVec.x, 0, moVec.y))/*))*/;
		Look();
//---------------------------------------------------------------------------------------------------------------------------------------//

//Disable Movement if in Action
		if ((!anim.GetCurrentAnimatorStateInfo(0).IsName("Ground") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Air"))) {
			if (!canMove) {
				if (anim.GetFloat("CrawlSpeed")>0) {
					//Set Custom Animation Speed
					rb.velocity = Vector3.Lerp(rb.velocity, new Vector3 (wantedVel.x*anim.GetFloat("ActionSpeed")*actionSpeed, rb.velocity.y, wantedVel.z*anim.GetFloat("ActionSpeed")*actionSpeed), Mathf.Clamp((tot_g_Accel*System.Convert.ToInt32(anim.GetBool("OnGround")))+(tot_a_Accel*System.Convert.ToInt32(!anim.GetBool("OnGround"))), 0, 10)*0.02f);
				} else {
					//Set Custom Animation Speed
					rb.velocity = Vector3.Lerp(rb.velocity, new Vector3 (vis.forward.x*anim.GetFloat("ActionSpeed")*actionSpeed, rb.velocity.y, vis.forward.z*anim.GetFloat("ActionSpeed")*actionSpeed), Mathf.Clamp((tot_g_Accel*System.Convert.ToInt32(anim.GetBool("OnGround")))+(tot_a_Accel*100*System.Convert.ToInt32(!anim.GetBool("OnGround"))), 0, 10)*0.02f);
				}
				
				rotationSpeed = anim.GetFloat("RotationSpeed");
			}
			action = true;
		} else {
			action = false;
			canMove = true;
			gravity_Vector = new Vector3 (0, tot_Gravity, 0);
			anim.SetFloat("CrawlSpeed", 0);
			chargeLevel = Mathf.Clamp(chargeLevel-0.2f, 0, (float)(charge_Damages.Length-1));
		}
//Check for Ground
		RaycastHit hit;
		bool onGround = false;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f+(System.Convert.ToInt32(anim.GetBool("OnGround"))*0.5f)+(System.Convert.ToInt32(anim.GetCurrentAnimatorStateInfo(0).IsName("Dive")||anim.GetCurrentAnimatorStateInfo(0).IsName("Diving"))*0.666f), lm)) {
			if (hit.normal.y > maxSlope/* && (rb.velocity.y <= 0 || anim.GetCurrentAnimatorStateInfo(0).IsName("Air"))*/ && !anim.GetCurrentAnimatorStateInfo(0).IsName("Jump")) {
				onGround = true;
			}
			if (hit.normal.y < 0.5f) {
				maxSlope += (0.02f);
			} else {
				maxSlope = 0;
			}
		}
		
		if (onGround) {
			//Setup
			rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
			anim.SetBool("OnGround", true);
			airJump = 0;
			if (g_Speed > 0) {
				anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), rb.velocity.magnitude/g_Speed, 0.08f));
			} else {
				anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), 0, 0.08f));
			}
			
			//Reset Rotaiton Speed
			if (!action) {
				rotationSpeed = g_Rot;
			}
			
			//Snap to Ground
			transform.position = new Vector3(transform.position.x, (hit.point.y+1f), transform.position.z);
			
			if (canMove) {
				//Grounded Movement
				Vector3 v = /*new Vector3 (vis.forward.x, 0, vis.forward.z)*/Vector3.Lerp(new Vector3 (rb.velocity.x, 0, rb.velocity.z), new Vector3 (wantedVel.x*g_Speed, 0, wantedVel.z*g_Speed), tot_g_Accel*0.02f)/*.magnitude*/;
				rb.velocity = new Vector3 (v.x, rb.velocity.y, v.z);
			}
			
			//Dodge Action
			if (dodge>0&&!action) {
				dodge = 0;
				Action ("Dodge", 0, false, g_r_Speed, 10, 0, 1);
			}
			
			//Jump Action
			if (jump>0&&!action&&landing<=0) {
				jump = 0;
				Action ("Jump", 0, true, 0, 10, 1, 1);
			}
			
			landing = Mathf.Clamp01(landing-0.02f);
			maxUp = rb.velocity.y;
		} else {
			landing = 0.0625f;
			//Setup
			rb.constraints = RigidbodyConstraints.FreezeRotation;
			anim.SetBool("OnGround", false);
			if (a_Speed > 0) {
				anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), rb.velocity.magnitude/a_Speed, 0.08f));
			} else {
				anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), 0, 0.08f));
			}
			
			//Reset Y Velocity
			yVel = 0;
			
			//Reset Rotaiton Speed
			if (!action) {
				rotationSpeed = a_Rot;
			}
			
			if (canMove) {
				//Aerial Movement
				Vector3 v = Vector3.Lerp(new Vector3 (rb.velocity.x, rb.velocity.y, rb.velocity.z), new Vector3 (wantedVel.x*a_Speed, rb.velocity.y, wantedVel.z*a_Speed), tot_a_Accel*0.02f);
				rb.velocity = new Vector3 (v.x, rb.velocity.y, v.z);
			}
			
			//Set Max Y Velocity So you can't fly up walls
			if (rb.velocity.y < maxUp) {
				maxUp = rb.velocity.y;
			} else {
				maxUp += (gravity_Vector.y*0.02f)*Globals.animationSpeed;
			}
			
			rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -Mathf.Infinity, maxUp), rb.velocity.z);
			
			//Air Dodge Action
			if (dodge>0&&!action) {
				dodge = 0;
				Action ("Dive", 0, false, a_r_Speed, 10, 0.5f, 2);
			}
			
			//Air Jump Action
			if (jump>0&&!action&&airJump<extra_Jumps) {
				airJump++;
				jump = 0;
				Action ("Jump", 0, true, 0, 10, 1, 1);
			}
		}
		
		//anim Speeds
		anim.SetFloat("GroundSpeedMulti", Mathf.Clamp((g_Speed/def_Speed)*anim.GetFloat("Speed"), 1f, 3f));
		anim.SetFloat("AirSpeedMulti", Mathf.Clamp((a_Speed/def_Speed)*anim.GetFloat("Speed"), 1f, 3f));
		if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude > 0) {
			anim.SetFloat("AbsoluteSpeed", (-rb.velocity.y/new Vector2(rb.velocity.x, rb.velocity.z).magnitude));
		} else {
			anim.SetFloat("AbsoluteSpeed", 1);
		}
		
		//Light Attack Action
		if (a1>0&&!action) {
			a1=0;
			if (onGround) {
				Action ("Attack", 0, false, g_Speed, 10, 0, 1);
			} else {
				Action ("Air Attack", 0, false, a_Speed, 10, 0, 1);
			}
		}
		
		//Heavy Attack Action
		if (a2>0&&!action) {
			a2=0;
			if (onGround) {
				Action ("Heavy Attack", 0, false, g_Speed, 10, 0, 1);
			} else {
				Action ("Heavy Air Attack", 0, false, a_Speed, 10, 0, 1);
			}
		}
		
		//Animation Canceling
		if (anim.GetFloat("RollCancel") > 0.5f) {
			//Dodge Action
			if (dodge>0) {
				dodge = 0;
				if (onGround) {
					Action ("Dodge", 0, false, g_r_Speed, 10, 0, 1);
				} else {
					Action ("Dive", 0, false, a_r_Speed, 10, 0.5f, 2);
				}
			}
		}
		
		if (anim.GetFloat("JumpCancel") > 0.5f) {
			//Jump Action
			if (jump>0) {
				jump = 0;
				Action ("Jump", 0, true, 0, 10, 1.25f, 1);
			}
		}
		
		WeaponMoves();
		
		//Move out of Ceiling
		RaycastHit ceil;
		if (Physics.Raycast(transform.position+(Vector3.up*0.25f), Vector3.up, out ceil, 0.75f, lm) && onGround) {
			Vector3 norm = new Vector3 (ceil.normal.x, 0, ceil.normal.z).normalized;
			Vector3 vel = new Vector3 (rb.velocity.x, 0, rb.velocity.z);
			float dot = Mathf.Clamp01(Mathf.Sign(Vector3.Dot(norm, vel.normalized)));
			Vector3 tmpRot = transform.eulerAngles;
			transform.LookAt(transform.position+norm);
			Vector3 velRel = transform.InverseTransformDirection(vel).normalized*vel.magnitude;
			velRel.z *= -0.25f;
			velRel.y = 0;
			velRel = transform.TransformDirection(velRel).normalized*vel.magnitude;
			transform.eulerAngles = tmpRot;
			rb.velocity = new Vector3((rb.velocity.x*dot)+norm.x, Mathf.Clamp(rb.velocity.y, -Mathf.Infinity, maxUp), (rb.velocity.z*dot)+norm.z);
		}
		
		//Get Y Velocity
		if (yVel < Mathf.Clamp((transform.position.y - preY)*50, 0, a_Speed/2.0f)&&onGround) {
			yVel = Mathf.Clamp((transform.position.y - preY)*50, 0, a_Speed/2.0f);
		} else {
			yVel = Mathf.Clamp(yVel-(0.2f*(a_Speed/2.0f)), 0, Mathf.Infinity);
		}
		preY = transform.position.y;
		//Update Trail
		weaponTrail.material = trailMats[System.Convert.ToInt32((anim.GetFloat("AttackFrames") > 0 && (""+(anim.GetFloat("AttackFrames")*100))==(""+(float)Mathf.Floor(anim.GetFloat("AttackFrames")*100))))];
		
		//Update Anim Layer
		anim.SetLayerWeight(1, Mathf.Floor(anim.GetFloat("CrawlSpeed")));
		
		//Set Charge Color
		Color color = new Color ((float)System.Convert.ToInt32((((chargeLevel+(6-(charge_Damages.Length-1)))+1)%6)<3), (float)System.Convert.ToInt32((((chargeLevel+(6-(charge_Damages.Length-1)))-1)%6)<3), (float)System.Convert.ToInt32((((chargeLevel+(6-(charge_Damages.Length-1)))+3)%6)<3), (float)chargeLevel/(float)(charge_Damages.Length));
		charge.material.SetColor("_BaseColor", color);
		
		//Set Charge Speed
		anim.SetFloat("Charge Speed", ((10+charge_Speed_Add)/10.0f)*charge_Speed_Multi);
		
		//Set Animation Speed
		/*if (stunTime<=0) {
			if (Globals.animationSpeed==1) {
				rb.velocity/=anim.speed;
			}
		}*/
		anim.speed = Globals.animationSpeed;
		/*if (stunTime<=0) {
			rb.velocity*=Globals.animationSpeed;
		}*/
		//Add Gravity to Player
		rb.AddForce(0, gravity_Vector.y*-Mathf.Clamp(rb.velocity.y/(-gravity_Vector.y), -Mathf.Infinity, -1f), 0, ForceMode.Acceleration);
	}
	
	void WeaponMoves () {
		if (anim.IsInTransition(0)) {
			//Update Trail
			//weaponTrail.material = trailMats[0];
			
			anim.SetBool("Trans", false);
			if (anim.GetCurrentAnimatorStateInfo(0).IsTag("LightAttack") && !anim.GetAnimatorTransitionInfo(0).IsName("Jump -> Air")) {
				a1=0;
			}
			if (anim.GetCurrentAnimatorStateInfo(0).IsTag("HeavyAttack") && !anim.GetAnimatorTransitionInfo(0).IsName("Jump -> Air")) {
				a2=0;
			}
			if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Jump") && !anim.GetAnimatorTransitionInfo(0).IsName("Jump -> Air")) {
				a1=0;
				a2=0;
			}
		}
		if (a1 > 0) {
			anim.SetBool("a1Pressed", true);
		} else {
			anim.SetBool("a1Pressed", false);
		}
		if (a1 == -1) {
			anim.SetBool("a1Released", true);
		} else {
			anim.SetBool("a1Released", false);
		}
		
		if (a2 > 0) {
			anim.SetBool("a2Pressed", true);
		} else {
			anim.SetBool("a2Pressed", false);
		}
		if (a2 == -1) {
			anim.SetBool("a2Released", true);
		} else {
			anim.SetBool("a2Released", false);
		}
		
		if (jump > 0) {
			anim.SetBool("jumpPressed", true);
		} else {
			anim.SetBool("jumpPressed", false);
		}
	}
	
	void Look() {
		// get the joystick input
		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");

		// if there is no input, exit
		if (Mathf.Abs(horizontal) < 0.01f && Mathf.Abs(vertical) < 0.01f) return;

		// calculate the direction to look at
		Vector3 direction = new Vector3(horizontal, 0, vertical);
		
		Vector3 oldRot = vis.eulerAngles;
		
		vis.eulerAngles = new Vector3 (0, cam.transform.eulerAngles.y, 0);
		direction = vis.TransformDirection(direction);
		
		vis.eulerAngles = oldRot;
		
		// smoothly rotate the object to look in the direction
		Quaternion targetRotation = Quaternion.LookRotation(direction);
		vis.rotation = Quaternion.Slerp(Quaternion.Euler(oldRot), targetRotation, rotationSpeed * (0.02f));
		Quaternion vTmp = vis.rotation;
		transform.rotation = vis.rotation;
		vis.rotation =  vTmp;
	}
	
	void Action (string animName, int layer, bool overridesMovement, float speedMulti, float slide, float jumpForce, float gravityScale) {
		anim.SetBool("Trans", false);
		action = true;
		anim.Play(animName, layer);
		canMove = overridesMovement;
		actionAccel = slide;
		actionSpeed = speedMulti;
		gravity_Vector = new Vector3 (0, tot_Gravity*gravityScale, 0);
		if (jumpForce != 0) {
			rb.constraints = RigidbodyConstraints.FreezeRotation;
			anim.SetBool("OnGround", false);
			rb.velocity = new Vector3 (rb.velocity.x, (j_Height*jumpForce)+Mathf.Clamp(yVel, 0, Mathf.Infinity), rb.velocity.z);
			maxUp = (j_Height*jumpForce)+Mathf.Clamp(yVel, 0, Mathf.Infinity);
		}
		anim.SetFloat("CrawlSpeed", 0);
	}
	
	public void DoJump (float jumpForce, bool doUp) {
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		anim.SetBool("OnGround", false);
		if (!doUp) {
				if (jumpForce>0) {
					rb.velocity = new Vector3 (rb.velocity.x, (j_Height*jumpForce)+Mathf.Clamp(yVel, 0, Mathf.Infinity), rb.velocity.z);
					maxUp = (j_Height*jumpForce)+Mathf.Clamp(yVel, 0, Mathf.Infinity);
				} else if (rb.velocity.y>jumpForce) {
					rb.velocity = new Vector3 (rb.velocity.x, jumpForce, rb.velocity.z);
					maxUp = jumpForce;
				}
		} else if (rb.velocity.y<(jump_Height*jumpForce)&&jumpForce>0) {
			if (rb.velocity.y < 0) {
				rb.velocity = new Vector3 (rb.velocity.x, rb.velocity.y+(jump_Height*jumpForce*(Mathf.Clamp((-rb.velocity.y), 1, Mathf.Infinity)/((jump_Height*jumpForce)*2.0f))), rb.velocity.z);
			}
			maxUp = rb.velocity.y;
		}
	}
	
	public void ChangeGravity (float gravityScale) {
		gravity_Vector = new Vector3 (0, tot_Gravity*gravityScale, 0);
	}
	
	public void ActionControl (float speed) {
		anim.SetFloat("CrawlSpeed", speed);
	}
	
	public void MakeGround () {
		actionSpeed = g_Speed;
	}
	
	public void MakeAir () {
		actionSpeed = a_Speed;
	}
	
	public void MakeRoll () {
		actionSpeed = g_r_Speed;
	}
	
	public void MakeDive () {
		actionSpeed = a_r_Speed;
	}
	
	void CalculateStats () {
		tot_Gravity = (-((gravity+((gravity_Add/10.0f)*gravity))*(float)gravity_Multi)/gravity_Div)*Globals.animationSpeed;
		weapon_Scale_Total = ((10+weapon_Scale_Add)*weapon_Scale_Multi)/weapon_Scale_Div;
		j_Height = ((jump_Height+((jump_Add/10.0f)*jump_Height))*jump_multi)/jump_Div;
		g_Speed = (((def_Speed+g_Add)*g_Multi)/g_Div)*Globals.animationSpeed;
		a_Speed = (((def_Speed+a_Add)*a_Multi)/a_Div)*Globals.animationSpeed;
		tot_g_Accel = Mathf.Clamp(((((g_Accel+(g_Accel_Add))*g_Accel_Multi)/g_Accel_Div)*Mathf.Clamp01((def_Speed/g_Speed)+((((g_Accel+(g_Accel_Add))*g_Accel_Multi)/g_Accel_Div)/10.0f)))*(1-(stunTime/stun)), 0, 10)*Globals.animationSpeed;
		tot_a_Accel = Mathf.Clamp(((((a_Accel+(a_Accel_Add))*a_Accel_Multi)/a_Accel_Div)*Mathf.Clamp01((def_Speed/a_Speed)+((((a_Accel+(a_Accel_Add))*a_Accel_Multi)/a_Accel_Div)/10.0f)))*(1-(stunTime/stun)), 0, 10)*Globals.animationSpeed;
		g_r_Speed = ((((def_Speed*roll_Speed)+roll_Add)*roll_Multi)/roll_Div)+(((def_Speed+g_Add)*g_Multi)*roll_Speed)*Globals.animationSpeed;
		a_r_Speed = ((((def_Speed*roll_Speed)+roll_Add)*roll_Multi)/roll_Div)+(((def_Speed+a_Add)*a_Multi)*roll_Speed)*Globals.animationSpeed;
		total_Damage = Mathf.Floor((((((((damage*anim.GetFloat("AttackFrames")))+(((charge_Damage_Add*System.Convert.ToInt32(chargeLevel<(charge_Damages.Length-1)&&chargeLevel>0))+(overcharge_Add*System.Convert.ToInt32(chargeLevel==(charge_Damages.Length-1)))))))*charge_Damages[(int)chargeLevel]*(((charge_Damage_Multi*System.Convert.ToInt32(chargeLevel<(charge_Damages.Length-1)))/charge_Damage_Div)+((overcharge_Multi*System.Convert.ToInt32(chargeLevel==(charge_Damages.Length-1)))/overcharge_Div)))+damage_Add)*damage_Multi)/damage_Div);
		if (stunTime>0) {
			stunTime-=0.02f*Globals.animationSpeed;
		} else {
			stunTime = 0;
		}
	}
	
	public bool Hit (Vector3 damage_Took) {
		
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dive") || anim.GetCurrentAnimatorStateInfo(0).IsName("Dive") || (anim.GetCurrentAnimatorStateInfo(0).IsName("Dodge") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime<0.4f) || stunTime>0) return(false);
		
		Vector3 add_Force = new Vector3(damage_Took.x, new Vector2(damage_Took.x, damage_Took.z).magnitude, damage_Took.z).normalized*Mathf.Log(Mathf.Clamp(new Vector3(damage_Took.x, new Vector2(damage_Took.x, damage_Took.z).magnitude, damage_Took.z).magnitude, 1, Mathf.Infinity)*100)*(1.5f*((System.Convert.ToInt32(anim.GetBool("OnGround")))+1));
		
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		anim.SetBool("OnGround", false);
		rb.velocity = add_Force;
		preVel = add_Force;
		maxUp = add_Force.y;
		stunTime = stun*new Vector2(damage_Took.x, damage_Took.z).magnitude;
		Instantiate(HitStun);
		
		return(true);
	}
}
