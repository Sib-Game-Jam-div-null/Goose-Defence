﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerFabric : Singleton<TowerFabric>
{
    public const int MaxTowerCount = 6;

    /// <summary>
    /// Количество не пустых мест
    /// </summary>
    public int TowerCount { get { return Places.Count(place => place.isFree); } }

    public GameObject PlacePrefab;

    public GameObject[] ProjectilePrefabs;
    public GameObject[] TowerPrefabs;
    public List<Tower> Towers;

    public GameObject[] place;
    public Place[] Places;


    public int TowerSelectedOrder;

    [SerializeField]
    Vector3 UpSpawnPoint;

    [SerializeField]
    Vector3 DownSpawnPoint;

    void Awake()
    {
        Towers = new List<Tower>();
        Places = new Place[MaxTowerCount];
        for (int i = 0; i < MaxTowerCount; i++)
        {
            Towers.Add(null);
            Vector3 pos = place[i].transform.position;
            pos.z = -3+Mathf.Abs(pos.y / 10);
            var obj = GameObject.Instantiate(PlacePrefab, pos, Quaternion.identity);
            Places[i] = obj.GetComponent<Place>();
            Places[i].Initialize(i, true, pos);
        }

        Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        UpSpawnPoint = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        DownSpawnPoint = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));

        //////////////////////////
        /// СПАВН ПСЕВДО СТЕНЫ
        var wall = new GameObject("wall");
        wall.transform.position = new Vector3(-10000, 0, 0);
        var blank = wall.AddComponent<Tower>();
        Towers.Add(blank);
    }

    public TowerStatsList GetStatsByOrder(int order)
    {
        if (Towers[order] != null)
            return Towers[order].info;
        return null;
    }

    public void placeTower(int order, TowerStatsList stats)
    {
        // НЕ ДЕЛАЮ БАЩНЮ ДОЧЕРНЕЙ 
        // + new Vector3(0,2f)
        Vector3 pos = Places[order].spawnPoint.position;
        GameObject tower = GameObject.Instantiate(TowerPrefabs[stats.PrefabId], pos, Quaternion.identity);
        Towers[order] = tower.AddComponent<Tower>();
        Towers[order].Initialize(stats, ProjectilePrefabs[stats.PrefabId / 3]);
        Towers[order].MakeDamage();
        Towers[order].TowerDestroyed += deleteTower;
        Places[order].isFree = false;
    }

	public TowerStatsList GetInfoByOrder(int order)
	{
		return Towers[order].info;
	}


	public Vector3 FindNearTower(Vector3 pos)
    {
        Vector3 temp = Vector3.down*1000;
        float distance = 9999;
        foreach (var tower in Towers)
        {
            if (tower == null)
                continue;
            Vector3 towerPos = tower.transform.position;
            var dist = Vector3.Distance(towerPos, pos);
            if ( dist<distance)
            {
                distance = dist;
                temp = towerPos;
            }
        }
        return (temp == Vector3.down * 1000) ? Towers[6].transform.position : temp;
    }

    public void TryDamageTower(int order, int dmg)
    {
        if (Towers[order])
            Towers[order].GetDamage(dmg);
        else
            Game.Instance.DamageWall(dmg);
    }

    /// <summary>
    /// Обработчик уничтожения башни
    /// </summary>
    /// <param name="tower"></param>
    void deleteTower(Tower tower)
    {
        int index = Towers.IndexOf(tower);
        destroyTower(index);
    }

    /// <summary>
    /// УНИЧТОЖАЕТ ЗАДАННУЮ БАШНЮ
    /// </summary>
    /// <param name="order"></param>
    public void destroyTower(int order)
    {
        Towers[order].RemoveTower();
        Towers[order] = null;
        Places[order].isFree = true;
    }
}