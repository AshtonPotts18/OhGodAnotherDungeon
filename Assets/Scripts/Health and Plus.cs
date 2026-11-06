using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SaveData;


public class HealthandPlus : MonoBehaviour, DamagingEnemy.IDamageable, IVisitable
{
    SaveData PlayerSave;  //Player save Data

    LoadData PlayerData;
    public Slider slider;

    public string GameOver;

    [SerializeField] private string TagName;

    [SerializeField] private float InvincFrames = 0.25f;

    private float clock = 0;

    public void Start()
    {
        if (GameObject.Find("SaveData").GetComponent<SaveData>())
        {
            PlayerSave = GameObject.Find("SaveData").GetComponent<SaveData>(); //gets the Player Save Data
        }

        if (GameObject.Find("LoadData").GetComponent<LoadData>())
        {
            PlayerData = GameObject.Find("LoadData").GetComponent<LoadData>();
        }
    }

    public void Accept(IVisitor visitor) //This is for SaveData and brings it to function to save the players Health
    {
        visitor.VisitHealth(this);
    }

    public void Save()
    {
        var Health = gameObject.GetComponent<IVisitable>();
        if (PlayerSave != null)
        {  //Needs set condition to not be recorded every second!!
            Health.Accept(PlayerSave);                           // This row as well.
        }

        if (PlayerData != null)
        {
            //new WaitForSeconds(0.5f);
            Health.Accept(PlayerData);
        }
        
    }
    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.CompareTag(TagName) && clock <= 0)
        {
            slider.value -= 5;
            clock = InvincFrames;
            string objectName = other.gameObject.name;
            PlayerPrefs.SetString("LastTouched", objectName);

            if(slider.value == 0)
            {
                GameManager.Instance.EndGame();
                SceneManager.LoadScene("Game Over");
                
            }

        }
        else
        {
            clock -= Time.deltaTime;
        }
    }




    public void SetMax(int Max)
    {
        slider.maxValue = Max;
        slider.value = Max;
    }
    public void SetHealth(int health)
    {
        slider.value = health;
    }

    public void TakeDamage(float amount)
    {
        if (slider == null || amount <= 0f)
            return;

        slider.value = Mathf.Max(0f, slider.value - amount);
    }

    public float GetHealth()
    {
        return slider.value;
    }

}
