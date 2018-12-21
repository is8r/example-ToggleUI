using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneUI : SingletonMonoBehaviour<PlaneUI>
{
    public GameObject player;
    public GameObject plane;

    private void Start()
    {
        plane.active = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OnToggleMenu();
        }
    }

    //表示をトグル
    public void OnToggleMenu()
    {
        OnUpdatePlane();
        plane.active = !plane.active;
    }

    //位置を調整
    public void OnUpdatePlane()
    {
        transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        transform.rotation = player.transform.rotation;
    }
}
