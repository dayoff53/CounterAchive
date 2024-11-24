using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : SkillEffect
{
    public override void ApplySkillEffect(UnitController user, UnitController target)
    {
        base.ApplySkillEffect(user, target);

        NormalAttack();
    }
}
