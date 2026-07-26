using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

	public Slider slider;
	public Gradient gradient;
	public Image fill;
	public GameObject boss;
	public int MaxHealth = 4000;
	
	[Header("Proximity Settings")]
	public float detectionDistance = 15f;
	private Transform playerTransform;
	private CanvasGroup canvasGroup;
	private float targetAlpha = 0f;
	public float fadeSpeed = 5f;

	void Start() {
		if (slider == null || fill == null)
		{
			Debug.LogError("HealthBar: slider hoặc fill chưa được gán trong Inspector.");
			enabled = false;
			return;
		}
		
		SetMaxHealth(MaxHealth);
		ResolvePlayerTransform();
		ResolveBossReference();
		
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
		
		canvasGroup.alpha = 0f;
	}

	void Update() {
		if (slider == null || fill == null || canvasGroup == null)
		{
			Debug.LogError("HealthBar: thiếu tham chiếu UI, hãy gán Slider/Fill/CanvasGroup.");
			return;
		}
		
		ResolvePlayerTransform();
		boss = ResolveBossReference();
		
		if (boss == null)
		{
			FadeTo(0f);
			return;
		}
		
		EnemyHealth enemyHealth = boss.GetComponent<EnemyHealth>();
		if (enemyHealth == null)
		{
			FadeTo(0f);
			return;
		}
		
		if (slider.maxValue != enemyHealth.maxHealth)
		{
			slider.maxValue = enemyHealth.maxHealth;
		}
		
		slider.value = Mathf.Clamp(enemyHealth.GetCurrentHealth(), 0f, slider.maxValue);
		fill.color = gradient.Evaluate(slider.normalizedValue);
		UpdateHealthBarVisibility();
		canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
	}

	void ResolvePlayerTransform() {
		if (playerTransform != null) return;
		
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		if (playerObj != null)
			playerTransform = playerObj.transform;
	}

	GameObject ResolveBossReference() {
		if (boss != null) return boss;
		
		BossSpawner spawner = FindObjectOfType<BossSpawner>();
		if (spawner != null)
		{
			boss = spawner.GetBossInstance();
			if (boss != null) return boss;
		}
		
		BossAI bossAI = FindObjectOfType<BossAI>();
		if (bossAI != null)
		{
			boss = bossAI.gameObject;
			return boss;
		}
		
		GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
		if (bossObj != null)
		{
			boss = bossObj;
			return boss;
		}
		
		return null;
	}

	void UpdateHealthBarVisibility() {
		if (playerTransform == null || boss == null) {
			FadeTo(0f);
			return;
		}
		
		float distanceToBoss = Vector3.Distance(playerTransform.position, boss.transform.position);
		targetAlpha = (distanceToBoss <= detectionDistance) ? 1f : 0f;
	}

	void FadeTo(float alpha) {
		targetAlpha = alpha;
		if (canvasGroup != null)
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
	}

	public void SetMaxHealth(int health) {
		if (slider == null || fill == null)
		{
			Debug.LogError("HealthBar: không thể set máu vì slider hoặc fill chưa được gán.");
			return;
		}
		
		slider.maxValue = health;
		slider.value = health;
		fill.color = gradient.Evaluate(1f);
	}
}
