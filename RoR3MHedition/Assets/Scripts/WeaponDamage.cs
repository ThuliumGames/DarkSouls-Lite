using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamage : MonoBehaviour {
	
	Animator anim;
	Collider[] coll;
	
	Movement m;
	
	public GameObject damageNumber, orb, lazer;
	
	Transform direction;
	
	List<Collider> alreadyHit = new List<Collider>();
	
	public float damage;
	
	void Start () {
		m = GameObject.FindObjectOfType<Movement>();
		if (GetComponentInParent<Animator>()) {
			anim = GetComponentInParent<Animator>();
			coll = GetComponents<Collider>();
			TMPro.TextMeshProUGUI t = Instantiate(damageNumber).GetComponentInChildren<TMPro.TextMeshProUGUI>();
			t.text = "";
			Instantiate(orb, Vector3.down*100, new Quaternion (0, 0, 0, 1));
			Instantiate(lazer, Vector3.down*100, new Quaternion (0, 0, 0, 1));
			direction = GameObject.Find("spine.006").transform;
		} else {
			direction = transform;
		}
	}
	
	void FixedUpdate () {
		
		if (anim != null) {
			transform.localScale = (Vector3.one*(m.weapon_Scale_Total/10.0f))*0.000375f;
			if (anim.GetFloat("AttackFrames") > 0 && (""+(anim.GetFloat("AttackFrames")*100))==(""+(float)Mathf.Floor(anim.GetFloat("AttackFrames")*100))) {
				if (!coll[(int)anim.GetFloat("Coll Type")].enabled) {
					if (Random.Range(0, 100)<m.chance_Orb) {
						Rigidbody r = Instantiate(orb, transform.GetChild(2).position, transform.GetChild(2).rotation).GetComponent<Rigidbody>();
						r.velocity = new Vector3 (direction.forward.x, 0, direction.forward.z).normalized*20;
					}
				}
				
				if (Random.Range(0, 100)<m.chance_Lazer) {
					Rigidbody r = Instantiate(lazer, transform.GetChild(2).position, transform.GetChild(2).rotation).GetComponent<Rigidbody>();
					r.velocity = new Vector3 (direction.forward.x, 0, direction.forward.z).normalized*20;
				}
					
				coll[(int)anim.GetFloat("Coll Type")].enabled = true;
			} else {
				foreach (Collider c in coll) {
					c.enabled = false;
				}
				alreadyHit.Clear();
			}
		} else {
			transform.localScale = (Vector3.one*(m.weapon_Scale_Total/10.0f))*0.75f;
		}
	}
	
	void OnTriggerEnter (Collider other) {
		if (other.gameObject.layer == LayerMask.NameToLayer("eHurtBox") && !alreadyHit.Contains(other)) {
			TMPro.TextMeshProUGUI t = Instantiate(damageNumber, other.ClosestPoint(transform.position), Quaternion.Euler(Vector3.zero)).GetComponentInChildren<TMPro.TextMeshProUGUI>();
			if (anim != null) {
				t.text = ""+m.total_Damage;
				alreadyHit.Add(other);
				other.SendMessageUpwards("Hit", ((new Vector3 (other.transform.position.x, 0, other.transform.position.z)-new Vector3 (m.transform.position.x, 0, m.transform.position.z)).normalized*(((m.total_Damage/m.damage_Multi)-m.damage_Add)/m.damage))+new Vector3(0, m.total_Damage, 0));
			} else {
				t.text = ""+damage;
				Destroy(gameObject);
			}
		}
	}
}
