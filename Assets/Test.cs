using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        
    }

    public void Attack()
    {
        anim.SetTrigger("Attack");
    }

    public void StrongAttack()
    {
        anim.SetTrigger("StrongAttack");
    }

    public void Damage_s()
    {
        anim.SetTrigger("Damage_s");
    }

    public void Damage_l()
    {
        anim.SetTrigger("Damage_l");
    }

    public void Dead()
    {
        anim.SetTrigger("Dead");
    }

    public bool IsDead()
    {
        return anim.GetBool("Dead");
    }

    public void HangOn()
    {
        anim.SetTrigger("HangOn");
    }
}
