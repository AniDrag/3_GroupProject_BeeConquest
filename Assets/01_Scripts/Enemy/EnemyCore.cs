using UnityEngine;

public class EnemyCore : Stats
{
    public long storeDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// On interval. it will store damage and then every 2s/1s it will send damage data to server so it syncs across
    /// </summary>
    void UpdateToServer()
    {
     
        storeDamage = 0;
    }
}
