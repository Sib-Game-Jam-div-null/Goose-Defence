using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_manager : Singleton<UI_manager>
{
	//"Папки с текстом либо с картинкой" main menu
	public GameObject background, history, pressKeyToStart;
	//"Папки с текстом либо с картинкой" game
	public GameObject moneyStat, dangerStat, scoreStat;
	public GameObject infoPanel;
	public GameObject buyPanel;
	public GameObject UIInMenu, UIInGame;
	public GameObject battlefield;
	//Если игра не запущена, значит мы в меню, если нет, то UI надо изменить
	bool isGameStarted = false, canSkip = false;
	string historyStr = "     Весь мир был поражён вирусом, из-за которого гуси стали неимоверно большими, голодными, и практически поработили человечество с помощью звона колокольчиков. Единственное, что осталось у людей - это большой колокол и фермы, на которых они выращивают корм чтобы кормить гусей ради своего спасения. Это место - последняя надежда человечества. С минуты на минуту гуси начнут своё последнее наступление, защитите колокол!";
	[Header("InfoAboutTower")]
	public Sprite[] towerPictures = new Sprite[9];
	public Image displayTower;
	public Button Accept;
	public Sprite musicOn, musicOff;
	public Text title, infoAboutTower, damage, radius, speed, reload, cost, health, attackRange;
	public Text upgradeDamage, upgradeRadius, upgradeSpeed, upgradeReload, upgradeHealth, upgradeAttackRange;
	public Animator transitor;
	Tower tower;
	Place place;
	TowerType selectedType;
	float selectedPrice;
	[SerializeField]
	float _gameEndTimeOut = 2.5f;

	public GameObject backGroundMusicButton;
	[Header("Audio")]
	public AudioSource ButtonCloseSound;
	public AudioSource ButtonSelect;
	public AudioSource ButtonBuy;
	public AudioSource ButtonBuild;
	public AudioSource Writing;
	public AudioSource BackGroundMusic;
	public AudioSource resultMusic;
	public AudioClip winSound;
	public AudioClip looseSound;
	//public AudioSource ButtonSelect;

	//public AudioSource ButtonBuy;
	//public AudioSource ButtonUpgrade;
	// public AudioSource Writing;


	[Header("ResultsAtTheEndOfGame")]
	public Text scoreText;
	public Image resultImage;
	public Image signImage;
	public Sprite[] resultSprites = new Sprite[2];
	public Sprite[] resultSigns = new Sprite[2];
	public GameObject resultScreen;
	public GameObject transitToEnd;

	public void UI_TurnOnMenu ()
	{
		ButtonCloseSound.Play();
		StartCoroutine(WaitForTransitionToMenu());
	}

	void UI_TurnOnGame ()
	{
		StartCoroutine(WaitForTransitionToGame());
	}

	void UI_SetAmountOfGold (int amount)
	{
		moneyStat.GetComponent<Text>().text = amount.ToString();
	}

	void setAmountOfMoney (int gold)
	{
		moneyStat.GetComponentInChildren<Text>().text = gold.ToString();
		setStatus(gold);
	}

	public void setDangerLvl (int lvl)
	{
		dangerStat.GetComponent<Text>().text = lvl + " уровень угрозы";
	}

	void setStatus (int gold)
	{
		if ( Accept.GetComponentInChildren<Text>().text == "" )
			Accept.interactable = false;
		else if ( selectedPrice <= gold )
			Accept.interactable = true;
		else
			Accept.interactable = false;
	}

	void WriteScore (int score)
	{
		scoreStat.GetComponent<Text>().text = "Счёт: " + score;
	}

	// Start is called before the first frame update
	void Start ()
	{
		Game.Instance.UpdateScore += WriteScore;
		Game.Instance.WinGame += PrintScore;
		Game.Instance.LooseGame += PrintScore;
		Game.Instance.UpdateGold += setAmountOfMoney;
		GooseFabric.Instance.UpdateGooseLvl += setDangerLvl;
		StartWriting();
	}



	public void StartWriting ()
	{
		isGameStarted = false;
		canSkip = true;
		UIInMenu.SetActive(true);
		StartCoroutine(ReadHistory());
		resultScreen.SetActive(false);
		UIInGame.SetActive(false);
		history.GetComponentInChildren<Text>().text = "";
		canSkip = false;
		isGameStarted = false;
		pressKeyToStart.GetComponent<Text>().text = "Нажмите на любую клавишу, чтобы пропустить историю";
	}


	public void CloseWindow ()
	{
		ButtonCloseSound.Play();
		Application.Quit();
	}

	public void CloseInfoPanel ()
	{
		ButtonCloseSound.Play();
		infoPanel.SetActive(false);
	}

	public void CloseBuyPanel ()
	{
		ButtonCloseSound.Play();
		buyPanel.SetActive(false);
	}

	public void SelectFirstTypeOfTower ()
	{
		ButtonSelect.Play();
		WindowBuyTower(TowerType.Tomato);
	}

	public void SelectSecondTypeOfTower ()
	{
		ButtonSelect.Play();
		WindowBuyTower(TowerType.Cabbage);
	}

	public void SelectThirdTypeOfTower ()
	{
		ButtonSelect.Play();
		WindowBuyTower(TowerType.Peas);
	}

	bool isBackSound = true;
	public void BackGroundSoundOnOff ()
	{
		if ( !isBackSound )
		{
			BackGroundMusic.Play();
			backGroundMusicButton.GetComponent<Image>().sprite = musicOn;
			isBackSound = true;
		}
		else
		{
			BackGroundMusic.Stop();
			backGroundMusicButton.GetComponent<Image>().sprite = musicOff;
			isBackSound = false;
		}
		ButtonSelect.Play();
	}

	public void BackGroundSoundOnOff (bool value)
	{
		isBackSound = value;
		if ( value )
		{
			BackGroundMusic.Play();
		}
		else
		{
			BackGroundMusic.Stop();
		}
	}


	public void WindowBuyTower (TowerType type)
	{
		selectedType = type;
		infoPanel.SetActive(true);
		//Прячем статы улучшения
		upgradeDamage.gameObject.SetActive(false);
		upgradeRadius.gameObject.SetActive(false);
		upgradeSpeed.gameObject.SetActive(false);
		upgradeReload.gameObject.SetActive(false);
		upgradeHealth.gameObject.SetActive(false);
		upgradeAttackRange.gameObject.SetActive(false);
		ShowMainStats(TowerFabric.Instance.TowerStats(type, 1));
		Accept.GetComponentInChildren<Text>().text = "Купить";
		setStatus(Game.Instance.Money);
		buyPanel.SetActive(false);
	}

	void WindowUpgradeTower (Tower tower)
	{
		if ( tower.Stats.Level == 3 )
		{
			//Прячем статы улучшения
			upgradeDamage.gameObject.SetActive(false);
			upgradeRadius.gameObject.SetActive(false);
			upgradeSpeed.gameObject.SetActive(false);
			upgradeReload.gameObject.SetActive(false);
			upgradeHealth.gameObject.SetActive(false);
			upgradeAttackRange.gameObject.SetActive(false);
			ShowMainStats(tower.Stats);
			infoAboutTower.text = "Башня максимального уровня";
			Accept.GetComponentInChildren<Text>().text = "";
			setStatus(Game.Instance.Money);
		}
		else
		{
			var newLevel = TowerFabric.Instance.NextTowerStats(tower.Stats);
			ShowMainStats(tower.Stats);
			upgradeDamage.gameObject.SetActive(true);
			upgradeRadius.gameObject.SetActive(true);
			upgradeSpeed.gameObject.SetActive(true);
			upgradeReload.gameObject.SetActive(true);
			upgradeHealth.gameObject.SetActive(true);
			upgradeAttackRange.gameObject.SetActive(true);
			ShowUpgradeStats(tower.Stats);
			selectedPrice = newLevel.Cost;
			cost.text = "Стоимость: " + selectedPrice;
			Accept.GetComponentInChildren<Text>().text = "Улучшить";
			setStatus(Game.Instance.Money);
		}
	}

	void ShowMainStats (TowerStats stats)
	{
		int towerRank = (int)stats.Type * 3 + stats.Level;
		title.text = stats.Name;
		infoAboutTower.text = stats.Description;
		displayTower.sprite = towerPictures[towerRank - 1];
		damage.text = "Урон: " + stats.Projectile.Damage;
		attackRange.text = "Радиус атаки: " + stats.Range;
		radius.text = "Зона поражения: " + stats.Projectile.ExplosionRange;
		speed.text = "Скорость снаряда: " + stats.Projectile.Velocity;
		reload.text = "Перезарядка: " + stats.AttackDelay;
		health.text = "Здоровье: " + stats.MaxHP;
		selectedPrice = stats.Cost;
		cost.text = "Стоимость: " + selectedPrice;
	}

	void ShowUpgradeStats (TowerStats stats)
	{
		var newStats = TowerFabric.Instance.NextTowerStats(stats);
		if ( newStats == null )
			return;
		upgradeDamage.text = "+" + ( newStats.Projectile.Damage - stats.Projectile.Damage );
		upgradeRadius.text = "+" + ( newStats.Projectile.ExplosionRange - stats.Projectile.ExplosionRange );
		upgradeAttackRange.text = "+" + ( newStats.Range - stats.Range );
		upgradeSpeed.text = "+" + ( newStats.Projectile.Velocity - stats.Projectile.Velocity );
		upgradeReload.text = "+" + ( newStats.AttackDelay - stats.AttackDelay );
		upgradeHealth.text = "+" + ( newStats.MaxHP - stats.MaxHP );
	}

	// Update is called once per frame
	void Update ()
	{
		if ( isGameStarted == false && canSkip == true && Input.anyKey == true )
		{
			isGameStarted = true;
			StartCoroutine(WaitForTransitionToGame());
		}
		if ( Input.GetMouseButtonDown(0) )
		{
			Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
			foreach ( var hit in Physics2D.RaycastAll(rayPos, Vector2.zero) )
			{
				if ( hit.transform.tag == "UI_tower" || hit.transform.tag == "Tower" || hit.transform.tag == "Place" )
				{
					if ( hit.transform.tag == "UI_tower" )
					{
						var parent = hit.transform.parent;
						tower = parent.transform.gameObject.GetComponent<Tower>();
						WindowUpgradeTower(tower);
						infoPanel.SetActive(true);
					}
					else if ( hit.transform.tag == "Tower" )
					{
						tower = hit.transform.gameObject.GetComponent<Tower>();
						WindowUpgradeTower(tower);
						infoPanel.SetActive(true);
					}
					else if ( hit.transform.tag == "Place" )
					{
						place = hit.transform.gameObject.GetComponent<Place>();
						if ( place.IsFree )
							buyPanel.SetActive(true);
					}
					break;
				}
			}
		}
	}

	public void ClickAccept ()
	{
		if ( Accept.GetComponentInChildren<Text>().text == "Улучшить" )
		{
			if ( tower == null || tower.IsDestroyed )
			{
				CloseInfoPanel();
				return;
			}
			Game.Instance.DecreaseMoney((int)selectedPrice);
			TowerFabric.Instance.UpgradeTower(tower.TowerOrder);
			infoPanel.SetActive(false);
			ButtonBuild.Play();
		}
		else if ( Accept.GetComponentInChildren<Text>().text == "Купить" )
		{
			Game.Instance.DecreaseMoney((int)selectedPrice);
			TowerFabric.Instance.PlaceTower(place.Order, selectedType, 1);
			infoPanel.SetActive(false);
			ButtonBuy.Play();
		}
		moneyStat.GetComponentInChildren<Text>().text = Game.Instance.Money.ToString();
	}

	IEnumerator ReadHistory ()
	{
		float textSpeed = 0.05f;
		Writing.Play();
		for ( int i = 0; i < historyStr.Length; i++ )
		{
			if ( canSkip == false && Input.anyKey == true )
			{
				textSpeed /= 10f;
			}
			history.GetComponentInChildren<Text>().text += historyStr[i];
			yield return new WaitForSeconds(textSpeed);
		}
		canSkip = true;
		pressKeyToStart.GetComponent<Text>().text = "Нажмите на любую клавишу, чтобы начать игру";
		Writing.Stop();
	}

	IEnumerator WaitForTransitionToMenu ()
	{
		transitor.SetTrigger("End");
		yield return new WaitForSeconds(1.5f);
		UIInMenu.SetActive(true);
		history.GetComponentInChildren<Text>().text = "";
		StartCoroutine(ReadHistory());
		resultScreen.SetActive(false);
		UIInGame.SetActive(false);
		canSkip = false;
		isGameStarted = false;
		transitor.SetTrigger("Start");
	}

	IEnumerator WaitForTransitionToGame ()
	{
		transitor.SetTrigger("End");
		yield return new WaitForSeconds(1.5f);
		UIInMenu.SetActive(false);
		infoPanel.SetActive(false);
		buyPanel.SetActive(false);
		UIInGame.SetActive(true);
		resultScreen.SetActive(false);
		transitor.SetTrigger("Start");
		//Начало игры
		//TowerFabric.Instance.placeTower(0, new TowerStatsList.TowerTomatoT1());
		//TowerFabric.Instance.placeTower(1, new TowerStatsList.TowerTomatoT2());
		//TowerFabric.Instance.placeTower(2, new TowerStatsList.TowerTomatoT3());
		//TowerFabric.Instance.placeTower(3, new TowerStatsList.TowerCabbageT1());
		//TowerFabric.Instance.placeTower(4, new TowerStatsList.TowerPeasT1());

		Game.Instance.StartGame();
		//TowerFabric.Instance.placeTower(0, new TowerStatsList.TowerPeasT3());
		//TowerFabric.Instance.placeTower(1, new TowerStatsList.TowerPeasT3());
	}

	public void PrintScore (bool result, int score)
	{
		StartCoroutine(Result(result, score));
	}

	public void ExitGame ()
	{
		ButtonCloseSound.Play();
		Application.Quit();
	}

	public void PlayAgain ()
	{
		ClearLevel.Instance.Clear();
		GooseFabric.Instance.Clear();
		ButtonSelect.Play();
		BackGroundSoundOnOff(true);
		StartWriting();
	}

	IEnumerator Result (bool result, int score)
	{
		yield return new WaitForSeconds(_gameEndTimeOut);
		BackGroundSoundOnOff(false);

		transitToEnd.SetActive(true);
		yield return new WaitForSeconds(1f);
		resultScreen.SetActive(true);
		scoreText.text = "Счёт: " + score;
		if ( result )
		{
			resultMusic.clip = winSound;
			resultMusic.Play();
			resultImage.sprite = resultSprites[0];
			signImage.sprite = resultSigns[0];
		}
		else
		{
			resultMusic.clip = looseSound;
			resultMusic.Play();
			resultImage.sprite = resultSprites[1];
			signImage.sprite = resultSigns[1];
		}
	}
}
