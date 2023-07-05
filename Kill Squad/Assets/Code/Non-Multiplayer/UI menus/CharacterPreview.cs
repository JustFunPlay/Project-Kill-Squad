using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPreview : MonoBehaviour
{
    [SerializeField] private CharacterInfoBase infoBase;

    [SerializeField] private TMPro.TextMeshProUGUI healthValue;
    [SerializeField] private TMPro.TextMeshProUGUI movementValue;
    [SerializeField] private TMPro.TextMeshProUGUI speedValue;
    [SerializeField] private TMPro.TextMeshProUGUI armorValue;
    [SerializeField] private TMPro.TextMeshProUGUI shootingValue;
    [SerializeField] private TMPro.TextMeshProUGUI meleeValue;
    [SerializeField] private TMPro.TextMeshProUGUI attackValue;

    // Start is called before the first frame update
    void Start()
    {
        healthValue.text = infoBase.health.ToString();
        movementValue.text = infoBase.movement.ToString();
        speedValue.text = infoBase.speed.ToString();
        armorValue.text = infoBase.armor.ToString();
        shootingValue.text = infoBase.ranged.ToString();
        meleeValue.text = infoBase.melee.ToString();
        attackValue.text = infoBase.attacks.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
