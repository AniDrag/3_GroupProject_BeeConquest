using UnityEngine;

public class BeeCore : Stats
{
    //float xpToLevelMulti = 1;
    float healthMulti = 1;
    float staminaMulti = 1;
    float magicMulti = 1;
    float physicalDefMulti = 1;
    float magicDefMulti = 1;
    float statusDefMulti = 1;

    int vit = 1;
    int str = 1;
    int dex = 1;
    int agi = 1;
    int mag = 1;

    private Vector3 target;

    #region Cobat Logic

    #endregion
    //----------------------------------------------
    #region Traversal Logic
    void GoToDestination()
    {

    }
    #endregion
    //----------------------------------------------
    #region OffServer Calculations
    

    #endregion
    //----------------------------------------------
    #region Server Senders
    void SendRequestCollectPollin()
    {
        // get location data and trigger traversal
    }

    public void SetDestination(Vector3 destination)
    {
        target = destination;
        GoToDestination();
    }
    #endregion
}
