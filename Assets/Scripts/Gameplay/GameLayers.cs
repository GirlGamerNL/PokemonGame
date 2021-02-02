using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{

   [SerializeField] public LayerMask solidObjectLayer;
   [SerializeField] public LayerMask interactableLayer;
   [SerializeField] public LayerMask grassLayer;
   [SerializeField] public LayerMask playerLayer;
   [SerializeField] public LayerMask fovLayer;

    public static GameLayers i { get; set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask SolidLayer { get => solidObjectLayer; }
    public LayerMask InteractableLayer { get => interactableLayer; }
    public LayerMask GrassLayer { get => grassLayer; }
    public LayerMask Player { get => playerLayer; }
    public LayerMask Fov { get => fovLayer; }

}
