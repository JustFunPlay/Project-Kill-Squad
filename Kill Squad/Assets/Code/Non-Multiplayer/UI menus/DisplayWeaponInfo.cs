using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayWeaponInfo : MonoBehaviour
{
    [SerializeField] private ScriptableWeapon weapon;
    [SerializeField] private TMPro.TextMeshProUGUI weaponName;
    [SerializeField] private TMPro.TextMeshProUGUI range;
    [SerializeField] private TMPro.TextMeshProUGUI attacks;
    [SerializeField] private TMPro.TextMeshProUGUI armourPen;
    [SerializeField] private TMPro.TextMeshProUGUI crit;
    [SerializeField] private TMPro.TextMeshProUGUI damage;

    void Start()
    {
        weaponName.text = weapon.weaponName;
        range.text = $"Range: {weapon.range}";
        if (weapon.type == WeaponType.Melee || weapon.type == WeaponType.Heavy)
            attacks.text = $"Attacks: +{weapon.attacks}";
        else if (weapon.type == WeaponType.Swift)
            attacks.text = $"Attacks: ×2";
        else
            attacks.text = $"Attacks: {weapon.attacks}";
        armourPen.text = $"AP: {weapon.armorPenetration}";
        crit.text = $"Crit: {weapon.crit}";
        damage.text = $"Damage: {weapon.damage}";
    }
}
