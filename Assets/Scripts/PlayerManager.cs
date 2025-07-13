using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager Instance;

    void Awake()
    {
        Instance = this;
    }
    #endregion
    public GameObject player;
}
