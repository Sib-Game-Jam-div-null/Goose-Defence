﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SocialPlatforms;
using System;

public delegate void TowerEvent(Tower tower);

public class Tower : MonoBehaviour
{
    public event TowerEvent TowerDestroyed;
    
    [SerializeField]
    public Animator Anim;

    [SerializeField]
    public GameObject SpawnPos;

    /// <summary>
    /// Точка спавна снарядов
    /// </summary>
    [SerializeField]
    Transform spawnPoint;
    /// <summary>
    /// Префаб снаряда
    /// </summary>
    [SerializeField]
    GameObject ProjectilePrefab;
	[SerializeField]
	public TowerStatsList info = new TowerStatsList.TowerCabbageT1();
	
	public bool isAvailable { get; set; }
    public bool Destroyed
    {
        get
        {
            return HP > 0;
        }
    }

    float selfHp = 100f;
	/// <summary>
	/// Хп Башни, вызывает событие уничтожения уничтожение
	/// </summary>
	[SerializeField]
	float HP
    {
        get
        {
            return selfHp;
        }
        set
        {
            if (value <= 0)
            {
                selfHp = 0;
                TowerDestroyed?.Invoke(this);
            }
            else
                selfHp = value;
        }
    }

    public bool GetDamage(float dmg)
    {
        if (HP <= 0)
            return false;
        HP -= dmg;
        return Destroyed;
    }

    public void Initialize(TowerStatsList info, GameObject projectilePref)
    {
		this.info = info;
		int tmpHP = info.MaxHP;
		HP = (info.MaxHP);
        spawnPoint = transform.Find("SpawnPoint");
        ProjectilePrefab = projectilePref;
    }

    public void MakeDamage()
    {
        StartCoroutine("Attack");
        isAvailable = true;
    }

    public void StopDamage()
    {
        StopCoroutine("Attack");
        isAvailable = false;
    }


    private void Start()
    {
        spawnPoint = transform.Find("spawn_point");
    }
    public IEnumerator Attack()
    {

        while (true)
        {
            Goose aim = GooseFabric.Instance.FindGoose(transform.position, info.Range);
            // null или далеко
            if (aim == null || spawnPoint == null)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            // добавляю скрипт на префаб
            var projectile = GameObject.Instantiate(ProjectilePrefab, spawnPoint.position, Quaternion.identity);
            Projectile proj = projectile.GetComponent<Projectile>();

            float distance = Vector3.Distance(spawnPoint.position, aim.transform.position);
            float u1 = Math.Abs(info.Projectile.Velocity);
            float u2 = Math.Abs(aim.goose_speed);
			float angle = Mathf.Deg2Rad * (Vector3.Angle(spawnPoint.position - aim.transform.position, aim.Movement));
			float time = Mathf.Abs((Mathf.Sqrt(2) * Mathf.Sqrt(2 * distance * distance * u1 * u1 + distance * distance * u2 * u2 * Mathf.Cos(2 * angle) - distance * distance * u2 * u2) - 2 * distance * u2 * Mathf.Cos(angle)) / (2 * (u1 * u1 - u2 * u2)));

			Vector3 prediction = aim.Movement * time;
			//Debug.Log($"Angle = {angle} Time = {time}");

            proj.Loauch(spawnPoint.position, aim.transform.position + prediction, info.Projectile);

            yield return new WaitForSeconds(info.AttackDelay);
            // может быть не нужен
            yield return new WaitForEndOfFrame();
        }

    }

    public void RemoveTower()
    {
        StopCoroutine("Attack");
        this.enabled = false;
        GameObject.Destroy(this);
    }
}