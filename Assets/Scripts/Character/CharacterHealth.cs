using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHealth : MonoBehaviour
{
    private const float MaxHealth = 125.0f;
    private bool IsHealing =>
        canHealing && Input.GetKey(healingKey);
    
    [Header("Настройки")]
    [SerializeField] private float currentHealth = 100.0f;
    [SerializeField] private int medicalSyringe = 2;
    
    [Header("Настройки Управления")]
    [SerializeField] private KeyCode healingKey = KeyCode.G;
    
    [Header("Функциональные настройки")]
    public static bool canHealing = true;
    
    [Header("UI")] 
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text medicalSyringeText;
    [SerializeField] private Image heathBar;

    //Events to change UI, Health and Kill Character if health < 0
    public static event Action OnGetDamage;
    public static event Action OnChangeHealth;
    public static event Action OnDead;

    //Start - Awake functions
    private void Awake()
    {
        currentHealth = 100.0f;
    }
    private void Start()
    {
        healthText.text = currentHealth.ToString();
        heathBar.fillAmount = currentHealth / 100;
    }
    
    private void OnEnable()
    {
        OnChangeHealth += ChangeUi;
    }
    private void OnDisable()
    {
        OnChangeHealth += ChangeUi;
    }
    private void OnDestroy()
    {
        OnChangeHealth += ChangeUi;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
            GetDamage(30);
        if(Input.GetKeyDown(healingKey))
            HealCharacter();
    }

    private void HealCharacter()
    {
        if (IsHealing)
        {
            if (medicalSyringe > 0 && (currentHealth + 25) <= MaxHealth)
            {
                medicalSyringe -= 1;
                currentHealth += 25;
                
                OnChangeHealth?.Invoke();
            }
        }
    }
    
    public void GetDamage(float damage)
    {
        currentHealth -= damage;
        OnChangeHealth?.Invoke();
        
        if (currentHealth > 0)
            OnGetDamage?.Invoke();
        else
            OnDead?.Invoke();

    }

    private void ChangeUi()
    {
        healthText.text = currentHealth.ToString();
        heathBar.fillAmount = currentHealth / 100;

        medicalSyringeText.text = medicalSyringe.ToString();
    }
}
