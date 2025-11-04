using System.Collections;
using System.Collections.Generic;
using SS.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class ScreenReference : ScriptableObject
{
    [SerializeField] protected GameObject _screenPrefab;

    public GameObject ScreenPrefab
    {
        get
        {
            return _screenPrefab;
        }

        set
        {
            _screenPrefab = value;
        }
    }
}
