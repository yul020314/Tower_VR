using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MyCharacterControllerDriver : CharacterControllerDriver
{
    void Update()
    {
        UpdateCharacterController();
    }
}
