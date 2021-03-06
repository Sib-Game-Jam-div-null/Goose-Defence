using UnityEngine;
using System.Collections.Generic;

public class Wall : Target
{
	/// <summary>
	/// Хп стены
	/// </summary>
	public ProgressBar HpBar;

	/// <summary>
	/// Спрайты стены
	/// </summary>
	public List<Sprite> WallSprites;

	/// <summary>
	/// Префаб полоски здоровья
	/// </summary>
	[SerializeField]
	private GameObject _hpBarPrefab = default;

	private SpriteRenderer _renderer;

	public void Initialize (int maxHp)
	{
		MaxHP = maxHp;
		HP = MaxHP;
		var hpBarObj = GameObject.Instantiate(_hpBarPrefab, new Vector3(-2f, 10f, -6f), Quaternion.identity);
		hpBarObj.transform.SetParent(transform);
		HpBar = hpBarObj.GetComponent<ProgressBar>();
		_renderer = GetComponentInChildren<SpriteRenderer>();
		HpBar.Initialize(HP);
		Damaged += _getDamage;

		HpBar.Hp = HP;
	}

	public override void DestroySelf ()
	{
		Destroy(HpBar.gameObject);
		for ( int i = 0; i < 3; i++ )
			Destroy(transform.Find("wall " + i.ToString())
				 .GetComponent<BoxCollider>());
		this.enabled = false;
	}

	public override void OnCollided (Collider collider)
	{
		base.OnCollided(collider);
		var goose = collider.gameObject.GetComponentInParent<Goose>();
		if ( goose == null )
			return;

		if ( goose.State != GooseState.Attack )
			goose.StartAttack(this);
	}

	private void _getDamage (Target obj)
	{
		HpBar.Hp = HP;
		if ( HP == MaxHP )
		{
			_renderer.sprite = WallSprites[0];
		}
		else if ( HP < MaxHP / 2 )
		{
			_renderer.sprite = WallSprites[1];
		}
		if ( HP <= 0 )
		{
			_renderer.sprite = WallSprites[2];
			HpBar.Destroy();
		}
	}
}
